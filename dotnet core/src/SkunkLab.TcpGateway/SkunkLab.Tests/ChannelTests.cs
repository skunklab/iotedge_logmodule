using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkunkLab.Channels.Tcp;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Tests
{
    [TestClass]
    public class ChannelTests
    {
        private static string hostname = "piraeus.eastus2.cloudapp.azure.com";
        private static int port = 8883;
        private static string pskIdentity = "Key1";
        private static byte[] psk = Encoding.UTF8.GetBytes("The quick brown fox");
        [TestMethod]
        public void ConnectTcpClientTest()
        {

            


            string k = "VGhlIHF1aWNrIGJyb3duIGZveA==";
            string k2 = Convert.ToBase64String(psk);

            bool b = k == k2;

            CancellationTokenSource source = new CancellationTokenSource();
            TcpClientChannel channel = new TcpClientChannel(hostname, port, pskIdentity, psk, 1024, source.Token);
            channel.OnOpen += Channel_OnOpen;
            channel.OnError += Channel_OnError;
            Task task = channel.OpenAsync();
            Task.WaitAll(task);

        }

        private void Channel_OnError(object sender, Channels.ChannelErrorEventArgs e)
        {
            Assert.Fail(e.Error.Message);
        }

        private void Channel_OnOpen(object sender, Channels.ChannelOpenEventArgs e)
        {
            Assert.IsNotNull(e.ChannelId);
        }
    }
}
