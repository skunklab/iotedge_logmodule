//using Microsoft.IdentityModel.Tokens;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IdentityModel.Tokens.Jwt;
//using System.Net;
//using System.Security.Claims;
//using System.Threading;

//namespace LussService
//{
//    public class JsonWebToken : Microsoft.IdentityModel.Tokens.SecurityToken
//    {
//        private string issuer;
//        private DateTime created;
//        private DateTime expires;
//        private string tokenString;
//        private string id;

//        public JsonWebToken(string securityKey, IEnumerable<Claim> claims, double? lifetimeMinutes, string issuer = null, string audience = null)
//        {
//            this.issuer = issuer;
//            id = Guid.NewGuid().ToString();
//            created = DateTime.UtcNow;
//            expires = created.AddMinutes(lifetimeMinutes.HasValue ? lifetimeMinutes.Value : 20);
//            SigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(securityKey));

//            JwtSecurityTokenHandler jwt = new JwtSecurityTokenHandler();
//            SecurityTokenDescriptor msstd = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor()
//            {
//                Issuer = issuer,
//                Subject = new ClaimsIdentity(claims),
//                Expires = expires,
//                IssuedAt = created,
//                NotBefore = created,
//                Audience = audience,
//                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(SigningKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
//            };

//            JwtSecurityToken jwtToken = jwt.CreateJwtSecurityToken(msstd);
//            tokenString = jwt.WriteToken(jwtToken);
//        }

//        public JsonWebToken(Uri address, string securityKey, string issuer, IEnumerable<Claim> claims)
//        {
//            this.issuer = issuer;
//            id = Guid.NewGuid().ToString();
//            created = DateTime.UtcNow;
//            expires = created.AddMinutes(20);
//            SigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(securityKey));

//            JwtSecurityTokenHandler jwt = new JwtSecurityTokenHandler();
//            SecurityTokenDescriptor msstd = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor()
//            {
//                Issuer = issuer,
//                Subject = new ClaimsIdentity(claims),
//                Expires = expires,
//                IssuedAt = created,
//                NotBefore = created,
//                Audience = address.ToString(),
//                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(SigningKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
//            };

//            JwtSecurityToken jwtToken = jwt.CreateJwtSecurityToken(msstd);
//            tokenString = jwt.WriteToken(jwtToken);
//        }

//        public JsonWebToken(Uri audience, string securityKey, string issuer, IEnumerable<Claim> claims, double lifetimeMinutes)
//        {
//            this.issuer = issuer;
//            id = Guid.NewGuid().ToString();
//            created = DateTime.UtcNow;
//            expires = created.AddMinutes(lifetimeMinutes);
//            SigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(securityKey));

//            JwtSecurityTokenHandler jwt = new JwtSecurityTokenHandler();
//            Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor msstd = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor()
//            {
//                Issuer = issuer,
//                Subject = new ClaimsIdentity(claims),
//                Expires = expires,
//                IssuedAt = created,
//                NotBefore = created,
//                Audience = audience.ToString(),
//                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(SigningKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
//            };


//            JwtSecurityToken jwtToken = jwt.CreateJwtSecurityToken(msstd);
//            tokenString = jwt.WriteToken(jwtToken);
//        }

//        public override string Id
//        {
//            get { return this.id; }
//        }



//        public override DateTime ValidFrom
//        {
//            get { return created; }
//        }

//        public override DateTime ValidTo
//        {
//            get { return expires; }
//        }

//        public override string Issuer
//        {
//            get { return this.issuer; }
//        }


//        public override SecurityKey SecurityKey
//        {
//            get { return null; }
//        }

//        public override SecurityKey SigningKey { get; set; }


//        public override string ToString()
//        {
//            return tokenString;
//        }

//        public void SetSecurityToken(HttpWebRequest request)
//        {
//            request.Headers.Add("Authorization", String.Format("Bearer {0}", tokenString));
//        }

//        public static void Authenticate(string token, string issuer, string audience, string signingKey)
//        {
//            try
//            {
//                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

//                TokenValidationParameters validationParameters = new TokenValidationParameters()
//                {
//                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(signingKey)),
//                    ValidIssuer = issuer,
//                    ValidAudience = audience,
//                    ValidateAudience = true,
//                    ValidateIssuer = true,
//                    ValidateIssuerSigningKey = true
//                };

//                Microsoft.IdentityModel.Tokens.SecurityToken stoken = null;

//                Thread.CurrentPrincipal = tokenHandler.ValidateToken(token, validationParameters, out stoken);

//            }
//            catch (Microsoft.IdentityModel.Tokens.SecurityTokenValidationException e)
//            {
//                Trace.TraceWarning("JWT validation has security token exception.");
//                Trace.TraceError(e.Message);
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceWarning("Exception in JWT validation.");
//                Trace.TraceError(ex.Message);
//            }
//        }

//    }
//}
