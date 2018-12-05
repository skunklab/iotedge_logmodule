using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Remove, "CaplPolicy")]
    public class RemoveCaplPolicyCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Access control policy URI string that identifies the policy.", Mandatory = true)]
        public string PolicyId;

        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api2/accesscontrol/deleteaccesscontrolpolicy?policyuristring={1}", ServiceUrl, PolicyId);
            RestRequestBuilder builder = new RestRequestBuilder("DELETE", url, RestConstants.ContentType.Json, true, SecurityToken);
            RestRequest request = new RestRequest(builder);

            request.Delete();
            
        }
    }
}
