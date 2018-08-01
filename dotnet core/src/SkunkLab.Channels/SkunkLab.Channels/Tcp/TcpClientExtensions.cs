using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace SkunkLab.Channels.Tcp
{
    public static class TcpClientExtensions
    {
        public static TlsClientProtocol ConnectPskTlsClient(this TcpClient client, string identity, byte[] psk, Stream stream)
        {
            SimplePskIdentity pskIdentity = new SimplePskIdentity(identity, psk);
            PskTlsClient2 pskTlsClient = new PskTlsClient2(pskIdentity);
            TlsClientProtocol protocol = new TlsClientProtocol(stream, new SecureRandom());
            protocol.Connect(pskTlsClient);
            return protocol;
        }

        public static TlsServerProtocol ConnectPskTlsServer(this TcpClient client, Dictionary<string, byte[]> psks, Stream stream)
        {
            TlsPskIdentityManager pskTlsManager = new PskIdentityManager(psks);
            PskTlsServer server = new PskTlsServer2(pskTlsManager);
            TlsServerProtocol protocol = new TlsServerProtocol(stream, new SecureRandom());
            protocol.Accept(server);
            return protocol;
        }      

        public static TlsClientProtocol ConnectPskTlsClientNonBlocking(this TcpClient client, string identity, byte[] psk)
        {
            SimplePskIdentity pskIdentity = new SimplePskIdentity(identity, psk);
            PskTlsClient2 pskTlsClient = new PskTlsClient2(pskIdentity);
            TlsClientProtocol protocol = new TlsClientProtocol(new SecureRandom());
            protocol.Connect(pskTlsClient);

            //while (!pskTlsClient.IsHandshakeComplete)
            //{
            //    //System.Threading.Tasks.Task.Delay(1000).Wait();
            //}

            return protocol;
        }

        public static TlsServerProtocol ConnectPskTlsServerNonBlocking(this TcpClient client, Dictionary<string, byte[]> psks)
        {
            TlsPskIdentityManager pskTlsManager = new PskIdentityManager(psks);
            PskTlsServer2 server = new PskTlsServer2(pskTlsManager);
            TlsServerProtocol protocol = new TlsServerProtocol(new SecureRandom());
            protocol.Accept(server);

            //while (!server.IsHandshakeComplete)
            //{
            //    System.Threading.Tasks.Task.Delay(1000).Wait();
            //}

            return protocol;

        }

    }
}
