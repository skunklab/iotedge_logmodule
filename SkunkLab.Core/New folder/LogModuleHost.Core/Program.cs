using LogModule;
using LogModule.Configuration;
using LogModule.DirectMethods;
using LogModule.EdgeHub;
using LogModule.WebHost;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Devices.Client;
using System;
using System.Threading.Tasks;

namespace LogModuleHost.Core
{
    class Program
    {
        private static DockerConfig dockerConfig;
        private static EdgeHubLogHost edgeHubHost;
        private static DirectMethodsHost directMethodsHost;
        private static ContainerLocal local;
        private static ContainerRemote remote;
        private static ModuleClient client;

        public static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            string portString = System.Environment.GetEnvironmentVariable("LM_Port");
            string accountName = System.Environment.GetEnvironmentVariable("LM_BlobStorageAccountName");
            string accountKey = System.Environment.GetEnvironmentVariable("LM_BlobStorageAccountKey");
            string features = System.Environment.GetEnvironmentVariable("LM_Features");

            dockerConfig = new DockerConfig(accountName, accountKey, string.IsNullOrEmpty(portString) ? 8888 : Convert.ToInt32(portString), features);

            Task task = RunAsync();
            Task.WhenAll(task);

        }

        public static async Task RunAsync()
        {
            if (dockerConfig.FeatureFlags.HasFlag(LogModuleFeatureFlags.EdgeHubHost) ||
                dockerConfig.FeatureFlags.HasFlag(LogModuleFeatureFlags.DirectMethodsHost))
            {
                client = await ModuleClient.CreateFromEnvironmentAsync(TransportType.Mqtt).ConfigureAwait(false);                
            }

            if(dockerConfig.FeatureFlags.HasFlag(LogModuleFeatureFlags.EdgeHubHost))
            {
                local = new ContainerLocal(dockerConfig.BlobStorageAccountName, dockerConfig.BlobStorageAccountKey);
                edgeHubHost = new EdgeHubLogHost(client, local);
            }

            if(dockerConfig.FeatureFlags.HasFlag(LogModuleFeatureFlags.DirectMethodsHost))
            {
                remote = new ContainerRemote(dockerConfig.BlobStorageAccountName, dockerConfig.BlobStorageAccountKey);
                directMethodsHost = new DirectMethodsHost(client, remote);
            }

            if(dockerConfig.FeatureFlags.HasFlag(LogModuleFeatureFlags.WebHost))
            {
                CreateWebHostBuilder(null);
            }
        }

        


        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Limits.MaxConcurrentConnections = 100;
                    options.Limits.MaxConcurrentUpgradedConnections = 100;
                    options.Limits.MaxRequestBodySize = 1000 * 1024;
                    options.Limits.MinRequestBodyDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    options.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    options.ListenAnyIP(dockerConfig.Port);

                });


        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();

            Console.WriteLine("********** Unobserved Exception Block **********");
            Console.WriteLine("Error = '{0}'", e.Exception.Message);

            Exception inner = e.Exception.InnerException;
            int indexer = 0;
            while (inner != null)
            {
                indexer++;
                Console.WriteLine("Inner index {0} '{1}'", indexer, inner.Message);
                if (String.IsNullOrEmpty(inner.Message))
                {
                    Console.WriteLine("-------------- Start Stack Trace {0} ---------------", indexer);
                    Console.WriteLine(inner.StackTrace);
                    Console.WriteLine("-------------- End Stack Trace {0} ---------------", indexer);
                }
                inner = inner.InnerException;
            }

            Console.WriteLine("********** End Unobserved Exception Block **********");
        }
    }
}
