using System.Configuration;
using Piraeus.Configuration.Security;

namespace Piraeus.Configuration.Channels
{
    public class X509Element : AsymmetricKeyElement
    {
        [ConfigurationProperty("authenticate")]
        public bool AuthenticateServer
        {
            get { return (bool)base["authenticate"]; }
            set { base["authenticate"] = value; }
        }
    }
}
