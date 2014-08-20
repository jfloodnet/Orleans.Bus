using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class ObservableCollectionFixture
    {
        readonly Type notification = typeof(FooPublished);

        IObserverCollection collection;
        IObserve client;
        IObserve proxy;

        [SetUp]
        public void SetUp()
        {
            client = new Observe();
            proxy  = SubscriptionManager.Instance.CreateProxy(client).Result;
            collection = new ObserverCollection();
        }

        [Test]
        public void Notify_when_no_observers()
        {
            Assert.DoesNotThrow(() => collection.Notify("test", Notification()));
        }

        [Test]
        public void Attach_is_idempotent()
        {
            collection.Attach(proxy, notification);
            
            Assert.DoesNotThrow(() => 
                collection.Attach(proxy, notification));

            Assert.AreEqual(1, GetObservers(notification).Count);
        }

        [Test]
        public void Detach_is_idempotent()
        {
            collection.Attach(proxy, notification);
            collection.Detach(proxy, notification);
            
            Assert.DoesNotThrow(() => 
                collection.Detach(proxy, notification));

            Assert.AreEqual(0, GetObservers(notification).Count);
        }

        static Notification Notification()
        {
            return new Notification(typeof(FooPublished), new FooPublished("foo"));
        }

        HashSet<IObserve> GetObservers(Type @event)
        {
            return ((ObserverCollection)collection).Observers(@event);
        }

        class Observe : IObserve
        {
            public void On(string source, params Notification[] notifications)
            {}
        }
    }
}