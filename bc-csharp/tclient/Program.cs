using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tclient
{
    class Program
    {
        private static IPEndPoint remoteEP;
        private static NetworkStream localStream;
        private static Stream stream;
        private static string pskString = "9/NkHW52KwlRCzeJ6sziOntgGHsDnJMX6x9i6gdaS+E=";
        private static TcpClient client;
        private static CancellationTokenSource cts;
        private static CancellationToken token;
        private static MemoryStream output;
        private static TlsClientProtocol protocol;

        static void Main(string[] args)
        {
            Console.WriteLine("TCP client");
            Console.ReadKey();
            output = new MemoryStream();
            cts = new CancellationTokenSource();
            token = cts.Token;

            remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 502);

            client = new TcpClient();
            client.LingerState = new LingerOption(true, 0);
            client.NoDelay = true;
            client.ExclusiveAddressUse = false;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.UseOnlyOverlappedIO = true;

            Task task = ConnectAsync();
            Task.WaitAll(task);

            int v = protocol.GetAvailableOutputBytes();
            byte[] buf = new byte[v];
            protocol.ReadOutput(buf, 0, v);
            
            Stream s = client.GetStream();
            s.WriteAsync(buf, 0, buf.Length);
            


            int v2 = protocol.GetAvailableInputBytes();

        }

        private static async Task ConnectAsync()
        {
            try
            {
                await client.ConnectAsync(remoteEP.Address, remoteEP.Port);

                TlsPskIdentity ident = new BasicTlsPskIdentity("MyIdentity", Convert.FromBase64String(pskString));
                PiraeusPskTlsClient pclient = new PiraeusPskTlsClient(ident);
                //PskTlsClient pskTlsClient = new PskTlsClient(ident);
                 protocol = new TlsClientProtocol(new SecureRandom());

                protocol.Connect(pclient);

                
               
                Console.WriteLine("connected");
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }


        }

    }
}
