using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class WebSocketSettings
    {
        public WebSocketSettings()
        {

        }
       
        public WebSocketSettings(int maxIncomingMessageSize = 0x400000, int receiveLoopBufferSize = 0x2000, int sendBufferSize = 0x2000, double closeTimeoutMilliseconds = 250.0)
        {
            MaxIncomingMessageSize = maxIncomingMessageSize;
            ReceiveLoopBufferSize = receiveLoopBufferSize;
            SendBufferSize = sendBufferSize;
            CloseTimeoutMilliseconds = closeTimeoutMilliseconds;
        }

        [JsonProperty("maxIncomingMessageSize")]
        public int MaxIncomingMessageSize { get; set; }

        [JsonProperty("receiveLoopBufferSize")]
        public int ReceiveLoopBufferSize { get; set; }


        [JsonProperty("sendBufferSize")]
        public int SendBufferSize { get; set; }

        [JsonProperty("closeTimeoutMilliseconds")]
        public double CloseTimeoutMilliseconds { get; set; }


    }
}
