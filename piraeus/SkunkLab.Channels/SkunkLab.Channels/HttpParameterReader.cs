//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Net.Http;
//using SkunkLab.Channels.Http;

//namespace SkunkLab.Channels
//{
//    public class HttpParameterReader : ParameterReader
//    {
//        public HttpParameterReader(HttpRequestMessage request)
//        {
//            this.request = request;
//        }

//        private HttpRequestMessage request;

//        public override void Read()
//        {
//            this.ContentType = request.Content.Headers.ContentType.MediaType;
//            this.Subscriptions = GetEnumerableHeaders(HttpChannelConstants.SUBSCRIBE_HEADER);
//            CheckUri(this.Subscriptions);
//            IEnumerable<string> resources = GetEnumerableHeaders(HttpChannelConstants.RESOURCE_HEADER);
//            CheckUri(resources);
//            SetResource(resources);
//            IEnumerable<string> indexes = GetEnumerableHeaders(HttpChannelConstants.INDEX_HEADER);
//            BuildIndexes(indexes);
//            IEnumerable<string> messageIds = GetEnumerableHeaders(HttpChannelConstants.MESSAGEID_HEADER);

//            if(resources != null || resources.Count() == 1)
//            {
//                this.Resource = resources.First();
//            }

//            if(messageIds != null || messageIds.Count() == 1)
//            {
//                this.MessageId = messageIds.First();
//            }

//            UriParameterReader ureader = new UriParameterReader(request.RequestUri.ToString());
//            ureader.Read();

//            this.Resource = this.Resource ?? ureader.Resource;
//            this.Indexes = this.Indexes ?? ureader.Indexes;
//            this.MessageId = this.MessageId ?? ureader.MessageId;
//            this.Subscriptions = this.Subscriptions ?? ureader.Subscriptions;
//        }

//        private void SetMessageId(IEnumerable<string> messageIds)
//        {
//            if (messageIds == null)
//            {
//                return;
//            }
//            else if (messageIds.Count() > 1)
//            {
//                throw new IndexOutOfRangeException("Number of message ids specified in request header must be 0 or 1.");
//            }
//            else
//            {
//                this.MessageId = messageIds.First();
//            }
//        }

//        private void SetResource(IEnumerable<string> resources)
//        {
//            if(resources == null)
//            {
//                return;
//            }
//            else if(resources.Count() > 1)
//            {
//                throw new IndexOutOfRangeException("Number of resources specified in request header must be 0 or 1.");
//            }
//            else
//            {
//                this.Resource = resources.First();
//            }
//        }

//        private IEnumerable<string> GetEnumerableHeaders(string key)
//        {
//            return request.Headers.GetValues(key);
//        }        

//        private IEnumerable<string> GetEnumerableParameters(string key)
//        {
//            IEnumerable<KeyValuePair<string, string>> kvps = request.GetQueryNameValuePairs();
//            return from kv in kvps where kv.Key.ToLower(CultureInfo.InvariantCulture) == key.ToLower(CultureInfo.InvariantCulture) select kv.Value.ToLower(CultureInfo.InvariantCulture);
//        }

        


//    }
//}
