using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Configuration.Security
{
    public class AsymmetricKeyElement : ConfigurationElement
    {
        [ConfigurationProperty("store", IsRequired =true)]
        public string Store
        {
            get { return (string)base["store"]; }
            set { base["store"] = value; }
        }

        [ConfigurationProperty("location", IsRequired = true)]
        public string Location
        {
            get { return (string)base["location"]; }
            set { base["location"] = value; }
        }

        [ConfigurationProperty("thumbprint", IsRequired = true)]
        public string Thumbprint
        {
            get { return (string)base["thumbprint"]; }
            set { base["thumbprint"] = value; }
        }
            

    }
}
