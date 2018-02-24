using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Piraeus.Security.Tokens
{
    public class SwtValidationHandler : DelegatingHandler
    {
        public SwtValidationHandler()
        {
        }

        private string signingKey;
        private string audience;
        private string issuer;

        private static bool TryRetrieveToken(HttpRequestMessage request, out string token)
        {
            token = null;
            IEnumerable<string> authzHeaders;
            if (!request.Headers.TryGetValues("Authorization", out authzHeaders) || authzHeaders.Count() > 1)
            {
                return false;
            }
            var bearerToken = authzHeaders.ElementAt(0);
            token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;
            return true;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {            
            HttpStatusCode statusCode;
            string token;

            if (!TryRetrieveToken(request, out token))
            {
                statusCode = HttpStatusCode.Unauthorized;
                return Task<HttpResponseMessage>.Factory.StartNew(() =>
                            new HttpResponseMessage(statusCode));
            }

            try
            {
                SimpleWebToken swt = SimpleWebToken.FromString(token);
                if (swt.Validate(Convert.FromBase64String(signingKey), issuer, audience))
                {
                   if(swt.ExpiresOn < DateTime.UtcNow)
                    {
                        throw new SecurityException("Unauthorized SWT expired.");
                    }
                }
                else
                {
                    throw new SecurityException("Unauthorized SWT not validated.");
                }

                Thread.CurrentPrincipal = new ClaimsPrincipal(swt.Identity);
                HttpContext.Current.User = Thread.CurrentPrincipal;

                return base.SendAsync(request, cancellationToken);
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenValidationException e)
            {
                Trace.TraceWarning("SWT validation has security token exception.");
                Trace.TraceError(e.Message);
                statusCode = HttpStatusCode.Unauthorized;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Exception in SWT validation.");
                Trace.TraceError(ex.Message);
                statusCode = HttpStatusCode.InternalServerError;
            }
            return Task<HttpResponseMessage>.Factory.StartNew(() =>
                  new HttpResponseMessage(statusCode));
        }
    }
}
