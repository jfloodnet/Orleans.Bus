using System;
using System.Threading;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class PubSubFixture
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
            const string grainId = "11";

            using (var proxy = await ObservableProxy.Create())
            {
                var received = new AutoResetEvent(false);

                string source = null;
                FooPublished @event = null;

                await proxy.Attach(grainId, new Callback(typeof(FooPublished), (s, n) =>
                {
                    source = s;
                    @event = n.Message as FooPublished;
                    received.Set();
                }));

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
            const string grainId = "22";

            using (var proxy = await ObservableProxy.Create())
            {
                var received = new AutoResetEvent(false);

                string source = null;
                FooPublished @event = null;

                await proxy.Attach(grainId, new Callback(typeof(FooPublished), (s, n) => {}));

                await proxy.Attach(grainId, new Callback(typeof(FooPublished), (s, n) =>
                {
                    source = s;
                    @event = n.Message as FooPublished;
                    received.Set();
                }));

                await bus.Send(grainId, new PublishFoo("foo"));
                received.WaitOne(TimeSpan.FromSeconds(5));

                Assert.NotNull(@event);
                Assert.AreEqual("foo", @event.Foo);
                Assert.AreEqual(grainId, source);
            }
        }

        [Test]
        public async void Batch_subscription()
        {
            const string grainId = "33";

            using (var proxy = await ObservableProxy.Create())
            {
                var received = new CountdownEvent(2);

                string fooPublishedSource = null;
                string barPublishedSource = null;

                FooPublished fooPublished = null;
                BarPublished barPublished = null;

                var callbacks = new[]
                {
                   new Callback(typeof(FooPublished), (s, e) =>
                   {
                       fooPublishedSource = s;
                       fooPublished = e.Message as FooPublished;
                       received.Signal();
                   }),                   
                   
                   new Callback(typeof(BarPublished), (s, e) =>
                   {
                       barPublishedSource = s;
                       barPublished = e.Message as BarPublished;
                       received.Signal();
                   }),
                };

                await proxy.Attach(grainId, callbacks);

                await bus.Send(grainId, new PublishFoo("foo"));
                await bus.Send(grainId, new PublishBar("bar"));
                received.Wait(TimeSpan.FromSeconds(5));

                Assert.NotNull(fooPublished);
                Assert.AreEqual("foo", fooPublished.Foo);
                Assert.AreEqual(grainId, fooPublishedSource);                
                
                Assert.NotNull(barPublished);
                Assert.AreEqual("bar", barPublished.Bar);
                Assert.AreEqual(grainId, barPublishedSource);
            }
        }
    }
}