using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Bus;

namespace Sample
{
    [StorageProvider(ProviderName = "TopicStore")]
    public class TopicGrain : PocoGrain<int, int, int>, ITopic
    {
        Topic topic;

        public TopicGrain()
        {
            OnActivate =()=>
            {
                topic = new Topic
                {
                    Bus = Bus, 
                    Timers = Timers, 
                    Reminders = Reminders,
                    Storage = Storage
                };

                return topic.Activate();
            };

            OnCommand = command => topic.Handle((dynamic)command);
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return topic.OnReminder(reminderName);
        }
    }

    public class Topic
    {
        public IMessageBus Bus;
        public ITimerCollection Timers;
        public IReminderCollection Reminders;
        public IStorageProviderProxy<int, int, int> Storage;

        const int MaxRetries = 3;
        static readonly TimeSpan RetryPeriod = TimeSpan.FromSeconds(5);
        readonly IDictionary<string, int> retrying = new Dictionary<string, int>();

        public string Query
        {
            get; private set;
        }

        public int Total
        {
            get; private set;
        }

        public async Task Activate()
        {
            Total = await Storage.ReadStateAsync();
        }

        public async Task Handle(CreateTopic cmd)
        {
            Query = cmd.Query;

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
            Timers.Register(api, RetrySearch, api, RetryPeriod, RetryPeriod);
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

        void DisableSearch(string api)
        {
            Reminders.Unregister(api);
        }

        async Task Search(string api)
        {
            Total += await Bus.Query<int>(api, new Search(Query));
            await Storage.WriteStateAsync(Total);
        }
    }
}
