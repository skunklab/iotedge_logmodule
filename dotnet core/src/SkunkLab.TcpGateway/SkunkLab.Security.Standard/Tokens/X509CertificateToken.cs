
//namespace SkunkLab.Security.Tokens
//{
//    using System;
//    using System.Collections.ObjectModel;
//    using System.IdentityModel.Tokens;
//    using System.Net;
//    using System.Security.Cryptography.X509Certificates;
//    public class X509CertificateToken : SecurityToken
//    {
//        public X509CertificateToken(X509Certificate2 certificate)
//        {
//            this.certificate = certificate;   
//        }

//        private X509Certificate2 certificate;

//        public void SetSecurityToken(HttpWebRequest request)
//        {            
//            request.ClientCertificates.Add(this.certificate);
//        }

//        public override string Id
//        {
//            get { return this.certificate.GetSerialNumberString(); }
//        }

//        public override ReadOnlyCollection<SecurityKey> SecurityKeys
//        {
//            get { throw new System.NotImplementedException(); }
//        }

//        public override DateTime ValidFrom
//        {
//            get { return this.certificate.NotBefore; }
//        }

//        public override DateTime ValidTo
//        {
//            get { return this.certificate.NotAfter; }
//        }
//    }
//}
