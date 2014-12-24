using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture, Explicit]
    public class BenchmarkFixture
    {
        const int mb = (1024 * 1024);
        
        IMessageBus bus;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
            GC.Collect();
        }

        [Test]
        public void Memory_overhead_per_activation()
        {
            Console.WriteLine("Checking memory overhead per grain activation ...");

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
            
            Console.WriteLine("Total memory before (Mb): {0}", before / (double)mb);
            Console.WriteLine("Total memory after (Mb): {0}", after / (double)mb);
            Console.WriteLine("Memory used per grain activation (bytes): " + (after - before) / (double) count);
        }

        [Test]
        public void Memory_overhead_per_message_queue()
        {
            Console.WriteLine("Checking overhead per async message queue ...");

            var before = GC.GetTotalMemory(true);

            const int count = 50000;
            var queues = Enumerable
                .Range(1, count)
                .Select(i => new MessageQueue(o => TaskDone.Done))
                .ToList();

            var after = GC.GetTotalMemory(true);

            Console.WriteLine("{0} message queues created", queues.Count);
            Console.WriteLine("Total memory before (Mb): {0}", before / (double)mb);
            Console.WriteLine("Total memory after (Mb): {0}", after / (double)mb);
            Console.WriteLine("Memory used per single message queue (bytes): " + (after - before) / (double)queues.Count);
        } 
    }
}