using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusAuditConfig")]
    public class SetAuditConfigCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Azure table storage name to write audit reports.", Mandatory = true)]
        public string TableName;

        [Parameter(HelpMessage = "Connectionstring to Azure storage.", Mandatory = true)]
        public string Connectionstring;

        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api2/service/ConfigureAudit?tablename={1}&connectionstring={2}", ServiceUrl, TableName, Connectionstring);
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, SecurityToken);
            RestRequest request = new RestRequest(builder);

            request.Post();
        }
    }
}
