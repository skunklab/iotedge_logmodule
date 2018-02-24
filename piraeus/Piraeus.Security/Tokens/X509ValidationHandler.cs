using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Piraeus.Security.Tokens
{
    public class X509ValidationHandler : DelegatingHandler
    {
        public X509ValidationHandler()
        {

        }

        public X509ValidationHandler(string issuer)
        {
            this.issuer = issuer;
        }

        private string issuer;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            X509Certificate2 certificate = request.GetClientCertificate();
            CertificateValidator validator = new CertificateValidator(issuer);
            validator.Validate(certificate);

            X509CertificateClaimSet claimset = new X509CertificateClaimSet(certificate);
            List<System.Security.Claims.Claim> claims = new List<System.Security.Claims.Claim>();
            foreach(System.IdentityModel.Claims.Claim claim in claimset)
            {
                string value = null;
                if(claim.Resource as byte[] != null)
                {
                    value = Convert.ToBase64String(claim.Resource as byte[]);
                }
                else if (claim.Resource as X500DistinguishedName != null)
                {
                    value = ((X500DistinguishedName)claim.Resource).Name;
                }
                else
                {
                    value = claim.Resource.ToString();
                }

                System.Security.Claims.Claim c = new System.Security.Claims.Claim(claim.ClaimType, value);
            }

            ClaimsIdentity identity = new ClaimsIdentity(claims);
            Thread.CurrentPrincipal = new ClaimsPrincipal(identity);

            HttpContext.Current.User = Thread.CurrentPrincipal;

            return base.SendAsync(request, cancellationToken);
        }
    }
}
