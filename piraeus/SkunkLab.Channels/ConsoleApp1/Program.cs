using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Mqtt;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string topic = "devices/device1/messages/events/";
            string username = "slbhacktest.azure-devices.net/device1/api-version=2016-11-14";
            string password = "SharedAccessSignature sr=slbhacktest.azure-devices.net%2Fdevices%2Fdevice1&sig=xaUGaJoqPTT%2F%2BvPtbjn24chruNOy%2BR4I1D3oWU1aJV0%3D&se=1498516539";
            string deviceId = "device1";
            
            CancellationTokenSource src = new CancellationTokenSource();
            string endpoint = "wss://slbhacktest.azure-devices.net:8883";
            WebSocketConfig config = new WebSocketConfig();
            WebSocketClientChannel channel = new WebSocketClientChannel(new Uri(endpoint), config, src.Token);
            channel.OnClose += Channel_OnClose;
            channel.OnOpen += Channel_OnOpen;
            channel.OnStateChange += Channel_OnStateChange;
            channel.OnError += Channel_OnError;
            channel.OnReceive += Channel_OnReceive;

            channel.Open();
            SkunkLab.Protocols.Mqtt.ConnectMessage con = new ConnectMessage(deviceId, username, password, 240, false);

            byte[] connectionMsg = con.Encode();
            Task task = channel.SendAsync(connectionMsg);
            Task.WhenAll(task);

            
            ConnectMessage m = new ConnectMessage("myid",60, true);
            DisconnectMessage dm = new DisconnectMessage();
            PublishAckMessage ack = new PublishAckMessage(PublishAckType.PUBREL, 1);
            ack.MessageId = 5;
            byte[] ebytes = ack.Encode();

            ////PublishMessage pub = new PublishMessage(false, QualityOfServiceLevelType.AtMostOnce, false, 5, "aa", Encoding.UTF8.GetBytes("bb"));

            //Dictionary<string, QualityOfServiceLevelType> dict = new Dictionary<string, QualityOfServiceLevelType>();
            //dict.Add("aa", QualityOfServiceLevelType.AtLeastOnce);

            //SubscribeMessage sub = new SubscribeMessage(5, dict);
            //byte[] bytes = sub.Encode();
            //MqttMessage mm = MqttMessage.DecodeMessage(bytes);
            //Console.ReadKey();
            
        }

        private static void Channel_OnReceive(object sender, SkunkLab.Channels.ChannelReceivedEventArgs args)
        {
            MqttMessage msg = MqttMessage.DecodeMessage(args.Message);
        }

        private static void Channel_OnError(object sender, SkunkLab.Channels.ChannelErrorEventArgs args)
        {
            Console.WriteLine(args.Error.Message);
        }

        private static void Channel_OnStateChange(object sender, SkunkLab.Channels.ChannelStateEventArgs args)
        {
            Console.WriteLine(args.State);
        }

        private static void Channel_OnOpen(object sender, SkunkLab.Channels.ChannelOpenEventArgs args)
        {
            Console.WriteLine("Open");
        }

        private static void Channel_OnClose(object sender, SkunkLab.Channels.ChannelCloseEventArgs args)
        {
            Console.WriteLine("Closed");
        }
    }
}
