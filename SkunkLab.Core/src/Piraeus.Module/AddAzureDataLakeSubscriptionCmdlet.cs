using Piraeus.Core.Metadata;
using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusDataLakeSubscription")]
    public class AddAzureDataLakeSubscriptionCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource to subscribe.", Mandatory = true)]
        public string ResourceUriString;

        [Parameter(HelpMessage = "Azure Data Lake Store Account", Mandatory = true)]
        public string Account;

        [Parameter(HelpMessage = "AAD, e.g, microsoft.onmicrosoft.com", Mandatory = true)]
        public string Domain;

        [Parameter(HelpMessage = "Application ID for access from AAD.", Mandatory = true)]
        public string AppId;

        [Parameter(HelpMessage = "Secret for access from AAD", Mandatory = true)]
        public string ClientSecret;        

        [Parameter(HelpMessage = "Name of folder to write data.", Mandatory = true)]
        public string Folder;

        [Parameter(HelpMessage = "Name of filename to write data, but exclusive of an extension.", Mandatory = false)]
        public string Filename;

        [Parameter(HelpMessage = "Number of blob storage clients to use.", Mandatory = false)]
        public int NumClients;

        [Parameter(HelpMessage = "Description of the subscription.", Mandatory = false)]
        public string Description;

        protected override void ProcessRecord()
        {
            string uriString = Filename == null ? String.Format("adl://{0}.azuredatalakestore.net?domain={1}&appid={2}&folder={3}&clients={4}", Account, Domain, AppId, Folder, NumClients <= 0 ? 1 : NumClients) : String.Format("adl://{0}.azuredatalakestore.net?domain={1}&appid={2}&folder={3}&file={4}&clients={5}", Account, Domain, AppId, Folder, Filename, NumClients <= 0 ? 1 : NumClients);


            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                IsEphemeral = false,
                NotifyAddress = uriString,
                SymmetricKey = ClientSecret,
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
