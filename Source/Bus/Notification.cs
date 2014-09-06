using System;
using System.Linq;

using Orleans.Concurrency;

namespace Orleans.Bus
{
    /// <summary>
    /// Represents discrete notification message
    /// </summary>
    [Serializable, Immutable]
    public class Notification
    {
        /// <summary>
        /// Type of the notification message
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// Notification message
        /// </summary>
        public readonly object Message;

        /// <summary>
        /// Creates new notification
        /// </summary>
        /// <param name="type">Type of the notification message</param>
        /// <param name="message">Notification message</param>
        public Notification(Type type, object message)
        {
            Type = type;
            Message = message;
        }
    }
}
