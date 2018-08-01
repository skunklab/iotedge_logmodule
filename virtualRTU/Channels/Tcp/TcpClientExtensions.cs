using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;

namespace SkunkLab.Channels.Tcp
{
    public static class TcpClientExtensions
    {
        public static TlsClientProtocol ConnectPskTlsClient(this TcpClient client, string identity, byte[] psk, Stream stream)
        {
            SimplePskIdentity pskIdentity = new SimplePskIdentity(identity, psk);
            PiraeusPskTlsClient pskTlsClient = new PiraeusPskTlsClient(pskIdentity);
            TlsClientProtocol protocol = new TlsClientProtocol(stream, new SecureRandom());
            protocol.Connect(pskTlsClient);
            return protocol;
        }

        public static TlsServerProtocol ConnectPskTlsServer(this TcpClient client, Dictionary<string, byte[]> psks, Stream stream)
        {

            TlsPskIdentityManager pskTlsManager = new PskIdentityManager(psks);
            PskTlsServer server = new PiraeusPskTlsServer(pskTlsManager);
            TlsServerProtocol protocol = new TlsServerProtocol(stream, new SecureRandom());
            protocol.Accept(server);
            return protocol;
        }

        //public static TcpState GetState(this TcpClient tcpClient)
        //{
        //    var prop = IPGlobalProperties.GetIPGlobalProperties()
        //      .GetActiveTcpConnections()
        //      .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
        //    return prop != null ? prop.State : TcpState.Unknown;
        //}
    }
}
