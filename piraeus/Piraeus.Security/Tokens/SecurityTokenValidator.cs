
namespace Piraeus.Security.Tokens
{
    //using Piraeus.Configuration;
    using System;
    using System.Diagnostics;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.ServiceModel.Security.Tokens;
    using System.Threading;
    using Microsoft.IdentityModel.Tokens;

    public static class SecurityTokenValidator
    {
        public static bool Validate(string tokenString, SecurityTokenType tokenType)
        {
            if(tokenType == SecurityTokenType.None)
            {
                return false;
            }

            if(tokenType == SecurityTokenType.JWT)
            {
                return ValidateJwt(tokenString);
            }
            else if(tokenType == SecurityTokenType.SWT)
            {
                return ValidateSwt(tokenString);
            }
            else
            {
                return ValidateCertificate();
            }
        }

        private static bool ValidateJwt(string tokenString)
        {
            bool result = false;
            //get the issuer, audience, and key from configuration
            //SymmetricKeyTokenInfo info = PiraeusConfigurationManager.GetSigningTokenInfo("JWT");
            string signingKey = info.SigningKey;
            string issuer = info.Issuer;
            string audience = info.Audience;
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                TokenValidationParameters tvp = new TokenValidationParameters();

                TokenValidationParameters validationParameters = new TokenValidationParameters()
                {
                    IssuerSigningToken = new BinarySecretSecurityToken(Convert.FromBase64String(signingKey)),
                    ValidIssuer = issuer,
                    ValidAudience = audience
                };

                SecurityToken securityToken = null;

                Thread.CurrentPrincipal = tokenHandler.ValidateToken(tokenString, validationParameters, out securityToken);
                result = true;
            }
            catch (SecurityTokenValidationException e)
            {
                Trace.TraceWarning("JWT security token validation exception.");
                Trace.TraceError(e.Message);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("JWT security token authentication exception.");
                Trace.TraceError(ex.Message);
            }

            return result;
        }


        private static bool ValidateSwt(string tokenString)
        {
            bool result = false;

            //get key from cofiguration
            string key = null;

            try
            {
                SimpleWebToken token = SimpleWebToken.FromString(tokenString);
                result = token.SignVerify(Convert.FromBase64String(key));
                
                ClaimsPrincipal principal = new ClaimsPrincipal(token.Identity);
                Thread.CurrentPrincipal = principal;
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("SWT validation exception.");
                Trace.TraceError(ex.Message);
            }

            return result;
        }

        private static bool ValidateCertificate()
        {            
            return false;
        }
    }
}
