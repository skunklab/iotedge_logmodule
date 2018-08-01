using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tserver
{
    public class TestServer : PskTlsServer
    {
        public TestServer(TlsPskIdentityManager pskIdentityManager) : base(pskIdentityManager)
        {
        }

        public TestServer(TlsCipherFactory cipherFactory, TlsPskIdentityManager pskIdentityManager) : base(cipherFactory, pskIdentityManager)
        {
        }

        
        
    }
}
