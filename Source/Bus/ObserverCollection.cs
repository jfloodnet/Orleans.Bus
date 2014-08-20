using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// This  is a helper class for grains that support observers.
    /// It provides methods for attaching/detaching observers and for notifying about particular notifications.
    /// </summary>
    public interface IObserverCollection 
    {
        /// <summary>
        /// Attaches given observer for the given type of notification.
        /// </summary>
        /// <param name="observer">The observer proxy</param>
        /// <param name="notification">The type of notification message</param>
        /// <remarks>The operation is idempotent</remarks>
        void Attach(IObserve observer, Type notification);

        /// <summary>
        /// Detaches given observer for the given type of notification.
        /// </summary>
        /// <param name="observer">The observer proxy</param>
        /// <param name="notification">The type of notification message</param>
        /// <remarks>The operation is idempotent</remarks>
        void Detach(IObserve observer, Type notification);

        /// <summary>
        /// Notifies all attached observers about given notifications.
        /// </summary>
        /// <param name="source">The id of the source grain</param>
        /// <param name="notifications">The notification messages</param>
        void Notify(string source, params Notification[] notifications);
    }

    /// <summary>
    /// Default implementation of <see cref="IObserverCollection"/>
    /// </summary>
    public class ObserverCollection : IObserverCollection
    {
        readonly IDictionary<Type, HashSet<IObserve>> subscriptions = new Dictionary<Type, HashSet<IObserve>>();

        void IObserverCollection.Attach(IObserve observer, Type notification)
        {
            var observers = Observers(notification);

            if (observers == null)
            {
                observers = new HashSet<IObserve>();
                subscriptions.Add(notification, observers);
            }

            observers.Add(observer);
        }

        void IObserverCollection.Detach(IObserve observer, Type notification)
        {
            var observers = Observers(notification);
            
            if (observers != null)
                observers.Remove(observer);
        }

        void IObserverCollection.Notify(string source, params Notification[] notifications)
        {
            var failed = new List<IObserve>();

            foreach (var recipient in GroupByRecipient(notifications))
                TryDeliver(recipient.Key, recipient.Value.ToArray(), source, failed);

            Cleanup(failed);
        }

        Dictionary<IObserve, List<Notification>> GroupByRecipient(IEnumerable<Notification> notifications)
        {
            var recipients = new Dictionary<IObserve, List<Notification>>();

            foreach (var notification in notifications)
            {
                var observers = Observers(notification.Type);

                if (observers == null)
                    continue;

                foreach (var observer in observers)
                {
                    List<Notification> list;

                    if (!recipients.TryGetValue(observer, out list))
                    {
                        list = new List<Notification>();
                        recipients.Add(observer, list);
                    }

                    list.Add(notification);
                }
            }

            return recipients;
        }

        static void TryDeliver(IObserve observer, Notification[] notifications, string source, ICollection<IObserve> failed)
        {
            try
            {
                observer.On(source, notifications);
            }
            catch (Exception)
            {
                failed.Add(observer);
            }
        }

        void Cleanup(ICollection<IObserve> failed)
        {
            foreach (var subscription in subscriptions)
                subscription.Value.RemoveWhere(failed.Contains);
        }

        internal HashSet<IObserve> Observers(Type notification)
        {
            HashSet<IObserve> result;
            return subscriptions.TryGetValue(notification, out result) ? result : null;
        }
    }
}