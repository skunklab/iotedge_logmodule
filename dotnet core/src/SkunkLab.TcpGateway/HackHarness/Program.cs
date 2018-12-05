using Newtonsoft.Json;
using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.Edge.Gateway;
using SkunkLab.Edge.Gateway.Mqtt;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Security.Tokens;
using SkunkLab.VirtualRtu.Adapters;
using SkunkLab.VirtualRtu.ModBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HackHarness
{
    class Program
    {

        private static TestClient scadaClient;
        private static PiraeusMqttClient mqttClient;
        private static IChannel mqttChannel;
        private static CancellationTokenSource mqttSource;
        
        static void Main(string[] args)
        {
            Thread.Sleep(10000);
            CreateMqttClient();
            scadaClient = new TestClient(502);

            byte[] payload = Encoding.UTF8.GetBytes("My payload");
            MbapHeader header = new MbapHeader() { UnitId = 1, ProtocolId = 0, TransactionId = 111, Length = (ushort)payload.Length };
            scadaClient.Send(header, payload);



            Console.ReadKey();




        }

        private static void CreateMqttClient()
        {
            string token = GetSecurityToken();
            //piraeus.eastus2.cloudapp.azure.com
            string hostname = "piraeus.eastus2.cloudapp.azure.com";
            mqttSource = new CancellationTokenSource();
            string pskString = "The quick brown fox";
            byte[] psk = Encoding.UTF8.GetBytes(pskString);
            mqttChannel = new TcpClientChannel(hostname, 8883, null, "Key1", psk, 102400, mqttSource.Token);
            mqttClient = new PiraeusMqttClient(new SkunkLab.Protocols.Mqtt.MqttConfig(90.0), mqttChannel);
            Task<ConnectAckCode> t = mqttClient.ConnectAsync("c1", "JWT", token, 90);
            Task.WaitAll(t);
            ConnectAckCode code = t.Result;



            string topicUriString = "http://wwww.skunklab.io/vrtu/alberta/unitid1-in";
            mqttClient.SubscribeAsync(topicUriString, SkunkLab.Protocols.Mqtt.QualityOfServiceLevelType.AtMostOnce, RtuIn);

        }


        private static string GetSecurityToken()
        {
            string issuer = "http://www.skunklab.io/";
            string audience = "http://www.skunklab.io/";
            string key = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
            int min = 1000;
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("http://www.skunklab.io/name", "fieldgateway1"));
            claims.Add(new Claim("http://www.skunklab.io/role", "device"));

            JsonWebToken jwt = new JsonWebToken(key, claims, min, issuer, audience);
            return jwt.ToString();
        }

        private static void RtuIn(string uriString, string contentType, byte[] message)
        {
            Console.WriteLine("message received");
            string ct = "application/octet-stream";
            string topicUriString = "http://wwww.skunklab.io/vrtu/alberta/unitid1-out";
            Task task = mqttClient.PublishAsync(SkunkLab.Protocols.Mqtt.QualityOfServiceLevelType.AtMostOnce, topicUriString, ct, message);
            Task.WhenAll(task);
        }


        private async Task ConnectTwin()
        {
            EdgeTwin edgeTwin = new EdgeTwin();
            EdgeConfig config = await edgeTwin.ConnectAsync();
            if(config != null)
            {
                string jsonString = JsonConvert.SerializeObject(config);
                //(1) write to file on container
                //(2) close connection and re-open using the new config
                //(3) report back to the twin
                await edgeTwin.ReportAsync(config);            
            }
        }
    }
}
