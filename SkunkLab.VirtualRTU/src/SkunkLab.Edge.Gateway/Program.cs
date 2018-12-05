using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using SkunkLab.Edge.Gateway.Mqtt;
using SkunkLab.VirtualRtu.ModBus;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VirtualRtu.Common.Configuration;

namespace SkunkLab.Edge.Gateway
{
    class Program
    {
        //moduleClient = ModuleClient.CreateFromConnectionString("HostName=virtualrtu.azure-devices.net;DeviceId=edgedevice1;ModuleId=fieldgateway;SharedAccessKey=7xQe9eE6ep7TrHvDWiKWGTlEaGYVLBLCS9NxVVHTYt8=", TransportType.Mqtt);
        //private static string configpath = "./config/config.json";
        private static IssuedConfig config;
        //private static MqttEdgeClient mqttClient;
        private static CancellationTokenSource source;
        private static CancellationTokenSource localProcess;
        private static CancellationToken ctoken;
        private static EdgeMqttClient client;
        private static ManualResetEventSlim done;
        //private static EdgeModuleClient moduleClient;
        private static ModuleClient moduleClient;
        private static Twin twin;

        private static bool restarting;
       
        static void Main(string[] args)
        {
            
            //if(!Directory.Exists("./config"))
            //{
            //    Directory.CreateDirectory("./config");
            //}
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //localProcess = new CancellationTokenSource();
            //ctoken = localProcess.Token;
            //ctoken.Register(Restart);
            //Task task = StartAsync(ctoken);
            //Task.WhenAll(task);

            Console.WriteLine("------------------ Field Gateway Starting ------------------");

            Task t = RunAsync();
            Task.WhenAll(t);

            done = new ManualResetEventSlim(false);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("Field Gateway is blocking.");
            done.Wait();

            Console.WriteLine("------------------ Field Gateway Exiting ------------------");

        }

       
        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();

            if (!restarting)
            {
                restarting = true;
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
                Console.WriteLine("-----> Forcing Restart <-----");
                RestartAsync().ConfigureAwait(false);

            }
        }
        

        #region start

        private static async Task RestartAsync()
        {
            if(restarting)
            {
                return;
            }

            restarting = true;

            if(config == null)
            {
                Console.WriteLine("Trigger restart of container");
                done.Set();
            }
            else
            {
                Console.WriteLine("{0} Initializing module client", DateTime.Now.ToString("hh:MM:ss.ffff"));
                moduleClient = await ModuleClient.CreateFromEnvironmentAsync(TransportType.Mqtt);

                Console.WriteLine("{0} Module client created.", DateTime.Now.ToString("hh:MM:ss.ffff"));
                var context = moduleClient;

                Console.WriteLine("{0} Opening module client explcitly", DateTime.Now.ToString("hh:MM:ss.ffff"));
                await moduleClient.OpenAsync().ConfigureAwait(false);
                Console.WriteLine("{0} Module client explictly opened", DateTime.Now.ToString("hh:MM:ss.ffff"));

                Console.WriteLine("{0} Setting input message handler", DateTime.Now.ToString("hh:MM:ss.ffff"));
                await moduleClient.SetInputMessageHandlerAsync("fieldgatewayInput", InputAsync, context).ConfigureAwait(false);
                Console.WriteLine("{0} Input messsage handler set.", DateTime.Now.ToString("hh:MM:ss.ffff"));

                Console.WriteLine("{0} Restarting MQTT client", DateTime.Now.ToString("hh:MM:ss.ffff"));
                await StartMqttAsync();
            }

            restarting = false;

        }


