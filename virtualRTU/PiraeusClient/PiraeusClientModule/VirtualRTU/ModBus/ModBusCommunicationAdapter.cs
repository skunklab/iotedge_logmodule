using SkunkLab.Channels.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualRTU.ModBus
{
    public class ModBusCommunicationAdapter : CommunicationAdapter
    {

        public ModBusCommunicationAdapter(VRtuConfig config)
        {
            this.config = config;
            txmap = new TransactionMap();
            subscriptions = new HashSet<string>();
        }

        public override event System.EventHandler<ComAdapterEventArgs> OnClose;
        public override event System.EventHandler<ComAdapterEventArgs> OnError;
        private VRtuConfig config;
        //private PiraeusMqttClient client;
        private CancellationTokenSource cts;
        private RtuMapManager rmm;
        private string securityToken;
        private TransactionMap txmap;
        private HashSet<string> subscriptions;
        private IChannel p_channel;
        private CancellationTokenSource source;



        public override IChannel Channel { get; set; }

        private IPAddress GetIPAddress(string hostname)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(hostname);
            for (int index = 0; index < hostInfo.AddressList.Length; index++)
            {
                if (hostInfo.AddressList[index].AddressFamily == AddressFamily.InterNetwork)
                {
                    return hostInfo.AddressList[index];
                }
            }

            return null;
        }

        public override void Init()
        {
            rmm = RtuMapManager.Load(config.Map);
            securityToken = config.GetSecurityToken();
            cts = new CancellationTokenSource();
            IPAddress address = GetIPAddress(config.PiraeusHostname);
            IChannel piraeusChannel = ChannelFactory.Create(false, address, config.Port, null, config.PskIdentifier, config.PskBytes, config.TcpBlockSize, config.TcpMaxBufferSize, cts.Token);
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnOpen += Channel_OnOpen;

            client = new PiraeusMqttClient(new MqttConfig(), piraeusChannel);

            Task t = OpenMqttClientAsync();
            Task.WhenAll(t);

            

            
        }

        //private async Task OpenMqttClientAsync()
        //{
        //    ConnectAckCode code = await client.ConnectAsync(Guid.NewGuid().ToString(), config.SecurityTokenType, securityToken, 180);
        //    if (code == ConnectAckCode.ConnectionAccepted)
        //    {
        //        ushort[] keys = rmm.GetUnitIds();
        //        foreach (ushort key in keys)
        //        {
        //            string subResource = rmm.GetSubscribeResource(key);
        //            client.RegisterTopic(subResource, Observe);
        //        }
        //    }
        //    else
        //    {
        //        await Channel.CloseAsync();
        //    }
        //}

        
            /// <summary>
            /// Receives the channel open event from the channel used by the adapter.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            /// <remarks>We use this an opportunity to open TCP channel to Piraeus using MQTT protocool.</remarks>
        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            //open a connection to Piraeus using MQTT, TCP, and PSK/TLSv1.2
            //if the channel closes to Piraeus...we will close the adapter's channel also.

            //open PSK/TLSv1.2 channel to Piraeus
            source = new CancellationTokenSource();
            p_channel = ChannelFactory.Create(false, GetIPAddress(config.PiraeusHostname), 8883, null, config.PskIdentifier, config.PskBytes, 1024, 2048, cts.Token);
            
            //create the MQTT client and connect to Piraeus

            
           
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            //recieved from client software, must read MBAP header for mapping to RTU
            MbapHeader header = MbapHeader.Decode(e.Message);
            if(header == null)
            {
                return;
            }

            //map the transaction for the request
            txmap.Add(header.TransactionId, header.UnitId);

            //get the piraeus resource to send
            string resource = RtuMapManager.Create().GetPublishResource(header.UnitId);

            //get the piraeus resource to subscribe
            string subscription = RtuMapManager.Create().GetSubscribeResource(header.UnitId);

            if (resource == null || subscription == null)
            {
                return;
            }

            if (!subscriptions.Contains(subscription))
            {
                //subscribe to Piraeus resource
                Task subTask = client.SubscribeAsync(subscription, QualityOfServiceLevelType.AtLeastOnce, Observe);
                Task.WhenAll(subTask);
            }


            //publish to piraeus resource
            Task pubTask = client.PublishAsync(QualityOfServiceLevelType.AtLeastOnce, resource, "application/octet-stream", e.Message);
            Task.WhenAll(pubTask);

        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            //close the channel...we got an error
            Task closeTask = Channel.CloseAsync();
            Task.WhenAll(closeTask);
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            Trace.TraceInformation("Channel '{0}' closing.", e.ChannelId);
        }

        
        /// <summary>
        /// Observes a subscription to a Piraeus resource
        /// </summary>
        /// <param name="resourceUriString"></param>
        /// <param name="contentType"></param>
        /// <param name="payload"></param>
        private void Observe(string resourceUriString, string contentType, byte[] payload)
        {
            MbapHeader header = MbapHeader.Decode(payload);
            
            //if the transaction map matches a previous request, then forward the message.
            if(txmap.IsMatch(header.TransactionId, header.UnitId))
            {
                txmap.Remove(header.TransactionId, header.UnitId);  //remove the Tx for the RTU
                Task sendTask = Channel.SendAsync(payload);
                Task.WhenAll(sendTask);
            }
        }

        public override void Dispose()
        {
            //dispose the adapter
        }

    }
}
