using System;
using System.Diagnostics;
using System.Threading;

using NUnit.Framework;

using Orleans.Bus;
[assembly: OrleansSiloForTestingAction]

namespace Orleans.Bus
{
    [TestFixture]
    public class ReactivePubSubFixture
    {
        IMessageBus bus;
        
        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
        }

        [Test]
        public async void Subscription()
        {
            const string grainId = "111";

            using (var proxy = await ReactiveObservableProxy.Create())
            {
                var received = new AutoResetEvent(false);

                string source = null;
                FooPublished @event = null;

                var observable = await proxy.Attach(grainId, typeof(FooPublished));
                observable.Subscribe(e =>
                {
                    source = e.Source;
                    @event = e.Payload.Message as FooPublished; 
                    received.Set();
                });

                await bus.Send(grainId, new PublishFoo("foo"));
                received.WaitOne(TimeSpan.FromSeconds(5));

                Assert.NotNull(@event);
                Assert.AreEqual("foo", @event.Foo);
                Assert.AreEqual(grainId, source);
            }
        }
        
        [Test]
        public async void Subscription_is_idempotent_and_callback_will_be_overriden()
        {
            const string grainId = "222";

            using (var proxy = await ReactiveObservableProxy.Create())
            {
                var received = new AutoResetEvent(false);

                string source = null;
                FooPublished @event = null;

                var first = await proxy.Attach(grainId, typeof(FooPublished));
                Debug.Assert(first != null);

                var observable = await proxy.Attach(grainId, typeof(FooPublished));
                observable.Subscribe(e =>
                {
                    source = e.Source;
                    @event = e.Payload.Message as FooPublished; 
                    received.Set();
                });

                await bus.Send(grainId, new PublishFoo("foo"));
                received.WaitOne(TimeSpan.FromSeconds(5));

                Assert.NotNull(@event);
                Assert.AreEqual("foo", @event.Foo);
                Assert.AreEqual(grainId, source);
            }
        }
    }
}