using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.Protocols.Mqtt;


namespace MqttIotHubApp
{
    class Program
    {
        private static GenericMqttClient client;
        private static string clientId;
        private static string hostname;
        private static string username;
        private static string password;
        private static int port;
        private static string certPath;
        private static string certPassword;
        private static int blockSize;
        private static int maxBufferSize;
        private static byte[] payload;
        private static int maxMessages;
        private static int batchSize;
        private static int ackCount;
        private static Stopwatch ackStopwatch;
        private static Stopwatch sendStopwatch;
        private static Stopwatch sentStopwatch;
        private static bool complete;
        private static int sentCount = -1;
        private static string unobservedException;
        private static int unobservedExceptionCount;
        private static QualityOfServiceLevelType qosType;
        private static CancellationTokenSource src;
        private static string topic = "devices/device1/messages/events/";

        static void Main(string[] args)
        {           
            WriteHeader();
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Configure();
            X509Certificate2 cert = new X509Certificate2(certPath, certPassword);
            src = new CancellationTokenSource();

            //create the channel
            IChannel channel = new TcpClientChannel2(hostname, port, cert, blockSize, maxBufferSize, src.Token);
            channel.OnReceive += Channel_OnReceive; //used to count ACKs
            channel.OnSent += Channel_OnSent; //used to count when message is actually sent

            //use default mqtt config
            MqttConfig config = new MqttConfig();

            //create the client and wire up channel events.
            client = new GenericMqttClient(config, channel);
            client.OnChannelStateChange += Client_OnChannelStateChange;
            client.OnChannelError += Client_OnChannelError;
            

            //MQTT connect to IoT Hub
            Task task = client.ConnectAsync(clientId, username, password, 180, true);
            Task.WaitAll(task);

            //wait for the connection ACK
            while(!client.MqttConnectCode.HasValue)
            {
                Console.WriteLine("waiting for connect ACK...");
                Task t0 = Task.Delay(200);
                Task.WaitAll(t0);
            }

            Console.WriteLine("Connect ACK {0}", client.MqttConnectCode.Value);
            if(client.MqttConnectCode.Value != ConnectAckCode.ConnectionAccepted && args.Length == 0)
            {
                Console.WriteLine("Press any key to terminate...");
                Console.ReadKey();
            }
            else if(args.Length == 0)
            {
                Console.WriteLine("Prepare for publishing");
                SetMessageSize();
                SetNumberOfMessages();
                SetBatchSize();
                SetQosLevel();
            }
            else
            {
                SetPayloadType(args[0]);
                maxMessages = Convert.ToInt32(args[1]);
                batchSize = Convert.ToInt32(args[2]);
                qosType = (QualityOfServiceLevelType)Convert.ToInt32(args[3]);
            }

            if(args.Length == 0)
            {
                Console.WriteLine("Press any key to start publishing...");
                Console.ReadKey();
                
                Console.Write("sending...");
                StartTransmission(qosType);
                Console.ReadKey();                
            }
            else
            {
                StartTransmission(qosType);
                while(!complete)
                {
                    Task t = Task.Delay(10);
                    Task.WaitAll(t);                    
                }
            }
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            unobservedExceptionCount++;
            e.SetObserved();
        }

        private static void WriteHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("IoT Hub MQTT Client");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("configuring...");
            Console.ResetColor();
        }

        private static void Channel_OnSent(object sender, ChannelSentEventArgs args)
        {
            sentCount++;

            if(sentCount > 0 && sentCount == maxMessages  && qosType == QualityOfServiceLevelType.AtMostOnce)
            {
                sentStopwatch.Stop();
                GC.Collect();
                WriteOutput();
                complete = true;
            }
        }

