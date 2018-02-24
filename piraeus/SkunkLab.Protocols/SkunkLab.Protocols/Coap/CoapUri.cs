using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;
using SkunkLab.Protocols.Utilities;

namespace SkunkLab.Protocols.Coap
{
    public class CoapUri : Uri
    {        
        public CoapUri(string uriString)
            : base(uriString, UriKind.Absolute)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            NameValueCollection nvc = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(uriString)).Query);

            for (int i = 0; i < nvc.Count; i++)
            {
                string key = nvc.Keys[i];
                string[] values = nvc.GetValues(i);
                foreach (string val in values)
                {
                    list.Add(new KeyValuePair<string, string>(key, val));
                }
            }

            items = list.ToArray();

            //ContentType = GetSingleParameter(QueryStringConstants.CONTENT_TYPE);
            MessageId = GetSingleParameter(QueryStringConstants.MESSAGE_ID);
            Resource = GetSingleParameter(QueryStringConstants.RESOURCE);
            TokenType = GetSingleParameter(QueryStringConstants.TOKEN_TYPE);
            SecurityToken = GetSingleParameter(QueryStringConstants.SECURITY_TOKEN);
            
            //Subscriptions = GetEnumerableParameters(QueryStringConstants.SUBSCRIPTION);
            Indexes = BuildIndexes(GetEnumerableParameters(QueryStringConstants.INDEX));

        }

        public string Resource { get; internal set; }
        public string ContentType { get; internal set; }
        public string MessageId { get; internal set; }
        public IEnumerable<KeyValuePair<string, string>> Indexes { get; internal set; }
        public IEnumerable<string> Subscriptions { get; internal set; }
        public string SecurityToken { get; internal set; }
        public string TokenType { get; internal set; }

        private IEnumerable<KeyValuePair<string, string>> items;

        private IEnumerable<string> GetEnumerableParameters(string key)
        {
            return from kv in items where kv.Key.ToLower(CultureInfo.InvariantCulture) == key.ToLower(CultureInfo.InvariantCulture) select kv.Value.ToLower(CultureInfo.InvariantCulture);
        }

        private string GetSingleParameter(string key)
        {
            IEnumerable<string> parameters = GetEnumerableParameters(key);

            if (parameters.Count() > 1)
            {
                throw new IndexOutOfRangeException(key);
            }

            return parameters.Count() == 0 ? null : parameters.First();
        }

        private KeyValuePair<string, string>[] BuildIndexes(IEnumerable<string> indexes)
        {
            List<KeyValuePair<string, string>> indexList = new List<KeyValuePair<string, string>>();
            foreach (string index in indexes)
            {
                string[] parts = index.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    throw new IndexOutOfRangeException("indexes");
                }
                else
                {
                    indexList.Add(new KeyValuePair<string, string>(parts[0], parts[1]));
                }
            }

            return indexList.Count > 0 ? indexList.ToArray() : null;

        }



    }
}
