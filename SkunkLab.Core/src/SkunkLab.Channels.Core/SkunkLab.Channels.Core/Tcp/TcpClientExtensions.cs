using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace SkunkLab.Channels.Tcp
{
    public static class TcpClientExtensions
    {
        public static TlsClientProtocol ConnectPskTlsClient(this TcpClient client, string identity, byte[] psk, Stream stream)
        {
            try
            {
                SimplePskIdentity pskIdentity = new SimplePskIdentity(identity, psk);
                PskTlsClient2 pskTlsClient = new PskTlsClient2(pskIdentity);
                TlsClientProtocol protocol = new TlsClientProtocol(stream, new SecureRandom());
                protocol.Connect(pskTlsClient);
                return protocol;
            }
            catch (AggregateException ae)
            {
                string msg = String.Format("AggregateException in TLS protocol connnection '{0}'", ae.Flatten().InnerException.Message);
                Console.WriteLine(msg);
                throw new Exception(msg, ae.Flatten().InnerException);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in TLS protocol connnection '{0}'", ex.Message);
                throw ex;
            }
        }

        public static TlsServerProtocol ConnectPskTlsServer(this TcpClient client, Dictionary<string, byte[]> psks, Stream stream)
        {
            try
            {
                TlsPskIdentityManager pskTlsManager = new PskIdentityManager(psks);
                PskTlsServer server = new PskTlsServer2(pskTlsManager);
                TlsServerProtocol protocol = new TlsServerProtocol(stream, new SecureRandom());
                protocol.Accept(server);
                return protocol;
            }
            catch (AggregateException ae)
            {
                string msg = String.Format("AggregateException in TLS protocol connnection '{0}'", ae.Flatten().InnerException.Message);
                Console.WriteLine(msg);
                throw new Exception(msg, ae.Flatten().InnerException);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in TLS protocol connnection '{0}'", ex.Message);
                throw ex;
            }
        }      

        public static TlsClientProtocol ConnectPskTlsClientNonBlocking(this TcpClient client, string identity, byte[] psk)
        {
            try
            {
                SimplePskIdentity pskIdentity = new SimplePskIdentity(identity, psk);
                PskTlsClient2 pskTlsClient = new PskTlsClient2(pskIdentity);
                TlsClientProtocol protocol = new TlsClientProtocol(new SecureRandom());
                protocol.Connect(pskTlsClient);
                return protocol;
            }
            catch (AggregateException ae)
            {
                string msg = String.Format("AggregateException in TLS protocol connnection '{0}'", ae.Flatten().InnerException.Message);
                Console.WriteLine(msg);
                throw new Exception(msg, ae.Flatten().InnerException);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in TLS protocol connnection '{0}'", ex.Message);
                throw ex;
            }
        }

        public static TlsServerProtocol ConnectPskTlsServerNonBlocking(this TcpClient client, Dictionary<string, byte[]> psks)
        {
            try
            {
                TlsPskIdentityManager pskTlsManager = new PskIdentityManager(psks);
                PskTlsServer2 server = new PskTlsServer2(pskTlsManager);
                TlsServerProtocol protocol = new TlsServerProtocol(new SecureRandom());
                protocol.Accept(server);

                return protocol;
            }
            catch (AggregateException ae)
            {
                string msg = String.Format("AggregateException in TLS protocol connnection '{0}'", ae.Flatten().InnerException.Message);
                Console.WriteLine(msg);
                throw new Exception(msg, ae.Flatten().InnerException);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in TLS protocol connnection '{0}'", ex.Message);
                throw ex;
            }

        }

    }
}
