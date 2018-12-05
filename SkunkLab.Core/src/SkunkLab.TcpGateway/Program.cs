using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Piraeus.Configuration.Core;
using Piraeus.Configuration.Settings;
using Piraeus.GrainInterfaces;
using SkunkLab.TcpGateway.Listeners;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.TcpGateway
{
    class Program
    {
        const int initializeAttemptsBeforeFailing = 8;
        private static int attempt = 0;
        private static OrleansConfig orleansConfig;
        private static PiraeusConfig piraeusConfig;
        private static IClusterClient client;
        private static Dictionary<int, Tuple<TcpServerListener, CancellationTokenSource>> listeners;
        private static string hostname;
        private static IPAddress address;
        private static ManualResetEventSlim done;

        static void Main(string[] args)
        {
            //Thread.Sleep(5000);
            orleansConfig = GetOrleansConfiguration();
            piraeusConfig = GetPiraeusConfiguration();
            hostname = GetLocalHostName();
            address = GetIPAddress(hostname);


            CreateOrleansClient();

            Task t = client.Connect(RetryFilter);
            Task.WaitAll(t);
            

            if(!client.IsInitialized)
            {
                Console.WriteLine("Orleans client failed to intialize.");
                return;
            }

            Console.WriteLine("Orleans client initialized.");

            //start the TCP server
            listeners = new Dictionary<int, Tuple<TcpServerListener, CancellationTokenSource>>();

            foreach(int port in piraeusConfig.Channels.Tcp.Ports)
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                TcpServerListener listener = new TcpServerListener(address, port, piraeusConfig, cts.Token);
                listener.OnError += Listener_OnError;
                Task task = listener.StartAsync();
                Task.WhenAll(task);
                Tuple<TcpServerListener, CancellationTokenSource> tuple = new Tuple<TcpServerListener, CancellationTokenSource>(listener, cts);
                listeners.Add(port, tuple);
                Console.WriteLine("TCP listener on port {0} initialized", port);
            }

            done = new ManualResetEventSlim(false);


            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("TCP Gateway is running...");
            done.Wait();
        }

        private static void CreateOrleansClient()
        {
            if(orleansConfig.Dockerized)
            {
                CreateProductionClient();
            }
            else
            {
                CreateLocalClient();
            }
        }

        private static void CreateProductionClient()
        {
            var clientBuilder = new ClientBuilder()
            // Clustering information
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = orleansConfig.OrleansClusterId;
                options.ServiceId = orleansConfig.OrleansServiceId;
            })
            .Configure<NetworkingOptions>(options =>
            {
                options.MaxSockets = 1000;
            })
            // Clustering provider
            .UseAzureStorageClustering(options => options.ConnectionString = orleansConfig.OrleansDataConnectionString)
            // Application parts: just reference one of the grain interfaces that we use
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly))
            .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning).AddConsole());

            client = clientBuilder.Build();
        }

        private static void CreateLocalClient()
        {
            var clientBuilder = new ClientBuilder()
            // Clustering information
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = orleansConfig.OrleansClusterId;
                options.ServiceId = orleansConfig.OrleansServiceId;
            })
            .Configure<NetworkingOptions>(options =>
            {
                options.MaxSockets = 1000;
            })
            .UseLocalhostClustering()
            // Clustering provider
            //.UseAzureStorageClustering(options => options.ConnectionString = orleansConfig.OrleansDataConnectionString)
            // Application parts: just reference one of the grain interfaces that we use
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly))
            .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning).AddConsole());

            client = clientBuilder.Build();
        }

        private static void Listener_OnError(object sender, TcpServerErrorEventArgs e)
        {
            if(listeners.ContainsKey(e.Port))
            {
                Tuple<TcpServerListener, CancellationTokenSource> tcpTuple = listeners[e.Port];
                CancellationTokenSource source = tcpTuple.Item2;
                if(!source.IsCancellationRequested)
                {
                    source.Cancel();
                }

                listeners.Remove(e.Port);

                try
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    TcpServerListener listener = new TcpServerListener(address, e.Port, piraeusConfig, cts.Token);
                    listener.OnError += Listener_OnError;
                    Task task = listener.StartAsync();
                    Task.WhenAll(task);
                    Tuple<TcpServerListener, CancellationTokenSource> tuple = new Tuple<TcpServerListener, CancellationTokenSource>(listener, cts);
                    listeners.Add(e.Port, tuple);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Failed to restart TCP listener on port '{0}'", e.Port);
                    Console.WriteLine(ex.Message);
                }


            }
        }

        private static async Task<bool> RetryFilter(Exception exception)
        {
            if (exception.GetType() != typeof(SiloUnavailableException) && initializeAttemptsBeforeFailing == attempt)
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
            await Task.Delay(TimeSpan.FromSeconds(5));
            return true;
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
