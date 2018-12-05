using System;
using System.Management.Automation;
using Piraeus.Core.Metadata;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Get, "PiraeusResourceMetadata")]
    public class GetResourceMetadataCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource.", Mandatory = true)]
        public string ResourceUriString;

        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api2/resource/GetResourceMetadata?ResourceUriString={1}", ServiceUrl, ResourceUriString);
            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, SecurityToken);
            RestRequest request = new RestRequest(builder);
                       

            ResourceMetadata metadata = request.Get<ResourceMetadata>();

            WriteObject(metadata);
        }
    }
}
