using System;

using Microsoft.WindowsAzure.Storage;

using Orleans;
using Orleans.Bus;

namespace Sample
{
    public static class Program
    {
        static OrleansSilo silo;

        public static void Main()
        {
            var args = new[]
            {
                "UseDevelopmentStorage=true" // # TopicStorageAccount
            };

            var hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = StartSilo,
                AppDomainInitializerArguments = args,
            });

            OrleansClient.Initialize("ClientConfiguration.xml");
            RunClient();

            Console.WriteLine("Press Enter to terminate ...");
            Console.ReadLine();

            hostDomain.DoCallBack(StopSilo);
        }

        static void StartSilo(string[] args)
        {
            TopicStorage.Init(CloudStorageAccount.Parse(args[0]));

            silo = new OrleansSilo();
            silo.Start();
        }

        static void StopSilo()
        {
            silo.Stop();
        }

        static void RunClient()
        {
            var client = new Client(MessageBus.Instance);
            client.Run();
        }
    }
}
