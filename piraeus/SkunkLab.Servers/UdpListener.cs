using SkunkLab.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Servers
{
    public class UdpListener
    {
        public UdpListener(IPEndPoint localEP, CancellationToken token)
        {
            this.localEP = localEP;
            this.token = token;
        }

        private IPEndPoint localEP;
        private CancellationToken token;
        Dictionary<string, IChannel> container;

        public async Task StartAsync()
        {
            UdpClient server = new UdpClient();
            server.ExclusiveAddressUse = false;
            server.DontFragment = true;
            server.Client.Bind(localEP);

            UdpReceiveResult result = await server.ReceiveAsync();
            
            string key = String.Format("{0}:{1}", result.RemoteEndPoint.Address.ToString(), result.RemoteEndPoint.Port);
            
            if(!container.ContainsKey(key))
            {
                //create new IChannel
                //add key and IChannel to container
            }

            //inject message into IChannel
            
        }
    }
}
