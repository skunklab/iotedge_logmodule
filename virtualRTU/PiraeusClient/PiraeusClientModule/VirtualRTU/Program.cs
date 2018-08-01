using Channels;
using Channels.Tcp.Listener;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtualRTU.ModBus;

namespace VirtualRTU
{
    class Program
    {
        private static CancellationTokenSource cts;
        static void Main(string[] args)
        {


            //VRtuConfig config = new VRtuConfig();
            //config.Audience = "http://www.skunklab.io/";
            //config.Issuer = "http://www.skunklab.io/";
            //config.PiraeusHostname = "piraeus.eastus2.cloudapp.net";
            //config.Port = 8883;
            //config.Psk = "9/NkHW52KwlRCzeJ6sziOntgGHsDnJMX6x9i6gdaS+E=";
            //config.PskIdentifier = "MyPresharedKey";
            //config.SecurityTokenKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
            //config.SecurityTokenType = "JWT";
            //config.TcpBlockSize = 2048;
            //config.TcpMaxBufferSize = 1024000;
            //config.TokenLifeTimeMinutes = TimeSpan.FromHours(8);

            //List<JsonClaim> claims = new List<JsonClaim>();
            //claims.Add(new JsonClaim("http://www.skunklab.io/piraeus/name", "vrtu"));
            //claims.Add(new JsonClaim("http://www.skunklab.io/piraeus/role", "vrtu"));

            //config.Claims = claims.ToArray();

            //List<RtuMapItem> items = new List<RtuMapItem>();
            //items.Add(new RtuMapItem(0, "http://www.skunklab.io/rtu0", "http://www.skunklab.io/rtu"));
            //items.Add(new RtuMapItem(1, "http://www.skunklab.io/rtu1", "http://www.skunklab.io/rtu"));
            //items.Add(new RtuMapItem(2, "http://www.skunklab.io/rtu2", "http://www.skunklab.io/rtu"));

            //config.Map = items.ToArray();

            //string jstring = JsonConvert.SerializeObject(config, Formatting.Indented);

            //File.WriteAllBytes("test.json", Encoding.UTF8.GetBytes(jstring));

            cts = new CancellationTokenSource();
            string hostname = System.Net.Dns.GetHostName();
            hostname = "localhost";
            IPEndPoint endpoint = new IPEndPoint(GetIPAddress(hostname), 502);
            VRtuConfig config = VRtuConfig.Load();


            TcpServerListener listener = new TcpServerListener(GetIPAddress(hostname), 502, typeof(ModBusCommunicationAdapter), cts.Token, new object[] { config });
            Task task = listener.StartAsync();
            Task.WhenAll(task);
            


            Console.ReadKey();
        }

        static IPAddress GetIPAddress(string hostname)
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
