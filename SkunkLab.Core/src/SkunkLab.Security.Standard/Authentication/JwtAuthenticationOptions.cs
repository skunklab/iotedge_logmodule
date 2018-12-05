using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkunkLab.Security.Authentication
{
    public class JwtAuthenticationOptions : AuthenticationSchemeOptions
    {
        public JwtAuthenticationOptions()
        {

        }
        public JwtAuthenticationOptions(string signingKey, string issuer = null, string audience = null)
        {
            SigningKey = signingKey;
            Issuer = issuer == null ? null : issuer.ToLowerInvariant();
            Audience = audience == null ? null : audience.ToLowerInvariant();
        }

        public string Scheme
        {
            get { return "SkunkLabJwt"; }
        }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string SigningKey { get; set; }
    }
}
