using System;
using System.Linq;
using System.Threading.Tasks;

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

    /// <summary>
    /// Internal iterface used only by infrastructure!
    /// </summary>
    public interface IObservableGrain : IGrain
    {
        /// <summary>
        /// Attaches given selective observer for the given types of notification messages.
        /// </summary>
        /// <param name="observer">The observer proxy</param>
        /// <param name="notifications">The types of notification messages</param>
        Task Attach(IObserve observer, params Type[] notifications);

        /// <summary>
        /// Detaches given selective observer for the given types of notifications.
        /// </summary>
        /// <param name="observer">The observer proxy</param>
        /// <param name="notifications">The types of notification messages</param>
        Task Detach(IObserve observer, params Type[] notifications);
    }
}