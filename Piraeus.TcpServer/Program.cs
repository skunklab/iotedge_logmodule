using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SkunkLab.Servers;

namespace Piraeus.TcpServer
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

            IPAddress address = IPAddress.Parse(GetLocalIPAddress());
            int port = Convert.ToInt32(args[0]);            
            TcpServerListener listener = new TcpServerListener(address, port, source.Token);
            Task task = listener.StartAsync();
            Task.WhenAll(task);

            Console.WriteLine("TCP Server started on IP {0} and Port {1}", address.ToString(), port);

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
