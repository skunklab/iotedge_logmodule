using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Crypto.Tls;
using System.Linq;
using System.Security;

namespace SkunkLab.Channels.Core.Tcp
{
    public class PskIdentityManager : TlsPskIdentityManager
    {
        
        public PskIdentityManager(Dictionary<string, byte[]> psks)
        {
            container = psks;
        }

        //public PskIdentityManager(string identity, byte[] psk)
        //{
        //    this.identity = Encoding.UTF8.GetBytes(identity);
        //    this.psk = psk;
        //}

        private Dictionary<string, byte[]> container;

        public PskIdentityManager(byte[] psk)
        {
            this.psk = psk;
        }

        private byte[] psk;
        //private byte[] identity;
        public byte[] GetHint()
        {
            return null;
            //return Encoding.UTF8.GetBytes("hint");
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
            //return psk;
        }
    }
}
