using System.Collections.Generic;
using System.Configuration;

namespace Piraeus.Configuration.Identity
{
    [ConfigurationCollection(typeof(IndexElement))]
    public class IndexElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new IndexElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IndexElement)element).IndexName;
        }

        public List<KeyValuePair<string,string>> GetIndexes()
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            int index = 0;
            while(index < base.Count)
            {
                IndexElement elem = (IndexElement)BaseGet(index);
                list.Add(new KeyValuePair<string, string>(elem.ClaimType, elem.IndexName));
                index++;
            }

            return list;
        }
    }
}
