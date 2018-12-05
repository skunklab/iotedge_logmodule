using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class PiraeusConfig
    {
        public PiraeusConfig()
        {

        }

        public PiraeusConfig(ChannelSettings channels, ProtocolSettings protocols, IdentitySettings identity, SecuritySettings security)
        {
            Channels = channels;
            Protocols = protocols;
            Identity = identity;
            Security = security;
        }

        [JsonProperty("channels")]
        public ChannelSettings Channels { get; set; }

        [JsonProperty("protocols")]
        public ProtocolSettings Protocols { get; set; }

        [JsonProperty("identity")]
        public IdentitySettings Identity { get; set; }

        [JsonProperty("security")]
        public SecuritySettings Security { get; set; }
    }
}
