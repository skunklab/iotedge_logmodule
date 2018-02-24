//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Globalization;
//using System.Linq;
//using System.Net.Http;
//using System.Web;

//namespace SkunkLab.Channels
//{
//    public class UriParameterReader : ParameterReader
//    {
//        public UriParameterReader(HttpRequestMessage request)
//        {
//            items = request.GetQueryNameValuePairs();
//        }

//        public UriParameterReader(string uriString)
//        {            
//            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
//            NameValueCollection nvc = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(uriString));
            
//            for(int i=0;i<nvc.Count;i++)
//            {
//                string key = nvc[i];
//                string[] values = nvc.GetValues(i);
//                foreach(string val in values)
//                {
//                    list.Add(new KeyValuePair<string, string>(key, val));
//                }
//            }

//            items = list.ToArray();
//        }

//        private IEnumerable<KeyValuePair<string, string>> items;

//        public override void Read()
//        {
//            //read the query string
//            this.ContentType = GetSingleParameter(QueryStringConstants.CONTENT_TYPE);
//            this.Resource = GetSingleParameter(QueryStringConstants.RESOURCE);
//            this.TokenType = GetSingleParameter(QueryStringConstants.TOKEN_TYPE);
//            this.SecurityToken = GetSingleParameter(QueryStringConstants.SECURITY_TOKEN);
//            this.MessageId = GetSingleParameter(QueryStringConstants.MESSAGE_ID);
//            this.Subscriptions = GetEnumerableParameters(QueryStringConstants.SUBSCRIPTION);
//            IEnumerable<string> indexes = GetEnumerableParameters(QueryStringConstants.INDEX);
//            BuildIndexes(indexes);
//        }
                

//        private IEnumerable<string> GetEnumerableParameters(string key)
//        {
//            return from kv in items where kv.Key.ToLower(CultureInfo.InvariantCulture) == key.ToLower(CultureInfo.InvariantCulture) select kv.Value.ToLower(CultureInfo.InvariantCulture);
//        }

//        private string GetSingleParameter(string key)
//        {
//            IEnumerable<string> parameters = GetEnumerableParameters(key);

//            if(parameters.Count() > 1)
//            {
//                throw new IndexOutOfRangeException(key);
//            }

//            return parameters.Count() == 0 ? null : parameters.First();
//        }
//    }
//}
