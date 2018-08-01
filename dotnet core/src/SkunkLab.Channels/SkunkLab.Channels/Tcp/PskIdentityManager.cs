using Org.BouncyCastle.Crypto.Tls;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace SkunkLab.Channels.Tcp
{
    public class PskIdentityManager : TlsPskIdentityManager
    {
        
        public PskIdentityManager(Dictionary<string, byte[]> psks)
        {
            container = psks;
        }

        private Dictionary<string, byte[]> container;

        public PskIdentityManager(byte[] psk)
        {
            this.psk = psk;
        }

        private byte[] psk;
        public byte[] GetHint()
        {
            return null;
        }

        public byte[] GetPsk(byte[] identity)
        {
            string identityString = Encoding.UTF8.GetString(identity);
            if(container.ContainsKey(identityString))
            {
                return container[identityString];
            }
            else
            {
                throw new SecurityException("Identity not found for PSK");
            }
        }
    }
}
