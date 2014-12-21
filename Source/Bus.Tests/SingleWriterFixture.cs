using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class SingleWriterFixture
    {
        IMessageBus bus;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
        }

        [Test, Explicit]
        public async void Should_queue_writes_but_still_serve_reads_in_parallel()
        {
            const string grain = "rw-x";

            const int warmupRequests = 500;
            foreach (var i in Enumerable.Range(1, warmupRequests))
                await bus.Send(grain, new Warmup());
            
            var writeLatencies = new[] {200, 100, 50};
            var writersTotal = writeLatencies.Length;
            int readersTotal = writersTotal * 10;
            int readLatency  = writeLatencies.Sum() / readersTotal;

            var sequence = new List<int>();
            var reading = Task.Run(async () =>
            {
                foreach (var i in Enumerable.Range(0, readersTotal))
                {
                    sequence.Add(await bus.Query<int>(grain, new Read
                    {
                        Latency = TimeSpan.FromMilliseconds(readLatency)
                    }));
                }
            });

            var writing = Task.Run(async ()=>
            {
                await Task.WhenAll(Enumerable
                    .Range(0, writersTotal)
                    .Select(i => 
                        bus.Send(grain, new Write
                        {
                            Sequence = i + 1,
                            Latency = TimeSpan.FromMilliseconds(writeLatencies[i])
                        })));
            });

            await Task.WhenAll(writing, reading);

            Assert.That(sequence.Distinct(), Is.EquivalentTo(Enumerable.Range(0, writersTotal + 1)),
                "Should see all changes of the write sequence. O is default state inside grain");

            Assert.That(sequence.OrderBy(x => x).ToArray(), Is.EqualTo(sequence.ToArray()), 
                "All readers should see consistently incrementing sequence");
        }
    }
}
