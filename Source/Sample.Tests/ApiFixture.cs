using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Sample
{
    [TestFixture]
    public class ApiFixture : ApplicationGrainFixture
    {
        Api api;
        MockApiWorker worker;
        List<AvailabilityChanged> notifications;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            notifications = new List<AvailabilityChanged>();
            worker = new MockApiWorker();

            api = new Api
            {
                Id = "facebook",
                Timers = Timers,
                Notify = e => notifications.Add((AvailabilityChanged)e),
                Worker = worker
            };
        }

        [Test, Ignore("WIP")]
        public async void Locks_itself_and_notifies_when_failure_rate_exceeds_defined_threshold()
        {
            await api.Answer(new Search("ПТН ПНХ"));

            IsTrue(()=> api.IsAvailable == false);
            
            IsTrue(()=> notifications.Count == 1);
            IsTrue(()=> notifications[0].Available == false);
            IsTrue(()=> Timers.Count() == 1);

            var timer = CallbackTimer("check");
            IsTrue(() => timer.Callback == api.CheckAvailability);
            IsTrue(() => timer.Due == TimeSpan.FromSeconds(1));
            IsTrue(() => TimeSpan.FromSeconds(1) <= timer.Period && timer.Period <= TimeSpan.FromSeconds(3));
        }

        class MockApiWorker : IApiWorker
        {
            public Task<int> Search(string subject)
            {
                return Task.FromResult(1);
            }
        }
    }
}