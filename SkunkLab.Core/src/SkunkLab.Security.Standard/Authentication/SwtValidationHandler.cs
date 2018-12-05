using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SkunkLab.Security.Tokens;

namespace SkunkLab.Security.Authentication
{
    public class SwtValidationHandler : DelegatingHandler
    {
        public SwtValidationHandler(string signingKey, string issuer = null, string audience = null)
        {
            this.signingKey = signingKey;
            this.audience = audience;
            this.issuer = issuer;
        }

        private string signingKey;
        private string audience;
        private string issuer;

        private static bool TryRetrieveToken(HttpRequestMessage request, out string token)
        {
            token = null;
            IEnumerable<string> authzHeaders = null;
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
            string token = null;

            if (!TryRetrieveToken(request, out token))
            {
                statusCode = HttpStatusCode.Unauthorized;
                return Task<HttpResponseMessage>.Factory.StartNew(() =>
                            new HttpResponseMessage(statusCode));
            }

            try
            {

                if(SecurityTokenValidator.Validate(token, SecurityTokenType.SWT, signingKey, issuer, audience))
                {
                    //HttpContext.Current.User = Thread.CurrentPrincipal;
                }

                return base.SendAsync(request, cancellationToken);
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