        private static async Task RunAsync()
        {           
            Console.WriteLine("{0} Staring configuration", DateTime.Now.ToString("hh:MM:ss.ffff"));

            try
            {
                if(moduleClient != null)
                {
                    await moduleClient.CloseAsync().ConfigureAwait(false);
                    moduleClient = null;
                }

                Console.WriteLine("{0} Initializing module client", DateTime.Now.ToString("hh:MM:ss.ffff"));
                moduleClient = await ModuleClient.CreateFromEnvironmentAsync(TransportType.Mqtt);
                //moduleClient = ModuleClient.CreateFromConnectionString("HostName=virtualrtu.azure-devices.net;DeviceId=edgedevice1;ModuleId=fieldgateway;SharedAccessKey=7xQe9eE6ep7TrHvDWiKWGTlEaGYVLBLCS9NxVVHTYt8=", TransportType.Mqtt);
                
                Console.WriteLine("{0} Module client created.", DateTime.Now.ToString("hh:MM:ss.ffff"));
                var context = moduleClient;

                Console.WriteLine("{0} Opening module client explcitly", DateTime.Now.ToString("hh:MM:ss.ffff"));
                await moduleClient.OpenAsync().ConfigureAwait(false);
                
                Console.WriteLine("{0} Module client explictly opened", DateTime.Now.ToString("hh:MM:ss.ffff"));

                //Console.WriteLine("{0} Setting input message handler", DateTime.Now.ToString("hh:MM:ss.ffff"));
                //await moduleClient.SetInputMessageHandlerAsync("fieldgatewayInput", InputAsync, context).ConfigureAwait(false);
                Console.WriteLine("{0} Input messsage handler set (sort of)", DateTime.Now.ToString("hh:MM:ss.ffff"));

                Twin twin = null;

                if(config == null)
                {
                    Console.WriteLine("{0} Getting twin", DateTime.Now.ToString("hh:MM:ss.ffff"));
                    //get the twin and check for configuration
                    twin = await moduleClient.GetTwinAsync().ConfigureAwait(false);
                    
                    Console.WriteLine("{0} Twin open", DateTime.Now.ToString("hh:MM:ss.ffff"));
                    Console.WriteLine("{0} Getting twin collection", DateTime.Now.ToString("hh:MM:ss.ffff"));
                    TwinCollection collection = twin.Properties.Desired;
                    Console.WriteLine("{0} Reading luss and service url", DateTime.Now.ToString("hh:MM:ss.ffff"));
                    if (collection.Contains("luss") && collection.Contains("serviceUrl"))
                    {
                        string luss = collection["luss"];
                        string serviceUrl = collection["serviceUrl"];

                        Console.WriteLine("{0} Twin properites LUSS = {1}  Service URL = {2}", DateTime.Now.ToString("hh:MM:ss.ffff"), luss, serviceUrl);
                        if (!string.IsNullOrEmpty(luss) && !string.IsNullOrEmpty(serviceUrl))
                        {
                            Console.WriteLine("{0} Processing configuration", DateTime.Now.ToString("hh:MM:ss.ffff"));
                            await ProcessTwinConfig(luss, serviceUrl);
                            Console.WriteLine("{0} Configuration processing complete", DateTime.Now.ToString("hh:MM:ss.ffff"));
                        }
                    }
                }
                else
                {
                    Console.WriteLine("{0} Have config will try start mqtt", DateTime.Now.ToString("hh:MM:ss.ffff"));
                    await StartMqttAsync();
                    Console.WriteLine("{0} MQTT client started", DateTime.Now.ToString("hh:MM:ss.ffff"));
                }

                twin = null;

                //if (config != null)
                //{
                    
                //}


                

                //await moduleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);
            }
            catch(AggregateException ae)
            {
                Console.WriteLine("{0} Fault at start - AG {1}", DateTime.Now.ToString("hh:MM:ss.ffff"), ae.Flatten().InnerException.Message);
            }
            catch(Exception ex)
            {
                Console.WriteLine("{0} Fault at start - {1}", DateTime.Now.ToString("hh:MM:ss.ffff"), ex.Message);
            }


            restarting = false;

            
        }

        private static async Task<MessageResponse> InputAsync(Message message, object userContext)
        {
            //TaskCompletionSource<MessageResponse> tcs = new TaskCompletionSource<MessageResponse>();
            try
            {
                ModuleClient mclient = (ModuleClient)userContext;
                byte[] msg = message.GetBytes();
                byte[] headerBuffer = new byte[7];
                Buffer.BlockCopy(msg, 0, headerBuffer, 0, headerBuffer.Length);
                MbapHeader header = MbapHeader.Decode(headerBuffer);

                await mclient.CompleteAsync(message).ConfigureAwait(false);
                Console.WriteLine("Echo message received and completed");
                if(client != null)
                {
                    await client.PublishAsync(config.Resources.RtuOutputResource, msg);
                    Console.WriteLine("{0} Forwarding Transaction Id {1}", DateTime.Now.ToString("hh:MM:ss.ffff"), header.TransactionId);
                    //await mqttClient.SendAsync(msg);
                }

                //tcs.SetResult(MessageResponse.Completed);
            }
            catch (Exception ex)
            {
                //tcs.SetException(ex);
                Console.WriteLine("Fault on echo module reccieve with exception.");
            }

            //return tcs.Task;
            return MessageResponse.Completed;
        }

