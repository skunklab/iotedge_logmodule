using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Claims;

namespace Piraeus.Module
{
    [JsonObject]
    public class IdentityConfig
    {
        public IdentityConfig()
        {
        }

        [JsonProperty("claims")]
        public List<Claim> Claims { get; set; }

        [JsonProperty("certificate")]
        public byte[] Certificate { get; set; }
    }
}
