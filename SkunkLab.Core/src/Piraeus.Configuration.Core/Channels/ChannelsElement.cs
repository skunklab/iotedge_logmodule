using System.Configuration;

namespace Piraeus.Configuration.Channels
{
    public class ChannelsElement : ConfigurationElement
    {
        [ConfigurationProperty("websocket")]
        public WebSocketChannelElement WebSocket
        {
            get { return (WebSocketChannelElement)base["websocket"]; }
            set { base["websocket"] = value; }
        }


        [ConfigurationProperty("tcp")]
        public TcpChannelElement TCP
        {
            get { return (TcpChannelElement)base["tcp"]; }
            set { base["tcp"] = value; }
        }
    }
}
