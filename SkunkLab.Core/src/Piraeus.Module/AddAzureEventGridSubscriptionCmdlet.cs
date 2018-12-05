using Piraeus.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusEventGridSubscription")]
    public class AddAzureEventGridSubscriptionCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource to subscribe.", Mandatory = true)]
        public string ResourceUriString;

        [Parameter(HelpMessage = "Key for EventGrid topic access.", Mandatory = true)]
        public string TopicKey;

        [Parameter(HelpMessage = "Host name of EventGrid, e.g., piraeussampletopic.eastus-1.eventgrid.azure.net", Mandatory = true)]
        public string Host;

        [Parameter(HelpMessage = "Number of blob storage clients to use.", Mandatory = false)]
        public int NumClients;

        [Parameter(HelpMessage = "Description of the subscription.", Mandatory = false)]
        public string Description;

        protected override void ProcessRecord()
        {
            string uriString = String.Format("eventgrid://{0}?clients={1}", Host, NumClients <= 0 ? 1 : NumClients);


            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                IsEphemeral = false,
                NotifyAddress = uriString,
                SymmetricKey = TopicKey,
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
