using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Piraeus.Configuration.Identity
{
    [ConfigurationCollection(typeof(ServiceClaimElement))]
    public class ServiceClaimsElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ServiceClaimElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServiceClaimElement)element).ClaimType;
        }

        public List<KeyValuePair<string, string>> GetServiceClaims()
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            int index = 0;
            while (index < base.Count)
            {
                ServiceClaimElement elem = (ServiceClaimElement)BaseGet(index);
                list.Add(new KeyValuePair<string, string>(elem.ClaimType, elem.Value));
                index++;
            }

            return list;
        }
    }
}
