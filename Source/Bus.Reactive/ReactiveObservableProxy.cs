using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Allows to attach and detach to discrete notifications as reactive observable
    /// To delete underlying runtime referece call <see cref="IDisposable.Dispose"/> method
    /// </summary>
    /// <remarks>Instances of this type are not thread safe</remarks>
    public interface IReactiveObservableProxy : IDisposable
    {
        /// <summary>
        /// Attaches this observer proxy to receive notifications of a particular type from the given source
        /// </summary>
        /// <param name="source">Id of the source grain</param>
        /// <param name="notifications">Types of notifications</param>
        /// <returns>Hot observable</returns>
        Task<IObservable<ReactiveNotification>> Attach(string source, params Type[] notifications);

        /// <summary>
        /// Detaches this observer proxy from receiving notifications of a particular type from the given source 
        /// </summary>
        /// <param name="source">Id of the source grain</param>
        /// <param name="notifications">Types of notifications</param>
        /// <returns>Promise</returns>
        Task Detach(string source, params Type[] notifications);
    }

    /// <summary>
    /// Factory for <see cref="IReactiveObservableProxy"/>
    /// </summary>
    public class ReactiveObservableProxy : IReactiveObservableProxy, IObserve
    {
        /// <summary>
        /// Creates new <see cref="IReactiveObservableProxy"/>
        /// </summary>
        /// <returns>New instance of <see cref="IReactiveObservableProxy"/></returns>
        public static async Task<IReactiveObservableProxy> Create()
        {
            var observable = new ReactiveObservableProxy();
            
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

        async Task<IObservable<ReactiveNotification>> IReactiveObservableProxy.Attach(string source, params Type[] notifications)
        {
            var subject = new Subject<ReactiveNotification>();

            foreach (var notification in notifications)
            {
                handlers[notification] =
                    (s, n) => subject.OnNext(new ReactiveNotification(s, n));                
            }

            await SubscriptionManager.Instance.Subscribe(source, proxy, notifications);
            return subject;
        }

        async Task IReactiveObservableProxy.Detach(string source, params Type[] notifications)
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