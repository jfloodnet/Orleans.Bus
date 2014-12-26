using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Bus;
using Orleans.Runtime;

namespace Sample
{
    public class TopicGrain : MessageBasedGrain, ITopic
    {
        Topic topic;

        public override Task ActivateAsync()
        {
            var id = Identity.Of(this);
            var bus = MessageBus.Instance;
            var storage = TopicStorage.Instance;

            topic = new Topic
            {
                Id = id,
                Bus = bus,
                Timers = new TimerCollection(this, bus),
                Reminders = new ReminderCollection(this),
                Storage = storage
            };

            return topic.Activate();
        }

        public Task OnCommand(object cmd)
        {
            return topic.Handle((dynamic)cmd);
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return topic.OnReminder(reminderName);
        }
    }

    public class Topic
    {
        public string Id;
        public IMessageBus Bus;
        public ITimerCollection Timers;
        public IReminderCollection Reminders;
        public ITopicStorage Storage;
        public TopicState State;

        const int MaxRetries = 3;
        static readonly TimeSpan RetryPeriod = TimeSpan.FromSeconds(5);
        readonly IDictionary<string, int> retrying = new Dictionary<string, int>();

        string query;

        public async Task Activate()
        {
            State = await Storage.ReadStateAsync(Id);
        }

        public async Task Handle(CreateTopic cmd)
        {
            query = cmd.Query;

            foreach (var entry in cmd.Schedule)
                await Reminders.Register(entry.Key, TimeSpan.Zero, entry.Value);
        }

        public async Task OnReminder(string api)
        {
            try
            {
                if (!IsRetrying(api))
                    await Search(api);
            }
            catch (ApiUnavailableException)
            {
                ScheduleRetries(api);
            }
        }

        bool IsRetrying(string api)
        {
            return retrying.ContainsKey(api);
        }

        public void ScheduleRetries(string api)
        {
            retrying.Add(api, 0);
            Timers.Register(api, RetryPeriod, RetryPeriod, api, RetrySearch);
        }

        public async Task RetrySearch(object state)
        {
            var api = (string)state;
            
            try
            {
                await Search(api);
                CancelRetries(api);
            }
            catch (ApiUnavailableException)
            {
                RecordFailedRetry(api);

                if (MaxRetriesReached(api))
                {
                    DisableSearch(api);
                    CancelRetries(api);                   
                }
            }
        }

        void RecordFailedRetry(string api)
        {
            retrying[api] += 1;
        }

        bool MaxRetriesReached(string api)
        {
            return retrying[api] == MaxRetries;
        }

        void CancelRetries(string api)
        {
            Timers.Unregister(api);
            retrying.Remove(api);
        }

        async Task Search(string api)
        {
            State.Total += await Bus.Query<int>(api, new Search(query));
            await Storage.WriteStateAsync(Id, State);
        }

        void DisableSearch(string api)
        {
            Reminders.Unregister(api);
        }
    }

    public class TopicState
    {
        public int Total { get; set; }
    }
}
