using System;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// Callback interface need to be implemented by any client, 
    /// which want to be notified about particular notifications.
    /// </summary>
    /// <remarks>
    /// Internal iterface used by infrastructure
    /// </remarks>>
    public interface IObserve : IGrainObserver
    {
        /// <summary>
        /// Notifications will be delivered to this callback method
        /// </summary>
        /// <param name="source">The id of the source grain</param>
        /// <param name="notifications">The notification messages</param>
        void On(string source, params Notification[] notifications);
    }
}