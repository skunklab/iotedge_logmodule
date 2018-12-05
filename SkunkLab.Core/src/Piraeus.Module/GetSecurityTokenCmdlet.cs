using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace Piraeus.Module
{

    [Cmdlet(VerbsCommon.Get, "PiraeusManagementToken")]
    public class GetSecurityTokenCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "URL of service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Key used to retreive token.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api3/manage?code={1}", this.ServiceUrl, this.Key);
            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, null);
            RestRequest request = new RestRequest(builder);

            WriteObject(request.Get<string>());
        }
    }
}
