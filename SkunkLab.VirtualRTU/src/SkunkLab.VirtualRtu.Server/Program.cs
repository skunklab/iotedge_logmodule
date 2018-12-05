using SkunkLab.Protocols.Mqtt;
using SkunkLab.VirtualRtu.Adapters;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.VirtualRtu.Server
{
    public class Program
    {
        static Listener listener;
        static RtuMap rtuMap;
        static VRtuConfig rtuConfig;
        static MqttConfig mqttConfig;
        static CancellationTokenSource source;
        private static ManualResetEventSlim done;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Virtual RTU TCP Server");
            Console.ResetColor();
            Console.WriteLine();

            Task task = SetupAsync();
            Task.WhenAll(task);

            done = new ManualResetEventSlim(false);


            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("Virtual RTU is running...");
            done.Wait();

            //manual reset event to block
        }

        private static async Task StartAsync()
        {
            source = new CancellationTokenSource();
            listener = null;
            listener = new Listener();
            listener.OnError += Listener_OnError;
            await listener.StartAsync(rtuConfig, mqttConfig, rtuMap, source.Token);
        }

        private static async Task SetupAsync()
        {
            rtuConfig = VRtuConfig.LoadFromEnvironmentVariable();
            mqttConfig = new MqttConfig(90);
            rtuMap = RtuMap.LoadFromEnvironmentVariable();

            Console.WriteLine("RTU-CONFIG - {0}", rtuConfig.SecurityToken);
            Console.WriteLine("Rtu-MAP has resource {0}", rtuMap.HasResources(1));



            listener = new Listener();
            listener.OnError += Listener_OnError;
            Task task = StartAsync();
            await Task.WhenAll(task);
            Console.WriteLine("<----- Starting V-RTU ----->");
        }

        private static void Listener_OnError(object sender, VRtuListenerErrorEventArgs e)
        {
            //error in listener...stop and restart
            source.Cancel();
            Task waitTask = Task.Delay(5000);
            Task.WaitAll(waitTask);

            Task stopTask = listener.StopAsync();
            Task.WaitAll(stopTask);

            Trace.TraceWarning("Listener detected an error and will close.");
            Trace.TraceError(e.Error.Message);

            //Task task = StartAsync();
            //Task.WhenAll(task);
            Console.WriteLine("<---- Forcing Restart ----->");
            done.Set(); //should force restart of the container


        }
    }
}
