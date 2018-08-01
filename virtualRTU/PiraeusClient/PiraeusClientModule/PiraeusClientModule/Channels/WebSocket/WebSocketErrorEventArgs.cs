using System;

namespace PiraeusClientModule.Channels.WebSocket
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
