using System;
using Org.BouncyCastle.Crypto.Tls;

namespace SkunkLab.Channels.Tcp
{   
    public class PskTlsServer2 : PskTlsServer
    {
        public PskTlsServer2(TlsPskIdentityManager pskIdentityManager) : base(pskIdentityManager)
        {           
        }

        public PskTlsServer2(TlsCipherFactory cipherFactory, TlsPskIdentityManager pskIdentityManager) : base(cipherFactory, pskIdentityManager)
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


        public override void NotifyHandshakeComplete()
        {
            IsHandshakeComplete = true;
            base.NotifyHandshakeComplete();
        }




    }
}