        #region Configuration
        private static void Configure()
        {
            clientId = ConfigurationManager.AppSettings["deviceId"];
            hostname = ConfigurationManager.AppSettings["hostname"];
            username = ConfigurationManager.AppSettings["username"];
            password = Encoding.UTF8.GetString(Convert.FromBase64String(ConfigurationManager.AppSettings["base64Password"]));
            port = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);
            certPath = ConfigurationManager.AppSettings["certpath"];
            certPassword = ConfigurationManager.AppSettings["certpassword"];
            blockSize = Convert.ToInt32(ConfigurationManager.AppSettings["blockSize"]);
            maxBufferSize = Convert.ToInt32(ConfigurationManager.AppSettings["maxBufferSize"]);
        }

        #endregion

        private static void StartTransmission(QualityOfServiceLevelType qos)
        {
            int mod = maxMessages % batchSize;
            int segments = maxMessages / batchSize;
            maxMessages = maxMessages - mod;

            sendStopwatch = new Stopwatch();            
            sentStopwatch = new Stopwatch();

            sendStopwatch.Start();
            if (qosType == QualityOfServiceLevelType.AtLeastOnce)
            {
                ackStopwatch = new Stopwatch();
                ackStopwatch.Start();
            }
            sentStopwatch.Start();

            int index = 0;
            while(index < segments)
            {
                List<Task> taskList = new List<Task>();

                for(int i=0;i<batchSize;i++)
                {
                    taskList.Add(client.PublishAsync(topic, qos, false, false, payload));
                }

                Task.WhenAll(taskList);
                index++;
            }

            sendStopwatch.Stop();
            Console.WriteLine("Num messages not sent {0}", mod);

        }

        #region Interactive

        private static void SetQosLevel()
        {
            Console.Write("Set QoS Level (0,1) ? ");
            string val = Console.ReadLine();
            int num = 0;
            if (!Int32.TryParse(val, out num) || (num != 0 && num != 1))
            {
                Console.WriteLine("Try again...");
                SetQosLevel();
            }

            qosType = (QualityOfServiceLevelType)num;
        }
        private static void SetBatchSize()
        {
            Console.Write("Set batch size ? ");
            string val = Console.ReadLine();
            int num = 0;
            if (!Int32.TryParse(val, out num) || num < 0 || num > maxMessages)
            {
                Console.WriteLine("Try again...");
                SetBatchSize();
            }

            batchSize = num;            
        }

        private static void SetNumberOfMessages()
        {
            Console.Write("Number of messages to send ? ");
            string val = Console.ReadLine();
            int num = 0;
            if(!Int32.TryParse(val, out num) || num < 0)
            {
                Console.WriteLine("Try again...");
                SetNumberOfMessages();
            }

            maxMessages = num;
        }

        private static void SetMessageSize()
        {
            Console.Write("Message Size to send (S,M,L) ? ");
            string val = Console.ReadLine().ToLower();

            if(val != "s" && val != "m" && val != "l")
            {
                Console.WriteLine("Try again...");                
            }

            SetPayloadType(val);
        }

        private static void SetPayloadType(string sizeMoniker)
        {
            if (sizeMoniker == "s")
            {
                payload = File.ReadAllBytes("./files/small.json");
            }
            else if (sizeMoniker == "m")
            {
                payload = File.ReadAllBytes("./files/medium.json");
            }
            else if(sizeMoniker == "l")
            {
                payload = File.ReadAllBytes("./files/large.json");
            }
            else
            {
                throw new ArgumentOutOfRangeException("Invalid file size, must be s, m, or l.");
            }
        }

        #endregion

        #region client Events
        private static void Client_OnChannelError(object sender, ChannelErrorEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(args.Error.Message);
            Console.ResetColor();
        }

        private static void Client_OnChannelStateChange(object sender, ChannelStateEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Channel State {0}", args.State);
            Console.ResetColor();
        }

        #endregion
        
        #region Utility
        /// <summary>
        /// use as utility for converting the "password" used in app.config for MQTT in IoTHub
        /// </summary>
        /// <param name="sasForPassword"></param>
        /// <returns></returns>
        private static string ConvertSasToBase64(string sasForPassword)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sasForPassword));
        }

        #endregion

        #region Channel Events
        private static void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {
            //using to count ACKs for completeness
            MqttMessage msg = MqttMessage.DecodeMessage(args.Message);
            if(msg.MessageType == MqttMessageType.PUBACK)
            {
                ackCount++;
            }

            if(maxMessages > 0 && qosType == QualityOfServiceLevelType.AtLeastOnce && ackCount == maxMessages)
            {
                ackStopwatch.Stop();
                GC.Collect();
                WriteOutput();
                complete = true;
            }
        }

        #endregion

        #region File Output
        private static void WriteOutput()
        {
            string[] lines = new string[2];
            string filename = String.Format("{0}.txt", DateTime.Now.ToString("MM-dd-yyyyThh-mm-ss"));
            
            if (qosType == QualityOfServiceLevelType.AtLeastOnce)
            {
                lines[0] = String.Format("{0}{1}{2}{3}{4}{5}{6}{7}", "FileSize".PadRight(10), "Messages".PadRight(10), "BatchSize".PadRight(10), "SendTime".PadRight(10), "SentTime".PadRight(10), "AckTime".PadRight(10), "TaskErrorCount".PadRight(18), "UnobservedException".PadRight(50));
                lines[1] = String.Format("{0}{1}{2}{3}{4}{5}{6}{7}", payload.Length.ToString().PadRight(10), maxMessages.ToString().PadRight(10), batchSize.ToString().PadRight(10), sendStopwatch.ElapsedMilliseconds.ToString().PadRight(10), sentStopwatch.ElapsedMilliseconds.ToString().PadRight(10), ackStopwatch.ElapsedMilliseconds.ToString().PadRight(10), unobservedExceptionCount.ToString().PadRight(18), unobservedException);
            }
            else
            {
                lines[0] = String.Format("{0}{1}{2}{3}{4}{5}{6}", "FileSize".PadRight(10), "Messages".PadRight(10), "BatchSize".PadRight(10), "SendTime".PadRight(10), "SentTime".PadRight(10), "TaskErrorCount".PadRight(18), "UnobservedException".PadRight(50));
                lines[1] = String.Format("{0}{1}{2}{3}{4}{5}{6}", payload.Length.ToString().PadRight(10), maxMessages.ToString().PadRight(10), batchSize.ToString().PadRight(10), sendStopwatch.ElapsedMilliseconds.ToString().PadRight(10), sentStopwatch.ElapsedMilliseconds.ToString().PadRight(10), unobservedExceptionCount.ToString().PadRight(18), unobservedException);
            }


            string path = String.Format("{0}\\logs", Path.GetDirectoryName(Application.ExecutablePath));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllLines(String.Format("{0}\\{1}", path, filename), lines);

            Console.WriteLine("File output written");
            src.Cancel();
            Console.WriteLine("Finished. Press any key to terminate...");
            Console.ReadKey();
            

        }

        #endregion


    }
}
