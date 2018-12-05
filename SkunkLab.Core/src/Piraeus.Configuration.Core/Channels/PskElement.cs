using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Configuration.Channels
{
    public class PskElement  : ConfigurationElement
    {
        [ConfigurationProperty("identity")]
        public string Identity
        {
            get { return (string)base["identity"]; }
            set { base["identity"] = value; }
        }

        [ConfigurationProperty("key")]
        public string Key
        {
            get { return (string)base["key"]; }
            set { base["key"] = value; }
        }
    }
}
