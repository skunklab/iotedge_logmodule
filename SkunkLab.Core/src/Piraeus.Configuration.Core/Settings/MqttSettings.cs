using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class MqttSettings
    {
        public MqttSettings()
        {

        }
        public MqttSettings(double keepAliveSeconds = 180.0, double ackTimeoutSeconds = 2.0, double ackRandomFactor = 1.5, int maxRetransmit = 4, double maxLatencySeconds = 100.0)
        {
            KeepAliveSeconds = keepAliveSeconds;
            AckTimeoutSeconds = ackTimeoutSeconds;
            AckRandomFactor = ackRandomFactor;
            MaxRetransmit = maxRetransmit;
            MaxLatencySeconds = maxLatencySeconds;
        }

        [JsonProperty("keepAliveSeconds")]
        public double KeepAliveSeconds { get; set; }

        [JsonProperty("ackTimeoutSeconds")]
        public double AckTimeoutSeconds { get; set; }

        [JsonProperty("ackRandomFactor")]
        public double AckRandomFactor { get; set; }

        [JsonProperty("maxRetransmit")]
        public int MaxRetransmit { get; set; }

        [JsonProperty("maxLatencySeconds")]
        public double MaxLatencySeconds { get; set; }
    }
}
