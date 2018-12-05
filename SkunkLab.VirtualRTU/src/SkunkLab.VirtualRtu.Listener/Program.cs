using SkunkLab.Protocols.Mqtt;
using SkunkLab.VirtualRtu.Adapters;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.VirtualRtu.Listener
{
    class Program
    {
        static Listener listener;
        static RtuMap rtuMap;
        static VRtuConfig rtuConfig;
        static MqttConfig mqttConfig;
        static CancellationTokenSource source;
        private static ManualResetEventSlim done;

        static void Main(string[] args)
        {
            rtuConfig = VRtuConfig.LoadFromEnvironmentVariable();            
            mqttConfig = new MqttConfig(90);
            rtuMap = RtuMap.LoadFromEnvironmentVariable();            

            listener = new Listener();
            listener.OnError += Listener_OnError;
            Task task = StartAsync();
            Task.WhenAll(task);

            done = new ManualResetEventSlim(false);


            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("Virtual RTU is ready...");
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

            done.Set(); //should force restart of the container

            
        }
    }
}
