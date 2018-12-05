using System;
using System.Collections.Generic;
using System.Management.Automation;
using Piraeus.Core.Metadata;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusSubscription")]
    public class SubscribeCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource to subscribe.", Mandatory = true)]
        public string ResourceUriString;

        [Parameter(HelpMessage = "Security identity from claims; required for actively connected subsystems; otherwise omit.", Mandatory = false)]
        public string Identity;

        [Parameter(HelpMessage = "List of key/value indexes for the subscription.", Mandatory = false)]
        public List<KeyValuePair<string, string>> Indexes;

        [Parameter(HelpMessage = "Required for passively connected subsystems; otherwise omit.", Mandatory = false)]
        public string NotifyAddress;

        [Parameter(HelpMessage = "Type of security token used for passively connected subsystem; otherwise omit.", Mandatory = false)]
        public SecurityTokenType? TokenType;

        [Parameter(HelpMessage = "Symmetric key if a passively connection subsystem that uses SWT or JWT tokens; otherwise omit.", Mandatory = false)]
        public string SymmetricKey;

        [Parameter(HelpMessage = "Expiration of the subscription.", Mandatory = false)]
        public DateTime? Expires;

        [Parameter(HelpMessage = "Time-To-Live for retained messages.", Mandatory = false)]
        public TimeSpan? TTL;

        [Parameter(HelpMessage = "The rate retained messages are sent when the subsystem reconnects.", Mandatory = false)]
        public TimeSpan? SpoolRate;

        [Parameter(HelpMessage = "Durably persist messages for the TTL when the subsystem is disconnected.", Mandatory = false)]
        public bool DurableMessaging;

        protected override void ProcessRecord()
        {

            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                Identity = this.Identity,
                Indexes = this.Indexes,
                NotifyAddress = this.NotifyAddress,
                TokenType = this.TokenType,
                SymmetricKey = this.SymmetricKey,
                Expires = this.Expires,
                TTL = this.TTL,
                SpoolRate = this.SpoolRate,
                DurableMessaging = this.DurableMessaging
            };

            string url = String.Format("{0}/api2/resource/subscribe?resourceuristring={1}", ServiceUrl, ResourceUriString);
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, SecurityToken);
            RestRequest request = new RestRequest(builder);

            string subscriptionUriString = request.Post<SubscriptionMetadata, string>(metadata);

            WriteObject(subscriptionUriString);
        }
    }
}
