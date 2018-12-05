

//namespace SkunkLab.Security.Authentication
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Diagnostics;
//    using System.IdentityModel.Tokens.Jwt;
//    using System.Linq;
//    using System.Net;
//    using System.Net.Http;
//    using System.Threading;
//    using System.Threading.Tasks;
//    using System.Web;
//    using System.Web.Http;
//    using System.Web.Http.Dispatcher;
//    using Microsoft.IdentityModel.Tokens;

//    public class JwtValidationHandler : DelegatingHandler
//    {
             

//        public JwtValidationHandler(string signingKey, string issuer = null, string audience = null)
//        {
//            this.signingKey = signingKey;
//            this.audience = audience;
//            this.issuer = issuer;
//            this.InnerHandler = new HttpControllerDispatcher(new HttpConfiguration());
//        }

//        private string signingKey;
//        private string audience;
//        private string issuer;


//        private static bool TryRetrieveToken(HttpRequestMessage request, out string token)
//        {
//            token = null;
//            IEnumerable<string> authzHeaders = null;
//            if (!request.Headers.TryGetValues("Authorization", out authzHeaders) || authzHeaders.Count() > 1)
//            {
//                return false;
//            }
//            var bearerToken = authzHeaders.ElementAt(0);
//            token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;
//            return true;
//        }

//        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//        {
//            HttpStatusCode statusCode;
//            string token = null;

//            if (!TryRetrieveToken(request, out token))
//            {
//                statusCode = HttpStatusCode.Unauthorized;
//                return Task<HttpResponseMessage>.Factory.StartNew(() =>
//                            new HttpResponseMessage(statusCode));
//            }

//            try
//            {

//                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();   

//                TokenValidationParameters validationParameters = new TokenValidationParameters()
//                {                  
//                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(signingKey)),
//                    ValidIssuer = issuer,
//                    ValidAudience = audience,
//                    ValidateAudience = audience != null,
//                    ValidateIssuer = issuer != null,
//                    ValidateIssuerSigningKey = true                    
//                };

//                Microsoft.IdentityModel.Tokens.SecurityToken stoken = null;
                                
//                Thread.CurrentPrincipal = tokenHandler.ValidateToken(token, validationParameters, out stoken);
               
//                HttpContext.Current.User = Thread.CurrentPrincipal;
                
//                return base.SendAsync(request, cancellationToken);
//            }
//            catch (Microsoft.IdentityModel.Tokens.SecurityTokenValidationException e)
//            {
//                Trace.TraceWarning("JWT validation has security token exception.");
//                Trace.TraceError(e.Message);
//                statusCode = HttpStatusCode.Unauthorized;
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceWarning("Exception in JWT validation.");
//                Trace.TraceError(ex.Message);
//                statusCode = HttpStatusCode.InternalServerError;
//            }
//            return Task<HttpResponseMessage>.Factory.StartNew(() =>
//                  new HttpResponseMessage(statusCode));
//        }

//    }
//}
