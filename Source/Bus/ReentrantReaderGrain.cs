using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Orleans.Concurrency;

namespace Orleans.Bus
{
    public interface IReentrantReaderGrain : IMessageBasedGrain
    {
        [Handler] Task OnWrite(object cmd);
        [Handler] Task<object> OnRead(object query);    
    }

    [Reentrant]
    public abstract class ReentrantReaderGrain : MessageBasedGrain, IReentrantReaderGrain
    {
        ActionBlock<Write> queue;

        public override Task ActivateAsync()
        {
            queue = new ActionBlock<Write>(async write =>
            {
                try
                {
                    await OnCommand(write.Command);
                    write.Completion.SetResult(null);
                }
                catch (Exception ex)
                {
                    write.Completion.SetException(ex);
                }                
            });

            return base.ActivateAsync();
        }

        class Write
        {
            public object Command;
            public TaskCompletionSource<object> Completion;
        }

        public Task OnWrite(object cmd)
        {
            var source = new TaskCompletionSource<object>();

            queue.Post(new Write
            {
                Command = cmd,
                Completion = source
            });

            return source.Task;
        }

        public Task<object> OnRead(object query)
        {
            return OnQuery(query);
        }

        public abstract Task OnCommand(object cmd);
        public abstract Task<object> OnQuery(object query);
    }
}
