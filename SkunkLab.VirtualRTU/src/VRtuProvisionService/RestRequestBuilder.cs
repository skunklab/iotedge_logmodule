using System;
using System.Net;

namespace VRtuProvisionService
{
    public class RestRequestBuilder
    {
        public RestRequestBuilder(string method, string url, string contentType, bool zeroLength, string securityToken = null)
        {
            this.Method = method;
            this.ContentType = contentType;
            this.BaseUrl = url;
            this.IsZeroContentLength = zeroLength;
            this.SecurityToken = securityToken;
        }

        public RestRequestBuilder(string method, string url, string contentType, string securityKey)
        {
            this.Method = method;
            this.ContentType = contentType;
            this.BaseUrl = url;
            this.IsZeroContentLength = true;
            this.SecurityKey = securityKey;
        }
        public string ContentType { get; internal set; }
        public string Method { get; internal set; }
        public string BaseUrl { get; internal set; }
        public bool IsZeroContentLength { get; internal set; }

        public string SecurityToken { get; internal set; }

        public string SecurityKey { get; internal set; }

        public HttpWebRequest BuildRequest()
        {
            HttpWebRequest request = null;

            if (!string.IsNullOrEmpty(this.SecurityKey))
            {
                string url = String.Format("{0}?key={1}", this.BaseUrl, this.SecurityKey);
                request = (HttpWebRequest)HttpWebRequest.Create(url);
            }
            else
            {
                request = (HttpWebRequest)HttpWebRequest.Create(this.BaseUrl);
            }

            request.ContentType = this.ContentType;
            request.Method = this.Method;

            if (this.IsZeroContentLength)
            {
                request.ContentLength = 0;
            }

            if (!string.IsNullOrEmpty(this.SecurityToken))
            {
                request.Headers.Add("Authorization", String.Format("Bearer {0}", this.SecurityToken));
            }

            return request;
        }
    }
}
