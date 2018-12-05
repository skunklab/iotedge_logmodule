using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Get, "PiraeusResourceList")]
    public class GetResourceListCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api2/resource/getresourcelist", ServiceUrl);
            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, SecurityToken);
            RestRequest request = new RestRequest(builder);

            IEnumerable<string> resourceList = request.Get<IEnumerable<string>>();
            WriteObject(resourceList);
        }
    }
}
