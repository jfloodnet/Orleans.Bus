using System;
using System.Net;

using Orleans.Host;

namespace Sample
{
    internal class OrleansSilo
    {
        OrleansSiloHost host;

        public void Start()
        {
            host = new OrleansSiloHost(Dns.GetHostName())
            {
                ConfigFileName = "ServerConfiguration.xml"
            };
            
            host.InitializeOrleansSilo();
            host.StartOrleansSilo();
            host.LoadOrleansConfig();
        }

        public void Stop()
        {
            host.StopOrleansSilo();
            host.Dispose();
        }
    }
}
