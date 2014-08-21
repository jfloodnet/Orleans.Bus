using System;
using System.Collections.Generic;
using System.Linq;

using Orleans.Bus;

namespace Sample
{
    public class Client
    {
        readonly IMessageBus bus;

        public Client(IMessageBus bus)
        {
            this.bus = bus;
        }

        public async void Run()
        {
            foreach (var i in Enumerable.Range(1, 10))
            {
                var topic = i.ToString();

                await bus.Send(topic, new CreateTopic("[" + i + "]", new Dictionary<string, TimeSpan>
                {
                    {"facebook", TimeSpan.FromMinutes(2)},
                    {"twitter", TimeSpan.FromMinutes(1)},
                }));
            }
        }
    }
}
