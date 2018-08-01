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

namespace SkunkLab.Channels.Core.Tcp
{
    public class TcpServerChannel : TcpChannel
    {
        public TcpServerChannel(TcpClient client, int maxBufferSize, CancellationToken token)
        {
            this.client = client;
            this.token = token;
            this.maxBufferSize = maxBufferSize;
            this.token.Register(async () => await CloseAsync());
            Id = "tcp-" + Guid.NewGuid().ToString();
            Port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
        }

        public TcpServerChannel(TcpClient client, X509Certificate2 certificate, bool clientAuth, int maxBufferSize, CancellationToken token)
        {
            this.client = client;
            this.certificate = certificate;
            this.clientAuth = clientAuth;
            this.token = token;
            this.maxBufferSize = maxBufferSize;
            this.token.Register(async () => await CloseAsync());
            Id = "tcp-" + Guid.NewGuid().ToString();
            Port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
        }

        public TcpServerChannel(TcpClient client, Dictionary<string,byte[]> presharedKeys, int maxBufferSize, CancellationToken token)
        {
            this.client = client;
            this.presharedKeys = presharedKeys;
            this.token = token;
            this.maxBufferSize = maxBufferSize;
            this.token.Register(async () => await CloseAsync());
            Id = "tcp-" + Guid.NewGuid().ToString();
            Port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
        }


        private TlsServerProtocol protocol;
        private int maxBufferSize;
        private Dictionary<string, byte[]> presharedKeys;
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

        public override event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public override event EventHandler<ChannelCloseEventArgs> OnClose;
        public override event EventHandler<ChannelOpenEventArgs> OnOpen;
        public override event EventHandler<ChannelErrorEventArgs> OnError;
        public override event EventHandler<ChannelStateEventArgs> OnStateChange;

        public override string Id { get; internal set; }

        public override bool RequireBlocking
        {
            get { return false; }
        }

        public override string TypeId { get { return "TCP"; } }

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
            await Task.CompletedTask;
        }
        public override async Task CloseAsync()
        {
            if(protocol != null)
            {
                protocol.Close();
            }

            if(client != null)
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

            await TaskDone.Done;
        }

        public override async Task OpenAsync()
        {
            State = ChannelState.Connecting;

            readConnection = new SemaphoreSlim(1);
            writeConnection = new SemaphoreSlim(1);

            localStream = client.GetStream();

            if (presharedKeys != null)
            {
                protocol = client.ConnectPskTlsServer(presharedKeys, localStream);
                stream = protocol.Stream;
                IsEncrypted = true;
            }
            else if (certificate != null)
            {
                stream = new SslStream(localStream, true, new RemoteCertificateValidationCallback(ValidateCertificate));
                IsEncrypted = true;

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
            //await Log.LogInfoAsync("Channel {0} tcp server channel receiving.", Id);

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
                        if(bytesRead == 0)
                        {
                            await CloseAsync();
                            return;
                        }
                        offset += bytesRead;
                    }

                    //ensure the length prefix is ordered correctly to calc the remaining length
                    prefix = BitConverter.IsLittleEndian ? prefix.Reverse().ToArray() : prefix;
                    remainingLength = BitConverter.ToInt32(prefix, 0);

                    if (remainingLength >= maxBufferSize)
                    {
                        throw new IndexOutOfRangeException("TCP server channel receive message exceeds max buffer size for receiveasync");
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

            if (message == null || message.Length == 0)
            {
                throw new IndexOutOfRangeException("TCP client channel cannot send null or 0-length message for sendasync-1");
            }

            if (message.Length > maxBufferSize)
            {
                throw new IndexOutOfRangeException("TCP server channel message exceeds max buffer size for sendasync-1");
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
                }
                else
                {
                    //await Log.LogInfoAsync("Channel {0} tcp server channel cannot send because stream is not writable at this time.");
                }

                writeConnection.Release();
                
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

                protocol = null;

                if (client != null)
                {
                    State = ChannelState.ClosedReceived;
                    client.Close();
                    stream.Close();
                    client = null;
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
