using Org.BouncyCastle.Crypto.Tls;
using System;

namespace tserver
{
    public class PiraeusPskTlsServer : PskTlsServer
    {
        public PiraeusPskTlsServer(TlsPskIdentityManager pskIdentityManager) : base(pskIdentityManager)
        {
        }

        public PiraeusPskTlsServer(TlsCipherFactory cipherFactory, TlsPskIdentityManager pskIdentityManager) : base(cipherFactory, pskIdentityManager)
        {
        }

        public bool IsHandshakeComplete { get; set; }

        protected override ProtocolVersion MaximumVersion
        {
            get { return ProtocolVersion.TLSv12; }
        }

        public override void NotifyAlertRaised(byte alertLevel, byte alertDescription, string message, Exception cause)
        {
            Console.WriteLine(message);
            base.NotifyAlertRaised(alertLevel, alertDescription, message, cause);
        }

        protected override int[] GetCipherSuites()
        {
            return base.GetCipherSuites();
        }


        public override void NotifyHandshakeComplete()
        {
            IsHandshakeComplete = true;
            base.NotifyHandshakeComplete();
        }




    }
}
