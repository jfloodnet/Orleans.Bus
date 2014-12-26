using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Bus;

namespace Sample
{
    public class ApiGrain : MessageBasedGrain, IApi
    {
        Api api;

        public override Task ActivateAsync()
        {
            api = new Api
            {
                Id = Identity.Of(this),
                Timers = new TimerCollection(this),
                Notify = e => Notify(new[]{new Notification(e.GetType(), e)}),
                Worker = new DemoWorker(Identity.Of(this))  // ApiWorker.Create(Id())
            };

            return Task.FromResult(api);
        }

        public async Task<object> OnQuery(object query)
        {
            return await api.Answer((dynamic)query);
        }
    }

    public class Api
    {
        const int FailureThreshold = 1;

        public string Id;
        public ITimerCollection Timers;
        public Action<Event> Notify;
        public IApiWorker Worker;

        int failures;
        bool available = true;

        public bool IsAvailable
        {
            get { return available; }
        }

        public async Task<int> Answer(Search search)
        {
            Console.WriteLine("*{0}* is processing request {1} ...", Id, search.Subject);

            if (!available)
                throw new ApiUnavailableException(Id);

            try
            {
                var result = await Worker.Search(search.Subject);
                ResetFailureCounter();

                return result;
            }
            catch (ApiUnavailableException)
            {
                IncrementFailureCounter();
                
                if (!HasReachedFailureThreshold())
                    throw;

                Lock();

                NotifyUnavailable();
                ScheduleAvailabilityCheck();

                throw;
            }
        }

        bool HasReachedFailureThreshold()
        {
            return failures == FailureThreshold;
        }

        void IncrementFailureCounter()
        {
            failures++;
        }

        void ResetFailureCounter()
        {
            failures = 0;
        }

        void ScheduleAvailabilityCheck()
        {
            var due = TimeSpan.FromSeconds(1);
            var period = TimeSpan.FromSeconds(2);

            Timers.RegisterReentrant("check", due, period, CheckAvailability);
        }

        public async Task CheckAvailability()
        {
            try
            {
                await Worker.Search("test");
                Timers.Unregister("check");

                Unlock();
                NotifyAvailable();
            }
            catch (ApiUnavailableException)
            {}
        }

        void Lock()
        {
            available = false;
            Console.WriteLine("*{0}* gone wild. Unavailable!", Id);
        }

        void Unlock()
        {
            available = true;
            Console.WriteLine("*{0}* is back available again!", Id);
        }

        void NotifyAvailable()
        {
            Notify(new AvailabilityChanged(Id, true));
        }

        void NotifyUnavailable()
        {
            Notify(new AvailabilityChanged(Id, false));
        }
    }
}
