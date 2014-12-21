using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Concurrency;

namespace Orleans.Bus
{
    [Immutable, Serializable]
    public class Warmup : Command
    {}

    [Immutable, Serializable]
    public class Operation
    {
        public int Sequence;
        public TimeSpan Latency;
    }
    
    [Immutable, Serializable]
    public class Write : Operation, Command
    {}

    [Immutable, Serializable]
    public class Read : Operation, Query<int>
    {}

    [Handles(typeof(Warmup))]
    [Handles(typeof(Write))]
    [Answers(typeof(Read))]
    [ExtendedPrimaryKey]
    public interface ITestSingleWriterGrain : IMessageBasedGrain
    {
        [Handler] Task OnCommand(object cmd);
        [Handler] Task<object> OnQuery(object query);
    }
}