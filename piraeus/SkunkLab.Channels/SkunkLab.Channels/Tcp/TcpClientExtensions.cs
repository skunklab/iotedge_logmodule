using System.Net.Sockets;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;

namespace SkunkLab.Channels.Tcp
{
    public static class TcpClientExtensions
    {
        public static TlsClientProtocol ConnectPskTlsClient(this TcpClient client, string identity, byte[] psk, SecureRandom srandom)
        {
            TlsPskIdentity ident = new BasicTlsPskIdentity(identity, psk);
            PskTlsClient pskTlsClient = new PskTlsClient(ident);
            TlsClientProtocol protocol = new TlsClientProtocol(client.GetStream(), srandom);
            protocol.Connect(pskTlsClient);
            return protocol;
        }

        public static TlsServerProtocol ConnectPskTlsServer(this TcpClient client, string identity, byte[] psk, SecureRandom srandom)
        {
            TlsServerProtocol tlsProtocol = new TlsServerProtocol(client.GetStream(), srandom);
            TlsPskIdentityManager manager = new PskIdentityManager(identity, psk);
            PskTlsServer server = new PskTlsServer(manager);
            tlsProtocol.Accept(server);
            return tlsProtocol;
        }

        public static TlsServerProtocol ConnectPskTlsServer(this TcpClient client, byte[] psk, SecureRandom srandom)
        {
            TlsServerProtocol tlsProtocol = new TlsServerProtocol(client.GetStream(), srandom);
            TlsPskIdentityManager manager = new PskIdentityManager( psk);
            PskTlsServer server = new PskTlsServer(manager);
            tlsProtocol.Accept(server);
            return tlsProtocol;
        }
    }
}
