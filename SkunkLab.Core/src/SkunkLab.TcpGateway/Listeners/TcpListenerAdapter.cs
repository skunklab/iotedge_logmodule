using SkunkLab.Channels;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.TcpGateway.Listeners
{
    public abstract class TcpListenerAdapter
    {
        public event EventHandler<GenericTcpMessageEventArgs> OnReceive;
        public abstract void Initialize(TcpClient client);

        public abstract Task SendAsync(byte[] message);

        public static TcpListenerAdapter Create<T>(T config, IChannel channel)
        {
            return null;
        }
    }
}
