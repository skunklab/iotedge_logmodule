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
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;

namespace SkunkLab.Channels.Tcp
{
    public class TcpClientChannel : TcpChannel
    {
        #region ctor
        public TcpClientChannel(string hostname, int port, CancellationToken token)
            : this(hostname, port, null, null, token)
        {            
        }

        public TcpClientChannel(string hostname, int port, IPEndPoint localEP, CancellationToken token)
            : this(hostname, port, localEP, null, token)
        {
        }

        public TcpClientChannel(IPEndPoint remoteEndpoint, CancellationToken token)
            : this(remoteEndpoint, null, null, token)
        {
        }

        public TcpClientChannel(IPEndPoint remoteEndpoint, IPEndPoint localEP, CancellationToken token)
            : this(remoteEndpoint, localEP, null, token)
        {
        }

        public TcpClientChannel(IPAddress address, int port, CancellationToken token)
            : this(address, port, null, null, token)
        {
        }

        public TcpClientChannel(IPAddress address, int port, IPEndPoint localEP, CancellationToken token)
            : this(address, port, localEP, null, token)
        {
        }

        public TcpClientChannel(string hostname, int port,  X509Certificate2 certificate, CancellationToken token)
            :this(hostname, port, null, certificate, token)
        {
        }

        public TcpClientChannel(string hostname, int port, IPEndPoint localEP, X509Certificate2 certificate, CancellationToken token)
        {
            this.hostname = hostname;
            this.port = port;
            this.localEP = localEP;
            this.certificate = certificate;
            this.token = token;
            this.token.Register(async () => await CloseAsync());

            Port = port;           
        }
        
        public TcpClientChannel(IPEndPoint remoteEndpoint, X509Certificate2 certificate, CancellationToken token)
            : this(remoteEndpoint, null, certificate, token)
        {
        }

        public TcpClientChannel(IPEndPoint remoteEndpoint, IPEndPoint localEP, X509Certificate2 certificate, CancellationToken token)
        {
            if(remoteEndpoint == null)
            {
                throw new ArgumentNullException("remoteEndpoint");
            }

            remoteEP = remoteEndpoint;
            this.localEP = localEP;
            this.certificate = certificate;
            this.token = token;
            this.token.Register(async () => await CloseAsync());

            if (certificate != null)
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(remoteEndpoint.Address);
                hostname = ipHostInfo.HostName;
            }

            Port = remoteEndpoint.Port;
        }

        public TcpClientChannel(IPAddress address, int port, X509Certificate2 certificate, CancellationToken token)
            : this(address, port, null, certificate, token)
        {

        }
        public TcpClientChannel(IPAddress address, int port, IPEndPoint localEP, X509Certificate2 certificate, CancellationToken token)
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
            this.token.Register(async () => await CloseAsync());

            if (certificate != null)
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(address);
                hostname = ipHostInfo.HostName;
            }

            Port = port;
        }

        public TcpClientChannel(IPAddress address, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, CancellationToken token)
        {
            this.address = address;
            this.port = port;
            this.pskIdentity = pskIdentity;
            this.psk = psk;
            this.token = token;
        }

        //public TcpClientChannel(IPEndPoint remoteEndpoint, string pskIdentity, byte[] psk, CancellationToken token)
        //{
        //    remoteEP = remoteEndpoint;
        //    this.pskIdentity = pskIdentity;
        //    this.psk = psk;
        //    this.token = token;
        //}
        #endregion

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

        public override event ChannelReceivedEventHandler OnReceive;
        public override event ChannelCloseEventHandler OnClose;
        public override event ChannelOpenEventHandler OnOpen;
        public override event ChannelErrorEventHandler OnError;
        public override event ChannelStateEventHandler OnStateChange;
        public override event ChannelRetryEventHandler OnRetry;
        public override event ChannelSentEventHandler OnSent;

        public override bool IsConnected
        {
            get
            {
                return State == ChannelState.Open;
            }
        }

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
            await TaskDone.Done;
        }
        public override async Task CloseAsync()
        {
            if (client != null && IsConnected)
            {
                State = ChannelState.ClosedReceived;
                client.Close();
            }

            if (State != ChannelState.Closed)
            {
                State = ChannelState.Closed;
                OnClose?.Invoke(this, new ChannelCloseEventArgs(Id));
            }
            await TaskDone.Done;
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
                        stream.Dispose();
                        throw new AuthenticationException("SSL stream is not both encrypted and signed.");
                    }                    
                }
                catch(AggregateException ae)
                {
                    State = ChannelState.Aborted;
                    Trace.TraceError(ae.Flatten().Message);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ae));
                    throw;
                }
                catch (Exception ex)
                {
                    stream.Dispose();
                    State = ChannelState.Aborted;
                    Trace.TraceError(ex.Message);
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
                        offset += bytesRead;
                    }

                    //ensure the length prefix is ordered correctly to calc the remaining length
                    prefix = BitConverter.IsLittleEndian ? prefix.Reverse().ToArray() : prefix;
                    remainingLength = BitConverter.ToInt32(prefix, 0);

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
                }

                writeConnection.Release();
                OnSent?.Invoke(this, new ChannelSentEventArgs(Id, null));
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
                if (client != null && IsConnected)
                {
                    client.Close();
                }

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
                if (client.Client != null)
                {
                    client.Client.Dispose();
                }

                client = null;
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
