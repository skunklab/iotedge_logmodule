
namespace SkunkLab.Protocols.Coap
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web;

    [Serializable]
    public class CoapUri2 : Uri
    {
        private Dictionary<string, string> queryParameters;
        public CoapUri2(string uriString)
            : base(uriString.ToLower(CultureInfo.InvariantCulture))
        {
            if (this.Scheme.ToLower(CultureInfo.InvariantCulture) != "coaps")
            {
                throw new UriFormatException("Invalid CoAP resource URI, scheme not 'coaps'.");
            }

            this.NormalizedPath = this.LocalPath.ToLower(CultureInfo.InvariantCulture).IndexOf('/') == 0 ? this.LocalPath.ToLower(CultureInfo.InvariantCulture).Substring(1, this.LocalPath.Length - 1) : this.LocalPath;

            if (this.NormalizedPath != "publish" && this.NormalizedPath != "subscribe" && this.NormalizedPath != "unsubscribe" && this.NormalizedPath != "renew" && this.NormalizedPath != "getstatus")
            {
                throw new UriFormatException("Invalid CoAP resource path.");
            }

            this.ServiceType = (CoapServiceType)Enum.Parse(typeof(CoapServiceType), this.NormalizedPath, true);

            NameValueCollection nvc = HttpUtility.ParseQueryString(this.Query);

            this.queryParameters = new Dictionary<string, string>();
            int index = 0;
            while (index < nvc.Count)
            {
                string key = nvc.GetKey(0);
                string value = nvc[key];
                queryParameters.Add(key, value);
                index++;
            }

            this.QueryParameters = queryParameters;

            if (this.QueryParameters.ContainsKey("subscription"))
            {
                this.Subscription = this.QueryParameters["subscription"];
            }

            if (this.QueryParameters.ContainsKey("topic"))
            {
                this.Topic = this.QueryParameters["topic"];
            }

            if (this.QueryParameters.ContainsKey("ttl"))
            {
                this.TTL = TimeSpan.Parse(this.QueryParameters["ttl"]);
            }

            if (this.QueryParameters.ContainsKey("expires"))
            {
                TimeSpan expiry = TimeSpan.Zero;
                DateTime expires = DateTime.MinValue;
                if (TimeSpan.TryParse(this.QueryParameters["expires"], out expiry))
                {
                    this.Expires = DateTime.UtcNow.Add(expiry);
                }
                else if(DateTime.TryParse(this.QueryParameters["expires"], out expires))
                {
                    this.Expires = DateTime.Parse(this.QueryParameters["expires"]);
                }
                else
                {
                    throw new UriFormatException("Expires value is not formatted as a TimeSpan or DateTime.");
                }
            }

            if(this.QueryParameters.ContainsKey("tokentype"))
            {
                this.TokenType = this.QueryParameters["tokentype"];
            }

            if(this.QueryParameters.ContainsKey("token"))
            {
                this.Token = this.QueryParameters["token"];
            }
        }        

        public string Topic { get; internal set; }
        public string Subscription { get; internal set; }
        public TimeSpan? TTL { get; internal set; }
        public DateTime? Expires { get; internal set; }
        
        public string TokenType { get; internal set; }

        public string Token { get; internal set; }
        public string NormalizedPath { get; internal set; }
        public Dictionary<string, string> QueryParameters { get; internal set; }
        public CoapServiceType ServiceType { get; internal set; }

        
    }
}
