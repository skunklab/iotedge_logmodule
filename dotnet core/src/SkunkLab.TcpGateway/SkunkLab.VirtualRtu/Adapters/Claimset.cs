using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace SkunkLab.VirtualRtu.Adapters
{
    [JsonObject]
    public class Claimset
    {
        public Claimset()
        {
            Claims = new List<GenericClaim>();
        }

        [JsonProperty("claims")]
        public List<GenericClaim> Claims { get; set; }


        public void AddClaim(string type, string value)
        {
            Claims.Add(new GenericClaim(type, value));
        }


        public IEnumerable<Claim> ToClaims()
        {
            List<Claim> list = new List<Claim>();
            foreach (var item in Claims)
            {
                list.Add(new Claim(item.ClaimType, item.Value));
            }

            if (list.Count > 0)
            {
                return list;
            }
            else
            {
                return null;
            }
        }
    }
}
