//using SkunkLab.VirtualRtu.ModBus;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Threading.Tasks;
//using VirtualRtu.Common.Configuration;

//namespace FieldGatewayMicroservice.Connections
//{
//    public class EdgeClient
//    {
//        public static event System.EventHandler<MqttErrorEventArgs> OnError;
//        public static event System.EventHandler<MessageEventArgs> OnMessage;
//        private static HttpClient httpClient;
//        private static int delay = 30000;

//        public static EdgeMqttClient Client { get; internal set; }
//        public static IssuedConfig Config { get; internal set; }
               
//        public static async Task Init(IssuedConfig config)
//        {    
//            if (config == null)
//            {
//                Console.WriteLine("Config is null.  Cannot start mqtt client.");
//                return;
//            }

//            Config = config;

//            if (Client != null)
//            {
//                try
//                {
//                    await Client.CloseAsync();
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine("Fault closing MQTT client '{0}'", ex.Message);
//                }
//            }

//            Client = null;
//            httpClient = null;
//            httpClient = new HttpClient();

//            try
//            {                
//                Console.WriteLine("Starting MQTT with the following parameters");
//                Console.WriteLine("Hostname = {0} Port = {1} PSK_Identity = {2}  PSK = {3}", config.Hostname, config.Port, config.PskIdentity, config.PSK);

//                Client = new EdgeMqttClient(config.Hostname, config.Port, config.PskIdentity, Convert.FromBase64String(config.PSK));
//                Client.OnError += Client_OnError;
//                Client.OnMessage += Client_OnMessage;
//                await Client.ConnectAsync(config.SecurityToken);
//                Console.WriteLine("MQTT client connected :-)");
//                await Client.Subscribe(config.Resources.RtuInputResource);
//                Console.WriteLine("Subscribed for input :-)");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Fault opening MQTT client '{0}'", ex.Message);
//            }
//        }

//        private static void Client_OnMessage(object sender, MessageEventArgs e)
//        {
//            Console.WriteLine("{0} - Received Piraeus message", DateTime.Now.ToString("hh:MM:ss.ffff"));
//            byte[] buffer = new byte[7];
//            Buffer.BlockCopy(e.Message, 0, buffer, 0, buffer.Length);
//            MbapHeader header = MbapHeader.Decode(buffer);
//            Console.WriteLine("{0} - Transaction ID {1}", DateTime.Now.ToString("hh:MM:ss.ffff"), header.TransactionId);
//            //forward to Modbus Protocol Adapter

//            try
//            {                
//                Console.WriteLine("{0} - Begin echo", DateTime.Now.ToString("hh:MM:ss.ffff"));
//                //IPHelper.Queue.Enqueue(e.Message);
//                HttpContent content = new System.Net.Http.ByteArrayContent(e.Message);
//                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
//                content.Headers.ContentLength = e.Message.Length;
//                string requestUrl = IPHelper.GetAddress();
//                Task<HttpResponseMessage> task = httpClient.PostAsync(requestUrl, content);
//                Task.WaitAll(task);
//                HttpResponseMessage response = task.Result;
//                Console.WriteLine("{0} - Sent echo message with status code '{1}'", DateTime.Now.ToString("hh:MM:ss.ffff"), response.StatusCode);
//                //Console.WriteLine("Transaction Id '{0}' sent with status code {1}", header.TransactionId, response.StatusCode);
                


//            }
//            catch (WebException we)
//            {
//                Console.WriteLine("Web exception - {0}", we.Message);
//                if (we.InnerException != null)
//                {
//                    Console.WriteLine("Web inner exception - {0}", we.InnerException.Message);

//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Post exception - {0}", ex.Message);
//                if (ex.InnerException != null)
//                {
//                    Console.WriteLine("Post inner exception - {0}", ex.InnerException.Message);

//                }
//            }

//            httpClient = null;
//        }

//        private static void Client_OnError(object sender, MqttErrorEventArgs e)
//        {
//            Console.WriteLine("MQTT client error '{0}'", e.Error.Message);
//            Console.WriteLine("MQTT cleint error stack trace '{0}'", e.Error.StackTrace);
//            try
//            {
//                Task closeTask = Client.CloseAsync();
//                Task.WaitAll(closeTask);
//                Console.WriteLine("MQTT client closed");
                
//            }
//            catch(Exception ex)
//            {
//                Console.WriteLine("FAULT closing MQTT client '{0}'", ex.Message);
//            }

            
//            Task delayTask = Task.Delay(delay);
//            Task.WaitAll(delayTask);

//            delay = delay * 2;
//            if(delay/1000 == 240)  //30s, 1min, 2min, 4min, then back to 30s
//            {
//                delay = 30000;
//            }

//            Init(Config).LogExceptions();
//        }


//        //private async Task RetryConnectionAsync()
//        //{
//        //    try
//        //    {
//        //        await Retry.ExecuteAsync(async () =>
//        //        {
//        //            await Init(Config);
//        //        }, TimeSpan.FromSeconds(60), 4, true);
//        //    }
//        //    catch (RetryException re)
//        //    {
//        //        Console.WriteLine("Failed to retry connect after 4 retries with 20 sec interval with exponential backoff...restarting retry.");
//        //        Console.WriteLine("RetryException '{0}'", re.Message);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Console.WriteLine("Exception during retry connect must restart. '{0}'", ex.Message);
//        //    }
//        //}
//    }
//}
