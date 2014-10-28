using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Concurrency;

namespace Orleans.Bus
{
    [Immutable, Serializable]
    public class Scratch : Command
    {}

    [Handles(typeof(Scratch))]
    [ExtendedPrimaryKey]
    public interface ITestBareGrain : IMessageBasedGrain
    {
        [Handler] Task HandleCommand(object cmd);
    }
}