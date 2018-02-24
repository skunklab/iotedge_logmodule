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
    public class TcpServerChannel : TcpChannel
    {
        public TcpServerChannel(TcpClient client, CancellationToken token)
        {
            this.client = client;
            this.token = token;
            this.token.Register(async () => await CloseAsync());
            Id = Guid.NewGuid().ToString();
            Port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
        }

        public TcpServerChannel(TcpClient client, X509Certificate2 certificate, bool clientAuth, CancellationToken token)
        {
            this.client = client;
            this.certificate = certificate;
            this.clientAuth = clientAuth;
            this.token = token;
            this.token.Register(async () => await CloseAsync());
            Id = Guid.NewGuid().ToString();
        }

        public TcpServerChannel(TcpClient client, string pskIdentity, byte[] psk, CancellationToken token)
        {
            this.client = client;
            this.pskIdentity = pskIdentity;
            this.psk = psk;
            this.token = token;
            this.token.Register(async () => await CloseAsync());
            Id = Guid.NewGuid().ToString();
        }

        private SecureRandom srandom;
        private string pskIdentity;
        private byte[] psk;
        private bool clientAuth;
        private X509Certificate2 certificate;
        private CancellationToken token;
        private TcpClient client;
        private bool disposed;
        private ChannelState _state;
        private Stream stream;
        private SemaphoreSlim readConnection;
        private SemaphoreSlim writeConnection;
        private NetworkStream localStream;

        public override event ChannelReceivedEventHandler OnReceive;
        public override event ChannelCloseEventHandler OnClose;
        public override event ChannelOpenEventHandler OnOpen;
        public override event ChannelErrorEventHandler OnError;
        public override event ChannelStateEventHandler OnStateChange;
        public override event ChannelRetryEventHandler OnRetry;
        public override event ChannelSentEventHandler OnSent;

        public override string Id { get; internal set; }

        public override int Port { get; internal set; }

        public override bool IsEncrypted { get; internal set; }

        public override bool IsAuthenticated { get; internal set; }

        public override bool IsConnected { get { return State == ChannelState.Open; } }

        public override ChannelState State        {
            get
            {
                return _state;
            }

            internal set
            {
                if(value != _state)
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
            if(client != null && IsConnected)
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

            readConnection = new SemaphoreSlim(1);
            writeConnection = new SemaphoreSlim(1);
            
            try
            {
                localStream = client.GetStream();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if(psk != null)
            {
                srandom = new SecureRandom();
                TlsServerProtocol protocol = client.ConnectPskTlsServer(psk, srandom);
                stream = protocol.Stream;
            }
            else if (certificate != null)
            {
                stream = new SslStream(localStream, true, new RemoteCertificateValidationCallback(ValidateCertificate));

                try
                {                 
                    await ((SslStream)stream).AuthenticateAsServerAsync(certificate, clientAuth, SslProtocols.Tls12, true);                   
                }
                catch (AggregateException ae)
                {
                    State = ChannelState.Aborted;
                    Trace.TraceError(ae.Flatten().Message);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ae));
                    throw;
                }
                catch (Exception ex)
                {
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
            catch (AggregateException ae)
            {
                State = ChannelState.Aborted;
                Trace.TraceError(ae.Flatten().Message);
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ae));
            }
            catch (Exception ex)
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
            catch (AggregateException ae)
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

            if (error != null)
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
                if(client != null && IsConnected)
                {
                    client.Close();                   
                }

                readConnection.Release();
                readConnection.Dispose();

                writeConnection.Release();
                writeConnection.Dispose();

                if(client.Client != null)
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
