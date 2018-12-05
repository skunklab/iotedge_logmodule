using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusIdentityConfig")]
    public class SetIdentityConfigCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Additional claims for Piraeus identity.", Mandatory = false)]
        public List<Claim> Claims;

        [Parameter(HelpMessage = "X509 client certificate for Piraeus identity.", Mandatory = false)]
        public X509Certificate2 Certificate;

        protected override void ProcessRecord()
        {
            IdentityConfig config = new IdentityConfig()
            {
                 Claims = this.Claims,                
            };

            if(Certificate != null)
            {
                config.Certificate = this.Certificate.RawData;
            }

            string url = String.Format("{0}/api2/service/ConfigureIdentity", ServiceUrl);
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, SecurityToken);
            RestRequest request = new RestRequest(builder);

            request.Post<IdentityConfig>(config);
        }


    }
}
