using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tserver
{
    public class TestServerProtocol : TlsServerProtocol
    {
        public TestServerProtocol(SecureRandom secureRandom) : base(secureRandom)
        {
        }

        public TestServerProtocol(Stream stream, SecureRandom secureRandom) : base(stream, secureRandom)
        {
        }

        public TestServerProtocol(Stream input, Stream output, SecureRandom secureRandom) : base(input, output, secureRandom)
        {
        }


        public override void Accept(TlsServer tlsServer)
        {
            base.Accept(tlsServer);
            SafeReadRecord();
        }

    }
}
