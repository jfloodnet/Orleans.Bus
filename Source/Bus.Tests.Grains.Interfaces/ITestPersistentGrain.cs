using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Concurrency;

namespace Orleans.Bus
{
    [Serializable, Immutable]
    public class SetValue : Command
    {
        public int Value;
    }

    [Serializable, Immutable]
    public class ClearValue : Command
    {}

    [Serializable, Immutable]
    public class GetValue : Query<string>
    {}

    [Handles(typeof(SetValue))]
    [Handles(typeof(ClearValue))]
    [Answers(typeof(GetValue))]
    [ExtendedPrimaryKey]
    public interface ITestPersistentGrain : IMessageBasedGrain
    {
        [Handler] Task HandleCommand(object cmd);
        [Handler] Task<object> AnswerQuery(object query);
    }
}    
