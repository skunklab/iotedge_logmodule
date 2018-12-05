using System;
using System.Collections.Generic;
using System.Text;

namespace TokenService
{
    public class ServiceConfig
    {
        public string PiraeusHostname { get; set; }
        public string NameClaimType { get; set; }

        public string RoleClaimType { get; set; }

        public string SymmetricKey { get; set; }

        public int LifetimeMinutes { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string PiraeusApiHostname { get; set; }

        public string PiraeusApiToken { get; set; }

        public string[] PskIdentities { get; set; }

        public string[] Psks { get; set; }
    }
}
