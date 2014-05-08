﻿using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public abstract class Grain : MessageBasedGrain
    {}

    public abstract class Grain<TState> : MessageBasedGrain<TState>
        where TState : class, IGrainState
    {}

    public abstract class ObservableGrain : Grain, IObservableGrain
    {
        readonly IObserverCollection observers = new ObserverCollection();

        Task IObservableGrain.Attach(IObserve o, Type e)
        {
            observers.Attach(o, e);
            return TaskDone.Done;
        }

        Task IObservableGrain.Detach(IObserve o, Type e)
        {
            observers.Detach(o, e);
            return TaskDone.Done;
        }

        protected void Notify<TEvent>(TEvent e) where TEvent : Event
        {
            observers.Notify(this.Id(), e);
        }
    }

    public abstract class ObservableGrain<TState> : Grain<TState>, IObservableGrain
        where TState : class, IGrainState
    {
        readonly IObserverCollection observers = new ObserverCollection();

        Task IObservableGrain.Attach(IObserve o, Type e)
        {
            observers.Attach(o, e);
            return TaskDone.Done;
        }

        Task IObservableGrain.Detach(IObserve o, Type e)
        {
            observers.Detach(o, e);
            return TaskDone.Done;
        }

        protected void Notify<TEvent>(TEvent e) where TEvent : Event
        {
            observers.Notify(this.Id(), e);
        }
    }
}
