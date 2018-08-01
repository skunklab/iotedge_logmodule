using System;

namespace SkunkLab.TcpGateway.Listeners
{
    public class GenericTcpMessageEventArgs : EventArgs
    {
        public GenericTcpMessageEventArgs(byte[] message)
        {
            Message = message;
        }

        public byte[] Message { get; internal set; }
    }
}
