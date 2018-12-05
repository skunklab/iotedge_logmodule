using System.Configuration;

namespace Piraeus.Configuration.Security
{
    public class SymmetricKeyElement : ConfigurationElement
    {
        [ConfigurationProperty("securityTokenType", DefaultValue = "JWT")]
        public string SecurityTokenType
        {
            get { return (string)base["securityTokenType"]; }
            set { base["securityTokenType"] = value; }
        }

        [ConfigurationProperty("symmetricKey")]
        public string SharedKey
        {
            get { return (string)base["symmetricKey"]; }
            set { base["symmetricKey"] = value; }
        }

        [ConfigurationProperty("issuer", IsRequired = false)]
        public string Issuer
        {
            get { return (string)base["issuer"]; }
            set { base["issuer"] = value; }
        }

        [ConfigurationProperty("audience", IsRequired = false)]
        public string Audience
        {
            get { return (string)base["audience"]; }
            set { base["audience"] = value; }
        }
    }
}
