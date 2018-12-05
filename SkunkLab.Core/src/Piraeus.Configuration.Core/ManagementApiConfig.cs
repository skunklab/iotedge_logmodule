using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Configuration.Core
{
    public class ManagementApiConfig : WebConfig
    {
        public ManagementApiConfig()
        {

        }

        #region Management API 
        public string ApiAudience { get; set; }

        public string ApiIssuer { get; set; }

        public string ApiSymmetricKey { get; set; }

        public string ApiNameClaimType { get; set; }

        public string ApiRoleClaimType { get; set; }

        public string ApiRoleClaimValue { get; set; }

        public string ApiSecurityCodes { get; set; }

        #endregion
    }
}
