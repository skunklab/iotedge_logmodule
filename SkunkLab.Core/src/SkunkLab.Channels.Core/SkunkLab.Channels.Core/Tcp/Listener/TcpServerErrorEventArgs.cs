using System;

namespace SkunkLab.Channels.Tcp.Listener
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
