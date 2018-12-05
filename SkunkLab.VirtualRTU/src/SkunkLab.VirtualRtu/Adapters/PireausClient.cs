using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.VirtualRtu.Adapters
{
    public class PiraeusClient : IDisposable
    {
        private IChannel channel;
        private CancellationTokenSource source;
        private PiraeusMqttClient client;
        private const string CONTENT_TYPE = "applicaton/octet-stream";
        private VRtuConfig config;
        private bool disposed;

        public event System.EventHandler<SubscriptionEventArgs> OnReceive;

        public PiraeusClient(VRtuConfig config, string pskString)
        {
            this.config = config;
            source = new CancellationTokenSource();
            channel = ChannelFactory.Create(false, config.Hostname, config.Port, config.PskIdentity, Convert.FromBase64String(pskString), config.BlockSize, config.MaxBufferSize, source.Token);
            MqttConfig mqttConfig = new MqttConfig(config.KeepAliveInterval);
            client = new PiraeusMqttClient(mqttConfig, channel);
        }


        public async Task<ConnectAckCode> ConnectAsync()
        {
            try
            {
                ConnectAckCode code = await client.ConnectAsync(Guid.NewGuid().ToString(), "JWT", config.SecurityToken, config.KeepAliveInterval);
                return code;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Trace.TraceError(ex.Message);
            }
        }

        public async Task SubscribeAsync(string resourceUriString)
        {
            try
            {
                await client.SubscribeAsync(resourceUriString, QualityOfServiceLevelType.AtMostOnce, SubscriptionResult);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        public async Task SendAsync(string resourceUriString, byte[] message)
        {
            try
            {
                await client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, resourceUriString, CONTENT_TYPE, message);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        private void SubscriptionResult(string resourceUriString, string contentType, byte[] message)
        {
            //raise receive event
            OnReceive?.Invoke(this, new SubscriptionEventArgs(resourceUriString, contentType, message));
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    try
                    {
                        if (client != null)
                        {
                            Task x = client.CloseAsync();
                            Task.WaitAll(x);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Trace.TraceError(ex.Message);
                    }
                }

                disposed = true;
            }
        }
    }
}
