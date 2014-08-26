using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    [Immutable, Serializable]
    public class RegisterTimer : Command
    {
        public readonly string Text;

        public RegisterTimer(string text)
        {
            Text = text;
        }
    }

    [Immutable, Serializable]
    public class UnregisterTimer : Command
    {}
    
    [Immutable, Serializable]
    public class SetTextByTimer : Command
    {
        public readonly string Text;

        public SetTextByTimer(string text)
        {
            Text = text;
        }
    }

    [Immutable, Serializable]
    public class GetTextSetByTimer : Query<string>
    {}

    [Handles(typeof(RegisterTimer))]
    [Handles(typeof(UnregisterTimer))]
    [Handles(typeof(SetTextByTimer))]
    [Answers(typeof(GetTextSetByTimer))]
    [ExtendedPrimaryKey]
    public interface ITestCommandTimerGrain : IMessageBasedGrain
    {
        [Handler] Task HandleCommand(object cmd);
        [Handler] Task<object> AnswerQuery(object query);
    }
}
