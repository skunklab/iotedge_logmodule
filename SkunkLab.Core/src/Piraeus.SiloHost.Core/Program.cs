using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Piraeus.Configuration.Core;
using Piraeus.Configuration.Settings;
using Piraeus.GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.SiloHost.Core
{
    class Program
    {
        private static OrleansConfig orleansConfig;
        private static PiraeusConfig piraeusConfig;
        private static string hostname;
        private static IPAddress address;
        private static ISiloHost host;
        private static ManualResetEventSlim done;

        static void Main(string[] args)
        {
            orleansConfig = GetOrleansConfiguration();
            //piraeusConfig = GetPiraeusConfiguration();
            //hostname = GetLocalHostName();
            //address = GetIPAddress(hostname);

            CreateSiloHost();

            Task task = host.StartAsync();
            Task.WhenAll(task);

            done = new ManualResetEventSlim(false);


            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("TCP Gateway is running...");
            done.Wait();

        }

        private static void CreateSiloHost()
        {
            if(orleansConfig.Dockerized)
            {
                CreateClusteredSiloHost();
            }
            else
            {
                CreateLocalSiloHost();
            }
        }

        private static void CreateClusteredSiloHost()
        {
            var silo = new SiloHostBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = orleansConfig.OrleansClusterId;
                    options.ServiceId = orleansConfig.OrleansServiceId;
                })
                .UseAzureStorageClustering(options => options.ConnectionString = orleansConfig.OrleansDataConnectionString)
                .ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000)
                .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning).AddConsole());

            host = silo.Build();
        }

        private static void CreateLocalSiloHost()
        {
            var builder = new SiloHostBuilder()
            // Use localhost clustering for a single local silo
            .UseLocalhostClustering()
            // Configure ClusterId and ServiceId
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = orleansConfig.OrleansClusterId;
                options.ServiceId = orleansConfig.OrleansServiceId;
            })            
            // Configure connectivity
            .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
            // Configure logging with any logging framework that supports Microsoft.Extensions.Logging.
            // In this particular case it logs using the Microsoft.Extensions.Logging.Console package.
            .ConfigureLogging(logging => logging.AddConsole());
            
            host = builder.Build();
        }

        private static OrleansConfig GetOrleansConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("orleansconfig.json")
                .AddEnvironmentVariables("OR_");
            IConfigurationRoot root = builder.Build();
            OrleansConfig config = new OrleansConfig();
            root.Bind(config);
            return config;
        }

        private static PiraeusConfig GetPiraeusConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("piraeusconfig.json")
                .AddEnvironmentVariables("PI_");
            IConfigurationRoot root = builder.Build();
            PiraeusConfig config = new PiraeusConfig();
            root.Bind(config);
            return config;
        }


        static string GetLocalHostName()
        {
            return orleansConfig.Dockerized ? piraeusConfig.Channels.Tcp.Hostname : "localhost";
        }

        static IPAddress GetIPAddress(string hostname)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(hostname);
            for (int index = 0; index < hostInfo.AddressList.Length; index++)
            {
                if (hostInfo.AddressList[index].AddressFamily == AddressFamily.InterNetwork)
                {
                    return hostInfo.AddressList[index];
                }
            }

            return null;
        }
    }
}
