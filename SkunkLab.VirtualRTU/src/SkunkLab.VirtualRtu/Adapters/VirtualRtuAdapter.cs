using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.VirtualRtu.ModBus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.VirtualRtu.Adapters
{
    /// <summary>
    /// Virtual RTU adapter for communication with SCADA software of ModBus-TCP
    /// </summary>
    public class VirtualRtuAdapter : IDisposable
    {
        public VirtualRtuAdapter(VRtuConfig rtuConfig, RtuMap map,  IChannel channel)
        {
            this.rtuConfig = rtuConfig;
            this.map = map;
            pskString = Convert.ToBase64String(rtuConfig.Psk);
            cache = new MbapCache();
            source = new CancellationTokenSource();
            subscribed = new HashSet<ushort>();            

            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnOpen += Channel_OnOpen;            
        }

        private string pskString;
        private PiraeusMqttClient client;
        private CancellationTokenSource ptokenSource;
        private VRtuConfig rtuConfig;
        private RtuMap map;
        private MbapCache cache;
        private CancellationTokenSource source;
        public IChannel Channel { get; internal set; }
        private const string contentType = "application/octet-stream";
        private HashSet<ushort> subscribed;
        private bool disposed;

        public event System.EventHandler<AdapterEventArgs> OnError;
        public event System.EventHandler<AdapterEventArgs> OnClose;

       
        #region SCADA Software Channel
        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            try
            {
                Task task = OpenPiraeusClient();
                Task.WaitAll(task);
            }
            catch(Exception ex)
            {
                Console.Write("Fault opening client channel - {0}", ex.Message);
                OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, ex));
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            byte[] buffer = null;
            MbapHeader header = null;

            try
            {
                buffer = new byte[7];
                Buffer.BlockCopy(e.Message, 0, buffer, 0, buffer.Length);
                header = MbapHeader.Decode(buffer);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Failed to read MBAP Header with '{0}'", ex.Message);
                OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, ex));
                return;
            }

            if (!map.Map.ContainsKey(header.UnitId))
            {
                this.OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, new InvalidOperationException("Unit ID is not cached/mapped in MBAP header.")));
                return;
            }

            if (!subscribed.Contains(header.UnitId))
            {
                try
                {
                    Task task = SubscribePiraeus(e.Message);
                    Task.WaitAll(task);

                    subscribed.Add(header.UnitId);
                    Console.WriteLine("----- V-RTU Subscribed to {0} -----", map.Map[header.UnitId].RtuOutputResource);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("**** V-RTU FAILED to SUBSCRIBE to '{0}' *****", ex.Message);
                    OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, ex));
                    return;
                }                
            }

            if (!cache.IsCached(header.UnitId, header.TransactionId))
            {
                cache.Set(header.UnitId, header.TransactionId);
            }

            try
            {
                Task pubTask = SendPiraeusAsync(e.Message);
                Task.WaitAll(pubTask);
                Console.WriteLine("V-RTU published to '{0}'", map.GetResources(header.UnitId).RtuInputResource);
            }
            catch(Exception ex)
            {
                Console.WriteLine("**** V-RTU FAILED to PUBLISH to '{0}' *****", ex.Message);
                OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, ex));
                return;
            }
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            //need to close the channel to the SCADA software
            //and Piraeus
            try
            {   
                Task disconnectTask = client.DisconnectAsync();
                Task.WhenAll(disconnectTask);
            }
            catch(Exception ex)
            {
                Trace.TraceError("MQTT client disconnect faulted with '{0}'.", ex.Message);
            }

            Task task = Channel.CloseAsync();
            Task.WhenAll(task);            

            
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            Console.WriteLine("SCADA client channel closing");
            OnClose?.Invoke(this, new AdapterEventArgs(Channel.Id));
        }

        #endregion


        #region Piraeus client

        private async Task OpenPiraeusClient()
        {
            try
            {
                ptokenSource = new CancellationTokenSource();
                byte[] psk = Convert.FromBase64String(pskString);
                IPAddress address = IPAddress.Parse(rtuConfig.Hostname);
                IChannel pchannel = ChannelFactory.Create(false, address, rtuConfig.Port, rtuConfig.PskIdentity, psk, rtuConfig.BlockSize, rtuConfig.MaxBufferSize, ptokenSource.Token);
                client = new PiraeusMqttClient(new MqttConfig(90), pchannel);
                client.OnChannelError += Client_OnChannelError;
                
                ConnectAckCode code = await client.ConnectAsync(Guid.NewGuid().ToString(), "JWT", rtuConfig.SecurityToken, 90);
                if (code != ConnectAckCode.ConnectionAccepted)
                {
                    Console.WriteLine("Piraeus return Code = {0}", code);
                    OnError?.Invoke(this, new AdapterEventArgs(Channel.Id));
                }
                else
                {
                    Console.WriteLine("Connected to Piraeus");
                }
            }
            catch(Exception ex)
            {
                OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, ex));
            }

        }

        private void Client_OnChannelError(object sender, ChannelErrorEventArgs args)
        {
            //channel error in client
            Console.WriteLine("WARNING: Client channel error on {0}", args.ChannelId);
            Console.WriteLine("ERROR: Client channel error {0}", args.Error.Message);
            OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, args.Error));
        }

        public async Task SendPiraeusAsync(byte[] message)
        {
            try
            {
                byte[] buffer = new byte[7];
                Buffer.BlockCopy(message, 0, buffer, 0, buffer.Length);

                MbapHeader header = MbapHeader.Decode(buffer);

                if(map.HasResources(header.UnitId))
                {
                    string inputResource = map.GetResources(header.UnitId).RtuInputResource;
                    await client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, inputResource, "application/octet-stream", message);
                }
                else
                {
                    Console.WriteLine("Unit Id = '{0}' not available for request.", header.UnitId);
                    OnError?.Invoke(this, new AdapterEventArgs(Channel.Id));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("WARNING: Fault on client publishing");
                Console.WriteLine("ERROR: Client publish error - '{0}'", ex.Message);
                OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, ex));
            }
        }
        
        public async Task SubscribePiraeus(byte[] message)
        {
            try
            {
                byte[] buffer = new byte[7];
                Buffer.BlockCopy(message, 0, buffer, 0, buffer.Length);
                MbapHeader header = MbapHeader.Decode(buffer);

                if(map.HasResources(header.UnitId))
                {
                    string resourceUri = map.GetResources(header.UnitId).RtuOutputResource;
                    await client.SubscribeAsync(resourceUri, QualityOfServiceLevelType.AtMostOnce, ReceivePiraeus);
                    Console.WriteLine("Subscribed to RTU resource '{0}'", resourceUri);
                }
                else
                {
                    Console.WriteLine("Unit Id {0} not mapped for subscription.", header.UnitId);
                    OnError?.Invoke(this, new AdapterEventArgs(Channel.Id));
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine("WARNING: Fault on client subscribe");
                Console.WriteLine("ERROR: Client subscribe error - '{0}'", ex.Message);
                OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, ex));
            }            
        }    

        private void ReceivePiraeus(string resourceUriString, string contentType, byte[] message)
        {
            //forward message up the channel as raw binary
            byte[] buffer = null;
            MbapHeader header = null;

            try
            {
                buffer = new byte[7];
                Buffer.BlockCopy(message, 0, buffer, 0, buffer.Length);
                header = MbapHeader.Decode(buffer);
            }
            catch(Exception ex)
            {
                Console.WriteLine("***** Cannot read MBAP header received from Piraeus *****");
                OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, ex));
                return;
            }

            
            if (cache.IsCached(header.UnitId, header.TransactionId))
            {
                cache.Remove(header.UnitId, header.TransactionId);

                //forward down the TCP SCADA channel
                try
                {
                    Task task = Channel.SendAsync(message);
                    Task.WaitAll(task);
                    Console.WriteLine("Forwarding message wiith Unit Id {0} to SCADA client", header.UnitId);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("*****  FAULT RETURNING PIRAEUS MESSAGE TO SCADA CLIENT *****");
                    OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, ex));
                }
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
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
                        Channel.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Fault disposing SCADA channel - '{0}'", ex.Message);
                    }

                    try
                    {
                        if (client != null)
                        {
                            Task disTask = client.DisconnectAsync();
                            Task.WaitAll(disTask);
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Fault disconnecting Piraeus MQTT Client - '{0}'", ex.Message); 
                    }
                }

                disposed = true;
            }
        }

        #endregion


    }
}
