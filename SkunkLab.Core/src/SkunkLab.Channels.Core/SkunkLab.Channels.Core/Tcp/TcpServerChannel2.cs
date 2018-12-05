using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Tcp
{
    public class TcpServerChannel2 : TcpChannel
    {
        #region ctor
        public TcpServerChannel2(TcpClient client, int blockSize, int maxBufferSize, CancellationToken token)
        {
            this.client = client;
            this.blockSize = blockSize;
            this.maxBufferSize = maxBufferSize;
            this.token = token;
            this.token.Register(async () => await CloseAsync());
            Id = "tcp-" + Guid.NewGuid().ToString();
            Port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
            this.queue = new Queue<byte[]>();
        }

        public TcpServerChannel2(TcpClient client, X509Certificate2 certificate, bool clientAuth, int blockSize, int maxBufferSize, CancellationToken token)
        {
            this.client = client;
            this.certificate = certificate;
            this.clientAuth = clientAuth;
            this.blockSize = blockSize;
            this.maxBufferSize = maxBufferSize;
            this.token = token;
            this.token.Register(async () => await CloseAsync());
            Id = "tcp-" + Guid.NewGuid().ToString();
            Port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
            this.queue = new Queue<byte[]>();
        }

        public TcpServerChannel2(TcpClient client, Dictionary<string, byte[]> presharedKeys, int blockSize, int maxBufferSize, CancellationToken token)
        {
            this.client = client;
            this.presharedKeys = presharedKeys;
            this.blockSize = blockSize;
            this.maxBufferSize = maxBufferSize;
            this.token = token;
            this.token.Register(async () => await CloseAsync());
            Id = "tcp-" + Guid.NewGuid().ToString();
            Port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
            this.queue = new Queue<byte[]>();
        }

        #endregion


        #region private member variables

        private TlsServerProtocol protocol;
        private readonly Dictionary<string, byte[]> presharedKeys;
        private readonly bool clientAuth;
        private readonly X509Certificate2 certificate;
        private CancellationToken token;
        private TcpClient client;
        private bool disposed;
        private ChannelState _state;
        private Stream stream;
        private SemaphoreSlim readConnection;
        private SemaphoreSlim writeConnection;
        private NetworkStream localStream;
        private readonly int maxBufferSize;
        private readonly int blockSize;
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
        public override string Id { get; internal set; }

        public override bool RequireBlocking
        {
            get { return presharedKeys != null; }
        }

        public override string TypeId { get { return "TCP2"; } }

        public override int Port { get; internal set; }

        public override bool IsEncrypted { get; internal set; }

        public override bool IsAuthenticated { get; internal set; }

        public override bool IsConnected { get { return State == ChannelState.Open; } }

        public override ChannelState State
        {
            get
            {
                return _state;
            }

            internal set
            {
                if (value != _state)
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

        public override async Task OpenAsync()
        {
            State = ChannelState.Connecting;

            readConnection = new SemaphoreSlim(1);
            writeConnection = new SemaphoreSlim(1);

            try
            {
                localStream = client.GetStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fault opening TCP Channel 2  - {0}", ex.Message);
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            }

            if (presharedKeys != null)
            {
                try
                {
                    protocol = client.ConnectPskTlsServer(presharedKeys, localStream);
                    stream = protocol.Stream;
                    IsEncrypted = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fault opening TLS connection {0}", ex.Message);
                    State = ChannelState.Aborted;
                    Trace.TraceError(ex.Message);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
                    return;
                }
            }
            else if (certificate != null)
            {
                try
                {
                    stream = new SslStream(localStream, true, new RemoteCertificateValidationCallback(ValidateCertificate));
                    IsEncrypted = true;
                    await ((SslStream)stream).AuthenticateAsServerAsync(certificate, clientAuth, SslProtocols.Tls12, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fault opening TCP Channel 2  via Cert - {0}", ex.Message);
                    State = ChannelState.Aborted;
                    Trace.TraceError(ex.Message);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
                    return;
                }
            }
            else
            {
                stream = localStream;
            }

            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, null));
        }

        public override async Task SendAsync(byte[] msg)
        {
            if (msg == null || msg.Length == 0)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, new IndexOutOfRangeException("TCP server channel cannot send null or 0-length message for sendasync-2")));
            }

            if (msg.Length > maxBufferSize)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, new IndexOutOfRangeException("TCP server channel message exceeds max buffer size for sendasync-2")));
            }

            await writeConnection.WaitAsync();
            queue.Enqueue(msg);

            while (queue.Count > 0)
            {
                byte[] message = queue.Dequeue();

                try
                {

                    if (protocol != null)
                    {
                        stream.Write(msg, 0, msg.Length);
                        stream.Flush();
                    }
                    else
                    {
                        await stream.WriteAsync(msg, 0, msg.Length);
                        await stream.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fault sending TCP Channel 2  - {0}", ex.Message);
                    Trace.TraceError(ex.Message);
                    State = ChannelState.Aborted;
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
                }
            }

            writeConnection.Release();
        }

        public override async Task ReceiveAsync()
        {
            Exception error = null;
            byte[] buffer = null;
            int bytesRead = 0;
            byte[] msgBuffer = null;

            try
            {
                while (client != null && client.Connected && !token.IsCancellationRequested)
                {
                    try
                    {
                        await readConnection.WaitAsync();
                        using (MemoryStream bufferStream = new MemoryStream())
                        {
                            do
                            {
                                buffer = new byte[16384];

                                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                                if (bytesRead == 0 && bufferStream.Length == 0)
                                {
                                    Console.WriteLine("Forcing return");
                                    return; //closing instruction from client
                                }

                                if (bytesRead + bufferStream.Length > maxBufferSize)
                                {
                                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, new InvalidDataException("Message exceeds max buffer size to read.")));
                                    return;
                                }

                                await bufferStream.WriteAsync(buffer, 0, bytesRead);


                            } while (localStream.DataAvailable && bytesRead == 16384);

                            await bufferStream.FlushAsync(); //flush the writable stream
                            bufferStream.Position = 0;  //position to beginning

                            //load the messag buffer with the data
                            msgBuffer = new byte[bufferStream.Length];
                            await bufferStream.ReadAsync(msgBuffer, 0, msgBuffer.Length);

                            readConnection.Release();
                        }


                        if (msgBuffer != null && msgBuffer.Length > 0)  //make sure data is available for the message
                        {
                            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, msgBuffer));
                        }


                    }
                    finally
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fault receiving TCP Channel 2  - {0}", ex.Message);
                error = ex;
            }
            finally
            {
                Console.WriteLine("TCP Channel 2 receiver is closing.");
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id,  error ?? new TimeoutException("Receiver closing")));
            }
        }

        public override async Task CloseAsync()
        {
            Console.WriteLine("Starting TCP Channel 2 close action");

            if (State == ChannelState.Closed || State == ChannelState.ClosedReceived)
            {
                Console.WriteLine("TCP channel 2 is already closed...returning");
                return;
            }

            State = ChannelState.ClosedReceived;

            try
            {
                if (protocol != null)
                {
                    protocol.Close();
                }
            }
            catch { }

            protocol = null;

            if (client != null && client.Client != null && (client.Client.Connected && client.Client.Poll(10, SelectMode.SelectRead)))
            {
                if (client.Client.UseOnlyOverlappedIO)
                {
                    client.Client.DuplicateAndClose(Process.GetCurrentProcess().Id);
                }
                else
                {
                    client.Close();
                }
            }

            client = null;

            if (readConnection != null)
            {
                readConnection.Dispose();
            }

            if (writeConnection != null)
            {
                writeConnection.Dispose();
            }

            State = ChannelState.Closed;
            OnClose?.Invoke(this, new ChannelCloseEventArgs(Id));

            await Task.CompletedTask;
        }

        #endregion

        #region dispose
        protected void Disposing(bool dispose)
        {
            if (dispose & !disposed)
            {
                disposed = true;
                if (!(State == ChannelState.Closed || State == ChannelState.ClosedReceived))
                {
                    try
                    {
                        Task task = CloseAsync();
                        Task.WaitAll(task);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception Dispose/Closing TCP Server 2 {0}", ex.Message);
                        Console.WriteLine("***** Inner Exception {0} *****", ex.InnerException);
                        Console.WriteLine("***** Stack Trace {0} *****", ex.InnerException.StackTrace);
                    }
                }

                protocol = null;
                client = null;
                readConnection = null;
                writeConnection = null;
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

