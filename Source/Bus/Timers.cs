using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Concurrency;

namespace Orleans.Bus
{
    /// <summary>
    /// Manages registration of grain timers
    /// </summary>
    public interface ITimerCollection
    {
        /// <summary>
        /// Registers a timer to send periodic callbacks to this grain in a reentrant way.
        /// <para>This is a default Orleans' timer behavior</para>
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para>
        /// This timer will not prevent the current grain from being deactivated.
        ///             If the grain is deactivated, then the timer will be discarded.
        /// 
        /// </para>
        /// 
        /// <para>
        /// Until the Task returned from the <paramref name="callback"/> is resolved,
        ///             the next timer tick will not be scheduled.
        ///             That is to say, timer callbacks never interleave their turns.
        /// 
        /// </para>
        /// 
        /// <para>
        /// The timer may be stopped at any time by calling the <see cref="Unregister(string)"/> method
        /// 
        /// </para>
        /// 
        /// <para>
        /// Any exceptions thrown by or faulted Task's returned from the  <paramref name="callback"/>
        ///             will be logged, but will not prevent the next timer tick from being queued.
        /// </para>
        /// 
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        void RegisterReentrant(string id, TimeSpan due, TimeSpan period, Func<Task> callback);

        /// <summary>
        /// Registers a timer to send periodic callbacks to this grain in a reentrant way.
        /// 
        /// <para>
        ///     This is a default Orleans' timer behavior
        /// </para>
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para>
        /// This timer will not prevent the current grain from being deactivated.
        ///             If the grain is deactivated, then the timer will be discarded.
        /// 
        /// </para>
        /// 
        /// <para>
        /// Until the Task returned from the <paramref name="callback"/> is resolved,
        ///             the next timer tick will not be scheduled.
        ///             That is to say, timer callbacks never interleave their turns.
        /// 
        /// </para>
        /// 
        /// <para>
        /// The timer may be stopped at any time by calling the <see cref="Unregister(string)"/> method
        /// 
        /// </para>
        /// 
        /// <para>
        /// Any exceptions thrown by or faulted Task's returned from the  <paramref name="callback"/>
        ///             will be logged, but will not prevent the next timer tick from being queued.
        /// </para>
        /// 
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        /// <param name="state">State object that will be passed as argument when calling the  <paramref name="callback"/>.</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        void RegisterReentrant<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback);

        /// <summary>
        /// Registers timer which call back grain in a non-reentrant way (non-interleaved semantics).
        /// 
        /// <para>
        ///  The callback invocation will be synchronized and will conform to usual turn based execution semantics
        /// </para>
        /// </summary>
        /// 
        /// <remarks> 
        /// <para>
        ///     The non-interleaved semantics is only guaranteed, 
        ///         if grain is not marked as <see cref="ReentrantAttribute"/>
        /// </para>
        /// 
        /// <para>
        /// This timer WILL prevent the current grain from being deactivated.
        /// 
        /// </para>
        /// 
        /// <para>
        /// Until the Task returned from the grain <see cref="IMessageBasedGrain.OnTimer"/> is resolved,
        ///              the next timer tick will not be scheduled.
        /// 
        /// </para>
        /// 
        /// <para>
        /// The timer may be stopped at any time by calling the <see cref="Unregister(string)"/> method
        /// 
        /// </para>
        /// 
        /// <para>
        /// Any exceptions thrown by or faulted Task's returned from the  <see cref="IMessageBasedGrain.OnTimer"/>
        ///             will be logged, but will not prevent the next timer tick from being queued.
        /// </para>
        /// 
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        /// <param name="state">The state to be dispatched on timer callback</param>
        void Register(string id, TimeSpan due, TimeSpan period, object state);

        /// <summary>
        /// Unregister previously registered timer. 
        /// </summary>
        /// <param name="id">Unique id of the timer</param>
        void Unregister(string id);

        /// <summary>
        /// Checks whether timer with the given name was registered before
        /// </summary>
        /// <param name="id">Unique id of the timer</param>
        /// <returns><c>true</c> if timer was the give name was previously registered, <c>false</c> otherwise </returns>
        bool IsRegistered(string id);

        /// <summary>
        /// Returns ids of all currently registered timers
        /// </summary>
        /// <returns>Sequence of <see cref="string"/> elements</returns>
        IEnumerable<string> Registered();
    }

    /// <summary>
    /// Default Orleans bound implementation of <see cref="ITimerCollection"/>
    /// </summary>
    public class TimerCollection : ITimerCollection
    {
        readonly IDictionary<string, IDisposable> timers = new Dictionary<string, IDisposable>();
        readonly MessageBasedGrain grain;
        readonly Type @interface;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerCollection"/> class.
        /// </summary>
        /// <param name="grain">The grain which requires timer services.</param>
        public TimerCollection(MessageBasedGrain grain)
        {
            this.grain = grain;

            @interface = grain
                .GetType()
                .GetInterfaces()
                .Single(i => 
                    i != typeof(IMessageBasedGrain) 
                      && typeof(IMessageBasedGrain).IsAssignableFrom(i));
        }

        void ITimerCollection.RegisterReentrant(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
        {
            DoRegister(id, due, period, callback);
        }

        void DoRegister(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
        {
            timers.Add(id, ((IExposeGrainInternals)grain).RegisterTimer(s => callback(), null, due, period));
        }

        void ITimerCollection.RegisterReentrant<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback)
        {
            DoRegister(id, due, period, state, callback);
        }

        void DoRegister<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback)
        {
            timers.Add(id, ((IExposeGrainInternals)grain).RegisterTimer(s => callback((TState)s), state, due, period));
        }

        void ITimerCollection.Register(string id, TimeSpan due, TimeSpan period, object state)
        {
            DoRegister(id, due, period, state, s => NonReentrantCallback(id, s));
        }

        void ITimerCollection.Unregister(string id)
        {
            DoUnregister(id);
        }

        void DoUnregister(string id)
        {
            var timer = timers[id];
            timers.Remove(id);
            timer.Dispose();
        }

        bool ITimerCollection.IsRegistered(string id)
        {
            return timers.ContainsKey(id);
        }

        IEnumerable<string> ITimerCollection.Registered()
        {
            return timers.Keys;
        }

        Task NonReentrantCallback(string id, object state)
        {
            var reference = (IMessageBasedGrain) 
                DynamicGrainFactory
                    .Instance
                    .GetReference(@interface, Identity.Of(grain));

            return reference.OnTimer(id, state).UnwrapExceptions();
        }
    }
}
