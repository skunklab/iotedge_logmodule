using LogModule.Configuration;
using LogModule.Hosts;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Devices.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LogModule
{
    public class Program
    {
        private static DockerConfig dockerConfig;
        private static EdgeHubLogHost edgeHubHost;
        private static DirectMethodsHost directMethodsHost;
        private static ContainerLocal local;
        private static ContainerRemote remote;
        private static ModuleClient edgeHubClient;
        private static ModuleClient directMethodsClient;
        private static ManualResetEventSlim done;

        public static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            string portString = System.Environment.GetEnvironmentVariable("LM_Port");
            string accountName = System.Environment.GetEnvironmentVariable("LM_BlobStorageAccountName");
            string accountKey = System.Environment.GetEnvironmentVariable("LM_BlobStorageAccountKey");
            string features = System.Environment.GetEnvironmentVariable("LM_Features");
            
            WriteConfigValidation(portString, accountName, accountKey, features);
                       
            dockerConfig = new DockerConfig(accountName, accountKey, string.IsNullOrEmpty(portString) ? 8877 : Convert.ToInt32(portString), features);
            
            Task task = RunAsync(args);
            Task.WhenAll(task);

            done = new ManualResetEventSlim(false);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("Log Module is running...");
            done.Wait();

        }

        private static async Task RunAsync(string[] args)
        {
            try
            {
                if (dockerConfig.FeatureFlags.HasFlag(LogModuleFeatureFlags.EdgeHubHost))
                {
                    edgeHubClient = await ModuleClient.CreateFromEnvironmentAsync(TransportType.Mqtt);
                    await edgeHubClient.OpenAsync();
                    Console.WriteLine("Edge Hub client created");

                    local = ContainerLocal.Create(dockerConfig.BlobStorageAccountName, dockerConfig.BlobStorageAccountKey);
                    edgeHubHost = new EdgeHubLogHost(edgeHubClient, local);
                    edgeHubHost.Init();
                    Console.WriteLine("Edge Hub Host Initialized");
                }
                else
                {
                    Console.WriteLine("-----> Edge Hub Host NOT configured <-----");
                }

                if (dockerConfig.FeatureFlags.HasFlag(LogModuleFeatureFlags.DirectMethodsHost))
                {
                    directMethodsClient = await ModuleClient.CreateFromEnvironmentAsync(TransportType.Mqtt);
                    await directMethodsClient.OpenAsync();
                    Console.WriteLine("Direct Methods client created");

                    remote = ContainerRemote.Create(dockerConfig.BlobStorageAccountName, dockerConfig.BlobStorageAccountKey);
                    directMethodsHost = new DirectMethodsHost(directMethodsClient, remote);
                    directMethodsHost.Init();
                    Console.WriteLine("Direct Methods Host Initialized");
                }
                else
                {
                    Console.WriteLine("-----> Direct Methods Host NOT configured <-----");
                }

                if (dockerConfig.FeatureFlags.HasFlag(LogModuleFeatureFlags.WebHost))
                {
                    CreateWebHostBuilder(args).Build().Run();
                    Console.WriteLine("Web Host Initialized");
                }
                else
                {
                    Console.WriteLine("-----> Web Host NOT configured <-----");
                }

                Console.WriteLine("Started all services");
            }
            catch(Exception ex)
            {
                Console.WriteLine("---------- WARNING ----------");
                Console.WriteLine("-----> ERROR: CONFIG FAILED <-----");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("----------------------------");
                throw ex;
            }

        }

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
            Console.WriteLine("----->  Forcing a restart <-----");
            done.Set();
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

        private static void WriteConfigValidation(string portString, string accountName, string accountKey, string features)
        {
            if (string.IsNullOrEmpty(portString))
            {
                Console.WriteLine("LM_Port not configured; using default 8877 for Kestrel server");
            }

            if (string.IsNullOrEmpty(accountName))
            {
                Console.WriteLine("Azure Blob storage account name not configured");
            }

            if (string.IsNullOrEmpty(accountKey))
            {
                Console.WriteLine("Azure Blob storage key not configured");
            }

            if (string.IsNullOrEmpty(features))
            {
                Console.WriteLine("Log Module features not configured");
            }
        }
    }
}
