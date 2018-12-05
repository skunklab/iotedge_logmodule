using System;
using System.Configuration;
using System.Net;
using Orleans.Runtime.Configuration;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using Orleans.Storage.Redis;
using System.Collections.Generic;
using Orleans.Hosting;
using Orleans.Configuration;

namespace Piraeus.SiloHost
{
    public class Silo
    {
        private static OrleansHostWrapper hostWrapper;
        private static bool dockerized;


        public static int Run(string[] args)
        {
            dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);

            return StartSilo(args);
        }
               

        private static int StartSilo(string[] args)
        {

            var silo = new SiloHostBuilder();
            silo.ConfigureEndpoints(siloPort: 1111, gatewayPort: 30000);

            if (dockerized)
            {
                silo.UseAzureStorageClustering(options => { options.ConnectionString = ""; options.TableName = ""; options.MaxStorageBusyRetries = 10; });
                silo.UseAzureTableReminderService(options => { options.ConnectionString = ""; options.TableName = ""; });
                silo.Configure<ClusterOptions>(options => { options.ClusterId = ""; options.ServiceId = ""; });                
            }
            else
            {
                silo.AddMemoryGrainStorage("store", options =>
                {
                    options.NumStorageGrains = 1000;
                });

                silo.UseLocalhostClustering();
            }

           
              //  .AddAzureBlobGrainStorage("store", options =>
              //{
              //    options.ConnectionString = System.Environment.GetEnvironmentVariable("ORLEANS_STORAGE_CONTAINER_NAME");
              //    options.ContainerName = System.Environment.GetEnvironmentVariable("ORLEANS_STORAGE_CONTAINER_NAME");
              //});
                



            // define the cluster configuration   
            ClusterConfiguration config = null;
            string hostname = System.Net.Dns.GetHostName();
            Console.WriteLine("Host Name {0}", hostname);

            if (dockerized)
            {                
                //USE for production and clustering
                config = new ClusterConfiguration();
                config.Globals.DataConnectionString = System.Environment.GetEnvironmentVariable("ORLEANS_LIVENESS_DATACONNECTIONSTRING");
                config.Globals.DataConnectionStringForReminders = System.Environment.GetEnvironmentVariable("ORLEANS_LIVENESS_DATACONNECTIONSTRING");
                config.Globals.ClusterId = System.Environment.GetEnvironmentVariable("ORLEANS_DEPLOYMENT_ID");
                config.Globals.LivenessEnabled = true;
                config.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.AzureTable;               
                config.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.AzureTable;
                config.Globals.ResponseTimeout = TimeSpan.FromSeconds(300.0);
                config.Globals.ClientSenderBuckets = 32768;
                

                config.Defaults.PropagateActivityId = true;
                config.Defaults.HostNameOrIPAddress = hostname;
                config.Defaults.Port = 11111;
                config.Defaults.ProxyGatewayEndpoint = new IPEndPoint(IPAddress.Any, 30000);
                config.Defaults.SiloName = hostname;

                //Console.WriteLine("Variables for Dockerization");
                //Console.WriteLine("Globals CS {0}", config.Globals.DataConnectionString);
                //Console.WriteLine("Globals CS Reminders {0}", config.Globals.DataConnectionStringForReminders);
                //Console.WriteLine("Deployment ID {0}", config.Globals.DeploymentId);
                //Console.WriteLine("Liveness {0}", config.Globals.LivenessEnabled);
                //Console.WriteLine("LivenessType {0}", config.Globals.LivenessType);
                //Console.WriteLine("ReminderServiceType{0}", config.Globals.ReminderServiceType);
                //Console.WriteLine("PropagateActiveId {0}", config.Defaults.PropagateActivityId);
                //Console.WriteLine("HostName {0}", config.Defaults.HostNameOrIPAddress);
                //Console.WriteLine("Port {0}", config.Defaults.Port);
                //Console.WriteLine("ProxyGatewayEndpoint {0}:{1}", config.Defaults.ProxyGatewayEndpoint.Address.ToString(), config.Defaults.ProxyGatewayEndpoint.Port);
                //Console.WriteLine("SiloName {0}", hostname);


                string factor = string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("SERVICEPOINT_CORE_FACTOR")) ? "24" : System.Environment.GetEnvironmentVariable("SERVICEPOINT_CORE_FACTOR");


                ServicePointManager.DefaultConnectionLimit = Convert.ToInt32(factor)  * Environment.ProcessorCount;
                ServicePointManager.UseNagleAlgorithm = false;
               

                IDictionary<string, string> properties = GetStorageProviderProperties();

                string providerName = System.Environment.GetEnvironmentVariable("ORLEANS_STORAGE_PROVIDER_TYPE").ToLowerInvariant();
                if (providerName == "azureblobstore")
                {
                    config.Globals.RegisterLogConsistencyProvider<Orleans.Storage>("store", properties);
                    config.Globals.RegisterStorageProvider<Orleans.Storage.AzureBlobGrainStorage>("store", properties);
                    config.Globals.RegisterStorageProvider<Orleans.Storage.AzureBlobStorage>("store", properties);
                }
                else if (providerName == "redisstore")
                {
                    config.Globals.RegisterStorageProvider<RedisStorageProvider>("store", properties);
                }
                else if (providerName == "memorystore")
                {

                    config.Globals.RegisterStorageProvider<Orleans.Storage.MemoryGrainStorage>("store", properties);
                    //config.Globals.RegisterStorageProvider<Orleans.Storage.MemoryStorage>("store", properties);
                }
                else
                {
                    Console.WriteLine("Orleans Storage Provider NAME not understood.");
                    throw new ArgumentOutOfRangeException("Provider name is not recognized for Orleans storage provider.");
                }
            }
            else
            {
                Console.WriteLine("Identified as localhost deployment.");
                //USE for demo or local testing
                config = ClusterConfiguration.LocalhostPrimarySilo();
                config.AddMemoryStorageProvider("store", 1000);
            }


            var siloHost = new Orleans.Runtime.Host.SiloHost(hostname, config);
           
            Console.WriteLine("Silo host initialized.");

            hostWrapper = new OrleansHostWrapper(config, args);
            Console.WriteLine("Starting host wrapper run.");
            return hostWrapper.Run();
        }

        private static IDictionary<string,string> GetStorageProviderProperties()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string providerName = System.Environment.GetEnvironmentVariable("ORLEANS_STORAGE_PROVIDER_TYPE").ToLowerInvariant();

            if (providerName == "azureblobstore")
            {
                dict.Add("ContainerName", System.Environment.GetEnvironmentVariable("ORLEANS_STORAGE_CONTAINER_NAME"));
                dict.Add("DataConnectionString", System.Environment.GetEnvironmentVariable("ORLEANS_PROVIDER_DATACONNECTIONSTRING"));
            }
            if(providerName == "redisstore")
            {
                dict.Add("DataConnectionString", System.Environment.GetEnvironmentVariable("ORLEANS_PROVIDER_DATACONNECTIONSTRING"));
            }

            if(providerName == "memorystore")
            {
                dict.Add("NumStorageGrains", System.Environment.GetEnvironmentVariable("ORLEANS_MAXMEMORY_STORAGE_GRAINS"));
            }
            

            return dict;
        }

        private static int ShutdownSilo()
        {
            Console.WriteLine("Shutdown Hostwrapper");
        
            if (hostWrapper != null)
            {
                return hostWrapper.Stop();
            }
            return 0;
        }
    }
}
