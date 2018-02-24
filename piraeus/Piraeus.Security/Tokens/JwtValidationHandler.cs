

namespace Piraeus.Security.Tokens
{
    //using Piraeus.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.ServiceModel.Security.Tokens;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.IdentityModel.Tokens.Jwt;
    using Microsoft.IdentityModel.Tokens;

    public class JwtValidationHandler : DelegatingHandler
    {
        public JwtValidationHandler()
        {
            //SymmetricKeyTokenInfo info = PiraeusConfigurationManager.GetSigningTokenInfo("JWT");

            //this.signingKey = info.SigningKey;
            //this.audience = info.Audience;
            //this.issuer = info.Issuer;
        }

        public JwtValidationHandler(string signingKey, string issuer = null, string audience = null)
        {
            this.signingKey = signingKey;
            this.issuer = issuer;
            this.audience = audience;
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

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                TokenValidationParameters validationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(signingKey)),
                    ValidAudience = audience,
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrEmpty(audience),
                    ValidateIssuer = !string.IsNullOrEmpty(issuer)
                };

                Microsoft.IdentityModel.Tokens.SecurityToken stoken = null;

                Thread.CurrentPrincipal = tokenHandler.ValidateToken(token, validationParameters, out stoken);
                HttpContext.Current.User = Thread.CurrentPrincipal;

                return base.SendAsync(request, cancellationToken);
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenValidationException e)
            {
                Trace.TraceWarning("JWT validation has security token exception.");
                Trace.TraceError(e.Message);
                statusCode = HttpStatusCode.Unauthorized;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Exception in JWT validation.");
                Trace.TraceError(ex.Message);
                statusCode = HttpStatusCode.InternalServerError;
            }
            return Task<HttpResponseMessage>.Factory.StartNew(() =>
                  new HttpResponseMessage(statusCode));
        }

    }
}
