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

        void ITimerCollection.Register(string id, Func<Task> callback, TimeSpan due, TimeSpan period)
        {
            recorded.Add(new RecordedTimer(id, callback, due, period));
        }

        void ITimerCollection.Register<TState>(string id, Func<TState, Task> callback, TState state, TimeSpan due, TimeSpan period)
        {
            recorded.Add(new RecordedTimer<TState>(id, callback, state, due, period));
        }

        void ITimerCollection.Unregister(string id)
        {
            recorded.RemoveAll(x => x.Id == id);
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

    public class RecordedTimer
    {
        public readonly string Id;
        public readonly Func<Task> Callback;
        public readonly TimeSpan Due;
        public readonly TimeSpan Period;

        public RecordedTimer(string id, Func<Task> callback, TimeSpan due, TimeSpan period)
        {
            Id = id;
            Callback = callback;
            Due = due;
            Period = period;
        }
    }

    public class RecordedTimer<TState> : RecordedTimer
    {
        public new readonly Func<TState, Task> Callback;
        public readonly TState State;

        public RecordedTimer(string id, Func<TState, Task> callback, TState state, TimeSpan due, TimeSpan period)
            : base(id, null, due, period)
        {
            Callback = callback;
            State = state;
        }
    }
}
