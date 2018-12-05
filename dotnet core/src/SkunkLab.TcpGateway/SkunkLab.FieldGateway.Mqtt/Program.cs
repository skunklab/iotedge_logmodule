using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.FieldGateway.Mqtt
{
    class Program
    {
        private static string audience = "http://www.skunklab.io/";
        private static string issuer = "http://www.skunklab.io/";
        private static string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        private static double lifetimeMinutes = 1000.0;
        private static string securityToken;
        private static string hostname = "piraeus.eastus2.cloudapp.azure.com";
        private static IChannel channel;
        private static string pskIdentity = "Key1";
        private static byte[] psk = Encoding.UTF8.GetBytes("The quick brown fox");
        private static PiraeusMqttClient client;

        static void Main(string[] args)
        {
            CancellationTokenSource src = new CancellationTokenSource();
            securityToken = GetSecurityToken();
            //securityToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vd3d3LnNrdW5rbGFiLmlvL25hbWUiOiJhc2QiLCJodHRwOi8vd3d3LnNrdW5rbGFiLmlvL3JvbGUiOiJBIiwibmJmIjoxNTMzNTkwOTQxLCJleHAiOjE1MzM1OTYzNDEsImlhdCI6MTUzMzU5MDk0MSwiaXNzIjoiaHR0cDovL3d3dy5za3Vua2xhYi5pby8iLCJhdWQiOiJodHRwOi8vd3d3LnNrdW5rbGFiLmlvLyJ9.1wN66qnTaufzngw9YlkCOsKk79cA-QnJ3soPiMH5cnY";
            //channel = new TcpClientChannel2(hostname, 8883, pskIdentity, psk, 2048, src.Token);
            channel = new TcpClientChannel2(hostname, 8883, pskIdentity, psk, 1024, 2048, src.Token);
            //channel = ChannelFactory.Create(false, hostname, 8883, null, pskIdentity, psk, 2048, 4096, src.Token);
            MqttConfig config = new MqttConfig(150, 2, 1.5, 4, 100);
            client = new PiraeusMqttClient(config, channel);
            Task<ConnectAckCode> task = client.ConnectAsync("sessionId", "JWT", securityToken, 90);
            Task.WaitAll(task);
            Console.WriteLine(task.Result);
            string topicUriString = "http://www.skunklab.io/vtru/alberta/unitid1-in";
            Task subTask = client.SubscribeAsync(topicUriString, QualityOfServiceLevelType.AtMostOnce, InputMessage);
            Task.WhenAll(subTask);

            Console.ReadKey();
        }

        private static void InputMessage(string resourceUriString, string contentType, byte[] message)
        {            
            string topicUriString = "http://www.skunklab.io/vtru/alberta/unitid1-out";
            Console.WriteLine("Received message");
            Task task = client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, topicUriString, contentType, message);
            Task.WhenAll(task);
            Console.WriteLine("Echo sent");

        }

        static string GetSecurityToken()
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("http://www.skunklab.io/name", "fieldgateway1"));

            SkunkLab.Security.Tokens.JsonWebToken token = new Security.Tokens.JsonWebToken(symmetricKey, null, lifetimeMinutes, issuer, audience);
            return token.ToString();
        }
    }
}
