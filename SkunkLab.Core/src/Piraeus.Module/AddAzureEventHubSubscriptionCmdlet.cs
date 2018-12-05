using Piraeus.Core.Metadata;
using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusEventHubSubscription")]
    public class AddAzureEventHubSubscriptionCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource to subscribe.", Mandatory = true)]
        public string ResourceUriString;

        [Parameter(HelpMessage = "Account name of EventHub, e.g, <account>.servicebus.windows.net", Mandatory = true)]
        public string Account;

        [Parameter(HelpMessage = "Name of EventHub", Mandatory = true)]
        public string Hub;

        [Parameter(HelpMessage = "Name of key used for authentication.", Mandatory = true)]
        public string KeyName;

        [Parameter(HelpMessage = "(Optional) ID of partition if you want to send message to a single partition.", Mandatory = false)]
        public string PartitionId;

        [Parameter(HelpMessage = "Token used for authentication.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "Number of blob storage clients to use.", Mandatory = false)]
        public int NumClients;


        [Parameter(HelpMessage = "Description of the subscription.", Mandatory = false)]
        public string Description;

        protected override void ProcessRecord()
        {
            string uriString = String.Format("eh://{0}.servicebus.windows.net?hub={1}&keyname={2}&clients={3}", Account, Hub, KeyName, NumClients <= 0 ? 1 : NumClients);

            if(PartitionId != null)
            {
                uriString = String.Format("{0}&partitionid={1}", uriString, PartitionId);
            }

            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                IsEphemeral = false,
                NotifyAddress = uriString,
                SymmetricKey = Key,
                Description = this.Description
            };

            string url = String.Format("{0}/api2/resource/subscribe?resourceuristring={1}", ServiceUrl, ResourceUriString);
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, SecurityToken);
            RestRequest request = new RestRequest(builder);

            string subscriptionUriString = request.Post<SubscriptionMetadata, string>(metadata);

            WriteObject(subscriptionUriString);
        }



    }
}
