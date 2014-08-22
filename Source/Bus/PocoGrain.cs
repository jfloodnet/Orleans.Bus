using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Base interface for POCO grains
    /// </summary>
    public interface IPocoGrain : IMessageBasedGrain
    {
        /// <summary>
        /// Generic command handler
        /// </summary>
        [Handler] Task HandleCommand(object cmd);

        /// <summary>
        /// Generic query handler
        /// </summary>
        [Handler] Task<object> AnswerQuery(object query);
    }    
    
    /// <summary>
    /// Base class for POCO grains
    /// </summary>
    public abstract class PocoGrain : MessageBasedGrain, IPocoGrain
    {
        /// <summary>
        /// Set this activator delegate in subclass to create and activate new instance of POCO
        /// </summary>
        protected Func<Task> OnActivate = 
            () => { throw new InvalidOperationException("Please create activator in subclass constructor"); };        
        
        /// <summary>
        /// Set this optional deactivator delegate in subclass to call deactivation behavior on instance of POCO
        /// </summary>
        protected Func<Task> OnDeactivate =()=> TaskDone.Done;

        /// <summary>
        /// Set this handler delegate in subclass to dispatch incoming command to an instance of POCO
        /// </summary>
        protected Func<object, Task> OnCommand =
            cmd => { throw new InvalidOperationException("Please set dispatcher in subclass constructor"); };

        /// <summary>
        /// Set this handler delegate in subclass to dispatch incoming query to an instance of POCO
        /// </summary>
        protected Func<object, Task<object>> OnQuery =
            query => { throw new InvalidOperationException("Please set dispatcher in subclass constructor"); };

        /// <summary>
        /// This method is called at the end of the process of activating a grain.
        /// It is called before any messages have been dispatched to the grain.
        /// For grains with declared persistent state, this method is called after the State property has been populated.
        /// </summary>
        public override Task ActivateAsync()
        {            
            return OnActivate();
        }

        /// <summary>
        /// This method is called at the begining of the process of deactivating a grain.
        /// </summary>
        public override Task DeactivateAsync()
        {
            return OnDeactivate();
        }

        Task IPocoGrain.HandleCommand(object cmd)
        {
            return OnCommand(cmd);
        }

        Task<object> IPocoGrain.AnswerQuery(object query)
        {
            return OnQuery(query);
        }
    }

    /// <summary>
    /// Base class for persistent POCO grains
    /// </summary>
    /// <typeparam name="TState">Type of persistent state</typeparam>
    public abstract class PocoGrain<TState> : MessageBasedGrain<TState>, IPocoGrain         
    {
        /// <summary>
        /// Set this activator delegate in subclass to create and activate new instance of POCO
        /// </summary>
        protected Func<Task> OnActivate =
            () => { throw new InvalidOperationException("Please create activator in subclass constructor"); };

        /// <summary>
        /// Set this optional deactivator delegate in subclass to call deactivation behavior on instance of POCO
        /// </summary>
        protected Func<Task> OnDeactivate = () => TaskDone.Done;

        /// <summary>
        /// Set this handler delegate in subclass to dispatch incoming command to an instance of POCO
        /// </summary>
        protected Func<object, Task> OnCommand =
            cmd => { throw new InvalidOperationException("Please set dispatcher in subclass constructor"); };

        /// <summary>
        /// Set this handler delegate in subclass to dispatch incoming query to an instance of POCO
        /// </summary>
        protected Func<object, Task<object>> OnQuery =
            query => { throw new InvalidOperationException("Please set dispatcher in subclass constructor"); };

        /// <summary>
        /// This method is called at the end of the process of activating a grain.
        /// It is called before any messages have been dispatched to the grain.
        /// For grains with declared persistent state, this method is called after the State property has been populated.
        /// </summary>
        public override async Task ActivateAsync()
        {
            await base.ActivateAsync();
            await OnActivate();
        }

        /// <summary>
        /// This method is called at the begining of the process of deactivating a grain.
        /// </summary>
        public override Task DeactivateAsync()
        {
            return OnDeactivate();
        }
        
        Task IPocoGrain.HandleCommand(object cmd)
        {
            return OnCommand(cmd);
        }

        Task<object> IPocoGrain.AnswerQuery(object query)
        {
            return OnQuery(query);
        }
    }       
}
