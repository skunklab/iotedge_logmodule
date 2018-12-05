using System.Configuration;

namespace Piraeus.Configuration.Identity
{
    public class ServiceClaimElement : ConfigurationElement
    {
        [ConfigurationProperty("claimType", IsKey =true)]
        public string ClaimType
        {
            get { return (string)base["claimType"]; }
            set { base["claimType"] = value; }
        }

        [ConfigurationProperty("value")]
        public string Value
        {
            get { return (string)base["value"]; }
            set { base["value"] = value; }
        }
    }
}
