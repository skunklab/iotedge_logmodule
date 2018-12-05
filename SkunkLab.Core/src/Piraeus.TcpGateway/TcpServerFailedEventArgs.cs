using System;

namespace Piraeus.TcpGateway
{
    public class ServerFailedEventArgs : EventArgs
    {
        public ServerFailedEventArgs(string channelType, int port)
        {
            ChannelType = channelType;
            Port = port;
        }

        public string ChannelType { get; internal set; }

        public int Port { get; internal set; }
    }
}
