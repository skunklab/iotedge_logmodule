using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Configuration.Security
{
    public class SecurityElement : ConfigurationElement
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
