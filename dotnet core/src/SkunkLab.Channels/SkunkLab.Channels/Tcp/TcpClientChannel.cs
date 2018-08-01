using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Tcp
{
    public class TcpClientChannel : TcpChannel
    {
        #region ctor
        public TcpClientChannel(string hostname, int port, int maxBufferSize, CancellationToken token)
        {
            this.hostname = hostname;
            this.port = port;
            this.maxBufferSize = maxBufferSize;
            this.token = token;
            this.queue = new Queue<byte[]>();
        }

        public TcpClientChannel(string hostname, int port, IPEndPoint localEP, int maxBufferSize, CancellationToken token)
            : this(hostname, port, localEP, null, maxBufferSize, token)
        {
        }

        public TcpClientChannel(IPEndPoint remoteEndpoint, int maxBufferSize, CancellationToken token)
        {
            this.remoteEP = remoteEndpoint;
            this.maxBufferSize = maxBufferSize;
            this.token = token;
            this.queue = new Queue<byte[]>();
        }

        public TcpClientChannel(IPEndPoint remoteEndpoint, IPEndPoint localEP, int maxBufferSize, CancellationToken token)
            : this(remoteEndpoint, localEP, null, maxBufferSize, token)
        {
        }

        public TcpClientChannel(IPAddress address, int port, int maxBufferSize, CancellationToken token)
        {
            this.address = address;
            this.port = port;
            this.maxBufferSize = maxBufferSize;
            this.token = token;
            this.queue = new Queue<byte[]>();
        }

        public TcpClientChannel(IPAddress address, int port, IPEndPoint localEP, int maxBufferSize, CancellationToken token)
            : this(address, port, localEP, null, maxBufferSize, token)
        {
        }

        public TcpClientChannel(string hostname, int port,  X509Certificate2 certificate, int maxBufferSize, CancellationToken token)
            :this(hostname, port, null, certificate, maxBufferSize, token)
        {
        }

        public TcpClientChannel(string hostname, int port, IPEndPoint localEP, X509Certificate2 certificate, int maxBufferSize, CancellationToken token)
        {
            this.hostname = hostname;
            this.port = port;
            this.localEP = localEP;
            this.certificate = certificate;
            this.token = token;
            this.maxBufferSize = maxBufferSize;
            Id = "tcp-" + Guid.NewGuid().ToString();
            this.token.Register(async () => await CloseAsync());

            Port = port;
            this.queue = new Queue<byte[]>();
        }
        
        public TcpClientChannel(IPEndPoint remoteEndpoint, X509Certificate2 certificate, int maxBufferSize, CancellationToken token)
            : this(remoteEndpoint, null, certificate, maxBufferSize, token)
        {
        }

        public TcpClientChannel(IPEndPoint remoteEndpoint, IPEndPoint localEP, X509Certificate2 certificate, int maxBufferSize, CancellationToken token)
        {
            
            if(remoteEndpoint == null)
            {
                throw new ArgumentNullException("remoteEndpoint");
            }

            remoteEP = remoteEndpoint;
            this.localEP = localEP;
            this.certificate = certificate;
            this.token = token;
            this.maxBufferSize = maxBufferSize;
            Id = "tcp-" + Guid.NewGuid().ToString();
            this.token.Register(async () => await CloseAsync());

            if (certificate != null)
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(remoteEndpoint.Address);
                hostname = ipHostInfo.HostName;
            }

            Port = remoteEndpoint.Port;
            this.queue = new Queue<byte[]>();
        }

        public TcpClientChannel(IPAddress address, int port, X509Certificate2 certificate, int maxBufferSize, CancellationToken token)
            : this(address, port, null, certificate, maxBufferSize, token)
        {

        }
        public TcpClientChannel(IPAddress address, int port, IPEndPoint localEP, X509Certificate2 certificate, int maxBufferSize, CancellationToken token)
        {
            if(address == null)
            {
                throw new ArgumentNullException("address");
            }

            this.address = address;
            this.port = port;
            this.localEP = localEP;
            this.certificate = certificate;
            this.token = token;
            this.maxBufferSize = maxBufferSize;
            Id = "tcp-" + Guid.NewGuid().ToString();
            this.token.Register(async () => await CloseAsync());

            if (certificate != null)
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(address);
                hostname = ipHostInfo.HostName;
            }

            Port = port;
            this.queue = new Queue<byte[]>();
        }

        public TcpClientChannel(IPAddress address, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, int maxBufferSize, CancellationToken token)
        {
            this.address = address;
            this.port = port;
            this.pskIdentity = pskIdentity;
            this.psk = psk;
            this.token = token;
            this.maxBufferSize = maxBufferSize;
            Id = "tcp-" + Guid.NewGuid().ToString();
            this.queue = new Queue<byte[]>();
        }

        

        public TcpClientChannel(IPAddress address, int port, string pskIdentity, byte[] psk, int maxBufferSize, CancellationToken token)
            : this(address, port, null, pskIdentity, psk, maxBufferSize, token)
        {
        }


        public TcpClientChannel(string hostname, int port, string pskIdentity, byte[] psk, int maxBufferSize, CancellationToken token)
            : this(hostname, port, null, pskIdentity, psk, maxBufferSize, token)
        {

        }

        public TcpClientChannel(string hostname, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, int maxBufferSize, CancellationToken token)
        {
            this.hostname = hostname;
            this.port = port;
            this.pskIdentity = pskIdentity;
            this.localEP = localEP;
            this.psk = psk;
            this.maxBufferSize = maxBufferSize;
            this.token = token;
            Id = "tcp-" + Guid.NewGuid().ToString();
            this.queue = new Queue<byte[]>();
        }

        public TcpClientChannel(IPEndPoint remoteEP, string pskIdentity, byte[] psk, int maxBufferSize, CancellationToken token)
            : this(remoteEP, null, pskIdentity, psk, maxBufferSize, token)
        {

        }

        public TcpClientChannel(IPEndPoint remoteEP, IPEndPoint localEP, string pskIdentity, byte[] psk, int maxBufferSize, CancellationToken token)
        {
            this.remoteEP = remoteEP;
            this.localEP = localEP;
            this.pskIdentity = pskIdentity;
            this.psk = psk;
            this.maxBufferSize = maxBufferSize;
            this.token = token;
            Id = "tcp-" + Guid.NewGuid().ToString();
            this.queue = new Queue<byte[]>();
        }

        #endregion

        #region private member variables

        private TlsClientProtocol protocol;
        private int maxBufferSize;
        private string pskIdentity;
        private byte[] psk;
        private IPEndPoint localEP;
        private X509Certificate2 certificate;
        private TcpClient client;
        private int port;
        private IPAddress address;
        private string hostname;
        private IPEndPoint remoteEP;
        private CancellationToken token;        
        private Stream stream;
        private SemaphoreSlim readConnection;
        private SemaphoreSlim writeConnection;
        private NetworkStream localStream;
        private bool disposed;
        private ChannelState _state;
        private Queue<byte[]> queue;

        #endregion

        #region events

        public override event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public override event EventHandler<ChannelCloseEventArgs> OnClose;
        public override event EventHandler<ChannelOpenEventArgs> OnOpen;
        public override event EventHandler<ChannelErrorEventArgs> OnError;
        public override event EventHandler<ChannelStateEventArgs> OnStateChange;

        #endregion

        #region Properties

        public override bool IsConnected
        {
            get
            {
                return State == ChannelState.Open;
            }
        }

        public override bool RequireBlocking
        {
            get { return psk != null; }
        }
        public override string TypeId { get { return "TCP"; } }

        public override int Port { get; internal set; }

        public override string Id { get; internal set; }

        public override bool IsEncrypted { get; internal set; }

        public override bool IsAuthenticated { get; internal set; }

        public override ChannelState State
        {
            get
            {
                return _state;
            }
            internal set
            {
                if(_state != value)
                {
                    OnStateChange?.Invoke(this, new ChannelStateEventArgs(Id, value));
                }

                _state = value;
            }
        }

        #endregion

        #region methods
        public override async Task AddMessageAsync(byte[] message)
        {
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
            await Task.CompletedTask;
        }
        public override async Task CloseAsync()
        {
            if (protocol != null)
            {
                protocol.Close();
            }

            if (client != null)
            {
                State = ChannelState.ClosedReceived;
                stream.Close();
                client.Close();
            }

            if (State != ChannelState.Closed)
            {
                State = ChannelState.Closed;
                OnClose?.Invoke(this, new ChannelCloseEventArgs(Id));
            }
            await Task.CompletedTask;
        }

        public override async Task OpenAsync()
        {
            State = ChannelState.Connecting;

            if(localEP != null)
            {
                client = new TcpClient(localEP);
            }
            else
            {
                client = new TcpClient();
            }
                        
            if(remoteEP != null)
            {                
                await client.ConnectAsync(remoteEP.Address, remoteEP.Port);
            }
            else if(address != null)
            {
                await client.ConnectAsync(address, port);
            }
            else if(!String.IsNullOrEmpty(hostname))
            {
                await client.ConnectAsync(hostname, port);
            }
            else
            {
                State = ChannelState.Aborted;
                InvalidOperationException ioe = new InvalidOperationException("Tcp client connection parameters not sufficient.");
                Trace.TraceError(ioe.Message);
                throw ioe;
            }            
            
            readConnection = new SemaphoreSlim(1);
            writeConnection = new SemaphoreSlim(1);

            localStream = client.GetStream();

            if (psk != null)
            {
                protocol = client.ConnectPskTlsClient(pskIdentity, psk, localStream);
                stream = protocol.Stream;
                IsEncrypted = true;
            }
            else if (certificate != null)
            {                
                stream = new SslStream(localStream, true, new RemoteCertificateValidationCallback(ValidateCertificate));
                IsEncrypted = true;

                try
                {
                    X509CertificateCollection certificates = new X509CertificateCollection();
                    X509Certificate cert = new X509Certificate(certificate.RawData);
                    certificates.Add(cert);
                    SslStream sslStream = (SslStream)stream;                    
                    await sslStream.AuthenticateAsClientAsync(hostname, certificates, SslProtocols.Tls12, true);

                    if(!sslStream.IsEncrypted || !sslStream.IsSigned)
                    {
                        throw new AuthenticationException("SSL stream is not both encrypted and signed.");
                    }                    
                }
                catch (Exception ex)
                {
                    State = ChannelState.Aborted;
                    Trace.TraceError("TCP client channel open error {0}", ex.Message);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
                    throw;
                }
            }
            else
            {
                stream = localStream;
            }

            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, null));            
        }

        public override async Task ReceiveAsync()
        {
            byte[] buffer = null;
            byte[] prefix = null;
            int remainingLength = 0;
            int offset = 0;
            int bytesRead = 0;

            try
            {
                while (client.Connected && !token.IsCancellationRequested)
                {
                    await readConnection.WaitAsync();

                    while (offset < 4) //read the prefix to discover the length of the message (big endian)
                    {
                        if (offset == 0)
                        {
                            prefix = new byte[4];
                        }
                        bytesRead = await stream.ReadAsync(prefix, offset, prefix.Length - offset);

                        if (bytesRead == 0)
                        {                            
                            await CloseAsync();
                            return;
                        }

                        offset += bytesRead;
                    }

                    //ensure the length prefix is ordered correctly to calc the remaining length
                    prefix = BitConverter.IsLittleEndian ? prefix.Reverse().ToArray() : prefix;
                    remainingLength = BitConverter.ToInt32(prefix, 0);

                    if(remainingLength >= maxBufferSize)
                    {
                        throw new IndexOutOfRangeException("TCP client channel receive message exceeds max buffer size for receiveasync");
                    }

                    offset = 0;

                    byte[] message = new byte[remainingLength];

                    //loop through the messages to ensure they are pieced together based on the prefix length
                    while (remainingLength > 0)
                    {
                        buffer = new byte[remainingLength];
                        bytesRead = await stream.ReadAsync(buffer, 0, remainingLength);
                        remainingLength -= bytesRead;
                        Buffer.BlockCopy(buffer, 0, message, offset, bytesRead);
                        offset += bytesRead;
                    }

                    OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
                    
                    offset = 0;
                    readConnection.Release();
                }
            }
            catch(Exception ex)
            {
                State = ChannelState.Aborted;
                Trace.TraceError(ex.Message);
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            }
            finally
            {
                await CloseAsync();
            }
        }

        public override async Task SendAsync(byte[] msg)
        {

            if(msg == null || msg.Length == 0)
            {
                throw new IndexOutOfRangeException("TCP client channel cannot send null or 0-length message for sendasync-1");
            }

            if (msg.Length > maxBufferSize)
            {
                throw new IndexOutOfRangeException("TCP server channel message exceeds max buffer size for sendasync-1");
            }

            queue.Enqueue(msg);

            while (queue.Count > 0)
            {
                byte[] message = queue.Dequeue();

                try
                {
                    await writeConnection.WaitAsync();

                    byte[] prefix = BitConverter.IsLittleEndian ? BitConverter.GetBytes(message.Length).Reverse().ToArray() : BitConverter.GetBytes(message.Length);
                    byte[] buffer = new byte[prefix.Length + message.Length];
                    Buffer.BlockCopy(prefix, 0, buffer, 0, prefix.Length);
                    Buffer.BlockCopy(message, 0, buffer, prefix.Length, message.Length);

                    if (stream != null && stream.CanWrite)
                    {
                        if (psk != null)
                        {
                            stream.Write(buffer, 0, buffer.Length);
                            stream.Flush();
                        }
                        else
                        {
                            stream.Write(buffer, 0, buffer.Length);
                            stream.Flush();
                        }                        
                    }
                    else
                    {
                        throw new IOException(String.Format("Channel {0} tcp client channel cannot send because stream is not writable at this time.", Id));
                    }
                }
                catch (AggregateException ae)
                {
                    State = ChannelState.Aborted;
                    Trace.TraceError("Channel {0} tcp client channel failed to send with {1}", Id, ae.Flatten().InnerException.Message);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ae.Flatten().InnerException));

                }
                catch (Exception ex)
                {
                    State = ChannelState.Aborted;
                    Trace.TraceError("Channel {0} tcp client channel failed to send with {1}", Id, ex.Message);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
                }
                finally
                {
                    writeConnection.Release();
                }
            }
        }

        protected void Disposing(bool dispose)
        {
            if (dispose & !disposed)
            {

                if (readConnection != null)
                {
                    readConnection.Dispose();
                }

                if (writeConnection != null)
                {
                    writeConnection.Dispose();
                }

                if (protocol != null)
                {
                    protocol.Close();
                }

                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }

                if (client != null && IsConnected)
                {
                    client.Close();
                    client.Dispose();
                }

                disposed = true;
            }
        }
        
        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        #endregion


        #region private methods
        private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            if (sslpolicyerrors != SslPolicyErrors.None)
            {
                return false;
            }

            if (certificate == null)
            {
                return false;
            }
            else
            {
                X509Certificate2 cert = new X509Certificate2(certificate);
                return (cert.NotBefore < DateTime.Now && cert.NotAfter > DateTime.Now);
            }
        }
        #endregion
    }
}
