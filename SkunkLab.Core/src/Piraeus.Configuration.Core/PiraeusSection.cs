using System.Configuration;
using Piraeus.Configuration.Channels;
using Piraeus.Configuration.Identity;
using Piraeus.Configuration.Protocols;
using Piraeus.Configuration.Security;

namespace Piraeus.Configuration
{
    public class PiraeusSection : ConfigurationSection
    {
        [ConfigurationProperty("channels")]
        public ChannelsElement Channels
        {
            get { return (ChannelsElement)base["channels"]; }
            set { base["channels"] = value; }
        }

       [ConfigurationProperty("protocols", IsRequired =true)]
       public ProtocolsElement Protocols
        {
            get { return (ProtocolsElement)base["protocols"]; }
            set { base["protocols"] = value; }
        }

        [ConfigurationProperty("identity", IsRequired =true)]
        public IdentityElement Identity
        {
            get { return (IdentityElement)base["identity"]; }
            set { base["identity"] = value; }
        }

        [ConfigurationProperty("security", IsRequired =true)]
        public SecurityElement Security
        {
            get { return (SecurityElement)base["security"]; }
            set { base["security"] = value; }
        }
    }
}
