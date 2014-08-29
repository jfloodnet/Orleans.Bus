using System;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestEnvelopeSupportGrain : MessageBasedGrain, ITestEnvelopeSupportGrain
    {
        Guid previousCommandId;

        public Task HandleCommand(object message)
        {
            var envelope = (CommandEnvelope) message;
            previousCommandId = envelope.Id;
            return TaskDone.Done;
        }

        public Task<object> AnswerQuery(object message)
        {
            var envelope = (QueryEnvelope<MetadataPassedInEnvelope>)message;
            return Task.FromResult((object)new MetadataPassedInEnvelope(previousCommandId, envelope.Id));
        }
    }
}