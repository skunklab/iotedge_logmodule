using Org.BouncyCastle.Crypto.Tls;
using System;
using System.IO;

namespace tclient
{
    public class PiraeusPskTlsClient : PskTlsClient
    {
        public PiraeusPskTlsClient(TlsCipherFactory cipherFactory, TlsPskIdentity pskIdentity)
            : base(cipherFactory, pskIdentity)
        {

        }
        public PiraeusPskTlsClient(TlsPskIdentity pskIdentity)
            : base(pskIdentity)
        {

        }

        public override ProtocolVersion MinimumVersion
        {
            get { return ProtocolVersion.TLSv12; }
        }

        public bool IsHandshakeComplete { get; set; }

        //public override void NotifyAlertRaised(byte alertLevel, byte alertDescription, string message, Exception cause)
        //{
        //    Console.WriteLine(message);
        //    base.NotifyAlertRaised(alertLevel, alertDescription, message, cause);
        //}

        public override void NotifyHandshakeComplete()
        {
            IsHandshakeComplete = true;
            base.NotifyHandshakeComplete();
        }

        public override void NotifyAlertRaised(byte alertLevel, byte alertDescription, string message, Exception cause)
        {
            TextWriter output = (alertLevel == AlertLevel.fatal) ? Console.Error : Console.Out;
            output.WriteLine("TLS client raised alert: " + AlertLevel.GetText(alertLevel)
                + ", " + AlertDescription.GetText(alertDescription));
            if (message != null)
            {
                output.WriteLine("> " + message);
            }
            if (cause != null)
            {
                output.WriteLine(cause);
            }
        }

        public override void NotifyAlertReceived(byte alertLevel, byte alertDescription)
        {
            TextWriter output = (alertLevel == AlertLevel.fatal) ? Console.Error : Console.Out;
            output.WriteLine("TLS client received alert: " + AlertLevel.GetText(alertLevel)
                + ", " + AlertDescription.GetText(alertDescription));
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
