using Org.BouncyCastle.Crypto.Tls;
using System.Text;

namespace SkunkLab.Channels.Tcp
{
    public class SimplePskIdentity : TlsPskIdentity
    {
        public SimplePskIdentity(string hint, byte[] psk)
        {
            this.hint = hint;
            this.psk = psk;
        }

        private string hint;
        private byte[] psk;


        public byte[] GetPsk()
        {
            return psk;
        }

        public byte[] GetPskIdentity()
        {
            return Encoding.UTF8.GetBytes(hint);
        }

        public void NotifyIdentityHint(byte[] psk_identity_hint)
        {
            //Console.WriteLine("Notify Identity Hint {0}", Encoding.UTF8.GetString(psk_identity_hint));
        }

        public void SkipIdentityHint()
        {
            //Console.WriteLine("Skip identity hint");
        }
    }
}
