using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Configuration.Core
{
    public class WebServerConfig : WebConfig 
    {
        public WebServerConfig()
        {

        }

        #region Client Name Claim Type and Indexes
        public string WebIdentityNameClaimType { get; set; }

        public string WebIdentityIndexClaimTypes { get; set; }

        public string WebIdentityIndexClaimValues { get; set; }

        #endregion


        #region Client Identity Authentication

        public string WebAuthnCertificateFilename { get; set; }

        public string WebSecurityTokenType { get; set; }

        public string WebSymmetricKey { get; set; }

        public string WebIssuer { get; set; }

        public string WebAudience { get; set; }

        #endregion


    }
}
