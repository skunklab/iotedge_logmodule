using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Diagnostics.Logging;

namespace Piraeus.Security.Tokens
{
    public class CertificateValidator : X509CertificateValidator
    {
        public CertificateValidator()
        {

        }

        public CertificateValidator(string issuer)
        {
            this.issuer = issuer;
        }

        private string issuer;
        public override void Validate(X509Certificate2 certificate)
        {
            if (new X509Chain().Build(certificate))
            {
                if (certificate.Verify())
                {
                    if (certificate.Issuer == issuer)
                    {
                        return;
                    }
                }
                else
                {
                    Task task = Log.LogErrorAsync("Certificate failed validation.");
                    Task.WhenAll(task);

                    throw new SecurityTokenException("Certificate not validated.");
                }
            }

            throw new SecurityTokenValidationException();
        }
    }
}
