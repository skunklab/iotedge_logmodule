using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SkunkLab.Servers;

namespace Piraeus.UdpServer
{
    class Program
    {
        private static CancellationTokenSource source;
        static ManualResetEvent quitEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            source = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eArgs) => {
                source.Cancel();
                eArgs.Cancel = true;
            };

            int port = Convert.ToInt32(args[0]);

            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), port);
            UdpListener listener = new UdpListener(endpoint, source.Token);
            Task task = listener.StartAsync();
            Task.WhenAll(task);

            Console.WriteLine("UDP Server started on IP {0} and Port {1}", endpoint.Address.ToString(), port);

            quitEvent.WaitOne();
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
    }
}
