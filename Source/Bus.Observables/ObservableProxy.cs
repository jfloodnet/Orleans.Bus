using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Allows to attach and detach to discrete event notifications
    /// To delete underlying runtime reference call <see cref="IDisposable.Dispose"/>
    /// </summary>
    /// <remarks> Instances of this type are not thread safe </remarks>
    public interface IObservableProxy: IDisposable
    {          
        /// <summary>
        /// Attaches this observer proxy to receive event notifications from the given source
        /// and deliver them back via provided <paramref name="callbacks"/>
        /// </summary>
        /// <param name="source">Id of the source grain</param>
        /// <param name="callbacks">Notification callbacks</param>
        /// <returns>Promise</returns>
        Task Attach(string source, params Callback[] callbacks);

        /// <summary>
        /// Detaches this observer proxy from receiving event notifications of a particular type from the given source 
        /// </summary>
        /// <param name="source">Id of the source grain</param>
        /// <param name="notifications">Types of notification messages to detach from</param>
        /// <returns>Promise</returns>
        Task Detach(string source, params Type[] notifications);
    }

    /// <summary>
    /// Special structure to specify notification callback handler
    /// </summary>
    public class Callback
    {
        internal readonly Type Notification;
        internal readonly Action<string, Notification> Handler;

        /// <summary>
        /// Creates new instance of notification <see cref="Callback"/>
        /// </summary>
        /// <param name="notification">Type of notification message to subscribe</param>
        /// <param name="handler">Delegate handler</param>        
        public Callback(Type notification, Action<string, Notification> handler)
        {
            Notification = notification;
            Handler = handler;
        }
    }

    /// <summary>
    /// Factory for <see cref="IObservableProxy"/>
    /// </summary>
    public class ObservableProxy : IObservableProxy, IObserve
    {
        /// <summary>
        /// Creates new <see cref="IObservableProxy"/>
        /// </summary>
        /// <returns>New instance of <see cref="IObservableProxy"/></returns>
        public static async Task<IObservableProxy> Create()
        {
            var observable = new ObservableProxy();
            
            var proxy = await SubscriptionManager.Instance.CreateProxy(observable);
            observable.Initialize(proxy);

            return observable;
        }

        readonly IDictionary<Type, Action<string, Notification>> handlers =
             new Dictionary<Type, Action<string, Notification>>();

        IObserve proxy;

        void Initialize(IObserve proxy)
        {
            this.proxy = proxy;
        }

        void IDisposable.Dispose()
        {        
            SubscriptionManager.Instance.DeleteProxy(proxy);
        }

        async Task IObservableProxy.Attach(string source, params Callback[] callbacks)
        {
            foreach (var callback in callbacks)
                handlers[callback.Notification] = callback.Handler;

            var notifications = callbacks
                .Select(x => x.Notification)
                .ToArray();

            await SubscriptionManager.Instance.Subscribe(source, proxy, notifications);
        }

        async Task IObservableProxy.Detach(string source, params Type[] notifications)
        {
            foreach (var notification in notifications)
                handlers.Remove(notification);

            await SubscriptionManager.Instance.Unsubscribe(source, proxy, notifications);
        }

        void IObserve.On(string source, params Notification[] notifications)
        {
            foreach (var notification in notifications)
            {
                var callback = handlers[notification.Type];
                callback(source, notification);   
            }
        }
    }
}