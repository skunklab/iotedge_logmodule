using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Configuration.Channels
{
    [ConfigurationCollection(typeof(PskElement))]
    public class PskCollectionElement : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new PskElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PskElement)element).Identity;
        }

        public Dictionary<string, byte[]> GetPresharedKeys()
        {
            Dictionary<string, byte[]> dict = new Dictionary<string, byte[]>();
            int index = 0;
            while (index < base.Count)
            {
                PskElement elem = (PskElement)BaseGet(index);
                byte[] key = Convert.FromBase64String(elem.Key);
                dict.Add(elem.Identity, key);
                index++;
            }

            return dict.Count > 0 ? dict : null;
        }

    }
}
