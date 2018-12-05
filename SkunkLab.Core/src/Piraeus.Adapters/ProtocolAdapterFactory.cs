using Piraeus.Configuration.Settings;
using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Security.Authentication;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Piraeus.Adapters
{
    public class ProtocolAdapterFactory
    {
        /// <summary>
        /// Create protocol adapter for rest service or Web socket
        /// </summary>
        /// <param name="config"></param>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <param name="authenticator"></param>
        /// <returns></returns>
        public static ProtocolAdapter Create(PiraeusConfig config, HttpRequestMessage request, CancellationToken token, IAuthenticator authenticator = null)
        {
            IChannel channel = null;

            //HttpContext context = HttpContext.Current;
            //if (context.IsWebSocketRequest ||
            //    context.IsWebSocketRequestUpgrading)
                if (HttpHelper.HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocketConfig webSocketConfig = GetWebSocketConfig(config);
                channel = ChannelFactory.Create(request, webSocketConfig, token);

                
                //if (context.WebSocketRequestedProtocols.Contains("mqtt"))
                if(HttpHelper.HttpContext.WebSockets.WebSocketRequestedProtocols.Contains("mqtt"))
                {
                    return new MqttProtocolAdapter(config, authenticator, channel);
                }
                else if (HttpHelper.HttpContext.WebSockets.WebSocketRequestedProtocols.Contains("coapv1"))  //(context.WebSocketRequestedProtocols.Contains("coapv1"))
                {
                    return new CoapProtocolAdapter(config, authenticator, channel);
                }
                //else if (context.WebSocketRequestedProtocols.Count == 0)
                //{
                //    //wsn protocol
                //    //return new WsnProtocolAdapter(config, channel);
                //}
                else
                {
                    throw new InvalidOperationException("invalid web socket subprotocol");
                }
            }

            if (request.Method != HttpMethod.Post && request.Method != HttpMethod.Get)
            {
                throw new HttpRequestException("Protocol adapter requires HTTP get or post.");
            }
            else
            {
                channel = ChannelFactory.Create(request);
                return new RestProtocolAdapter(config, channel);
            }
        }

        /// <summary>
        /// Creates a protocol adapter for TCP server channel
        /// </summary>
        /// <param name="client">TCP client initialized by TCP Listener on server.</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public static ProtocolAdapter Create(PiraeusConfig config, IAuthenticator authenticator, TcpClient client, CancellationToken token)
        {
            IChannel channel = null;

            if (config.Channels.Tcp.PresharedKeys != null)
            {
                Dictionary<string,byte[]> pskClone = config.Channels.Tcp.GetPskClone();
                channel = ChannelFactory.Create(config.Channels.Tcp.UseLengthPrefix, client, pskClone, config.Channels.Tcp.BlockSize, config.Channels.Tcp.MaxBufferSize, token);
                
            }
            else
            {
                channel = ChannelFactory.Create(config.Channels.Tcp.UseLengthPrefix, client, config.Channels.Tcp.BlockSize, config.Channels.Tcp.MaxBufferSize, token);
            }

            IPEndPoint localEP = (IPEndPoint)client.Client.LocalEndPoint;
            int port = localEP.Port;

            if (port == 5684) //CoAP over TCP
            {
                return new CoapProtocolAdapter(config, authenticator, channel);
            }
            else if (port == 1883 || port == 8883) //MQTT over TCP
            {
                //MQTT
                return new MqttProtocolAdapter(config, authenticator, channel);
            }
            else
            {
                throw new ProtocolAdapterPortException("TcpClient port does not map to a supported protocol.");
            }

        }

        public static ProtocolAdapter Create(PiraeusConfig config, IAuthenticator authenticator, UdpClient client, IPEndPoint remoteEP, CancellationToken token)
        {
            IPEndPoint endpoint = client.Client.LocalEndPoint as IPEndPoint;

            IChannel channel = ChannelFactory.Create(client, remoteEP, token);
            if (endpoint.Port == 5683)
            {
                return new CoapProtocolAdapter(config, authenticator, channel);
            }
            else if (endpoint.Port == 5883)
            {
                return new MqttProtocolAdapter(config, authenticator, channel);
            }
            else
            {
                throw new ProtocolAdapterPortException("UDP port does not map to a supported protocol.");
            }

        }

        #region configurations
        private static WebSocketConfig GetWebSocketConfig(PiraeusConfig config)
        {
            return new WebSocketConfig(config.Channels.WebSocket.MaxIncomingMessageSize,
                config.Channels.WebSocket.ReceiveLoopBufferSize,
                config.Channels.WebSocket.SendBufferSize,
                config.Channels.WebSocket.CloseTimeoutMilliseconds);
        }

        //private static CoapConfig GetCoapConfig(PiraeusConfig config, IAuthenticator authenticator)
        //{
        //    CoapConfigOptions options = config.Protocols.Coap.ObserveOption && config.Protocols.Coap.NoResponseOption ? CoapConfigOptions.Observe | CoapConfigOptions.NoResponse : config.Protocols.Coap.ObserveOption ? CoapConfigOptions.Observe : config.Protocols.Coap.NoResponseOption ? CoapConfigOptions.NoResponse : CoapConfigOptions.None;
        //    return new CoapConfig(authenticator, config.Protocols.Coap.HostName, options, config.Protocols.Coap.AutoRetry,
        //        config.Protocols.Coap.KeepAliveSeconds, config.Protocols.Coap.AckTimeoutSeconds, config.Protocols.Coap.AckRandomFactor,
        //        config.Protocols.Coap.MaxRetransmit, config.Protocols.Coap.NStart, config.Protocols.Coap.DefaultLeisure, config.Protocols.Coap.ProbingRate, config.Protocols.Coap.MaxLatencySeconds);
        //}

        //private static MqttConfig GetMqttConfig(PiraeusConfig config, IAuthenticator authenticator)
        //{
        //    MqttConfig mqttConfig = new MqttConfig(authenticator, config.Protocols.Mqtt.KeepAliveSeconds,
        //           config.Protocols.Mqtt.AckTimeoutSeconds, config.Protocols.Mqtt.AckRandomFactor, config.Protocols.Mqtt.MaxRetransmit, config.Protocols.Mqtt.MaxLatencySeconds);
        //    mqttConfig.IdentityClaimType = config.Identity.Client.IdentityClaimType;
        //    mqttConfig.Indexes = config.Identity.Client.Indexes;

        //    return mqttConfig;
        //}

        #endregion
    }
}
