

namespace PiraeusClientModule.Channels.WebSocket
{
    
        using System.Net.WebSockets;

        internal sealed class WebSocketMessage
        {
            public readonly object Data;
            public readonly WebSocketMessageType MessageType;

            public WebSocketMessage(object data, WebSocketMessageType messageType)
            {
                this.Data = data;
                this.MessageType = messageType;
            }
        }
    

}
