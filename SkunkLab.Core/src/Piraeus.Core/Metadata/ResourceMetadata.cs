using System;
using Newtonsoft.Json;

namespace Piraeus.Core.Metadata
{
    [Serializable]
    [JsonObject]
    public class ResourceMetadata
    {
        public ResourceMetadata()
        {
        }

        //private bool _enabled = true;

        [JsonProperty("resourceUriString")]
        public string ResourceUriString { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("discoveryUrl")]
        public string DiscoveryUrl { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("expires")]
        public DateTime? Expires { get; set; }

        [JsonProperty("maxSubscriptionDuration")]
        public TimeSpan? MaxSubscriptionDuration { get; set; }

        [JsonProperty("audit")]
        public bool Audit { get; set; }

        [JsonProperty("requireEncryptedChannel")]
        public bool RequireEncryptedChannel { get; set; }

        [JsonProperty("publishPolicyUriString")]
        public string PublishPolicyUriString { get; set; }

        [JsonProperty("subscribePolicyUriString")]
        public string SubscribePolicyUriString { get; set; }
    }
}
