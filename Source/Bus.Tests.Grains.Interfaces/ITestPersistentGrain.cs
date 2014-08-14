using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    [Serializable, Immutable]
    public class ChangeValue : Command
    {
        public readonly int Value;

        public ChangeValue(int value)
        {
            Value = value;
        }
    }

    [Serializable, Immutable]
    public class ClearValue : Command
    {}

    [Serializable, Immutable]
    public class GetValue : Query<string>
    {}

    [Serializable]
    public class ValueChanged : Event
    {
        public readonly int Value;

        public ValueChanged(int value)
        {
            Value = value;
        }
    }

    [Handles(typeof(ChangeValue))]
    [Handles(typeof(ClearValue))]
    [Answers(typeof(GetValue))]
    [Notifies(typeof(ValueChanged))]
    [ExtendedPrimaryKey]
    public interface ITestPersistentGrain : IMessageBasedGrain
    {
        [Handler] Task HandleCommand(object cmd);
        [Handler] Task<object> AnswerQuery(object query);
    }
}
