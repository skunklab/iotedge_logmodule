using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Security.Authentication
{
    public class X509AuthenticationHandler : AuthenticationHandler<X509AuthenticationOptions>
    {
        protected X509AuthenticationHandler(IOptionsMonitor<X509AuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            X509Certificate2 certificate = HttpHelper.HttpContext.Connection.ClientCertificate;

            if(certificate == null)
            {
                return Task.FromResult<AuthenticateResult>(AuthenticateResult.Fail("No client certificate to authenticate."));
            }

            if(string.IsNullOrEmpty(Options.StoreName) || string.IsNullOrEmpty(Options.Location) || string.IsNullOrEmpty(Options.Thumbprint))
            {
                return Task.FromResult<AuthenticateResult>(AuthenticateResult.Fail("No certificate in chain to check."));
            }

            try
            {
                StoreName storeName = (StoreName)Enum.Parse(typeof(StoreName), Options.StoreName);
                StoreLocation location = (StoreLocation)Enum.Parse(typeof(StoreLocation), Options.Location);
                if(X509Util.Validate(storeName, location, X509RevocationMode.Online, X509RevocationFlag.EntireChain, certificate, Options.Thumbprint))
                {
                    List<Claim> claimset = X509Util.GetClaimSet(certificate);
                    Claim nameClaim = claimset.Find((obj) => obj.Type == System.Security.Claims.ClaimTypes.Name);
                    GenericIdentity identity = new GenericIdentity(nameClaim.Value);                    
                    identity.AddClaims(claimset);                                       
                    Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
                    
                    var ticket = new AuthenticationTicket((ClaimsPrincipal)Thread.CurrentPrincipal, Options.Scheme);
                    return Task.FromResult<AuthenticateResult>(AuthenticateResult.Success(ticket));
                }
                else
                {
                    return Task.FromResult<AuthenticateResult>(AuthenticateResult.Fail("Not authenticated."));
                }
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.Message);
                return Task.FromResult<AuthenticateResult>(AuthenticateResult.Fail("Not authenticated."));
            }

        }
    }
}
