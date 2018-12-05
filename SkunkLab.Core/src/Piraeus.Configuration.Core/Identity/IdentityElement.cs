using System.Configuration;

namespace Piraeus.Configuration.Identity
{
    public class IdentityElement : ConfigurationElement
    {
        [ConfigurationProperty("client")]
        public ClientElement Client
        {
            get { return (ClientElement)base["client"]; }
            set { base["client"] = value; }
        }

        [ConfigurationProperty("service")]
        public ServiceElement Service
        {
            get { return (ServiceElement)base["service"]; }
            set { base["service"] = value; }
        }
    }
}
