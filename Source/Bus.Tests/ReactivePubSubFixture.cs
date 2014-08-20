using System;
using System.Diagnostics;
using System.Threading;

using NUnit.Framework;

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
                TextPublished @event = null;

                var observable = await proxy.Attach<TextPublished>(grainId);
                observable.Subscribe(e =>
                {
                    source = e.Source;
                    @event = e.Message;
                    received.Set();
                });

                await bus.Send(grainId, new PublishText("sub"));
                received.WaitOne(TimeSpan.FromSeconds(5));

                Assert.NotNull(@event);
                Assert.AreEqual("sub", @event.Text);
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
                TextPublished @event = null;

                var first = await proxy.Attach<TextPublished>(grainId);
                Debug.Assert(first != null);

                var observable = await proxy.Attach<TextPublished>(grainId);
                observable.Subscribe(e =>
                {
                    source = e.Source;
                    @event = e.Message;
                    received.Set();
                });

                await bus.Send(grainId, new PublishText("sub"));
                received.WaitOne(TimeSpan.FromSeconds(5));

                Assert.NotNull(@event);
                Assert.AreEqual("sub", @event.Text);
                Assert.AreEqual(grainId, source);
            }
        }

        [Test]
        public async void Generic_subscription()
        {
            const string grainId = "333";

            using (var proxy = await GenericReactiveObservableProxy.Create())
            {
                var received = new AutoResetEvent(false);

                string source = null;
                TextPublished @event = null;

                var observable = await proxy.Attach<TextPublished>(grainId);
                observable.Subscribe(e =>
                {
                    source = e.Source;
                    @event = (TextPublished) e.Message;
                    received.Set();
                });

                await bus.Send(grainId, new PublishText("sub"));
                received.WaitOne(TimeSpan.FromSeconds(5));

                Assert.NotNull(@event);
                Assert.AreEqual("sub", @event.Text);
                Assert.AreEqual(grainId, source);
            }
        }

        [Test]
        public async void Generic_subscription_is_idempotent_and_callback_will_be_overriden()
        {
            const string grainId = "444";

            using (var proxy = await GenericReactiveObservableProxy.Create())
            {
                var received = new AutoResetEvent(false);

                string source = null;
                TextPublished @event = null;

                var first = await proxy.Attach<TextPublished>(grainId);
                Debug.Assert(first != null);

                var observable = await proxy.Attach<TextPublished>(grainId);
                observable.Subscribe(e =>
                {
                    source = e.Source;
                    @event = (TextPublished) e.Message;
                    received.Set();
                });

                await bus.Send(grainId, new PublishText("sub"));
                received.WaitOne(TimeSpan.FromSeconds(5));

                Assert.NotNull(@event);
                Assert.AreEqual("sub", @event.Text);
                Assert.AreEqual(grainId, source);
            }
        }
    }
}