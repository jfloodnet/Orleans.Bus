using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    public abstract class MessageBasedGrain : GrainBase, IMessageBasedGrain, IExposeGrainInternals
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
            Timers = new TimerCollection(this);
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

        Task<IOrleansReminder> IExposeGrainInternals.GetReminder(string reminderName)
        {
            return GetReminder(reminderName);
        }

        Task<List<IOrleansReminder>> IExposeGrainInternals.GetReminders()
        {
            return GetReminders();
        }

        Task<IOrleansReminder> IExposeGrainInternals.RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterOrUpdateReminder(reminderName, dueTime, period);
        }

        Task IExposeGrainInternals.UnregisterReminder(IOrleansReminder reminder)
        {
            return UnregisterReminder(reminder);
        }

        IOrleansTimer IExposeGrainInternals.RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterTimer(asyncCallback, state, dueTime, period);
        }
    }

    /// <summary>
    /// Base class for all persistent message based grains
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    public abstract class MessageBasedGrainBase<TState> : GrainBase<TState>, IMessageBasedGrain, IExposeGrainInternals 
        where TState : class, IGrainState
    {
        /// <summary>
        /// Reference to <see cref="IMessageBus" />. Points to global runtime-bound implementation by default.
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
        protected MessageBasedGrainBase()
        {
            Bus = MessageBus.Instance;
            Id = () => Identity.Of(this);
            Timers = new TimerCollection(this);
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

        /// <summary>
        /// Deactivates the on idle.
        /// </summary>
        void IExposeGrainInternals.DeactivateOnIdle()
        {
            DeactivateOnIdle();
        }

        /// <summary>
        /// Delays the deactivation.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        void IExposeGrainInternals.DelayDeactivation(TimeSpan timeSpan)
        {
            DelayDeactivation(timeSpan);
        }

        /// <summary>
        /// Gets the reminder.
        /// </summary>
        /// <param name="reminderName">Name of the reminder.</param>
        /// <returns></returns>
        Task<IOrleansReminder> IExposeGrainInternals.GetReminder(string reminderName)
        {
            return GetReminder(reminderName);
        }

        /// <summary>
        /// Gets the reminders.
        /// </summary>
        /// <returns></returns>
        Task<List<IOrleansReminder>> IExposeGrainInternals.GetReminders()
        {
            return GetReminders();
        }

        /// <summary>
        /// Registers the or update reminder.
        /// </summary>
        /// <param name="reminderName">Name of the reminder.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        Task<IOrleansReminder> IExposeGrainInternals.RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterOrUpdateReminder(reminderName, dueTime, period);
        }

        /// <summary>
        /// Unregisters the reminder.
        /// </summary>
        /// <param name="reminder">The reminder.</param>
        /// <returns></returns>
        Task IExposeGrainInternals.UnregisterReminder(IOrleansReminder reminder)
        {
            return UnregisterReminder(reminder);
        }

        /// <summary>
        /// Registers the timer.
        /// </summary>
        /// <param name="asyncCallback">The asynchronous callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        IOrleansTimer IExposeGrainInternals.RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterTimer(asyncCallback, state, dueTime, period);
        }
    }

    /// <summary>
    /// Base class for persistent message-based grains with generic state
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    public abstract class MessageBasedGrain<TState> : MessageBasedGrainBase<IStateHolder<TState>>
    {
        /// <summary>
        /// Strongly typed accessor for the grain state
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public new TState State
        {
            get { return Holder.State; }
            set { Holder.State = value; }
        }

        /// <summary>
        /// Gets or sets current instance of state holder. By default it's initialized to the one created by Orleans runtime.
        /// </summary>
        /// <value>
        /// The holder.
        /// </value>
        /// <remarks>
        /// Could be substiuted for unit-testing purposes
        /// </remarks>
        public IStateHolder<TState> Holder
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets current instance of storage provider proxy for controlling state checkpointing
        /// </summary>
        /// <value>
        /// The storage.
        /// </value>
        /// <remarks>
        /// Could be substiuted for unit-testing purposes
        /// </remarks>
        public IStateStorage Storage
        {
            get; set;
        }

        /// <summary>
        /// This method is called at the end of the process of activating a grain.
        /// It is called before any messages have been dispatched to the grain.
        /// For grains with declared persistent state, this method is called after the State property has been populated.
        /// </summary>
        /// <returns></returns>
        public override Task ActivateAsync()
        {
            Holder = Holder ?? base.State;
            Storage = Storage ?? new DefaultStateStorage(base.State);
            return TaskDone.Done;
        }
    }   
    
    interface IExposeGrainInternals
    {
        void DeactivateOnIdle();
        void DelayDeactivation(TimeSpan timeSpan);

        Task<IOrleansReminder> GetReminder(string reminderName);
        Task<List<IOrleansReminder>> GetReminders();
        Task<IOrleansReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period);
        Task UnregisterReminder(IOrleansReminder reminder);

        IOrleansTimer RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period);         
    }
}