using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SkunkLab.Core.Utilities
{
    public class MessageUri : Uri
    {
        public MessageUri(HttpRequestMessage request)
            : base(request.RequestUri.ToString())
        {
            items = request.GetQueryNameValuePairs();
            Read(request);
        }

        public MessageUri(string uriString)
            : base(uriString)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            NameValueCollection nvc = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(uriString));

            for (int i = 0; i < nvc.Count; i++)
            {
                string key = nvc[i];
                string[] values = nvc.GetValues(i);
                foreach (string val in values)
                {
                    list.Add(new KeyValuePair<string, string>(key, val));
                }
            }

            items = list.ToArray();
            Read(uriString);
        }

        public string Resource { get; internal set; }
        public string ContentType { get; internal set; }
        public string MessageId { get; internal set; }
        public IEnumerable<KeyValuePair<string, string>> Indexes { get; internal set; }
        public IEnumerable<string> Subscriptions { get; internal set; }
        public string SecurityToken { get; internal set; }
        public string TokenType { get; internal set; }

        private IEnumerable<KeyValuePair<string, string>> items;

        private void Read(string uriString)
        {
            ContentType = GetSingleParameter(QueryStringConstants.CONTENT_TYPE);
            Resource = GetSingleParameter(QueryStringConstants.RESOURCE);
            TokenType = GetSingleParameter(QueryStringConstants.TOKEN_TYPE);
            SecurityToken = GetSingleParameter(QueryStringConstants.SECURITY_TOKEN);
            MessageId = GetSingleParameter(QueryStringConstants.MESSAGE_ID);
            Subscriptions = GetEnumerableParameters(QueryStringConstants.SUBSCRIPTION);
            Indexes = BuildIndexes(GetEnumerableParameters(QueryStringConstants.INDEX));
        }

        private void Read(HttpRequestMessage request)
        {
            ContentType = request.Content.Headers.ContentType.MediaType;
            Subscriptions = GetEnumerableHeaders(HttpHeaderConstants.SUBSCRIBE_HEADER, request);
            CheckUri(Subscriptions);
            IEnumerable<string> resources = GetEnumerableHeaders(HttpHeaderConstants.RESOURCE_HEADER, request);
            CheckUri(resources);
            SetResource(resources);
            IEnumerable<string> indexes = GetEnumerableHeaders(HttpHeaderConstants.INDEX_HEADER, request);
            Indexes = BuildIndexes(indexes);
            IEnumerable<string> messageIds = GetEnumerableHeaders(HttpHeaderConstants.MESSAGEID_HEADER, request);

            if (resources != null || resources.Count() == 1)
            {
                this.Resource = resources.First();
            }

            if (messageIds != null || messageIds.Count() == 1)
            {
                this.MessageId = messageIds.First();
            }

            string contentType = GetSingleParameter(QueryStringConstants.CONTENT_TYPE);
            string resource = GetSingleParameter(QueryStringConstants.RESOURCE);
            string tokenType = GetSingleParameter(QueryStringConstants.TOKEN_TYPE);
            string securityToken = GetSingleParameter(QueryStringConstants.SECURITY_TOKEN);
            string messageId = GetSingleParameter(QueryStringConstants.MESSAGE_ID);
            IEnumerable<string> subscriptions = GetEnumerableParameters(QueryStringConstants.SUBSCRIPTION);
            KeyValuePair<string, string>[] queryStringIndexes = BuildIndexes(GetEnumerableParameters(QueryStringConstants.INDEX));
           
            Resource = Resource ?? resource;
            Indexes = Indexes ?? queryStringIndexes;
            MessageId = MessageId ?? messageId;
            Subscriptions = Subscriptions ?? subscriptions;
            TokenType = TokenType ?? tokenType;
            SecurityToken = SecurityToken ?? securityToken;
            ContentType = ContentType ?? contentType;
        }

        private void SetResource(IEnumerable<string> resources)
        {
            if (resources == null)
            {
                return;
            }
            else if (resources.Count() > 1)
            {
                throw new IndexOutOfRangeException("Number of resources specified in request header must be 0 or 1.");
            }
            else
            {
                this.Resource = resources.First();
            }
        }
        
        private IEnumerable<string> GetEnumerableHeaders(string key, HttpRequestMessage request)
        {
            return request.Headers.GetValues(key);
        }
        private IEnumerable<string> GetEnumerableParameters(string key, HttpRequestMessage request)
        {
            IEnumerable<KeyValuePair<string, string>> kvps = request.GetQueryNameValuePairs();
            return from kv in kvps where kv.Key.ToLower(CultureInfo.InvariantCulture) == key.ToLower(CultureInfo.InvariantCulture) select kv.Value.ToLower(CultureInfo.InvariantCulture);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal KeyValuePair<string,string>[] BuildIndexes(IEnumerable<string> indexes)
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal void CheckUri(IEnumerable<string> uriStrings)
        {
            if (uriStrings == null)
            {
                return;
            }

            foreach (string uriString in uriStrings)
            {
                CheckUri(uriString);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal void CheckUri(string uriString)
        {
            if (string.IsNullOrEmpty(uriString))
            {
                return;
            }

            if (!Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
            {
                throw new UriFormatException("uriString");
            }
        }

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

    }
}
