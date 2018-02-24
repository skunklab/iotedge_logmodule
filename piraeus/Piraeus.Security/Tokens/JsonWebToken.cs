
namespace Piraeus.Security.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using System.Security.Claims;

    public class JsonWebToken : SecurityToken, INotificationSecurityToken
    {
        public JsonWebToken(Uri address, string securityKey, string issuer, IEnumerable<Claim> claims)
        {
            this.id = Guid.NewGuid().ToString();
            this.created = DateTime.UtcNow;
            this.expires = created.AddMinutes(20);

            JwtSecurityTokenHandler jwt = new JwtSecurityTokenHandler();
            Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor std = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = address.ToString(),
                NotBefore = created,
                Expires = expires,
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(securityKey)), SecurityAlgorithms.HmacSha256Signature)
            };
            
            



            //SecurityTokenDescriptor std = new SecurityTokenDescriptor()
            //{
            //    Subject = new ClaimsIdentity(claims),
            //    TokenIssuerName = issuer,
            //    AppliesToAddress = address.ToString(),               
            //    Lifetime = new Lifetime(this.created, this.expires),
            //    SigningCredentials = new SigningCredentials(
            //        new InMemorySymmetricSecurityKey(Convert.FromBase64String(securityKey)),
            //        "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
            //        "http://www.w3.org/2001/04/xmlenc#sha256")
            //};
            
         
            
            tokenString = jwt.WriteToken(jwt.CreateToken(std));
        }

        public JsonWebToken(Uri audience, string securityKey, string issuer, IEnumerable<Claim> claims, double lifetimeMinutes)
        {
            this.id = Guid.NewGuid().ToString();
            this.created = DateTime.UtcNow;
            this.expires = created.AddMinutes(lifetimeMinutes);
            

            JwtSecurityTokenHandler jwt = new JwtSecurityTokenHandler();
            Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor std = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                NotBefore = created,
                Expires = expires,
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(securityKey)), SecurityAlgorithms.HmacSha256Signature)
            };
            //SecurityTokenDescriptor std = new SecurityTokenDescriptor()
            //{
            //    Subject = new ClaimsIdentity(claims),
            //    TokenIssuerName = issuer,
            //    AppliesToAddress = audience.ToString(),
            //    Lifetime = new Lifetime(this.created, this.expires),
            //    SigningCredentials = new SigningCredentials(
            //        new InMemorySymmetricSecurityKey(Convert.FromBase64String(securityKey)),
            //        "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
            //        "http://www.w3.org/2001/04/xmlenc#sha256")
            //};

            tokenString = jwt.WriteToken(jwt.CreateToken(std));
        }

        //private JwtSecurityTokenHandler handler;
        private DateTime created;
        private DateTime expires;
        private string tokenString;
        private string id;
        public override string ToString()
        {
            return tokenString;
        }

        public void SetSecurityToken(HttpWebRequest request)
        {
            request.Headers.Add("Authorization", String.Format("Bearer {0}", tokenString));
        }


        public override string Id
        {
            get { return this.id; }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { throw new NotImplementedException(); }
        }

        public override DateTime ValidFrom
        {
            get { return this.created; }
        }

        public override DateTime ValidTo
        {
            get { return this.expires; }
        }
    }
}
