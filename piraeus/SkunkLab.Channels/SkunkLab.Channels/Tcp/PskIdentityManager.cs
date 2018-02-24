using System.Text;
using Org.BouncyCastle.Crypto.Tls;

namespace SkunkLab.Channels.Tcp
{
    public class PskIdentityManager : TlsPskIdentityManager
    {
        public PskIdentityManager(string identity, byte[] psk)
        {
            this.identity = Encoding.UTF8.GetBytes(identity);
            this.psk = psk;
        }

        public PskIdentityManager(byte[] psk)
        {
            this.psk = psk;
        }

        private byte[] psk;
        private byte[] identity;
        public byte[] GetHint()
        {
            return identity;
        }

        public byte[] GetPsk(byte[] identity)
        {
            return psk;
        }
    }
}
