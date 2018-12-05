using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Piraeus.Configuration.Core;
using Piraeus.Configuration.Settings;
using Piraeus.Core;
using Piraeus.GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.TcpGateway
{
    class Program
    {
        private static Dictionary<int, TcpServerListener> listeners;
        private static Dictionary<int, CancellationTokenSource> sources;
        private static PiraeusConfig config;
        private static ManualResetEventSlim done;
        private static int attempt;
        private static readonly int initializeAttemptsBeforeFailing = 8;
        private static IClusterClient clusterClient;


        static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            listeners = new Dictionary<int, TcpServerListener>();
            sources = new Dictionary<int, CancellationTokenSource>();

            OrleansConfig orleansConfig = GetOrleansConfig();
            config = GetPiraeusConfig();

            if (!orleansConfig.Dockerized)
            {
                var client = new ClientBuilder();
                client.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly));
                client.UseLocalhostClustering();
                clusterClient = client.Build();
            }
            else
            {
                clusterClient = GetClient(orleansConfig);
            }

            Task connectTask = ConnectAsync();
            Task.WaitAll(connectTask);

            if(!clusterClient.IsInitialized)
            {
                return;
            }

            foreach (var port in config.Channels.Tcp.Ports)
            {
                sources.Add(port, new CancellationTokenSource());
            }

            string hostname = config.Channels.Tcp.Hostname == null ? "localhost" : config.Channels.Tcp.Hostname;

            int index = 0;
            while(index < config.Channels.Tcp.Ports.Length)
            {
                listeners.Add(config.Channels.Tcp.Ports[index], new TcpServerListener(new IPEndPoint(GetIPAddress(hostname), config.Channels.Tcp.Ports[index]), config, sources[config.Channels.Tcp.Ports[index]].Token));
                index++;
            }

            KeyValuePair<int, TcpServerListener>[] tcpKvps = listeners.ToArray();

            foreach (var item in tcpKvps)
            {
                item.Value.OnError += Listener_OnError;
                item.Value.StartAsync().LogExceptions();
            }

            done = new ManualResetEventSlim(false);


            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("TCP Gateway is ready...");
            done.Wait();

        }

        private static void Listener_OnError(object sender, ServerFailedEventArgs e)
        {
            try
            {
                Console.WriteLine("Receiving error from Listener - attempting to shutdown the connection");
                if (sources.ContainsKey(e.Port))
                {
                    //cancel the server
                    sources[e.Port].Cancel();
                }
                else
                {
                    return;
                }

                if (listeners.ContainsKey(e.Port))
                {
                    listeners[e.Port].StopAsync().Ignore();
                    listeners.Remove(e.Port);
                    sources.Remove(e.Port);

                    //restart the server
                    sources.Add(e.Port, new CancellationTokenSource());

                    string hostname = config.Channels.Tcp.Hostname == null ? "localhost" : config.Channels.Tcp.Hostname;
                    listeners.Add(e.Port, new TcpServerListener(new IPEndPoint(GetIPAddress(hostname), e.Port), config, sources[e.Port].Token));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fault executing Listener_OnError - '{0}'", ex.Message);
                Console.WriteLine("-----> Forcing Restart <-----");
                done.Set();
            }
        }

        private static OrleansConfig GetOrleansConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(Environment.CurrentDirectory + "\\orleans.json")
                .AddEnvironmentVariables("OR_");

            IConfigurationRoot root = builder.Build();
            OrleansConfig config = new OrleansConfig();
            ConfigurationBinder.Bind(root, config);

            return config;
        }

        private static PiraeusConfig GetPiraeusConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(Environment.CurrentDirectory + "\\config.json")
                .AddEnvironmentVariables("PI_");

            IConfigurationRoot root = builder.Build();
            PiraeusConfig config = new PiraeusConfig();
            ConfigurationBinder.Bind(root, config);

            return config;
        }

        private static async Task ConnectAsync()
        {
            await clusterClient.Connect(RetryFilter);





        }

        private static IClusterClient GetClient(OrleansConfig config)
        {
            if (!config.Dockerized)
            {
                var client = new ClientBuilder()
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly))
                    .UseLocalhostClustering()
                    .Build();

                return client;
            }
            else if(config.OrleansClusterId != null && config.OrleansDataProviderType == "AzureStore")
            {
                var client = new ClientBuilder()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = config.OrleansClusterId;
                        options.ServiceId = config.OrleansServiceId;
                    })
                    .UseAzureStorageClustering(options => options.ConnectionString = config.OrleansDataConnectionString)
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly))
                    .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning))
                    .Build();

                return client;
            }
            else
            {
                throw new InvalidOperationException("Invalid configuration for Orleans client");                   
            }

        }

        private static IPAddress GetIPAddress(string hostname)
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


        private static async Task<bool> RetryFilter(Exception exception)
        {
            if (exception.GetType() != typeof(SiloUnavailableException))
            {
                Console.WriteLine($"Cluster client failed to connect to cluster with unexpected error.  Exception: {exception}");
                return false;
            }
            attempt++;
            Console.WriteLine($"Cluster client attempt {attempt} of {initializeAttemptsBeforeFailing} failed to connect to cluster.  Exception: {exception}");
            if (attempt > initializeAttemptsBeforeFailing)
            {
                return false;
            }
            await Task.Delay(TimeSpan.FromSeconds(10));
            return true;
        }

       

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Trace.TraceWarning("{0} - Unobserved TCP Gateway Exception", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
            Trace.TraceError("{0} - Unobserved  TCP Gateway Exception '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), e.Exception.Flatten().InnerException.Message);
            Trace.TraceError("{0} - Unobserved  TCP Gateway Exception Stack Trace '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), e.Exception.Flatten().StackTrace);
            Console.WriteLine("***** Unobserved Exception ******");
            Console.WriteLine("----- Forcing Restart of TCP Gateway -----");

        }
    }
}
