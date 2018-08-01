using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRTU.ModBus
{
    [JsonObject]
    [Serializable]
    public class RtuMapItem
    {
        public RtuMapItem()
        {

        }

        public RtuMapItem(ushort unitId, string pubResource, string subResource)
        {
            this.UnitId = unitId;
            this.PublishResource = pubResource;
            this.SubscribeResource = subResource;
        }

        [JsonProperty("unitId")]

        public ushort UnitId { get; set; }

        [JsonProperty("publishResource")]
        public string PublishResource { get; set; }

        [JsonProperty("subscribeResource")]
        public string SubscribeResource { get; set; }

    }
}
