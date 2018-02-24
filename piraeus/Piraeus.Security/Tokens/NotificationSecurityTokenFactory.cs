


//namespace Piraeus.Security.Tokens
//{
//    //using Piraeus.Configuration;
//    using System;
//    public abstract class NotificationSecurityTokenFactory
//    {
//        public static INotificationSecurityToken Create(Uri address, SecurityTokenType? tokenType, string symmetricKey)
//        {
//            if (!tokenType.HasValue || (tokenType.HasValue && tokenType.Value == SecurityTokenType.X509))
//            {
//                return new X509CertificateToken(PiraeusConfigurationManager.ServerCertificate);
//            }

//            if (tokenType.HasValue && tokenType.Value == SecurityTokenType.SWT)
//            {
//                SimpleWebToken swt = new SimpleWebToken(PiraeusConfigurationManager.Issuer, address.ToString(), DateTime.UtcNow.AddHours(1), null);
//                byte[] key = Convert.FromBase64String(symmetricKey);
//                swt.Sign(key);
//                return swt;
//            }

//            if (tokenType.HasValue && tokenType.Value == SecurityTokenType.JWT)
//            {
//                JsonWebToken jwt = new JsonWebToken(address, symmetricKey, PiraeusConfigurationManager.Issuer, null);
//                return jwt;
//            }

//            return null;
//        }

//        public static INotificationSecurityToken Create(SecurityTokenType? tokenType, string notifyAddress, string symmetricKey)
//        {
//            if (!tokenType.HasValue || (tokenType.HasValue && tokenType.Value == SecurityTokenType.X509))
//            {
//                return new X509CertificateToken(PiraeusConfigurationManager.ServerCertificate);
//            }

//            if (tokenType.HasValue && tokenType.Value == SecurityTokenType.SWT)
//            {
//                SimpleWebToken swt = new SimpleWebToken(PiraeusConfigurationManager.Issuer, notifyAddress, DateTime.UtcNow.AddHours(1), null); 
//                byte[] key = Convert.FromBase64String(symmetricKey);
//                swt.Sign(key);
//                return swt;
//            }

//            string issuer = PiraeusConfigurationManager.Issuer;

//            if (tokenType.HasValue && tokenType.Value == SecurityTokenType.JWT)
//            {
//                JsonWebToken jwt = new JsonWebToken(new Uri(notifyAddress), symmetricKey, PiraeusConfigurationManager.Issuer, null);
//                return jwt;
//            }

//            return null;
//        }

//    }
//}
