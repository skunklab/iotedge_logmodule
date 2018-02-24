using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels
{
    public abstract class ChannelFactory
    {
        public static IChannel Create(TcpClient client, CancellationToken token)
        {
            return TcpChannel.Create(client, token);
        }
    }
}
