using System;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Tcp.Listener
{
    public abstract class TcpServerListener
    {
        public abstract Task StartAsync();

        public abstract Task StopAsync();

        public event EventHandler<TcpServerErrorEventArgs> OnError;
    }
}
