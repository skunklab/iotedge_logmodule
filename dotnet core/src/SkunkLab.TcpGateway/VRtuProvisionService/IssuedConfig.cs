using Newtonsoft.Json;
using System;

namespace VRtuProvisionService
{
    [Serializable]
    [JsonObject]
    public class IssuedConfig
    {

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("securityToken")]
        public string SecurityToken { get; set; }

        [JsonProperty("pskIdentity")]
        public string PskIdentity { get; set; }

        [JsonProperty("psk")]
        public string PSK { get; set; }

        [JsonProperty("resources")]
        public ResourceItem Resources { get; set; }
    }
}
