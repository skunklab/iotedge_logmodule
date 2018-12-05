using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Security.Tokens;
using SkunkLab.VirtualRtu.ModBus;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestPirClient2
{
    class Program
    {

        private static string hostname;
        private static int port;
        private static string securityToken;
        private static string nameClaimType;
        private static string roleClaimType;
        private static string pskIdentity;
        private static byte[] psk;
        private static string audience;
        private static string issuer;
        private static List<Claim> claims;
        private static string nameClaimValue;
        private static string roleClaimValue;
        private static string signingKey;
        private static PiraeusMqttClient client;
        private static IChannel channel;
        private static CancellationTokenSource src;
        private static string resourceInUriString;
        private static string resourceOutUriString;


        static void Main(string[] args)
        {
            Console.WriteLine("Piraeus MQTT TCP/PSK Client 2");
            Console.ReadKey();
            Configure();
            src = new CancellationTokenSource();
            channel = new TcpClientChannel2(hostname, port, null, pskIdentity, psk, 1024, 2048, src.Token);
            channel.OnStateChange += Channel_OnStateChange;
            channel.OnError += Channel_OnError;

            MqttConfig config = new MqttConfig(90);
            client = new PiraeusMqttClient(config, channel);
            ConnectAckCode code = client.ConnectAsync("mysessionId", "JWT", securityToken, 90).Result;
            Console.WriteLine(code);

            Task task = client.SubscribeAsync(resourceInUriString, QualityOfServiceLevelType.AtMostOnce, SubscriptionResult);
            Task.WhenAll(task);
            Console.WriteLine("Subscribed to {0}", resourceInUriString);
            Console.WriteLine("Waiting to receive and echo");
            

            Console.ReadKey();
        }

        private static void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Error.Message);
            Console.ResetColor();
        }

        private static void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            Console.WriteLine("Channel State {0}", e.State);
        }

        private static void SubscriptionResult(string resourceUriString, string contentType, byte[] message)
        {
            MbapHeader header = MbapHeader.Decode(message);
            Console.WriteLine("Client 2 message received with Unit ID = '{0}'", header.UnitId);

            try
            {               
                Task pubTask = client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, resourceOutUriString, "application/octet-stream", message);
                Task.WhenAll(pubTask);
                Console.WriteLine("Echo'd message to '{0}'.", resourceOutUriString);
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        private static void Configure()
        {
            resourceInUriString = "http://www.skunklab.io/vrtu/alberta/unitid1-in";
            resourceOutUriString = "http://www.skunklab.io/vtru/alberta/unitid1-out";
            audience = "http://www.skunklab.io/";
            issuer = audience;
            signingKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
            hostname = "piraeus.eastus2.cloudapp.azure.com";
            port = 8883;
            nameClaimType = "http://www.skunklab.io/name";
            roleClaimType = "http://www.skunklab.io/role";
            nameClaimValue = "fieldgateway1";
            roleClaimValue = "device";
            pskIdentity = "Key1";
            psk = Encoding.UTF8.GetBytes("The quick brown fox");
            claims = new List<Claim>();
            claims.Add(new Claim(nameClaimType, nameClaimValue));
            claims.Add(new Claim(roleClaimType, roleClaimValue));
            securityToken = new JsonWebToken(signingKey, claims, 90.0, issuer, audience).ToString();
           


        }



    }
}
