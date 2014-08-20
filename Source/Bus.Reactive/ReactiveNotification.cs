using System;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// Wrapper for source notification
    /// </summary>
    public class ReactiveNotification
    {
        /// <summary>
        /// Id of the source grain
        /// </summary>
        public readonly string Source;

        /// <summary>
        /// Original notification
        /// </summary>
        public readonly Notification Payload;

        internal ReactiveNotification(string source, Notification payload)
        {
            Source = source;
            Payload = payload;
        }
    }
}