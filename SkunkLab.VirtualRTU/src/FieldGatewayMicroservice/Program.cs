using FieldGatewayMicroservice.Communications;
using FieldGatewayMicroservice.Connections;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Newtonsoft.Json;
using SkunkLab.Protocols.Mqtt;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VirtualRtu.Common.Configuration;

namespace FieldGatewayMicroservice
{
    public class Program
    {

        //private static ConfigService configService;
        private static IssuedConfig config;
        private static MqttClient client;
        private static HttpClient httpClient;
        private static bool enabled;
        
        public static void Main(string[] args)
        {
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("------------Starting Field Gateway ----------");
            Console.WriteLine("---------------------------------------------");


            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Task task = SetConfigAsync();
            Task.WaitAll(task);

            if(config == null)
            {
                Console.WriteLine("No configuration available to start MQTT client...terminating");               
                return;
            }
            else
            {
                httpClient = new HttpClient();
                while (!enabled)
                {
                    try
                    {
                        Task t = RunMqttAsync();
                        Task.WaitAll(t);
                    }
                    catch
                    {
                        Task delay = Task.Delay(20000);
                        Task.WaitAll();
                    }
                }
            }   

            CreateWebHostBuilder(args).Build().Run();
        }

        private static async Task SetConfigAsync()
        {
            ConfigFile fileOps = new ConfigFile();
            IotHubTwin twin = new IotHubTwin();
            config = await twin.GetModuleConfigAsync();
            if (config == null)
            {
                //check for file
                if (fileOps.HasDirectory && fileOps.HasFile)
                {
                    config = fileOps.ReadConfig();
                }
                else
                {
                    Console.WriteLine("Field Gateway cannot be configured.");
                }
            }
            else
            {
                fileOps.WriteConfig(config);
            }
        }

        private static async Task RunMqttAsync()
        {
            if(client != null)
            {
                await client.DisconnectAsync();
                await Task.Delay(10000); //delay 10 seconds because we are restarting the connection                
            }
            
            client = MqttClient.Create(config);
            client.OnError -= Client_OnError;
            client.OnMessage -= Client_OnMessage;
            client.OnError += Client_OnError;
            client.OnMessage += Client_OnMessage;

            ConnectAckCode code = await client.ConnectAsync();
            if (code == ConnectAckCode.ConnectionAccepted)
            {
                Console.WriteLine("*** MQTT connected ***");
                try
                {
                    await client.SubscribeAsync();
                    Console.WriteLine("*** MQTT client subscribed to '{0}' ***", config.Resources.RtuInputResource);
                    enabled = true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine("**** MQTT client FAILED to subscribe to '{0}' ****", config.Resources.RtuInputResource);
                    throw ex;
                }
            }
            else
            {
                Console.WriteLine("MQTT returned ACK CODE '{0}'", code);
            }
        }

        private static void Client_OnMessage(object sender, MessageEventArgs e)
        {
            //forward to MBPA
            try
            {
                Console.WriteLine("Received Piraeus msg");
                Task task = ForwardToProtocolAdapterAsync(e.Message);
                Task.WaitAll(task);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception forwarding message to protocol adapter - '{0}'", ex.Message);
            }

        }

        private static void Client_OnError(object sender, MqttErrorEventArgs e)
        {
            enabled = false;
            Console.WriteLine("MQTT client error - '{0}'", e.Error.Message);

            //restart the connection

            while (!enabled)
            {
                try
                {
                    Task runTask = RunMqttAsync();
                    Task.WaitAll(runTask);
                }
                catch
                {
                    Task delay = Task.Delay(20000);
                    Task.WaitAll();
                }
            }
        }

        private static async Task ForwardToProtocolAdapterAsync(byte[] message)
        {
            try
            {         
                HttpContent content = new System.Net.Http.ByteArrayContent(message);
                content.Headers.ContentType = new MediaTypeHeaderValue(Constants.CONTENT_TYPE);
                content.Headers.ContentLength = message.Length;
                string requestUrl = IPHelper.GetAddress();
                HttpResponseMessage response = await httpClient.PostAsync(requestUrl, content);
                Console.WriteLine("{0} - Forwarded msg to PA with status code '{1}'", DateTime.Now.ToString("hh:MM:ss.ffff"), response.StatusCode);
            }
            catch (WebException we)
            {
                Console.WriteLine("Web exception - {0}", we.Message);
                if (we.InnerException != null)
                {
                    Console.WriteLine("Web inner exception - {0}", we.InnerException.Message);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Post exception - {0}", ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Post inner exception - {0}", ex.InnerException.Message);
                }
            }
        }
        
        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            enabled = false;
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

            //Restart the MQTT Client

            while (!enabled)
            {
                try
                {
                    Task task = RunMqttAsync();
                    Task.WaitAll(task);
                }
                catch
                {
                    Task delay = Task.Delay(20000);
                    Task.WaitAll();
                }
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Limits.MaxConcurrentConnections = 100;
                    options.Limits.MaxConcurrentUpgradedConnections = 100;
                    options.Limits.MaxRequestBodySize = 10 * 1024;
                    options.Limits.MinRequestBodyDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    options.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    options.ListenAnyIP(8888);

                });

        

        //public static string GetAddress()
        //{
        //    string requestUrl = null;
        //    IPHostEntry entry = Dns.GetHostEntry("mbpa");
        //    string ipAddressString = null;

        //    foreach (var address in entry.AddressList)
        //    {
        //        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        //        {
        //            if (address.ToString().Contains("172"))
        //            {
        //                ipAddressString = address.ToString();
        //                break;
        //            }

        //        }
        //    }

        //    if (ipAddressString != null)
        //    {
        //        requestUrl = String.Format("http://{0}:8889/api/rtuinput", ipAddressString);
        //        Console.WriteLine("REQUEST URL = '{0}'", requestUrl);
        //    }
        //    else
        //    {
        //        Console.WriteLine("NO IP ADDRESS FOUND");
        //    }

        //    return requestUrl;
        //}

        //private static void ConfigService_OnModuleConfig(object sender, ModuleConfigurationEventArgs e)
        //{
        //    if (e.Config != null)
        //    {
        //        Console.WriteLine("Received config and will attempt to start mqtt client");
        //        config = e.Config;
        //    }
        //    else
        //    {
        //        try
        //        {
        //            Console.WriteLine("Attempting to read config file.");
        //            if (!File.Exists("./data/config.json"))
        //            {
        //                Console.WriteLine("WARNING: Config file does not exist.");
        //            }

        //            byte[] buffer = File.ReadAllBytes("./data/config.json");
        //            string jsonString = Encoding.UTF8.GetString(buffer);
        //            config = JsonConvert.DeserializeObject<IssuedConfig>(jsonString);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("WARNING: Config file not read.");
        //            Console.WriteLine("ERROR: Config file error {0}", ex.Message);
        //        }
        //    }

        //    EdgeClient.Init(config).LogExceptions();

        //}


    }
}
