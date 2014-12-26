using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class NonReentrantTimerFixture
    {
        IMessageBus bus;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
        }

        [Test]
        public async void When_registering_command_timer()
        {
            Assert.That(await bus.Query<string>("test", new GetTextSetByTimer()), 
                Is.EqualTo("NONE"));

            await bus.Send("test", new RegisterTimer("SOME"));
            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.That(await bus.Query<string>("test", new GetTextSetByTimer()),
                Is.EqualTo("SOME"));

            await bus.Send("test", new UnregisterTimer());
            await bus.Send("test", new RegisterTimer("CHANGE"));
            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.That(await bus.Query<string>("test", new GetTextSetByTimer()),
                Is.EqualTo("CHANGE"));
        }
    }
}