using Org.BouncyCastle.Crypto.Tls;

namespace SkunkLab.Channels.Core.Tcp
{
    public class PiraeusPskTlsServer : PskTlsServer
    {
        public PiraeusPskTlsServer(TlsPskIdentityManager pskIdentityManager) : base(pskIdentityManager)
        {
        }

        public PiraeusPskTlsServer(TlsCipherFactory cipherFactory, TlsPskIdentityManager pskIdentityManager) : base(cipherFactory, pskIdentityManager)
        {
        }

        protected override ProtocolVersion MaximumVersion
        {
            get { return ProtocolVersion.TLSv12; }
        }

        


    }
}
