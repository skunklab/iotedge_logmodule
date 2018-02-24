

namespace Piraeus.Protocols.Coap
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class CoapOptionCollection : IList<CoapOption>
    {
        public CoapOptionCollection()
        {
            list = new List<CoapOption>();
        }

        public CoapOptionCollection(IEnumerable<CoapOption> options)
        {
            list = new List<CoapOption>(options);
        }

        
        private List<CoapOption> list;
        public int IndexOf(CoapOption item)
        {            
            return list.IndexOf(item);
        }

        public void Insert(int index, CoapOption item)
        {
            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public CoapOption this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index] = value;
            }
        }

        public void Add(CoapOption item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(CoapOption item)
        {
            return list.Contains(item);
        }

        public void CopyTo(CoapOption[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(CoapOption item)
        {
            return list.Remove(item);
        }

        public IEnumerator<CoapOption> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public bool ContainsContentFormat()
        {
            foreach(CoapOption item in this.list)
            {
                if(item.Type == OptionType.ContentFormat)
                {
                    return true;
                }
            }

            return false;
        }

        public string GetContainFormat()
        {
            foreach (CoapOption item in this.list)
            {
                if (item.Type == OptionType.ContentFormat)
                {
                    return (string)item.Value;
                }
            }

            return null;
        }


        public Uri GetResourceUri()
        {
            UriBuilder builder = new UriBuilder();
            StringBuilder pathBuilder = new StringBuilder();
            StringBuilder queryBuilder = new StringBuilder();
            

            foreach(CoapOption item in this.list)
            {
                if(item.Type == OptionType.UriHost)
                {
                    builder.Scheme = "Coaps";
                    builder.Host = item.Value as string;
                }

                if(item.Type == OptionType.UriPort)
                {
                    builder.Port = Convert.ToInt32(item.Value);
                }

                if(item.Type == OptionType.UriPath)
                {
                    pathBuilder.Append(item.Value as string);
                    pathBuilder.Append("/");
                }

                if(item.Type == OptionType.UriQuery)
                {
                    queryBuilder.Append(item.Value as string);
                    queryBuilder.Append("&");
                }
            }

            string path = pathBuilder.ToString();
            string query = queryBuilder.ToString();

            if(path.Length > 0)
            {
                builder.Path = "/" + path.Substring(0, path.Length - 1);
            }

            if(query.Length > 0)
            {
                builder.Query = query.Substring(0, query.Length - 1);
            }

            return builder.Scheme == "http" ? null : builder.Uri;
        }

        

        public object GetOptionValue(OptionType type)
        {
            foreach (CoapOption op in this.list)
            {
                if (op.Type == type)
                {
                    return op.Value;
                }
            }

            return null;
        }

        public object[] GetOptionValues(OptionType type)
        {
            List<object> options = new List<object>();

            foreach(CoapOption op in this.list)
            {
                if(op.Type == type)
                {
                    options.Add(op.Value);
                }
            }

            if(options.Count > 0)
            {
                return options.ToArray();
            }
            else
            {                
                return null;
            }
        }
    }
}
