using System;
using System.Threading.Tasks;

namespace SkunkLab.TcpGateway.Listeners
{
    public abstract class TcpServerListenerBase
    {
        public abstract Task StartAsync();
        public abstract Task StopAsync();

        public abstract event EventHandler<TcpServerErrorEventArgs> OnError;
    }
}
