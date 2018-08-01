using SkunkLab.Protocols.Mqtt;
using SkunkLab.VirtualRtu.Adapters;
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

        static void Main(string[] args)
        {
            rtuConfig = VRtuConfig.LoadFromEnvironmentVariable();
            mqttConfig = new MqttConfig(90);
            rtuMap = RtuMap.LoadFromEnvironmentVariable();            

            listener = new Listener();
            listener.OnError += Listener_OnError;
            Task task = StartAsync();
            Task.WhenAll(task);

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


            Task task = StartAsync();
            Task.WhenAll(task);
        }
    }
}
