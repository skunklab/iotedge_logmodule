using System;

namespace SkunkLab.Channels.Core.WebSocket
{
    public class WebSocketErrorEventArgs : EventArgs
    {
        public WebSocketErrorEventArgs(Exception error)
        {
            Error = error;
        }

        public Exception Error { get; internal set; }
    }
}
