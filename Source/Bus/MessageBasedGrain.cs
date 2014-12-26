using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Runtime;

namespace Orleans.Bus
{
    /// <summary>
    /// Base interface for all message-based grains
    /// </summary>
    public interface IMessageBasedGrain : IRemindable
    {
        /// <summary>
        /// Internal. .Attaches given selective generic observer for the given types of notification messages.
        /// </summary>
        /// <param name="observer">The observer proxy</param>
        /// <param name="notifications">The types of notification messages</param>
        Task Attach(IObserve observer, params Type[] notifications);

        /// <summary>
        /// Internal. Detaches given selective generic observer for the given types of notifications.
        /// </summary>
        /// <param name="observer">The observer proxy</param>
        /// <param name="notifications">The types of notification messages</param>
        Task Detach(IObserve observer, params Type[] notifications);
    }

    /// <summary>
    /// Base class for all message based grains
    /// </summary>
    public abstract class MessageBasedGrain : Grain, IMessageBasedGrain, IExposeGrainInternals
    {
        readonly IObserverCollection observers;

        /// <summary>
        /// Default constructor, which initialize all local services to runtime-bound implementations by default.
        /// </summary>
        protected MessageBasedGrain()
        {
            observers = new ObserverCollection();
        }

        Task IMessageBasedGrain.Attach(IObserve observer, params Type[] notifications)
        {
            return OnAttach(observer, notifications);
        }

        /// <summary>
        /// Attaches given selective observer for the given types of notifications.
        /// </summary>
        /// <param name="observer">The observer proxy.</param>
        /// <param name="notifications">The types of notifications</param>
        /// <remarks>The operation is idempotent</remarks>
        protected virtual Task OnAttach(IObserve observer, params Type[] notifications)
        {
            foreach (var notification in notifications)
                observers.Attach(observer, notification);

            return TaskDone.Done;
        }

        Task IMessageBasedGrain.Detach(IObserve observer, params Type[] notifications)
        {
            return OnDetach(observer, notifications);
        }

        /// <summary>
        /// Detaches given selective observer for the given types of notifications.
        /// </summary>
        /// <param name="observer">The observer proxy.</param>
        /// <param name="notifications">The types of notifications</param>
        /// <remarks> The operation is idempotent </remarks>
        protected virtual Task OnDetach(IObserve observer, params Type[] notifications)
        {
            foreach (var notification in notifications)
                observers.Detach(observer, notification);

            return TaskDone.Done;
        }

        /// <summary>
        /// Notifies all attached observers about given notifications.
        /// </summary>
        /// <param name="notifications">The notification messages</param>
        protected void Notify(params Notification[] notifications)
        {
            observers.Notify(Identity.Of(this), notifications);
        }

        Task IRemindable.ReceiveReminder(string id, TickStatus status)
        {
            return OnReminder(id, status);
        }

        /// <summary>
        /// Receieves a reminder callback.
        /// </summary>
        /// <param name="id">Id of the reminder</param>
        /// <param name="status">Status of this reminder tick</param>
        /// <returns> Completion promise which the grain will resolve when it has finished processing this reminder callback </returns>
        /// <remarks>Override this method in subclass in order to react to previously registered reminder</remarks>
        public virtual Task OnReminder(string id, TickStatus status)
        {
            return OnReminder(id);
        }

        /// <summary>
        /// Receieves a reminder callback.
        /// </summary>
        /// <param name="id">Id of the reminder</param>
        /// <returns> Completion promise which the grain will resolve when it has finished processing this reminder callback </returns>
        /// <remarks>Override this method in subclass in order to react to previously registered reminder</remarks>
        public virtual Task OnReminder(string id)
        {
            return TaskDone.Done;
        }

        #region IExposeGrainInternals

        void IExposeGrainInternals.DeactivateOnIdle()
        {
            DeactivateOnIdle();
        }

        void IExposeGrainInternals.DelayDeactivation(TimeSpan timeSpan)
        {
            DelayDeactivation(timeSpan);
        }

        Task<IGrainReminder> IExposeGrainInternals.GetReminder(string reminderName)
        {
            return GetReminder(reminderName);
        }

        Task<List<IGrainReminder>> IExposeGrainInternals.GetReminders()
        {
            return GetReminders();
        }

        Task<IGrainReminder> IExposeGrainInternals.RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterOrUpdateReminder(reminderName, dueTime, period);
        }

        Task IExposeGrainInternals.UnregisterReminder(IGrainReminder reminder)
        {
            return UnregisterReminder(reminder);
        }

        IDisposable IExposeGrainInternals.RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterTimer(asyncCallback, state, dueTime, period);
        }

        #endregion
    }

    internal interface IExposeGrainInternals
    {
        void DeactivateOnIdle();
        void DelayDeactivation(TimeSpan timeSpan);

        Task<IGrainReminder> GetReminder(string reminderName);
        Task<List<IGrainReminder>> GetReminders();
        Task<IGrainReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period);
        Task UnregisterReminder(IGrainReminder reminder);

        IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period);         
    }
}