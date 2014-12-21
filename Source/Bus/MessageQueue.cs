using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Orleans.Bus
{
    /// <summary>
    /// Asynchronous in-memtory message queue. 
    /// Suitable to be used inside reentrant grains to queue  execution of incoming messages.
    /// </summary>
    public sealed class MessageQueue
    {
        readonly ActionBlock<Item> queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueue"/> class.
        /// </summary>
        /// <param name="dispatch">The actual message dispatcher.</param>
        public MessageQueue(Func<object, Task> dispatch)
        {
            queue = new ActionBlock<Item>(async item =>
            {
                try
                {
                    await dispatch(item.Message);
                    item.Completion.SetResult(null);
                }
                catch (Exception ex)
                {
                    item.Completion.SetException(ex);
                }
            });            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueue"/> class.
        /// </summary>
        /// <param name="dispatch">The actual message dispatcher.</param>
        public MessageQueue(Func<object, Task<object>> dispatch)
        {
            queue = new ActionBlock<Item>(async item =>
            {
                try
                {
                    var result = await dispatch(item.Message);
                    item.Completion.SetResult(result);
                }
                catch (Exception ex)
                {
                    item.Completion.SetException(ex);
                }
            });
        }

        class Item
        {
            public object Message;
            public TaskCompletionSource<object> Completion;
        }

        /// <summary>
        /// Enqueues the specified message for sequential execution.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The promise</returns>
        public Task Enqueue(object message)
        {
            var source = new TaskCompletionSource<object>();

            queue.Post(new Item
            {
                Message = message,
                Completion = source
            });

            return source.Task;
        }
    }
}
