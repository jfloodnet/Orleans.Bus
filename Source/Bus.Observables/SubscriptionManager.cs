using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    class SubscriptionManager
    {
        public static readonly SubscriptionManager Instance = 
           new SubscriptionManager(DynamicGrainFactory.Instance)
               .Initialize();

        readonly Dictionary<Type, Type> notifiers =
             new Dictionary<Type, Type>();

        readonly DynamicGrainFactory factory;

        SubscriptionManager(DynamicGrainFactory factory)
        {
            this.factory = factory;
        }

        SubscriptionManager Initialize()
        {
            foreach (var grain in factory.RegisteredGrainTypes())
                Register(grain);

            return this;
        }

        void Register(Type grain)
        {
            foreach (var attribute in grain.Attributes<NotifiesAttribute>())
                notifiers.Add(attribute.Event, grain);
        }

        public async Task<IObserve> CreateProxy(IObserve client)
        {
            return await ObserveFactory.CreateObjectReference(client);
        }

        public void DeleteProxy(IObserve observer)
        {
            ObserveFactory.DeleteObjectReference(observer);
        }

        public async Task Subscribe(string source, IObserve proxy, params Type[] notifications)
        {
            Type notifier = null;

            foreach (var notification in notifications)
            {
                Type grainType;
                if (!notifiers.TryGetValue(notification, out grainType))
                    throw new ApplicationException("Can't find source grain which handles notification type " + notification.FullName);

                if (notifier != null && notifier != grainType)
                    throw new ApplicationException("Can't subscribe to multiple grain types for the same source id");

                notifier = grainType;
            }

            var reference = factory.GetReference(notifier, source);
            var observable = (IMessageBasedGrain)reference;

            await observable.Attach(proxy, notifications);
        }

        public async Task Unsubscribe(string source, IObserve proxy, params Type[] notifications)
        {
            Type notifier = null;

            foreach (var notification in notifications)
            {
                Type grainType;
                if (!notifiers.TryGetValue(notification, out grainType))
                    throw new ApplicationException("Can't find source grain which handles notification type " + notification.FullName);

                if (notifier != null && notifier != grainType)
                    throw new ApplicationException("Can't unsubscribe from multiple grain types for the same source id");

                notifier = grainType;
            }

            var reference = factory.GetReference(notifier, source);
            var observable = (IMessageBasedGrain)reference;

            await observable.Detach(proxy, notifications);
        }
    }
}