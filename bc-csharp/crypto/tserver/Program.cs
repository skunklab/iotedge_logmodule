using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tserver
{
    class Program
    {
        static TcpListener listener;
        static CancellationToken token;
        private static TcpClient client;
        private static TlsServerProtocol protocol;
        private static string pskString = "9/NkHW52KwlRCzeJ6sziOntgGHsDnJMX6x9i6gdaS+E=";

        static void Main(string[] args)
        {
            Console.WriteLine("TCP Listener");
            int port = 502;
            IPAddress address = IPAddress.Parse("127.0.0.1");
            listener = new TcpListener(address, port);

            Task task = StartAsync();
            Task.WhenAll(task);

            Console.ReadKey();
        }

        static async Task StartAsync()
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                token = cts.Token;

                listener.ExclusiveAddressUse = false;

                listener.Start();

                Console.WriteLine("Listening");

                client = await listener.AcceptTcpClientAsync();
                client.LingerState = new LingerOption(true, 0);
                client.NoDelay = true;
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                client.Client.UseOnlyOverlappedIO = true;

                protocol = new TlsServerProtocol(new SecureRandom());
                TlsPskIdentityManager manager = new PskIdentityManager(Convert.FromBase64String(pskString));
                //PskTlsServer server = new PskTlsServer(manager);
                PiraeusPskTlsServer server = new PiraeusPskTlsServer(manager);
                protocol.Accept(server);

                
               

                Thread.Sleep(2000);




                Task dt1 = Task.Delay(2000);
                Task.WaitAll(dt1);

                //Task task = Receive2Async();
                //await Task.WhenAll(task);

                //Task dt2 = Task.Delay(2000);
                //Task.WaitAll(dt2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
