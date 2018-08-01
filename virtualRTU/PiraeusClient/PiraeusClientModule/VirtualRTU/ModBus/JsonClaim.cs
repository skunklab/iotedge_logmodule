using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRTU.ModBus
{
    [JsonObject]
    [Serializable]
    public class JsonClaim
    {
        public JsonClaim()
        {

        }

        public JsonClaim(string type, string value)
        {
            this.Type = type;
            this.Value = value;
        }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
