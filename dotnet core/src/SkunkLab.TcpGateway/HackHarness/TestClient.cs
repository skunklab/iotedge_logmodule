using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.VirtualRtu.ModBus;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HackHarness
{
    public class TestClient
    {
        public TestClient(string hostname, int port, byte[] psk, string securityToken, string subscriptionResourceUriString)
        {
            source = new CancellationTokenSource();
            channel = new TcpClientChannel(hostname, port, "Key1", psk, 102400, source.Token);
            client = new PiraeusMqttClient(new SkunkLab.Protocols.Mqtt.MqttConfig(), channel);
            Task<ConnectAckCode> t = client.ConnectAsync("myId", "JWT", securityToken, 90);
            Task.WaitAll(t);
            Console.WriteLine(t.Result);

            client.SubscribeAsync(subscriptionResourceUriString, SkunkLab.Protocols.Mqtt.QualityOfServiceLevelType.AtMostOnce, null);

        }

       private void SubscriptionMessage(string resourceUri, string contentType, byte[] message)
        {
            //echo
            string outputResource = resourceUri.Replace("-in", "-out");
            Task task = client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, outputResource, "application/octet-stream", message);
            Task.WhenAll(task);
        }

        private CancellationTokenSource source;
        private IChannel channel;
        private PiraeusMqttClient client;

        public TestClient(int port)
        {
            source = new CancellationTokenSource();
            channel = new TcpClientChannel(System.Net.Dns.GetHostName(), port, 102400, source.Token);
            //channel = new TcpClientChannel(new IPEndPoint(System.Net.Dns.GetHostName(), port), 102400, source.Token);
            channel.OnReceive += Channel_OnReceive;
            Task task = channel.OpenAsync();
            Task.WaitAll(task);

        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            MbapHeader header = MbapHeader.Decode(e.Message);
            Console.WriteLine("Received TestClient {0}", header.UnitId);
        }

        public void Send(MbapHeader header, byte[] message)
        {
            byte[] headerBytes = header.Encode();
            byte[] buffer = new byte[headerBytes.Length + message.Length];
            Buffer.BlockCopy(headerBytes, 0, buffer, 0, headerBytes.Length);
            Buffer.BlockCopy(message, 0, buffer, headerBytes.Length, message.Length);

            Task task = channel.SendAsync(buffer);
            Task.WhenAll(task);
        }
    }
}
