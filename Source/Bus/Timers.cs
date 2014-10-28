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
        /// Registers a timer to send periodic callbacks to this grain.
        /// 
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
        /// Also if grain is not marked as <see cref="ReentrantAttribute"/>
        ///             the callback invocation will be synchronized and will conform to usual turn based execution semantics
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
        /// 
        /// </para>
        /// 
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        void Register(string id, TimeSpan due, TimeSpan period, Func<Task> callback);

        /// <summary>
        /// Registers a timer to send periodic callbacks to this grain.
        /// </summary>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        /// <param name="state">State object that will be passed as argument when calling the  <paramref name="callback"/>.</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        void Register<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback);

        /// <summary>
        /// Registers command timer which will exhibit non-interleaved semantics
        /// </summary>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        /// <param name="command">The command to be dispatched on timer callback</param>
        void Register<TCommand>(TimeSpan due, TimeSpan period, TCommand command);

        /// <summary>
        /// Unregister previously registered timer. 
        /// </summary>
        /// <param name="id">Unique id of the timer</param>
        void Unregister(string id);

        /// <summary>
        /// Unregister previously registered command timer. 
        /// </summary>
        void Unregister<TCommand>();

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

    public class TimerCollection : ITimerCollection
    {
        readonly IDictionary<string, IDisposable> timers = new Dictionary<string, IDisposable>();
        readonly IExposeGrainInternals grain;
        readonly string id;
        readonly IMessageBus bus;

        public TimerCollection(IMessageBasedGrain grain, string id, IMessageBus bus)
        {
            this.grain = (IExposeGrainInternals) grain;
            this.id = id;
            this.bus = bus;
        }

        public void Register(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
        {
            timers.Add(id, grain.RegisterTimer(s => callback(), null, due, period));
        }

        public void Register<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback)
        {
            timers.Add(id, grain.RegisterTimer(s => callback((TState)s), state, due, period));
        }

        public void Register<TCommand>(TimeSpan due, TimeSpan period, TCommand command)
        {
            Register(typeof(TCommand).FullName, due, period, command, CommandTimerCallback);
        }

        public void Unregister(string id)
        {
            var timer = timers[id];
            timers.Remove(id);
            timer.Dispose();            
        }        
        
        public void Unregister<TCommand>()
        {
            Unregister(typeof(TCommand).FullName);
        }

        public bool IsRegistered(string id)
        {
            return timers.ContainsKey(id);
        }

        public IEnumerable<string> Registered()
        {
            return timers.Keys;
        }

        Task CommandTimerCallback<TCommand>(TCommand command)
        {
            return bus.Send(id, command);
        }
    }
}
