using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class ProtocolSettings
    {
        public ProtocolSettings()
        {

        }

        public ProtocolSettings(MqttSettings mqttSettings, CoapSettings coapSettings)
        {
            Mqtt = mqttSettings;
            Coap = coapSettings;
        }

        [JsonProperty("mqtt")]
        public MqttSettings Mqtt { get; set; }

        [JsonProperty("coap")]
        public CoapSettings Coap { get; set; }
    }
}
