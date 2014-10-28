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
    public interface IMessageBasedGrain : IObservableGrain
    {}

    /// <summary>
    /// Base class for all message based grains
    /// </summary>
    public abstract class MessageBasedGrain : Grain, IMessageBasedGrain, IExposeGrainInternals
    {
        /// <summary>
        /// Reference to <see cref="IMessageBus"/>. Points to global runtime-bound implementation by default.
        /// </summary>
        public IMessageBus Bus;

        /// <summary>
        /// Returns identity of this grain. Points to runtime-bound implementation by default.
        /// </summary>
        public Func<string> Id;

        /// <summary>
        /// Reference to grain timers collection. Points to runtime-bound implementation by default.
        /// </summary>
        public ITimerCollection Timers;

        /// <summary>
        /// Reference to grain reminders collection. Points to runtime-bound implementation by default.
        /// </summary>
        public IReminderCollection Reminders;

        /// <summary>
        /// Reference to grain activation service. Points to runtime-bound implementation by default.
        /// </summary>
        public IActivation Activation;

        /// <summary>
        /// Reference to observer collection. Points to runtime-bound implementation by default.
        /// </summary>
        public IObserverCollection Observers;

        /// <summary>
        /// Default constructor, which initialize all local services to runtime-bound implementations by default.
        /// </summary>
        protected MessageBasedGrain()
        {
            Bus = MessageBus.Instance;
            Id  = () => Identity.Of(this);
            Timers = new TimerCollection(this, Id, Bus);
            Reminders = new ReminderCollection(this);
            Observers = new ObserverCollection();
            Activation = new Activation(this);
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
                Observers.Attach(observer, notification);

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
                Observers.Detach(observer, notification);

            return TaskDone.Done;
        }

        /// <summary>
        /// Notifies all attached observers about given notifications.
        /// </summary>
        /// <param name="notifications">The notification messages</param>
        protected void Notify(params Notification[] notifications)
        {
            Observers.Notify(Id(), notifications);
        }

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
    
    interface IExposeGrainInternals
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