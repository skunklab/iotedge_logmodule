using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.VirtualRtu.Adapters;
using SkunkLab.VirtualRtu.ModBus;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        private static string nameClaimType = "http://www.skunklab.io/name";
        private static string roleClaimType = "http://www.skunklab.io/role";
        private static string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        private static string issuer = "http://www.skunklab.io/";
        private static string audience = "http://www.skunklab.io/";
        private static int lifetimeMinutes = 518400;

        

        static void Main(string[] args)
        {
            string key1 = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
            string issuer = "http://www.skunklab.io/";
            string audience = issuer;
            string token = GetSecurityToken();
            //string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vd3d3LnNrdW5rbGFiLmlvL25hbWUiOiJmaWVsZGdhdGV3YXkxIiwiaHR0cDovL3d3dy5za3Vua2xhYi5pby9yb2xlIjoiZGV2aWNlIiwibmJmIjoxNTM2NjgwODgzLCJleHAiOjE1Njc3ODQ4ODMsImlhdCI6MTUzNjY4MDg4MywiYXVkIjoiaHR0cDovL3d3dy5za3Vua2xhYi5pby8ifQ.onXPLHKGFRfAjIRZLlD7vMKa9Z3-sT-9B66QBDoWAsM";
            //JsonWebToken.Authenticate(token, issuer, audience, key1);
            //JwtSecurityToken jwtToken = JsonWebToken.Decode(token);
           
            string resourceUriString = "http://www.skunklab.io/alberta/unitid1-in";
            string resourceUriString2 = "http://www.skunklab.io/alberta/unitid1-out";

            CancellationTokenSource src = new CancellationTokenSource();
            byte[] key = Encoding.UTF8.GetBytes("The quick brown fox");
            IChannel channel = ChannelFactory.Create(false, "piraeus.eastus2.cloudapp.azure.com", 8883, "Key1", key, 1024, 4096, src.Token);
            PiraeusMqttClient client = new PiraeusMqttClient(new MqttConfig(60), channel);
            Task<ConnectAckCode> task = client.ConnectAsync("foo", "JWT", token, 60);
            Task.WaitAll(task);
            Console.WriteLine(task.Result);

            Console.WriteLine("start....");
            Console.ReadKey();

            Task subTask = client.SubscribeAsync(resourceUriString2, QualityOfServiceLevelType.AtMostOnce, ReceivedMessage);
            Task.WaitAll(subTask);
            Console.WriteLine("Subscribed");

            labelX:

            Console.Write("Number of messages to send ? ");
            int numMessages = Convert.ToInt32(Console.ReadLine());
            Console.Write("Delay in ms betweeen messages ? ");
            int delay = Convert.ToInt32(Console.ReadLine());

            
            int index = 0;
            while (index < numMessages)
            {

                long ticks = DateTime.Now.Ticks;
                string tickString = Convert.ToString(ticks);
                MbapHeader header = new MbapHeader() { UnitId = 1, TransactionId = 1, ProtocolId = 0, Length = (ushort)tickString.Length };
                byte[] encoded = header.Encode();

                byte[] body = Encoding.UTF8.GetBytes(tickString);
                byte[] msg = new byte[encoded.Length + body.Length];
                Buffer.BlockCopy(encoded, 0, msg, 0, encoded.Length);
                Buffer.BlockCopy(body, 0, msg, encoded.Length, body.Length);

                client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, resourceUriString, "application/octet-stream", msg);
                index++;
                Task delayTask = Task.Delay(delay);
                Task.WaitAll(delayTask);
            }
            //Console.WriteLine("Sent");
            //Console.ReadKey();

            Console.Write("Try again (Y/N) ? ");
            if (Console.ReadLine().ToLowerInvariant() == "y")
                goto labelX;
            
            ///*
            // * $resource_In = "http://www.skunklab.io/vrtu/"+ $groupid + "/unitid" + $unitid + "-in"
            //    $resource_Out = "http://www.skunklab.io/vtru/"+ $groupid + "/unitid" + $unitid + "-out"
            // */
            //string resourceIn = String.Format("http://www.skunklab.io/vrtu/{0}/unitId{1}-in", "alberta", 0);
            //string resourceOut = String.Format("http://www.skunklab.io/vrtu/{0}/unitId{1}-out", "alberta", 0);
            //RtuMap map = new RtuMap();
            //map.AddResource(0, resourceIn, resourceOut);


            //string jsonString = JsonConvert.SerializeObject(map);
            //File.WriteAllBytes("rtumap.json", Encoding.UTF8.GetBytes(jsonString));

            ////string securityToken = GetSecurityToken();
            ////byte[] psk = Convert.FromBase64String("VGhlIHF1aWNrIGJyb3duIGZveA==");

            ////VRtuConfig config = new VRtuConfig()
            ////{
            ////    Hostname = "piraeus.eastus2.cloudapp.azure.com",
            ////    BlockSize = 1024,
            ////    KeepAliveInterval = 90,
            ////    MaxBufferSize = 1024000,
            ////    Port = 8883,
            ////    PskIdentity = "Key1",
            ////    Psk = psk,
            ////    SecurityToken = securityToken
            ////};

            ////string jsonString = JsonConvert.SerializeObject(config);
            ////File.WriteAllBytes("vconfig.json", Encoding.UTF8.GetBytes(jsonString));
        }

        private static void ReceivedMessage(string resourceUriString, string contentType, byte[] message)
        {
            byte[] tickBytes = new byte[message.Length - 7];
            Buffer.BlockCopy(message, 7, tickBytes, 0, tickBytes.Length);
            string tickString = Encoding.UTF8.GetString(tickBytes);
            long ticks = Convert.ToInt64(tickString);

            long ticksNow = DateTime.Now.Ticks;

            long span = ticksNow - ticks;
            Console.WriteLine("Latency {0} ms", TimeSpan.FromTicks(span).TotalMilliseconds);
            //Console.WriteLine("Received message of {0} bytes", message.Length);
        }

        private static string GetSecurityToken()
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(nameClaimType, String.Format("virtualrtu-{0}", "alberta")));
            claims.Add(new Claim(roleClaimType, String.Format("vrtu")));

            JsonWebToken jwt = new JsonWebToken(symmetricKey, claims, Convert.ToDouble(lifetimeMinutes), issuer, audience);
            return jwt.ToString();
        }
    }
}
