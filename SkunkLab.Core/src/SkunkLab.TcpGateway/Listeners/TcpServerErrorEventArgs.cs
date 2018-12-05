using System;
using System.Collections.Generic;
using System.Text;

namespace SkunkLab.TcpGateway.Listeners
{
   
    public class TcpServerErrorEventArgs : EventArgs
    {
        public TcpServerErrorEventArgs(string channelType, int port, Exception exception)
        {
            ChannelType = channelType;
            Port = port;
            Error = exception;
        }

        public string ChannelType { get; internal set; }

        public int Port { get; internal set; }

        public Exception Error { get; internal set; }
    }
}
