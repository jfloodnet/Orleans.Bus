using System;

using Orleans;
using Orleans.Bus;

namespace Sample
{
    public static class Program
    {
        private static OrleansHostWrapper hostWrapper;

        public static void Main(string[] args)
        {
            var hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = args,
            });

            OrleansClient.Initialize("ClientConfiguration.xml");
            RunClient();

            Console.WriteLine("Press Enter to terminate ...");
            Console.ReadLine();

            hostDomain.DoCallBack(ShutdownSilo);
        }

        static void InitSilo(string[] args)
        {
            hostWrapper = new OrleansHostWrapper(args);

            if (!hostWrapper.Run())
                Console.Error.WriteLine("Failed to initialize Orleans silo");
        }

        static void ShutdownSilo()
        {
            if (hostWrapper == null)
                return;

            hostWrapper.Stop();
            hostWrapper.Dispose();

            GC.SuppressFinalize(hostWrapper);
        }

        static void RunClient()
        {
            var client = new Client(MessageBus.Instance);
            client.Run();
        }
    }
}
