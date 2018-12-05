using Piraeus.Core.Metadata;
using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusCosmosDbSubscription")]
    public class AddAzureCosmosDbSubscriptonCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource to subscribe.", Mandatory = true)]
        public string ResourceUriString;

        [Parameter(HelpMessage = "Account name of CosmosDb, e.g, <account>.documents.azure.com:443", Mandatory = true)]
        public string Account;

        [Parameter(HelpMessage = "Name of database.", Mandatory = true)]
        public string Database;

        [Parameter(HelpMessage = "Name of collection.", Mandatory = true)]
        public string Collection;

        [Parameter(HelpMessage = "CosmosDb read-write key", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "Number of blob storage clients to use.", Mandatory = false)]
        public int NumClients;       

        [Parameter(HelpMessage = "Description of the subscription.", Mandatory = false)]
        public string Description;

        protected override void ProcessRecord()
        {
            string uriString = String.Format("https://{0}.documents.azure.com:443?database={1}&collection={2}&clients={3}", Account, Database, Collection, NumClients <= 0 ? 1 : NumClients);

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
