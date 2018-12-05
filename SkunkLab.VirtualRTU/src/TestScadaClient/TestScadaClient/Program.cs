using SkunkLab.Channels;
using SkunkLab.VirtualRtu.ModBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestScadaClient
{
    class Program
    {

        private static Dictionary<ushort, string> dict;
        private static int port = 502;
        private static string payloadString;
        private static IChannel channel;
        private static ushort index;
        private static long startTicks;
        private static int maxMessages;
        private static byte[] randomPayload;
        private static Dictionary<ushort, long> container;
        private static string switchValue;
        private static CancellationTokenSource cts;
        private static CancellationTokenSource ctsChannel;
        private static string filename = "data.txt";
        private static int delay;
        private static string size;
        private static int bsize;
        private static ManualResetEventSlim done;
        static void Main(string[] args)
        {
            dict = new Dictionary<ushort, string>();
            container = new Dictionary<ushort, long>();


            if (args == null || args.Length == 0)
            {
                Console.WriteLine("TEST SCADA Client press any key to start...");
                Console.ReadKey();
            }
            else
            {
                switchValue = "A";
                delay = Convert.ToInt32(args[0]);
                size = "M";
            }

            Console.WriteLine("Opening channel...");

            OpenChannel();


            if (args == null || args.Length == 0)
            {
                Console.Write("Enter number of messages to send or 'A' for infinite loop ? ");
                switchValue = Console.ReadLine();

                Console.Write("Enter delay between messages in milliseconds ? ");
                delay = Convert.ToInt32(Console.ReadLine());

                Console.Write("Enter messages size (S/M/L/X) ? ");
                size = Console.ReadLine().ToUpperInvariant();                
            }

            SetSize();



            Random ran = new Random();
            randomPayload = new byte[bsize - 7];
            ran.NextBytes(randomPayload);

            Task t = StartMessageRun();
            Task.WhenAll(t);

            done = new ManualResetEventSlim(false);


            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("Client is running...");
            done.Wait();

        }

        private static void OpenChannel()
        {
            ctsChannel = new System.Threading.CancellationTokenSource();
            string hostname = "virtualrtu.eastus2.cloudapp.azure.com";
            //string hostname = System.Environment.MachineName;

            //channel = ChannelFactory.Create(false, hostname, 502, 1024, 1024000, ctsChannel.Token);
            string ipAddress = "52.167.0.54";
            channel = ChannelFactory.Create(false, new IPEndPoint(IPAddress.Parse(ipAddress), port), 1024, 4096, ctsChannel.Token);
            channel.OnOpen += Channel_OnOpen;
            channel.OnReceive += Channel_OnReceive;
            channel.OnError += Channel_OnError;
            channel.OnClose += Channel_OnClose;
            Task task = channel.OpenAsync();
            Task.WaitAll(task);

            Task tr = channel.ReceiveAsync();
            Task.WhenAll(tr);

            Console.WriteLine("Channel open and receiving");
        }


        private static async Task StartMessageRun()
        {
            if (switchValue == "A")
            {
                await RunUnlimited().ConfigureAwait(false);
            }
            else
            {
                maxMessages = Convert.ToInt32(switchValue);
                await RunLimited().ConfigureAwait(false);
            }
        }

        private static async Task RunLimited()
        {



            while (index < maxMessages)
            {

                MbapHeader header = new MbapHeader()
                {
                    UnitId = 13,
                    ProtocolId = 0,
                    TransactionId = index,
                    Length = 0
                };

                byte[] headerBytes = header.Encode();

                if(headerBytes.Length + randomPayload.Length >= Convert.ToInt32(UInt16.MaxValue))
                {
                    Console.WriteLine("Size exceeded trying to send length {0}", headerBytes.Length + randomPayload.Length);
                    break;
                }

                byte[] buffer = new byte[headerBytes.Length + randomPayload.Length];
                Buffer.BlockCopy(headerBytes, 0, buffer, 0, headerBytes.Length);
                Buffer.BlockCopy(randomPayload, 0, buffer, headerBytes.Length, randomPayload.Length);

                container.Add(index, DateTime.Now.Ticks);
                dict.Add(index, Convert.ToBase64String(buffer));

                index++;
                await channel.SendAsync(buffer);
                Console.WriteLine("Message {0} sent", index);
                

                await Task.Delay(delay);

            }
        }

        private static void Restart()
        {
            Console.WriteLine("---------------- Restarting ------------------------");
            done.Set();
            //Task task = Task.Delay(30000);
            //Task.WaitAll(task);

            //Task t = RunUnlimited();
            //Task.WhenAll(t);

        }


        private static void SetSize()
        {
            bsize = 7;

            if (size == "S")
                bsize += 100;
            if (size == "M")
                bsize += 1000;
            if (size == "L")
                bsize += 10000;
            if (size == "X")
                bsize += 50000;

            Console.WriteLine("Payload Size {0}", bsize);
        }

        private static async Task RunUnlimited()
        {
            cts = new CancellationTokenSource();
            cts.Token.Register(Restart);
            CancellationToken token = cts.Token;
            ushort tx = 0;
            

            while (!token.IsCancellationRequested)
            {
                if(tx == UInt16.MaxValue)
                {
                    Task task = WriteData("***** Loop *****");
                    Task.WaitAll(task);
                    tx = 0;
                }

                tx++;
                MbapHeader header = new MbapHeader()
                {
                    UnitId = 1,
                    ProtocolId = 0,
                    TransactionId = tx,
                    Length = 100
                };

                byte[] headerBytes = header.Encode();

                


                byte[] buffer = new byte[bsize];
                Buffer.BlockCopy(headerBytes, 0, buffer, 0, headerBytes.Length);
                Buffer.BlockCopy(randomPayload, 0, buffer, headerBytes.Length, randomPayload.Length);

                if(container.ContainsKey(tx))
                {
                    container.Remove(tx);
                }

                if(dict.ContainsKey(tx))
                {
                    dict.Remove(tx);
                }

                container.Add(tx, DateTime.Now.Ticks);
                dict.Add(tx, Convert.ToBase64String(buffer));

                await channel.SendAsync(buffer);
                Console.WriteLine("Message {0} sent", index + 1);
                index++;

                await Task.Delay(delay);

            }
        }



            

        private static void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            Console.WriteLine("Channel closed");
            if (switchValue == "A")
            {
                try
                {
                    cts.Cancel();
                    channel = null;
                    OpenChannel();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("FAULT--- {0}", ex.Message);
                }

                //Restart();
                done.Set();
            }
        }

        private static void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            Console.WriteLine("Channel error {0}", e.Error.Message);
            cts.Cancel();
            try
            {
                ctsChannel.Cancel();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Fault closing channel {0}", ex.Message);
            }

            Restart();
           
        }

        private static void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {

            long ticks = DateTime.Now.Ticks;
            byte[] headerBuffer = new byte[7];
            Buffer.BlockCopy(e.Message, 0, headerBuffer, 0, 7);
            MbapHeader header = MbapHeader.Decode(headerBuffer);
            string base64 = Convert.ToBase64String(e.Message);
            
            if(dict.ContainsKey(header.TransactionId) && dict[header.TransactionId] == base64)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                string data = String.Format("Message {0} - {1}", header.TransactionId + 1, Math.Round(TimeSpan.FromTicks(ticks - container[header.TransactionId]).TotalMilliseconds, 0));
                //Console.WriteLine("{0} Message {1} received in {2} ms", "Immuatable", header.TransactionId + 1, Math.Round(TimeSpan.FromTicks(ticks - container[header.TransactionId]).TotalMilliseconds, 0));
                Task t = WriteData(data);
                Task.WhenAll(t);
                Console.WriteLine(data);                
                Console.ResetColor();
                dict.Remove(header.TransactionId);
                container.Remove(header.TransactionId);
            }
            else
            {
                done.Set();
                //Console.ForegroundColor = ConsoleColor.Yellow;
                //Console.WriteLine("{0} Message {1} received in {2} ms", "Muatable", header.TransactionId, Math.Round(TimeSpan.FromTicks(ticks - container[header.TransactionId]).TotalMilliseconds, 0));
                //Console.ResetColor();
            }

        }

        private static void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            Console.WriteLine("Channel open");
        }


        private static async Task WriteData(string line)
        {
            string data = String.Format("{0} - {1}", DateTime.Now.ToString("hh:mm:ss.ffff"), line);
            using (StreamWriter writer = new StreamWriter(filename, true))
            {
                await writer.WriteLineAsync(data);
                writer.Flush();
                writer.Close();
            }
        }
    }
}
