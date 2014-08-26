using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TimerCollectionMock : ITimerCollection, IEnumerable<RecordedTimer>
    {
        readonly List<RecordedTimer> recorded = new List<RecordedTimer>();

        void ITimerCollection.Register(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
        {
            recorded.Add(new RecordedCallbackTimer(id, callback, due, period));
        }

        void ITimerCollection.Register<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback)
        {
            recorded.Add(new RecordedCallbackTimer<TState>(id, callback, state, due, period));
        }

        public void Register<TCommand>(TimeSpan due, TimeSpan period, TCommand command)
        {
            recorded.Add(new RecordedCommandTimer(command, due, period));
        }

        void ITimerCollection.Unregister(string id)
        {
            recorded.RemoveAll(x => x.Id == id);
        }

        public void Unregister<TCommand>()
        {
            ((ITimerCollection)this).Unregister(typeof(TCommand).FullName);
        }

        bool ITimerCollection.IsRegistered(string id)
        {
            return recorded.Exists(x => x.Id == id);
        }

        IEnumerable<string> ITimerCollection.Registered()
        {
            return recorded.Select(x => x.Id);
        }

        public IEnumerator<RecordedTimer> GetEnumerator()
        {
            return recorded.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public RecordedTimer this[int index]
        {
            get { return recorded.ElementAt(index); }
        }

        public RecordedTimer this[string id]
        {
            get { return recorded.SingleOrDefault(x => x.Id == id); }
        }

        public void Clear()
        {
            recorded.Clear();
        }
    }

    public abstract class RecordedTimer
    {
        public readonly string Id;
        public readonly TimeSpan Due;
        public readonly TimeSpan Period;

        public RecordedTimer(string id, TimeSpan due, TimeSpan period)
        {
            Id = id;
            Due = due;
            Period = period;
        }
    }
    
    public class RecordedCallbackTimer: RecordedTimer
    {
        public readonly Func<Task> Callback;

        public RecordedCallbackTimer(string id, Func<Task> callback, TimeSpan due, TimeSpan period)
            : base(id, due, period)
        {
            Callback = callback;
        }
    }

    public class RecordedCallbackTimer<TState> : RecordedTimer
    {
        public readonly Func<TState, Task> Callback;
        public readonly TState State;

        public RecordedCallbackTimer(string id, Func<TState, Task> callback, TState state, TimeSpan due, TimeSpan period)
            : base(id, due, period)
        {
            Callback = callback;
            State = state;
        }
    }

    public class RecordedCommandTimer : RecordedTimer
    {
        public readonly object Command;

        public RecordedCommandTimer(object command, TimeSpan due, TimeSpan period)
            : base(command.GetType().FullName, due, period)
        {
            Command = command;
        }
    }
}
