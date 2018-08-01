using System;

namespace SkunkLab.Channels.WebSocket
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
