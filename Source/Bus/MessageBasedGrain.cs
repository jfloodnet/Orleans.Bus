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
    public interface IMessageBasedGrain : IGrain
    {}

    /// <summary>
    /// Base interface for all observable message-based grains
    /// </summary>
    public interface IObservableMessageBasedGrain : IMessageBasedGrain, IObservableGrain
    {}

    /// <summary>
    /// Base class for all message based grains
    /// </summary>
    public abstract class MessageBasedGrain : Grain, IMessageBasedGrain, IExposeGrainInternals
    {
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
    }

    /// <summary>
    /// Base class for all observable message based grains
    /// </summary>
    public abstract class ObservableMessageBasedGrain : MessageBasedGrain, IObservableMessageBasedGrain
    {
        readonly Func<string> id;
        readonly IObserverCollection observers;

        /// <summary>
        /// Default constructor, which initialize all local services to runtime-bound implementations by default.
        /// </summary>
        protected ObservableMessageBasedGrain()
        {
            id =()=> Identity.Of(this);
            observers = new ObserverCollection();
        }

        /// <summary>
        /// Constructor, which allows injection for unit-testing purposes.
        /// </summary>
        protected ObservableMessageBasedGrain(Func<string> id, IObserverCollection observers)
        {
            this.id = id;
            this.observers = observers;
        }

        /// <summary>
        /// Attaches given selective observer for the given types of notifications.
        /// </summary>
        /// <param name="observer">The observer proxy.</param>
        /// <param name="notifications">The types of notifications</param>
        /// <remarks>
        /// The operation is idempotent
        /// </remarks>
        public virtual Task Attach(IObserve observer, params Type[] notifications)
        {
            foreach (var notification in notifications)
                observers.Attach(observer, notification);

            return TaskDone.Done;
        }

        /// <summary>
        /// Detaches given selective observer for the given types of notifications.
        /// </summary>
        /// <param name="observer">The observer proxy.</param>
        /// <param name="notifications">The types of notifications</param>
        /// <remarks>
        /// The operation is idempotent
        /// </remarks>
        public virtual Task Detach(IObserve observer, params Type[] notifications)
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
            observers.Notify(id(), notifications);
        }
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