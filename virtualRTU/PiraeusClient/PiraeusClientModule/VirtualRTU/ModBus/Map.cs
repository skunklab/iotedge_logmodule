using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRTU.ModBus
{
    [JsonObject]
    [Serializable]
    public class Map
    {
        [JsonProperty("unitId")]
        public ushort UnitId { get; set; }

        [JsonProperty("publishResource")]
        public string PublishResource { get; set; }

        [JsonProperty("subscribeResource")]
        public string SubscribeResource { get; set; }
    }
}
