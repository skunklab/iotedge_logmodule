using Newtonsoft.Json;
using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Sample.Mqtt.Clients
{
    class Program
    {
        static string jsonPayload;
        static int messageType;
        static int channelNo;
        static int index;
        static IChannel channel;
        static CancellationTokenSource source;
        static string publishResource;
        static string observeResource;
        static string role;
        static string clientName;
        static string contentType = "text/plain";
        static PiraeusMqttClient client;
        static long minTicks;
        static int counter;
        static string hostname;
        static int numMessages;
        static int delayms;
        static bool waitSend;
        static bool sender;
        static bool commandLine;
        static HashSet<string> hashSet = new HashSet<string>();
        static string pskKey;
        static string pskString;

        static void Main(string[] args)
        {
            source = new CancellationTokenSource();
            ParseArgs(args);

            if (hostname == null)
            {

                WriteHeader();  //descriptive header
                Console.Write("Select message type [1=Json Test, Enter=Default Test] ? ");
                messageType = Console.ReadLine() == "1" ? 1 : 0;
                if (messageType == 1)
                {
                    contentType = "application/json";
                    jsonPayload = RandomString(990);
                }


                SelectClientRole(); //select a role for the client

            }

            string securityToken = GetSecurityToken();  //get the security token with a unique name
            SetResources(); //setup the resources for pub and observe based on role.
            SelectPsk(); //optional PSK to use

            if (hostname == null)
            {
                //Note: Must start the Web gateway and/or TCP/UDP gateway to be able to communicate
                SelectChannel(); //pick a channel for communication 
            }

            channel = GetChannel(channelNo, securityToken);
            channel.OnStateChange += Channel_OnStateChange;
            channel.OnError += Channel_OnError;
            channel.OnClose += Channel_OnClose;


            MqttConfig config = new MqttConfig(150, 2, 1.5, 4, 100);
            client = new PiraeusMqttClient(config, channel);
            client.RegisterTopic(observeResource, ObserveResource);
            ConnectAckCode code = client.ConnectAsync("sessionId", "JWT", securityToken, 90).Result;
            Console.WriteLine("Connected with Ack Code {0}", code);

            try
            {
                Task subTask = client.SubscribeAsync(observeResource, QualityOfServiceLevelType.AtLeastOnce, ObserveResource).ContinueWith(SendMessages);
                Task.WaitAll(subTask);

                Console.WriteLine("Subscribed to {0}", observeResource);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Observe failed.");
                Console.WriteLine(ex.InnerException.Message);
                goto endsample;
            }

            //SendMessages();

            source.Cancel();

            endsample:
            Console.WriteLine("client is closed...");
            Console.ReadKey();
        }




        static void ParseArgs(string[] args)
        {
            if (args == null || (args.Length < 4 || args.Length > 7))
            {
                commandLine = false;
                waitSend = true;
                sender = true;
                return;
            }

            commandLine = true;
            messageType = 1;
            role = args[0].ToUpper();
            clientName = args[1];
            channelNo = Convert.ToInt32(args[2]);
            hostname = args[3];
            if (args.Length > 4)
            {
                sender = true;
                numMessages = Convert.ToInt32(args[4]);
                delayms = Convert.ToInt32(args[5]);
                waitSend = Convert.ToBoolean(args[6]);
            }
            contentType = "application/json";
            jsonPayload = RandomString(990);
        }



        static void SendMessages(Task task)
        {
            bool sending = true;

            if (waitSend && sender)
            {
                Console.WriteLine();
                Console.Write("Send messages (Y/N) ? ");
                sending = Console.ReadLine().ToLowerInvariant() == "y";
            }

            if (!waitSend && !sender)
            {
                Console.WriteLine("Waiting to receive...");
                Console.ReadLine();
            }


            if (sending)
            {
                if (!commandLine)
                {
                    Console.Write("Enter number of messages to send ? ");
                    numMessages = Int32.Parse(Console.ReadLine());
                }

                if (!commandLine)
                {
                    Console.Write("Enter delay between messages in milliseconds ? ");
                    delayms = Int32.Parse(Console.ReadLine());
                }

                //Thread.Sleep(2000);

                for (int i = 0; i < numMessages; i++)
                {
                    index++;
                    //send a message to a resource
                    string payloadString = null;
                    if (messageType == 1)
                    {
                        
                        MyMessage mmsg = new MyMessage() { Ticks = DateTime.UtcNow.Ticks, Payload = jsonPayload };
                        payloadString = JsonConvert.SerializeObject(mmsg);

                    }
                    else
                    {
                        payloadString = String.Format("{0} sent message {1}", clientName, index);
                    }



                    byte[] payload = Encoding.UTF8.GetBytes(payloadString);
                    //string cacheKey = String.Format("{0}{1}", "key", i);
                    //Task pubTask = client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, publishResource, contentType, payload, cacheKey);        
                    Task pubTask = client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, publishResource, contentType, payload);
                    Task.WhenAll(pubTask);

                    if (delayms > 0)
                    {
                        Thread.Sleep(delayms);
                    }
                }

                if (waitSend)
                {
                    SendMessages(task);
                }

                Console.WriteLine("Indexer - {0}", index);
            }
        }

        static void WriteHeader()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--- MQTT Client ---");
            Console.WriteLine("press any key to continue...");
            Console.WriteLine();
            Console.ResetColor();
            Console.ReadKey();
        }

        static void SelectClientRole()
        {
            Console.WriteLine();
            Console.Write("Enter Role for this client (A/B) ? ");
            role = Console.ReadLine().ToUpperInvariant();
            if (role != "A" && role != "B")
                SelectClientRole();
        }

        static string GetSecurityToken()
        {
            string nameClaimType = "http://www.skunklab.io/name";
            string roleClaimType = "http://www.skunklab.io/role";

            if (hostname == null)
            {
                Console.Write("Enter unique client name ? ");
                clientName = Console.ReadLine();
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim(nameClaimType, clientName),
                new Claim(roleClaimType, role)
            };

            string audience = "http://www.skunklab.io/";
            string issuer = "http://www.skunklab.io/";
            string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
            return CreateJwt(audience, issuer, claims, symmetricKey, 60.0);
        }

        static void SetResources()
        {
            string resource1 = "http://www.skunklab.io/resource-a";
            string resource2 = "http://www.skunklab.io/resource-b";
            publishResource = role == "A" ? resource1 : resource2;
            observeResource = role == "A" ? resource2 : resource1;
        }

        static void SelectChannel()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--- Select Channel ---");
            Console.WriteLine("(1) Web Socket");
            Console.WriteLine("(2) TCP");
            Console.WriteLine("(3) UDP");
            Console.Write("Enter selection # ? ");

            int num = 0;
            if (Int32.TryParse(Console.ReadLine(), out num) && num > 0 && num < 4)
            {
                channelNo = num;
            }
            else
            {
                Console.WriteLine("Try again...");
                SelectChannel();
            }

        }

        private static IChannel GetChannel(int num, string securityToken)
        {
            if (hostname == null)
            {
                Console.Write("Enter hostname, IP, or Enter for localhost ? ");
                hostname = Console.ReadLine();
            }
            IPAddress address = null;
            bool isIP = IPAddress.TryParse(hostname, out address);
            string authority = isIP ? address.ToString() : String.IsNullOrEmpty(hostname) ? "localhost" : hostname;


            if (num == 1)
            {
                int port = 8899;
                string uriString = authority.Contains("localhost") ?
                                    String.Format("ws://{0}:{1}/api/connect", authority, port) :
                                    String.Format("ws://{0}/api/connect", authority);

                return ChannelFactory.Create(new Uri(uriString), securityToken, "mqtt", new WebSocketConfig(), source.Token);

            }
            else if (num == 2)
            {
                if (string.IsNullOrEmpty(pskKey))
                {
                    return address == null ? ChannelFactory.Create(true, authority, 8883, 2024, 4096, source.Token) : ChannelFactory.Create(true, address, 8883, 1024, 2048, source.Token);
                }
                else
                {
                    address = GetAddress(authority);
                    return ChannelFactory.Create(false, address, 8883, null, pskKey, Encoding.UTF8.GetBytes(pskString), 2048, 4096, source.Token);

                }
                //IChannel channel = ChannelFactory.Create(false, address, 8883, null, role == "A" ? "Key1" : "Key2", role == "A" ? b1 : b2, 1024, 2048, source.Token);
                //IChannel channel = address == null ?
                //                     ChannelFactory.Create(true, authority, 8883, 1024, 2048, source.Token) :
                //                     ChannelFactory.Create(true, address, 8883, 1024, 2048, source.Token);

                //return channel;
            }
            else if (num == 3)
            {
                Console.Write("Enter UDP local port for this client ? ");
                int port = Int32.Parse(Console.ReadLine());

                if (address != null)
                {
                    IPEndPoint endpoint = new IPEndPoint(address, 5883);
                    return ChannelFactory.Create(port, endpoint, source.Token);
                }
                else
                {
                    return ChannelFactory.Create(port, hostname, 5883, source.Token);
                }
            }

            return null;
        }


        private static IPAddress GetAddress(string authority)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(authority);
            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return address;
                }
            }

            return null;

        }


        public static string CreateJwt(string audience, string issuer, List<Claim> claims, string symmetricKey, double lifetimeMinutes)
        {
            JsonWebToken jwt = new JsonWebToken(symmetricKey, claims, lifetimeMinutes, issuer, audience);
            return jwt.ToString();

            //JsonWebToken jwt = new JsonWebToken(new Uri(audience), symmetricKey, issuer, claims, lifetimeMinutes);
            //return jwt.ToString();
        }

        static void ObserveResource(string resourceUriString, string contentType, byte[] payload)
        {
            if (payload != null)
            {
                if (messageType != 1)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(Encoding.UTF8.GetString(payload));
                    Console.ResetColor();
                }
                else
                {

                    MyMessage mmsg = JsonConvert.DeserializeObject<MyMessage>(Encoding.UTF8.GetString(payload));
                    string hash = mmsg.GetHash();
                    //if(hashSet.Contains(hash))
                    //{
                    //    Console.ForegroundColor = ConsoleColor.Cyan;
                    //    Console.WriteLine("Duplicate--------------------------------------------------------------------------------------------------");
                    //}
                    //else
                    //{
                    //    hashSet.Add(hash);
                    //}

                    long ticks = DateTime.UtcNow.Ticks;

                    //long diff = ticks - mmsg.Ticks;
                    minTicks = minTicks == 0 ? mmsg.Ticks : minTicks > mmsg.Ticks ? mmsg.Ticks : minTicks;
                    counter++;
                    Console.WriteLine("{0} - {1} - {2}", counter, TimeSpan.FromTicks(ticks - minTicks).TotalMilliseconds, TimeSpan.FromTicks(ticks - mmsg.Ticks).TotalMilliseconds);

                    //Console.WriteLine("{0} - {1}", Math.Round(TimeSpan.FromTicks(DateTime.UtcNow.Ticks - minTicks).TotalMilliseconds,0), Math.Round(TimeSpan.FromTicks(diff).TotalMilliseconds, 0));
                }
            }
        }

        private static void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Channel State {0}", e.State);
            Console.ResetColor();
        }

        private static void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Error.Message);
            Console.WriteLine(e.Error.StackTrace);
            Console.ResetColor();
        }

        private static void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Channel closed");
            Console.ResetColor();
        }

        private static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        private static void SelectPsk()
        {
            Console.Write("Use PSK (Y/N) ?  NOTE: Piraeus must be configured for PSK to use. ");
            if (Console.ReadLine().ToLowerInvariant() == "y")
            {
                Console.Write("Enter PSK Key Name ? ");
                pskKey = Console.ReadLine();
                Console.Write("Enter PSK Key string ? ");
                pskString = Console.ReadLine();
            }
        }


    }
}
