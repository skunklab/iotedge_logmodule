using System;

namespace SkunkLab.Channels.Core.WebSocket
{
    public sealed class WebSocketConfig
    {
        public WebSocketConfig(int maxIncomingMessageSize = 0x400000, int receiveLoopBufferSize = 0x2000, int sendBufferSize = 0x2000, double closeTimeoutMilliseconds = 250.0)
        {
            if (maxIncomingMessageSize <= 0)
            {
                throw new ArgumentOutOfRangeException("maxIncomingMessageSize");
            }

            if (receiveLoopBufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("receiveLoopBufferSize");
            }

            if (closeTimeoutMilliseconds <= 0.0)
            {
                throw new ArgumentOutOfRangeException("closeTimeoutMilliseconds");
            }

            MaxIncomingMessageSize = maxIncomingMessageSize;
            ReceiveLoopBufferSize = receiveLoopBufferSize;
            SendBufferSize = sendBufferSize;
            CloseTimeout = TimeSpan.FromMilliseconds(closeTimeoutMilliseconds);

        }
        public int MaxIncomingMessageSize { get; internal set; }

        public int ReceiveLoopBufferSize { get; internal set; }

        public int SendBufferSize { get; internal set; }

        public TimeSpan CloseTimeout { get; internal set; }
    }
}
