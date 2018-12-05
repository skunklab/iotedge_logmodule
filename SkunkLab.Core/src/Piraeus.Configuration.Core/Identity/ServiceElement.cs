using System.Configuration;

namespace Piraeus.Configuration.Identity
{
    public class ServiceElement : ConfigurationElement
    {
        [ConfigurationProperty("claims")]
        public ServiceClaimsElementCollection Claims
        {
            get { return (ServiceClaimsElementCollection)base["claims"]; }
            set { base["claims"] = value; }
        }

        




    }
}
