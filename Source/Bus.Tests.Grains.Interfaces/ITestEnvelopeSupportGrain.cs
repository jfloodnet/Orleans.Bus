using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Concurrency;

namespace Orleans.Bus
{
    [Immutable, Serializable]
    public class HandleEnvelopedCommand : Command
    {}

    [Immutable, Serializable]
    public class AnswerEnvelopedQuery : Query<MetadataPassedInEnvelope>
    {}

    [Immutable, Serializable]
    public class MetadataPassedInEnvelope
    {
        public readonly Guid PreviousCommandId;
        public readonly Guid CurrentQueryId;

        public MetadataPassedInEnvelope(Guid previousCommandId, Guid currentQueryId)
        {
            PreviousCommandId = previousCommandId;
            CurrentQueryId = currentQueryId;
        }
    }

    [Handles(typeof(HandleEnvelopedCommand))]
    [Answers(typeof(AnswerEnvelopedQuery))]
    [ExtendedPrimaryKey]
    public interface ITestEnvelopeSupportGrain : IMessageBasedGrain
    {
        [Handler] Task HandleCommand(object message);
        [Handler] Task<object> AnswerQuery(object message);
    }
}
