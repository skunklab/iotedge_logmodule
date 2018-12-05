using System.Configuration;

namespace Piraeus.Configuration.Identity
{
    public class IndexElement : ConfigurationElement
    {
        [ConfigurationProperty("indexName", IsKey = true)]
        public string IndexName
        {
            get { return (string)base["indexName"]; }
            set { base["indexName"] = value; }
        }

        [ConfigurationProperty("claimType")]
        public string ClaimType
        {
            get { return (string)base["claimType"]; }
            set { base["claimType"] = value; }
        }

        
    }
}
