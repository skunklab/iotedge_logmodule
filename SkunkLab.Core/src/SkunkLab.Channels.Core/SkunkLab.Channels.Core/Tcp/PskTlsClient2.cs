using System;
using Org.BouncyCastle.Crypto.Tls;

namespace SkunkLab.Channels.Tcp
{
    public class PskTlsClient2 : PskTlsClient
    {
        public PskTlsClient2(TlsCipherFactory cipherFactory, TlsPskIdentity pskIdentity)
            : base(cipherFactory, pskIdentity)
        { 

        }
        public PskTlsClient2(TlsPskIdentity pskIdentity)
            : base(pskIdentity)
        {
               
        }

        public override ProtocolVersion MinimumVersion
        {
            get { return ProtocolVersion.TLSv12; }
        }

        public bool IsHandshakeComplete { get; set; }

        public override void NotifyAlertRaised(byte alertLevel, byte alertDescription, string message, Exception cause)
        {
            Console.WriteLine(message);
            base.NotifyAlertRaised(alertLevel, alertDescription, message, cause);
        }

        public override void NotifyHandshakeComplete()
        {
            IsHandshakeComplete = true;
            base.NotifyHandshakeComplete();
        }









        //public override int[] GetCipherSuites()
        //{
        //    //if (cipherSuite == 0)
        //    //{
        //    //    string identity = Encoding.UTF8.GetString(this.mPskIdentity.GetPskIdentity());
        //    //    cipherSuite = CiperSuiteSwitch.GetCipherSuite(identity);                
        //    //}
        //    //return new int[] { cipherSuite };


        //    return new int[]{ //CipherSuite.TLS_ECDHE_PSK_WITH_AES_256_CBC_SHA384,
        //        CipherSuite.TLS_DHE_PSK_WITH_AES_256_CBC_SHA384, 
        //        //CipherSuite.TLS_RSA_PSK_WITH_AES_256_CBC_SHA384,
        //        CipherSuite.TLS_PSK_WITH_AES_256_CBC_SHA };

        //    //return new int[]{ 
        //    //    CipherSuite.TLS_DHE_PSK_WITH_AES_256_CBC_SHA384, 
        //    //    CipherSuite.TLS_PSK_WITH_AES_256_CBC_SHA };
        //    //TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384
        //    //TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256
        //    //TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA38
        //    //TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384
        //    //TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384
        //    //TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA 
        //    //return new int[]{
        //    //    CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA256,
        //    //    CipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
        //    //    CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
        //    //    CipherSuite.TLS_DHE_RSA_WITH_AES_256_GCM_SHA384,
        //    //    CipherSuite.TLS_DHE_RSA_WITH_AES_128_GCM_SHA256,
        //    //    CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384,
        //    //    CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256,
        //    //    CipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384,
        //    //    CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,
        //    //    CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA,
        //    //    CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA,
        //    //    CipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,
        //    //    CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,
        //    //    CipherSuite.TLS_RSA_WITH_AES_256_GCM_SHA384,
        //    //    CipherSuite.TLS_RSA_WITH_AES_128_GCM_SHA256,
        //    //    CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA256,
        //    //    CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA,
        //    //    CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA,
        //    //    CipherSuite.TLS_RSA_WITH_3DES_EDE_CBC_SHA };

        //    /*
        //     * 

        //     */
        //}



    }
}
