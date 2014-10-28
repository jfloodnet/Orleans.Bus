using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class BenchmarkFixture
    {
        IMessageBus bus;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
        }

        [Test]
        public void Memory_overhead_per_activation()
        {
            var before = GC.GetTotalMemory(true);

            var sw = new Stopwatch();
            sw.Start();

            const int count = 50000;
            Task.WaitAll(Enumerable
                .Range(1, count)
                .Select(i => bus.Send("scratch" + i, new Scratch()))
                .ToArray());

            sw.Stop();
            Console.WriteLine("{0} activations done in {1} (s)", count, sw.Elapsed.TotalSeconds);
            Console.WriteLine("Time per activation {0} (ms)", sw.Elapsed.TotalMilliseconds / count);

            var after = GC.GetTotalMemory(true);

            const int mb = (1000 * 1000);
            Console.WriteLine("Total memory before (Mb): {0}", before / (double)mb);
            Console.WriteLine("Total memory after (Mb): {0}", after / (double)mb);
            Console.WriteLine("Memory used per grain activation (bytes): " + (after - before) / (double) count);
        } 
    }
}