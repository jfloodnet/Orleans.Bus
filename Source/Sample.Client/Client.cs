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
            var rand = new Random();

            foreach (var i in Enumerable.Range(1, 2000))
            {
                var topic = i.ToString();

                await bus.Send(topic, new CreateTopic("[" + i + "]", new Dictionary<string, TimeSpan>
                {
                    {"facebook", TimeSpan.FromMinutes(rand.Next(1, 3))},
                    {"twitter", TimeSpan.FromMinutes(rand.Next(1, 2))},
                }));
            }
        }
    }
}
