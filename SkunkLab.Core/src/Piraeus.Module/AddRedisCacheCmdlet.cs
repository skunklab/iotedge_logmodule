using Piraeus.Core.Metadata;
using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusRedisCacheSubscription")]
    public class AddRedisCacheCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource to subscribe.", Mandatory = true)]
        public string ResourceUriString;

        [Parameter(HelpMessage = "Azure Redis account, e.g., <account>.redis.cache.windows.net")]
        public string Account;

        [Parameter(HelpMessage = "Redis security key.", Mandatory = true)]
        public string SecurityKey;

        [Parameter(HelpMessage = "Optional Redis database number to use for the cache.  If omitted, will use the default database", Mandatory = false)]
        public int DatabaseNum;

        [Parameter(HelpMessage = "Optional expiry of a cached item.", Mandatory = false)]
        public TimeSpan? Expiry;

        [Parameter(HelpMessage = "Optional claim type for the identity used as the cache key.  If omitted, the resource URI query string must contain cachekey parameter and value to set the key.  If query string parameter is used it will override the claim type.")]
        public string ClaimType;

        [Parameter(HelpMessage = "Description of the subscription.", Mandatory = false)]
        public string Description;

        protected override void ProcessRecord()
        {            

            string uriString = String.Format("redis://{0}.redis.cache.windows.net", Account);

            if(DatabaseNum >= 0 && Expiry.HasValue)
            {
                uriString = String.Format("{0}?db={1}&expiry={2}", uriString, DatabaseNum, Expiry.ToString());
            }
            else if(DatabaseNum >= 0)
            {
                uriString = String.Format("{0}?db={1}", uriString, DatabaseNum);
            }
            else if(Expiry.HasValue)
            {
                uriString = String.Format("{0}?expiry={1}", uriString, Expiry.ToString());
            }
            else
            {
                //using defauls for database and cached item will never expire
            }

            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                IsEphemeral = false,
                NotifyAddress = uriString,
                Description = this.Description,
                SymmetricKey = SecurityKey
            };

            if(!string.IsNullOrEmpty(ClaimType))
            {
                metadata.ClaimKey = ClaimType.ToLowerInvariant();
            }

            string url = String.Format("{0}/api2/resource/subscribe?resourceuristring={1}", ServiceUrl, ResourceUriString);
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, SecurityToken);
            RestRequest request = new RestRequest(builder);

            string subscriptionUriString = request.Post<SubscriptionMetadata, string>(metadata);

            WriteObject(subscriptionUriString);
        }
    }
}
