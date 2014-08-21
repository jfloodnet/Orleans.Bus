using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;
using Orleans.Bus;

namespace Sample
{
    [TestFixture]
    public class TopicFixture : ApplicationGrainFixture
    {
        const int MaxRetries = 3;
        static readonly TimeSpan RetryPeriod = TimeSpan.FromSeconds(5);
        
        Topic topic;
        const string subject = "ПТН ПНХ";
        Dictionary<string, TimeSpan> schedule;

        StorageProviderProxyMock<int, int, int> storage;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            schedule = new Dictionary<string, TimeSpan>
            {
                {"facebook", TimeSpan.FromMinutes(10)},
                {"twitter",  TimeSpan.FromMinutes(5)},
            };

            topic = new Topic
            {
                Bus = Bus,
                Timers = Timers,
                Reminders = Reminders,
                Storage = storage = new StorageProviderProxyMock<int, int, int>()
            };
        }

        [Test]
        public async void When_created()
        {
            await TopicCreated();

            IsTrue(()=> Reminders.Count() == schedule.Count, 
                   "Should schedule recurrent search reminder per each api");

            AssertReminderScheduled("facebook");
            AssertReminderScheduled("twitter");
        }

        void AssertReminderScheduled(string api)
        {
            var scheduled = Reminder(api);

            IsTrue(()=> scheduled.Id == api);
            IsTrue(()=> scheduled.Due == TimeSpan.Zero);
            IsTrue(()=> scheduled.Period == schedule[api]);
        }

        [Test]
        public async void When_receives_reminder()
        {
            // given
            await TopicCreated();

            // when
            await ReceiveReminder("facebook");

            // then
            IsTrue(()=> That("facebook").Queries().Count() == 1);
            IsTrue(()=> That("facebook").FirstQuery<Search>().Subject == subject);
        }
       
        [Test]
        public async void Aggregates_results_received_from_api()
        {
            // arrange
            await TopicCreated();

            Expect("facebook", Query<Search>()
                .Return(100));

            // act
            await ReceiveReminder("facebook");
            await ReceiveReminder("facebook");

            // assert
            IsTrue(()=> topic.Total == 100 * 2);
        }

        [Test]
        public async void Flushes_state_after_every_successful_search()
        {
            // arrange
            await TopicCreated();

            const int result = 100;
            Expect("facebook", Query<Search>()
                .Return(result));

            // act
            await ReceiveReminder("facebook");
            await ReceiveReminder("facebook");

            // assert
            IsTrue(() => storage.Recorded.Count == 2);

            IsTrue(() => storage.Recorded.ElementAt(0).IsWriteState && 
                         storage.Recorded.ElementAt(0).Argument<int>() == result);
            
            IsTrue(() => storage.Recorded.ElementAt(1).IsWriteState && 
                         storage.Recorded.ElementAt(0).Argument<int>() == result);
        }

        [Test]
        public async void Schedules_periodic_retries_on_first_api_failure()
        {
            // arrange
            await TopicCreated();

            Expect("facebook", Query<Search>()
                .Throw(new ApiUnavailableException("facebook.com")));

            // act
            await ReceiveReminder("facebook");           

            // assert
            var scheduled = Timer<string>("facebook");
            IsTrue(()=> scheduled.State == "facebook");
            IsTrue(()=> scheduled.Callback == topic.RetrySearch);
            IsTrue(()=> scheduled.Due == RetryPeriod);
            IsTrue(()=> scheduled.Period == RetryPeriod);
        }

        [Test]
        public async void Ignores_executing_scheduled_searches_while_retrying()
        {
            // arrange
            await TopicCreated();
            RetriesScheduled("facebook");
            
            // act
            await ReceiveReminder("facebook");

            // assert
            IsTrue(()=> That("facebook").DidNotReceiveAnyQueries());
        }

        [Test]
        public async void Continue_executing_scheduled_searches_if_retry_succeeds()
        {
            // arrange
            await TopicCreated();
            RetriesScheduled("facebook");

            // see that timer was registered
            IsTrue(()=> Timers.Count() == 1);

            // return something
            Expect("facebook", Query<Search>()
                .Return(122));

            // act
            await topic.RetrySearch("facebook");

            // see that timer was registered
            IsFalse(() => Timers.Any(), "Should unregister retry timer");

            /* now, check regular searching works as expected  #1# */

            // arrange
            Reset();

            // act 
            await ReceiveReminder("facebook");

            // assert sent search query
            IsTrue(() => That("facebook").Queries().Count() == 1);
            IsTrue(() => That("facebook").FirstQuery<Search>().Subject == subject);            
        }

        [Test]
        public async void Disables_scheduled_searches_after_3_consecutive_falied_retries()
        {
            // arrange
            await TopicCreated();
            RetriesScheduled("facebook");

            // throw
            Expect("facebook", 
                Query<Search>()
                    .Throw(new ApiUnavailableException("facebook.com")));

            // act
            for (var i = 0; i < MaxRetries; i++)
                await topic.RetrySearch("facebook");

            // assert
            IsTrue(()=> Reminders.Count() == 1);
            AssertReminderScheduled("twitter");
        }

        [Test]
        public async void Rethrows_unknown_exceptions()
        {
            // arrange
            await TopicCreated();

            // throw
            Expect("facebook", 
                Query<Search>()
                    .Throw(new ApplicationException("unknown")));

            // assert
            Throws<ApplicationException>(()=> ReceiveReminder("facebook"));
        }

        Task TopicCreated()
        {
            return topic.Handle(new CreateTopic(subject, schedule));
        }

        Task ReceiveReminder(string name)
        {
            return topic.OnReminder(name);
        }

        void RetriesScheduled(string api)
        {
            topic.ScheduleRetries(api);
        }
    }
}