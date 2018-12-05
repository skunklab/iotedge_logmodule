using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Configuration.Identity
{
    public class ClientElement : ConfigurationElement
    {
        /// <summary>
        /// Unique identity 'name' claim
        /// </summary>
        [ConfigurationProperty("claimType", IsRequired =true)]
        public string IdentityClaimType
        {
            get { return (string)base["claimType"]; }
            set { base["claimType"] = value; }
        }

        [ConfigurationProperty("indexes", IsRequired = false)]
        public IndexElementCollection Indexes
        {
            get { return (IndexElementCollection)base["indexes"]; }
            set { base["indexes"] = value; }
        }


    }
}
