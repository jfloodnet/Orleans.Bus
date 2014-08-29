using System;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class EnvelopeSupportFixture
    {
        IMessageBus bus;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
        }

        [Test]
        public async void When_sending_command()
        {
            var commandEnvelope = new CommandEnvelope(
                Guid.NewGuid(), new HandleEnvelopedCommand());

            Assert.DoesNotThrow(async ()=> 
                await bus.Send("test", typeof(HandleEnvelopedCommand), commandEnvelope));

            var queryEnvelope = new QueryEnvelope<MetadataPassedInEnvelope>(
                Guid.NewGuid(), new AnswerEnvelopedQuery());

            var result = await bus.Query<MetadataPassedInEnvelope>("test", typeof(AnswerEnvelopedQuery), queryEnvelope);
            
            Assert.That(result.PreviousCommandId, 
                Is.EqualTo(commandEnvelope.Id));

            Assert.That(result.CurrentQueryId, 
                Is.EqualTo(queryEnvelope.Id));
        }
    }
}