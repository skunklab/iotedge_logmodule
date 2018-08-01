using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace HackHarness
{
    [JsonObject]
    public class GenericClaimset
    {
        public GenericClaimset()
        {
            Claims = new List<Claim>();
        }

        [JsonProperty("claims")]
        public List<Claim> Claims { get; set; }
    }
}
