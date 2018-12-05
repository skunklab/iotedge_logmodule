using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.VirtualRtu.ModBus;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.ScadaClient
{
    class Program
    {
        private static IChannel channel;
        static void Main(string[] args)
        {
            Console.WriteLine("SCADA Sample Client");
            Console.WriteLine("Press any key to connect to VRTU");
            Console.ReadKey();

            Console.Write("Enter host name of V-RTU ? "); //vrtu.eastus.cloudapp.azure.com
            string hostname = Console.ReadLine();
            hostname = System.Net.Dns.GetHostName();
            //IPAddress address = System.Net.Dns.GetHostAddresses(hostname)[0];

            Console.Write("Enter Unit ID of RTU ? ");
            int unit = Convert.ToUInt16(Console.ReadLine());

            CancellationTokenSource src = new CancellationTokenSource();
            //channel = new TcpClientChannel2(hostname, 502, pskIdentity, psk, 1024, 2048, src.Token);
            
            channel = new TcpClientChannel2(hostname, 502, 1024, 2048, src.Token);
            //channel = new TcpClientChannel2(new IPEndPoint(address, 502), 1024, 2048, src.Token);
            channel.OnError += Channel_OnError;
            channel.OnClose += Channel_OnClose;
            channel.OnOpen += Channel_OnOpen;
            channel.OnReceive += Channel_OnReceive;
            Task openTask = channel.OpenAsync();
            Task.WaitAll(openTask);

      label1x:
            Console.WriteLine("Press any key to publish message");
            Console.ReadKey();

            MbapHeader header = new MbapHeader() { UnitId = Convert.ToByte(unit), ProtocolId = 0, TransactionId = 2, Length = 93 };
            byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.Ticks.ToString());
            //Random ran = new Random();
            //byte[] data = new byte[93];
            //ran.NextBytes(data);
            
            byte[] headerBytes = header.Encode();
            byte[] buffer = new byte[headerBytes.Length + data.Length];
            Buffer.BlockCopy(headerBytes, 0, buffer, 0, headerBytes.Length);
            Buffer.BlockCopy(data, 0, buffer, headerBytes.Length, data.Length);

            Task sendTask = channel.SendAsync(buffer);
            Task.WhenAll(sendTask);

            Console.WriteLine("Waiting...");
            Console.ReadKey();

            goto label1x;
        }

        private static void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            Console.WriteLine("Message received of length {0}", e.Message.Length);
            MbapHeader header = MbapHeader.Decode(e.Message);
            Console.WriteLine("Received message with Unit ID {0}", header.UnitId);
            byte[] dataBuffer = new byte[e.Message.Length - 7];
            Buffer.BlockCopy(e.Message, 7, dataBuffer, 0, dataBuffer.Length);
            string ticksString = Encoding.UTF8.GetString(dataBuffer);
            long startTicks = Convert.ToInt64(ticksString);
            long tickDiff = DateTime.Now.Ticks - startTicks;
            Console.WriteLine("Latency {0} ms", TimeSpan.FromTicks(tickDiff).TotalMilliseconds);

        }

        private static void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            Console.WriteLine("Channel open");
            Task task = channel.ReceiveAsync();
            Task.WhenAll(task);
            Console.WriteLine("Receiving");            
        }

        private static void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            Console.WriteLine("Channel closed");
        }

        private static void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            Console.WriteLine("Channel error");
        }
    }
}
