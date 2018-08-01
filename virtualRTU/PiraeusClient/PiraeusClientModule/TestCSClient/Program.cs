using Channels;
using Piraeus.Clients.Mqtt;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VirtualRTU.ModBus;

namespace TestCSClient
{
    class Program
    {
        private static IChannel rtuChannel;
        private static IChannel clientChannel;
        private static CancellationTokenSource rtuSource;
        private static PiraeusMqttClient rtuClient;
        private static string pskIndentity = "MyPresharedKey";
        private static string base64Psk = "9/NkHW52KwlRCzeJ6sziOntgGHsDnJMX6x9i6gdaS+E=";
        //private static string base64Psk = "HyaNIRAVFhQ5zawjdXV/+Cun0R39BpOFyYzxDOq5Fj2b0wHACzPBL+CpmG0QA0WFQsC0qJPr8qraKx/VNwEEvbs47eNy0owBjWkEGRtwh9dywPxnlvMwuzIJJX/bWuJUN1zjfEjjtVID9jWd2xhl7G0SQUmmrEThz36yAttRYF8=";
        //private static string base64Psk = "4z9G+cYjgxctBaUXkZtjqnBRC2PbiURRDH2C4/UdcOESWci8YTtrc2DFKsXPDMfE4kaQgKLPjeDSk3OWfqEwT45wKBivy2dIRhtX7MBhKRmAJpwoO1mhv8i51Jpz8u1fz6XPSzgKSpZiOx3+sMUhtPD/zXe6eaIPtU8H2sFyuzCZy2mAIurOdbeeL9y5E4DrOukBUjRSoZrEJzI3hinrGXK8jzazkns9CH3r7PemfmVTBlL31ujY+vWe052uOuA61TizpfuMtzVwP8hu3CLUOAukuIfhE1HfUE1H0EdH37OniaQwEkD5e1+uiuhPG+rFW+8ZxJX5UnDDlEEJItUPOA==";
        //private static string hostname = "piraeus.eastus2.cloudapp.azure.com";
        private static string hostname = "localhost";
        private static CancellationTokenSource cts;

        static void Main(string[] args)
        {

            byte[] buf = new byte[256];
            System.Security.Cryptography.RNGCryptoServiceProvider.Create().GetNonZeroBytes(buf);
            string base64 = Convert.ToBase64String(buf);


            Console.WriteLine("Test Client");
            Console.ReadKey();

            Task rtuTask = StartRtuChannel(0);
            Task.WhenAll(rtuTask);

            Console.WriteLine("started rtu client");
            Console.ReadLine();

            Task oTask = StartClientChannel();
            Task.WhenAll(oTask);

            Console.ReadKey();

        }

        private static async Task StartRtuChannel(int index)
        {

            string token = GetRtuToken(index);
            IPAddress address = GetIPAddress(hostname);
            rtuSource = new CancellationTokenSource();
            rtuChannel = ChannelFactory.Create(false, address, 8883, null, pskIndentity, Convert.FromBase64String(base64Psk), 1024, 2048, rtuSource.Token);
            rtuClient = new PiraeusMqttClient(new SkunkLab.Protocols.Mqtt.MqttConfig(), rtuChannel);
            string topic = String.Format("http://www.skunklab/rtu-{0}", index);
            rtuClient.RegisterTopic(topic, RtuObserve);
            ConnectAckCode code = await rtuClient.ConnectAsync("foo", "JWT", token, 180);
            await rtuClient.SubscribeAsync(topic, SkunkLab.Protocols.Mqtt.QualityOfServiceLevelType.AtLeastOnce, RtuObserve);            
        }

        private static async Task StartClientChannel()
        {
            cts = new CancellationTokenSource();

            IChannel channel = ChannelFactory.Create(false, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 502), 1024, 2048, cts.Token);
            channel.OnReceive += Channel_OnReceive;
            await channel.OpenAsync();

            MbapHeader header = new MbapHeader()
            {
                UnitId = 0,
                ProtocolId = 0,
                TransactionId = 1,
                Length = 106
            };

            Random ran = new Random();
            byte[] buffer = new byte[100];
            ran.NextBytes(buffer);

            byte[] msg = new byte[107];
            Buffer.BlockCopy(header.Encode(), 0, msg, 0, 7);
            Buffer.BlockCopy(buffer, 0, msg, 7, buffer.Length);

            await channel.SendAsync(msg);
        }

        private static void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            Console.WriteLine("Received");
        }

        private static void RtuObserve(string resourceUriString, string contentType, byte[] payload)
        {
            string topic = String.Format("http://www.skunklab/rtu-response");
            Task task = rtuClient.PublishAsync(SkunkLab.Protocols.Mqtt.QualityOfServiceLevelType.AtLeastOnce, topic, "application/octet-stream", payload);
            Task.WhenAll(task);
        }

        private static string GetRtuToken(int index)
        {
            List<Claim> claimset = new List<Claim>();
            claimset.Add(new Claim("http://www.skunklab.io/piraeus/name", String.Format("rtu-{0}", index)));
            claimset.Add(new Claim("http://www.skunklab.io/role", "unit"));
            claimset.Add(new Claim("http://www.skunklab.io/role", String.Format("rtu-{0}", index)));

            string audience = "http://www.skunklab.io/";
            string issuer = "http://www.skunklab.io/";
            string key = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";

            JsonWebToken token = new JsonWebToken(new Uri(audience), key, issuer, claimset, TimeSpan.FromMinutes(30).TotalMinutes);
            return token.ToString();
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
    }
}