        private static async Task ProcessTwinConfig(string luss, string serviceUrl)
        {
            try
            {
                IssuedConfig issuedConfig = null;

                string requestUrl = String.Format("{0}&luss={1}", serviceUrl, luss);
                Console.WriteLine("Service URL = {0}", requestUrl);

                Console.WriteLine("{0} Making Azure function call", DateTime.Now.ToString("hh:MM:ss.ffff"));
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage message = await httpClient.GetAsync(requestUrl);
                if (message.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string jsonString = await message.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        issuedConfig = JsonConvert.DeserializeObject<IssuedConfig>(jsonString);
                        //Console.WriteLine("{0} Got issued config token", DateTime.Now.ToString("hh:MM:ss.ffff"));
                        //try
                        //{
                        //    File.WriteAllBytes("config/config.json", Encoding.UTF8.GetBytes(jsonString));
                        //}
                        //catch(Exception fileExc)
                        //{
                        //    Console.WriteLine("Fault writing file");
                        //    Console.WriteLine(fileExc.Message);
                        //}
                    }
                }
                else
                {
                    Console.WriteLine("{0} Request for configuration return HTTP status '{1}'", DateTime.Now.ToString("hh:MM:ss.ffff"), message.StatusCode);
                }

                if (issuedConfig != null)
                {
                    config = issuedConfig;
                    await StartMqttAsync();
                }
            }
            catch(AggregateException ae)
            {
                Console.WriteLine("Desired properties for module update error - AG.");
                Console.WriteLine(ae.Flatten().InnerException.Message);
                //Trace.TraceWarning("Desired properties for module update error - AG.");
                //Trace.TraceError(ae.Flatten().InnerException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Desired properties for module update error.");
                Console.WriteLine(ex.Message);
                //Trace.TraceWarning("Desired properties for module update error.");
                //Trace.TraceError(ex.Message);
            }
        }


        private static async Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            try
            {
                if(desiredProperties.Contains("luss") && desiredProperties.Contains("serviceUrl"))
                {
                    string luss = desiredProperties["luss"];
                    string url = desiredProperties["serviceUrl"];
                    if (!string.IsNullOrEmpty(luss) && !string.IsNullOrEmpty(url))
                    {
                        await ProcessTwinConfig(luss, url);
                    }
                    else
                    {
                        Console.WriteLine("no configuration received");
                    }
                }
                else
                {
                    Console.WriteLine("Desired properties luss = '{0}' ; serviceUrl = '{1}'", desiredProperties.Contains("luss"), desiredProperties.Contains("serviceUrl"));
                }
                
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Desired properties for module update error.");
                Trace.TraceError(ex.Message);
            }
        }


        //static async Task StartAsync(CancellationToken token)
        //{
        //    try
        //    {
        //        //if(File.Exists(configpath))
        //        //{
        //        //    byte[] jsonBytes = File.ReadAllBytes(configpath);
        //        //    config = JsonConvert.DeserializeObject<IssuedConfig>(Encoding.UTF8.GetString(jsonBytes));
        //        //}

        //        await StartEdgeModuleAsync();
        //    }
        //    catch (AggregateException ae)
        //    {
        //        Console.WriteLine("Failed to initialize Edge field gateway client.");
        //        Console.WriteLine(ae.Flatten().InnerException.Message);
        //        Console.WriteLine(ae.Flatten().StackTrace);
        //        await RestartAsync(ae.Flatten().InnerException);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Failed to initialize Edge field gateway client.");
        //        Console.WriteLine(ex.Message);
        //        Console.WriteLine(ex.InnerException.Message);
        //        Console.WriteLine(ex.StackTrace);
        //        await RestartAsync(ex);
        //    }
        //}
               
        
        //static async Task RestartAsync(Exception ex)
        //{
        //    if (ex != null)
        //    {
        //        Console.WriteLine("Restarting from error '{0}'", ex.Message);
        //        Trace.TraceWarning("Nonfatal exception starting field gateway must restart.");
        //        Trace.TraceError(ex.Message);
        //    }

        //    localProcess = new CancellationTokenSource();
        //    ctoken = localProcess.Token;
        //    ctoken.Register(Restart);

        //    await StartAsync(ctoken);

        //}

      
        private static async Task StopAsync()
        {
            moduleClient = null;
            client = null;
            //mqttClient = null;

            await Task.CompletedTask;
        }


