using System;
using System.Threading.Tasks;

using Orleans.Concurrency;

namespace Orleans.Bus
{
    [Reentrant]
    public class TestSingleWriterGrain : MessageBasedGrain, ITestSingleWriterGrain
    {
        MessageQueue commands;
        int state;

        public override Task ActivateAsync()
        {
            commands = new MessageQueue(cmd => this.Handle((dynamic)cmd));
            return base.ActivateAsync();
        }

        public Task OnCommand(object cmd)
        {
            return commands.Enqueue(cmd);
        }

        public async Task<object> OnQuery(object query)
        {
            return await this.Answer((dynamic)query);
        }

        public Task Handle(Warmup cmd)
        {
            state = 0;
            return TaskDone.Done;
        }

        public async Task Handle(Write c)
        {
            await Task.Delay(c.Latency);
            state = c.Sequence;
        }

        public async Task<int> Answer(Read q)
        {
            await Task.Delay(q.Latency);
            return state;
        }
    }
}