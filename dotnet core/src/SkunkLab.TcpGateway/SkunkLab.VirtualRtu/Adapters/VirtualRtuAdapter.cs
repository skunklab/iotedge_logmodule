using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.Protocols.Mqtt;
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
        public VirtualRtuAdapter(VRtuConfig rtuConfig, RtuMap map, MqttConfig mqttConfig, IChannel channel)
        {
            this.rtuConfig = rtuConfig;
            cache = new MbapCache();
            source = new CancellationTokenSource();

            //open the piraeus channel
            piraeusChannel = new TcpClientChannel(rtuConfig.Hostname, rtuConfig.Port, rtuConfig.PskIdentity, rtuConfig.Psk, rtuConfig.MaxBufferSize, source.Token);
            piraeusChannel.OnClose += PiraeusChannel_OnClose;
            //piraeusChannel.OnError += PiraeusChannel_OnError;

            //create the mqtt client
            client = new PiraeusMqttClient(mqttConfig, piraeusChannel);
            client.OnChannelError += Client_OnChannelError;

            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnOpen += Channel_OnOpen;            
        }
                
        private VRtuConfig rtuConfig;
        private RtuMap map;
        private PiraeusMqttClient client;
        private MbapCache cache;
        private IChannel piraeusChannel;
        private CancellationTokenSource source;
        public IChannel Channel { get; internal set; }
        private const string contentType = "application/octet-stream";
        private bool disposed;

        public event System.EventHandler<AdapterEventArgs> OnError;
        public event System.EventHandler<AdapterEventArgs> OnClose;

        #region Piraeus Channel     

        private void Client_OnChannelError(object sender, ChannelErrorEventArgs args)
        {
            Trace.TraceError("Piraeus channel '{0}' error '{1}'", args.ChannelId, args.Error.Message);

            //close the channel chain
            Task pchannelTask = piraeusChannel.CloseAsync();
            Task.WhenAll(pchannelTask);

            Task closeTask = Channel.CloseAsync();
            Task.WhenAll(closeTask);
        }

        private void PiraeusChannel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            Trace.TraceWarning("Piraeus channel '{0}' closing.", e.ChannelId);
        }

        #endregion
       
        #region SCADA Software Channel
        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            Task<ConnectAckCode> task = client.ConnectAsync(Guid.NewGuid().ToString(), "JWT", rtuConfig.SecurityToken, 
                                                            Convert.ToInt32(rtuConfig.KeepAliveInterval));
            Task.WaitAll(task);

            ConnectAckCode code = task.Result;

            if(code == ConnectAckCode.ConnectionAccepted)
            {
                Trace.TraceInformation("Piraeus TCP MQTT channel '{0}' open.", piraeusChannel.Id);

                //subscribe to all RTU resources
                Dictionary<ushort, ResourceItem>.Enumerator en = map.Map.GetEnumerator();
                while(en.MoveNext())
                {
                    string subscriptionUriString = en.Current.Value.RtuOutputResource;
                    Task subTask = client.SubscribeAsync(subscriptionUriString, QualityOfServiceLevelType.AtMostOnce, SubscriptionResult);
                    Task.WhenAll(subTask);
                }
            }
            else
            {
                Trace.TraceError("Piraeus TCP MQTT channel '{0}' faulted on connect message with code '{1}'.", code);
                //close the channel chain
                Trace.TraceWarning("Closing TCP channel chain due to fault on MQTT connect.");
                Task closeTask = Channel.CloseAsync();
                Task.WhenAll(closeTask);
            }          

        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            //read the transaction id and unit id from the MBAP header
            MbapHeader header = MbapHeader.Decode(e.Message);

            if(!cache.IsCached(header.UnitId, header.TransactionId))
            {
                cache.Set(header.UnitId, header.TransactionId);
            }

            //forward to Piraeus
            string resourceUriString = map.GetResources(header.UnitId).RtuInputResource;
            Task task = client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, resourceUriString, contentType, e.Message);
            Task.WhenAll(task);
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
        }

        #endregion

        #region Subscription Action

        private void SubscriptionResult(string uriString, string contentType, byte[] message)
        {
            MbapHeader header = MbapHeader.Decode(message);
            if(cache.IsCached(header.UnitId, header.TransactionId))
            {
                try
                {
                    Trace.TraceInformation("ModBus message received from RTU and forwarded with UnitId '{0}' and TransactionId '{1}'.", header.UnitId, header.TransactionId);
                    cache.Remove(header.UnitId, header.TransactionId);
                    Task task = Channel.SendAsync(message);
                    Task.WhenAll(task);
                }
                catch(Exception ex)
                {
                    Trace.TraceWarning("ModBus message received from RTU failed to be forward with UnitId '{0}' and TransactionId '{1}'.", header.UnitId, header.TransactionId);
                    Trace.TraceError("ModBus message forwarding from RTU failed with '{0}'.", ex.Message);
                }
                
            }
            else
            {
                //un-mapped
                Trace.TraceWarning("Received ModBus message received from RTU cannot map UnitId '{0}' and TransactionId '{1}'.", header.UnitId, header.TransactionId);
            }
        }



        #endregion
        
        public void Dispose()
        {
            //Channel.Dispose();
            
            throw new NotImplementedException();
        }


    }
}