#endregion

        #region Module Twin
        
        
        
        private static async Task StartMqttAsync()
        {
            client = null;

            try
            {
                client = new EdgeMqttClient(config.Hostname, config.Port, config.PskIdentity, Convert.FromBase64String(config.PSK));
                client.OnError += Client_OnError;
                client.OnMessage += Client_OnMessage;               
                await client.ConnectAsync(config.SecurityToken);
                Console.WriteLine("MQTT connected");
                await client.Subscribe(config.Resources.RtuInputResource);
                Console.WriteLine("MQTT client subscribed");
            }
            catch(CommunicationsException ce)
            {
                Console.WriteLine(ce.Message);

                if (ce.InnerException != null)
                {
                    Console.WriteLine(ce.InnerException.Message);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Start MQTT client fault.");
                Console.WriteLine(ex);
            }
                                 
            //try
            //{

            //    if (mqttClient != null)
            //    {
            //        Console.WriteLine("{0} MQTT client exists disconnecting", DateTime.Now.ToString("hh:MM:ss.ffff"));
            //        //try
            //        //{
            //        //    await mqttClient.DisconnectAsync();
            //        //}
            //        //catch
            //        //{ }

            //        mqttClient = null;
            //    }


            //    Console.WriteLine("{0} Create new mqtt client", DateTime.Now.ToString("hh:MM:ss.ffff"));
            //    mqttClient = new MqttEdgeClient(config);
            //    Console.WriteLine("{0} Create new mqtt client created", DateTime.Now.ToString("hh:MM:ss.ffff"));
            //    mqttClient.OnReceive += MqttClient_OnReceive;
            //    mqttClient.OnError += MqttClient_OnError;
            //    //Retry retry = new Retry();
            //    //retry.OnTimeout += MqttClient_OnTimeout;
            //    //Action action = async () => { await mqttClient.ConnectAsync(); };
            //    //await retry.ExecuteAsync(action, TimeSpan.FromSeconds(10), 4, SigmoidType.HalfSigmoid);
            //    Console.WriteLine("{0} Opening mqtt client", DateTime.Now.ToString("hh:MM:ss.ffff"));


            //    await mqttClient.ConnectAsync();
            //    Console.WriteLine("{0} MQTT client open", DateTime.Now.ToString("hh:MM:ss.ffff"));
            //    Console.WriteLine("Starting MQTT client...");
            //}
            //catch(AggregateException ae)
            //{
            //    Console.WriteLine("MQTT connect AG Exception");
            //    Console.WriteLine(ae.Flatten().InnerException.Message);
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine("MQTT connect Exception");
            //    Console.WriteLine(ex.Message);
            //}
        }

        private static void Client_OnMessage(object sender, MessageEventArgs e)
        {
            byte[] headerBuffer = new byte[7];
            Buffer.BlockCopy(e.Message, 0, headerBuffer, 0, headerBuffer.Length);
            MbapHeader header = MbapHeader.Decode(headerBuffer);
            Console.WriteLine("{0} Received Transaction ID {1} from Piraeus.", DateTime.Now.ToString("hh:MM:ss.ffff"), header.TransactionId);
            SendMessage(e.Message, header.TransactionId).ConfigureAwait(false);
            
        }

        private static async Task SendMessage(byte[] message, ushort txid)
        {
            if (moduleClient != null)
            {
                try
                {                    
                    await moduleClient.SendEventAsync("fieldgatewayOutput", new Message(message)).ConfigureAwait(false);
                    Console.WriteLine("{0} Sent Transaction Id {1} to IoT Hub", DateTime.Now.ToString("hh:MM:ss.ffff"), txid);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fault forwarding to module client '{0}'", ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Module client is null.  Cannot forward message");
            }
        }

        private static void Client_OnError(object sender, MqttErrorEventArgs e)
        {
            Console.WriteLine("Received error from MQTT client.");
            Console.WriteLine("MQTT Client Error '{0}'", e.Error.Message);
            Console.WriteLine("MQTT Client {0}", e.Error.StackTrace);

            try
            {
                Task task = client.CloseAsync();
                Task.WaitAll(task);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Fault closing MQTT client {0}", ex.Message);
            }

            Console.WriteLine("Waiting 30 sec to restart MQTT client from fault");
            Task d = Task.Delay(30000);
            Task.WaitAll(d);

            Task t = StartMqttAsync();
            Task.WhenAll(t);

        }

        //private static void MqttClient_OnTimeout(object sender, RetryEventArgs e)
        //{
        //    Console.WriteLine("MQTT client timeout");
        //    mqttClient = null;
        //    Task task = StartMqttAsync();
        //    Task.WhenAll(task);
        //}

        //private static void MqttClient_OnError(object sender, CommunicationsErrorEventArgs e)
        //{
        //    Console.WriteLine("MQTT error '{0}'", e.Error.Message);
        //    Console.WriteLine(e.Error.StackTrace);
        //    mqttClient = null;
        //    Task task = StartMqttAsync();
        //    Task.WhenAll(task);
        //    //Task task = StartMqttAsync();
        //    //Task.WhenAll(task);
        //}

        private static void MqttClient_OnReceive(object sender, MqttReceiveEventArgs e)
        {
            Console.WriteLine("{0} Received MQTT message", DateTime.Now.ToString("hh:MM:ss.ffff"));
            if (moduleClient != null)
            {
                try
                {
                    Console.WriteLine("{0} Sending mqtt message to IoT Hub", DateTime.Now.ToString("hh:MM:ss.ffff"));
                    Task task = moduleClient.SendEventAsync("fieldgatewayOutput", new Message(e.Message));                   
                    Task.WhenAll(task); 
                    Console.WriteLine("{0} Sent mqtt message to IoT Hub with WhenAll", DateTime.Now.ToString("hh:MM:ss.ffff"));                    
                }
                catch(AggregateException ae)
                {
                    Console.WriteLine("Fault forwarding to module client.");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Fault forwarding to module client.");
                }
            }
        }

        #endregion



        #region Edge Module

        //private static async Task StartEdgeModuleAsync()
        //{
        //    if (moduleClient == null)
        //    {
        //        moduleClient = new EdgeModuleClient("modbusOutput", "modbusInput", Microsoft.Azure.Devices.Client.TransportType.Mqtt);
        //        moduleClient.OnReceive += ModuleClient_OnReceive;
        //        moduleClient.OnTwinReceive += ModuleClient_OnTwinReceive;
        //        await moduleClient.ConnectAsync();

        //        Console.WriteLine("Module client connected to Edge Hub and Twin");

        //        if(config == null)
        //        {
        //            Console.WriteLine("No config...so starting twin to get it.");
        //            await moduleClient.StartTwinAsync();                    
        //        }
        //        else
        //        {
        //            Console.WriteLine("Config available...so starting MQTT");
        //            await StartMqttAsync();
        //        }
        //    }
        //    //Action action = (async () => { await moduleClient.ConnectAsync(); });
        //    //Retry retry = new Retry();
        //    //retry.OnTimeout += EdgeModule_OnTimeout;
        //    //await retry.ExecuteAsync(action, TimeSpan.FromSeconds(10), 4, SigmoidType.HalfSigmoid);
        //    //Console.WriteLine("IoT Hub module client starting");
        //}

        //private static void ModuleClient_OnTwinReceive(object sender, TwinMessageEventArgs e)
        //{
        //    Console.WriteLine("Received message from twin");
        //    IssuedConfig issuedConfig = null;

        //    if (!string.IsNullOrEmpty(e.Luss) && !string.IsNullOrEmpty(e.ServiceUrl))
        //    {
        //        ConfigurationModel model = new ConfigurationModel(e.Luss, e.ServiceUrl);
        //        Task<string> task = model.GetConfiguration();
        //        Task.WaitAll(task);
        //        string jsonString = task.Result;

        //        if (!string.IsNullOrEmpty(jsonString))
        //        {
        //            issuedConfig = JsonConvert.DeserializeObject<IssuedConfig>(jsonString);
        //            Console.WriteLine("Got issued config token");
        //        }
        //    }

        //    if (issuedConfig != null)
        //    {
        //        config = issuedConfig;
        //        string jstring = JsonConvert.SerializeObject(config);
        //        //File.WriteAllBytes(configpath, Encoding.UTF8.GetBytes(jstring));
        //    }

        //    if (config != null)
        //    {
        //        Task startMqttTask = StartMqttAsync();
        //        Task.WhenAll(startMqttTask);
        //    }

            
        //}

        //private static void ModuleClient_OnReceive(object sender, ModuleMessageEventArgs e)
        //{
        //    Console.WriteLine("IoT Hub module client received message.");
        //    if(mqttClient != null)
        //    {
        //        Task task = mqttClient.SendAsync(e.Message);
        //        Task.WhenAll(task);
        //        Console.WriteLine("Forwarding module client message to MQTT client");
        //    }
        //}

        //private static void EdgeModule_OnTimeout(object sender, RetryEventArgs e)
        //{
        //    Console.WriteLine("Restarting IoT Hub module client");
        //    Task task = StartEdgeModuleAsync();
        //    Task.WhenAll(task);
        //}

        #endregion
        

        

        
    }
}
