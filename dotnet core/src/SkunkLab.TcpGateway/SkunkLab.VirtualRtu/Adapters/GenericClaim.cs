using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkunkLab.VirtualRtu.Adapters
{
    [JsonObject]
    public class GenericClaim
    {
        public GenericClaim()
        {

        }

        public GenericClaim(string claimType, string value)
        {
            ClaimType = claimType;
            Value = value;
        }

        [JsonProperty("claimType")]
        public string ClaimType { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

    }
}
