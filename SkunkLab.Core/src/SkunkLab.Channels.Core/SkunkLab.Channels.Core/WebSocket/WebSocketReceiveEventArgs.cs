using System;

namespace SkunkLab.Channels.WebSocket
{
    public class WebSocketReceiveEventArgs : EventArgs
    {
        public WebSocketReceiveEventArgs(byte[] message)
        {
            Message = message;
        }

        public byte[] Message { get; internal set; }
    }
}
