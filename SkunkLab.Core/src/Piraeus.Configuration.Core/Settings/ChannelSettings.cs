using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class ChannelSettings
    {
        public ChannelSettings()
        {

        }

        public ChannelSettings(WebSocketSettings websocket, TcpSettings tcp)
        {
            WebSocket = websocket;
            Tcp = tcp;
        }

        [JsonProperty("websocket")]
        public WebSocketSettings WebSocket { get; set; }

        [JsonProperty("tcp")]
        public TcpSettings Tcp { get; set; }

        [JsonProperty("udp")]
        public UdpSettings Udp { get; set; }
    }
}
