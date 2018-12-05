using System.Configuration;

namespace Piraeus.Configuration.Security
{
    public class ServiceElement : ConfigurationElement
    {
        [ConfigurationProperty("asymmetricKey", IsRequired =false)]
        public AsymmetricKeyElement AsymmetricKey
        {
            get { return (AsymmetricKeyElement)base["asymmetricKey"]; }
            set { base["asymmetricKey"] = value; }
        }
        
    }
}
