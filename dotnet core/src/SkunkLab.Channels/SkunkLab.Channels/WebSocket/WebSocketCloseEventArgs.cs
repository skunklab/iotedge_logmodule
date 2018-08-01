using System;
using System.Net.WebSockets;

namespace SkunkLab.Channels.WebSocket
{
    public class WebSocketCloseEventArgs : EventArgs
    {
        public WebSocketCloseEventArgs(WebSocketCloseStatus status)
        {
            Status = status;
        }

        public WebSocketCloseStatus Status { get; internal set; }
    }
}
