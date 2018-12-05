using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.VirtualRtu.ModBus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.VirtualRtu.Adapters
{
    /// <summary>
    /// Virtual RTU adapter for communication with SCADA software of ModBus-TCP
    /// </summary>
    public class VirtualRtuAdapter : IDisposable
    {
        public VirtualRtuAdapter(VRtuConfig rtuConfig, RtuMap map, IChannel channel)
        {
            this.rtuConfig = rtuConfig;
            this.map = map;
            cache = new MbapCache();
            source = new CancellationTokenSource();
            subscribed = new HashSet<ushort>();            

            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnOpen += Channel_OnOpen;            
        }

        private void Client_OnReceive(object sender, SubscriptionEventArgs e)
        {
            MbapHeader header = MbapHeader.Decode(e.Message);
            if(cache.IsCached(header.UnitId, header.TransactionId))
            {
                cache.Remove(header.UnitId, header.TransactionId);

                //forward down the TCP SCADA channel
                Task task = Channel.SendAsync(e.Message);
                Task.WhenAll(task);
            }
            else
            {
                Trace.TraceWarning("Message received from Piraeus does not map to Unit ID and Transaction ID of SCADA client.");
            }
        }

        private PiraeusClient client;
        private VRtuConfig rtuConfig;
        private RtuMap map;
        //private PiraeusMqttClient client;
        private MbapCache cache;
        private IChannel piraeusChannel;
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
                client = new PiraeusClient(rtuConfig);
                client.OnReceive += Client_OnReceive;
                Task connectTask = client.ConnectAsync();
                Task.WhenAll(connectTask);
            }
            catch(Exception ex)
            {
                OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, ex));
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            Console.WriteLine("VRTU Adapter received message");
            //read the transaction id and unit id from the MBAP header
            MbapHeader header = MbapHeader.Decode(e.Message);

            if(!map.Map.ContainsKey(header.UnitId))
            {
                this.OnError?.Invoke(this, new AdapterEventArgs(Channel.Id, new InvalidOperationException("Unit ID is not mapped")));
                return;
            }

            if (!subscribed.Contains(header.UnitId))
            {
                //subscribe to the output of the RTU
                Task subTask = client.SubscribeAsync(map.Map[header.UnitId].RtuOutputResource);
                Task.WhenAll(subTask);
                
                subscribed.Add(header.UnitId);
                Console.WriteLine("V-RTU Subscribed to {0}", map.Map[header.UnitId].RtuOutputResource);
            }


            if(!cache.IsCached(header.UnitId, header.TransactionId))
            {
                cache.Set(header.UnitId, header.TransactionId);
            }
            
            Task pubTask = client.SendAsync(map.GetResources(header.UnitId).RtuInputResource, e.Message);
            Task.WhenAll(pubTask);
            Console.WriteLine("V-RTU published to {0}", map.GetResources(header.UnitId).RtuInputResource);
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            //need to close the channel to the SCADA software
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
            Trace.TraceWarning("SCADA channel closing.");
            OnClose?.Invoke(this, new AdapterEventArgs(Channel.Id));
        }

        #endregion

        
        #region Dispose

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
                        Task t = client.DisconnectAsync();
                        Task.WaitAll(t);
                    }
                    catch(AggregateException ae)
                    {
                        Trace.TraceError(ae.Flatten().InnerException.Message);
                    }
                    catch(Exception ex)
                    {
                        Trace.TraceError(ex.Message);
                    }

                    try
                    {
                        piraeusChannel.Dispose();
                    }
                    catch(Exception ex)
                    {
                        Trace.TraceError(ex.Message);
                    }
                }

                disposed = true;
            }
        }

        #endregion


    }
}
