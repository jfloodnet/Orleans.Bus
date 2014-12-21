using System;
using System.Threading.Tasks;

using Orleans.Concurrency;

namespace Orleans.Bus
{
    [Reentrant]
    public class TestReentrantReaderGrain : ReentrantReaderGrain, ITestReentrantReaderGrain
    {
        int state;

        public override Task OnCommand(object cmd)
        {
            return this.Handle((dynamic)cmd);
        }

        public override async Task<object> OnQuery(object query)
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