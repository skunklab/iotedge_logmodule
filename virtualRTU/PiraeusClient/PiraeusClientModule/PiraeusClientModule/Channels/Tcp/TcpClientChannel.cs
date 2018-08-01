using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using System;
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

namespace PiraeusClientModule.Channels.Tcp
{
    public class TcpClientChannel : TcpChannel
    {
        #region ctor
        public TcpClientChannel(string hostname, int port, int maxBufferSize, CancellationToken token)
            : this(hostname, port, null, null, maxBufferSize, token)
        {            
        }

        public TcpClientChannel(string hostname, int port, IPEndPoint localEP, int maxBufferSize, CancellationToken token)
            : this(hostname, port, localEP, null, maxBufferSize, token)
        {
        }

        public TcpClientChannel(IPEndPoint remoteEndpoint, int maxBufferSize, CancellationToken token)
            : this(remoteEndpoint, null, null, maxBufferSize, token)
        {
        }

        public TcpClientChannel(IPEndPoint remoteEndpoint, IPEndPoint localEP, int maxBufferSize, CancellationToken token)
            : this(remoteEndpoint, localEP, null, maxBufferSize, token)
        {
        }

        public TcpClientChannel(IPAddress address, int port, int maxBufferSize, CancellationToken token)
            : this(address, port, null, null, maxBufferSize, token)
        {
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
        }


        #endregion

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
        private SecureRandom srandom;

        public override event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public override event EventHandler<ChannelCloseEventArgs> OnClose;
        public override event EventHandler<ChannelOpenEventArgs> OnOpen;
        public override event EventHandler<ChannelErrorEventArgs> OnError;
        public override event EventHandler<ChannelStateEventArgs> OnStateChange;


        public override bool IsConnected
        {
            get
            {
                return State == ChannelState.Open;
            }
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


        public override async Task AddMessageAsync(byte[] message)
        {
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
            await Task.CompletedTask;
        }
        public override async Task CloseAsync()
        {
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
                srandom = new SecureRandom();
                TlsClientProtocol protocol = client.ConnectPskTlsClient(pskIdentity, psk, srandom);
                stream = protocol.Stream;
            }
            else if (certificate != null)
            {                
                stream = new SslStream(localStream, true, new RemoteCertificateValidationCallback(ValidateCertificate));

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
                    //await Log.LogErrorAsync("TCP client channel open error {0}", ex.Message);
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
            //await Log.LogInfoAsync("Channel {0} tcp client channel receiving.", Id);

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
                        throw new IndexOutOfRangeException("TCP client channel receive message exceeds max buffer size");
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
            catch(AggregateException ae)
            {
                State = ChannelState.Aborted;
                Trace.TraceError(ae.Flatten().Message);
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ae));
            }
            catch(Exception ex)
            {
                if (State != ChannelState.ClosedReceived && State != ChannelState.Closed)
                {
                    State = ChannelState.Aborted;
                    Trace.TraceError(ex.Message);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
                }
            }
            finally
            {
                await CloseAsync();
            }
        }

        public override async Task SendAsync(byte[] message)
        {
            Exception error = null;
            string errorMsg = null;

            if(message == null || message.Length == 0)
            {
                throw new IndexOutOfRangeException("TCP client channel cannot send null or 0-length message");
            }

            if (message.Length > maxBufferSize)
            {
                throw new IndexOutOfRangeException("TCP server channel message exceeds max buffer size");
            }

            try
            {
                await writeConnection.WaitAsync();

                byte[] prefix = BitConverter.IsLittleEndian ? BitConverter.GetBytes(message.Length).Reverse().ToArray() : BitConverter.GetBytes(message.Length);
                byte[] buffer = new byte[prefix.Length + message.Length];
                Buffer.BlockCopy(prefix, 0, buffer, 0, prefix.Length);
                Buffer.BlockCopy(message, 0, buffer, prefix.Length, message.Length);

                if (stream != null && stream.CanWrite)
                {
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    await stream.FlushAsync();
                    //await Log.LogInfoAsync("Channel {0} tcp client channel sent message.");
                }
                else
                {
                    //await Log.LogInfoAsync("Channel {0} tcp client channel cannot send because stream is not writable at this time.");
                }

                writeConnection.Release();
                
            }
            catch(AggregateException ae)
            {
                State = ChannelState.Aborted;
                errorMsg = ae.Flatten().Message;
                error = ae;
                
            }
            catch (Exception ex)
            {
                State = ChannelState.Aborted;
                errorMsg = ex.Message;
                error = ex;
            }

            if(error != null)
            {
                Trace.TraceError(errorMsg);
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, error));
                await CloseAsync();
            }
        }

        protected void Disposing(bool dispose)
        {
            if (dispose & !disposed)
            {

                if (readConnection != null)
                {
                    readConnection.Release();
                    readConnection.Dispose();
                }

                if (writeConnection != null)
                {
                    writeConnection.Release();
                    writeConnection.Dispose();
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

    }
}
