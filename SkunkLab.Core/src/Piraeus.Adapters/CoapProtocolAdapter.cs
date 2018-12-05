using Piraeus.Configuration.Settings;
using Piraeus.Core;
using SkunkLab.Channels;
using SkunkLab.Channels.Udp;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;
using SkunkLab.Security.Authentication;
using SkunkLab.Security.Identity;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Piraeus.Adapters
{
    public class CoapProtocolAdapter : ProtocolAdapter
    {
        public CoapProtocolAdapter(PiraeusConfig config, IAuthenticator authenticator, IChannel channel)
        {

            CoapConfigOptions options = config.Protocols.Coap.ObserveOption && config.Protocols.Coap.NoResponseOption ? CoapConfigOptions.Observe | CoapConfigOptions.NoResponse : config.Protocols.Coap.ObserveOption ? CoapConfigOptions.Observe : config.Protocols.Coap.NoResponseOption ? CoapConfigOptions.NoResponse : CoapConfigOptions.None;
            CoapConfig coapConfig = new CoapConfig(authenticator, config.Protocols.Coap.HostName, options, config.Protocols.Coap.AutoRetry,
                config.Protocols.Coap.KeepAliveSeconds, config.Protocols.Coap.AckTimeoutSeconds, config.Protocols.Coap.AckRandomFactor,
                config.Protocols.Coap.MaxRetransmit, config.Protocols.Coap.NStart, config.Protocols.Coap.DefaultLeisure, config.Protocols.Coap.ProbingRate, config.Protocols.Coap.MaxLatencySeconds);
            coapConfig.IdentityClaimType = config.Identity.Client.IdentityClaimType;
            coapConfig.Indexes = config.Identity.Client.Indexes;

            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnStateChange += Channel_OnStateChange;
            session = new CoapSession(coapConfig);
        }


        #region public members
        public override IChannel Channel { get; set; }

        public override event System.EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event System.EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event System.EventHandler<ChannelObserverEventArgs> OnObserve;
        #endregion

        #region private members

        private CoapSession session;
        private ICoapRequestDispatch dispatcher;
        private bool disposed;
        private bool forcePerReceiveAuthn;
        private UserAuditor userAuditor;
        private bool closing;

        #endregion

        #region init
        public override void Init()
        {
            Trace.TraceInformation("{0} - CoAP Protocol Adapter intialization on Channel '{1}'.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id);
            userAuditor = new UserAuditor();
            forcePerReceiveAuthn = Channel as UdpChannel != null;

            Task task = Channel.OpenAsync();
            Task.WaitAll(task);
        }

        #endregion

        #region events

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            Trace.TraceInformation("{0} - Channel {1} opened.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id);
            session.IsAuthenticated = Channel.IsAuthenticated;

            try
            {
                if (!Channel.IsAuthenticated && e.Message != null)
                {
                    CoapMessage msg = CoapMessage.DecodeMessage(e.Message);
                    CoapUri coapUri = new CoapUri(msg.ResourceUri.ToString());
                    session.IsAuthenticated = session.Authenticate(coapUri.TokenType, coapUri.SecurityToken);
                }

                if (session.IsAuthenticated)
                {
                    IdentityDecoder decoder = new IdentityDecoder(session.Config.IdentityClaimType, session.Config.Indexes);
                    session.Identity = decoder.Id;
                    session.Indexes = decoder.Indexes;

                    if (userAuditor.CanAudit)
                    {
                        UserLogRecord record = new UserLogRecord(Channel.Id, session.Identity, session.Config.IdentityClaimType, Channel.TypeId, "COAP", "Granted", DateTime.UtcNow);
                        userAuditor.WriteAuditRecordAsync(record).Ignore();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0} - Channel {1} not authenticated. Exception '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id, ex.Message);                
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }

            if (!session.IsAuthenticated && e.Message != null)
            {
                //close the channel
                Trace.TraceWarning("{0} - Channel {1} not authenticated. Closing channel.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id);
                Channel.CloseAsync().Ignore();
            }
            else
            {
                dispatcher = new CoapRequestDispatcher(session, Channel);
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            try
            {
                CoapMessage message = CoapMessage.DecodeMessage(e.Message);

                if (!session.IsAuthenticated || forcePerReceiveAuthn)
                {
                    CoapAuthentication.EnsureAuthentication(session, message, forcePerReceiveAuthn);
                    dispatcher.Identity = session.Identity;

                    if (userAuditor.CanAudit)
                    {
                        UserLogRecord record = new UserLogRecord(Channel.Id, session.Identity, session.Config.IdentityClaimType, Channel.TypeId, "COAP", "Granted", DateTime.UtcNow);
                        userAuditor.WriteAuditRecordAsync(record).Ignore();
                    }

                }

                OnObserve?.Invoke(this, new ChannelObserverEventArgs(message.ResourceUri.ToString(), MediaTypeConverter.ConvertFromMediaType(message.ContentType), message.Payload));

                Task task = Task.Factory.StartNew(async () =>
                {

                    CoapMessageHandler handler = CoapMessageHandler.Create(session, message, dispatcher);
                    CoapMessage msg = await handler.ProcessAsync();

                    if (msg != null)
                    {
                        byte[] payload = msg.Encode();
                        await Channel.SendAsync(payload);
                    }

                });

                task.LogExceptions();
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0} - OnReceive error in Channel {1} with '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id, ex.Message);
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));                
                Channel.CloseAsync().Ignore();
            }

        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            Trace.TraceWarning("{0} - Error received in CoAP protocol on Channel {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id);
            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, e.Error));
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            if (!closing)
            {
                closing = true;

                Trace.TraceInformation("{0} - Channel {1} closing.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id);                

                if (userAuditor.CanAudit)
                {
                    UserLogRecord record = userAuditor.GetAuditRecord(Channel.Id, session.Identity);
                    record.LogoutTime = DateTime.UtcNow;
                    userAuditor.WriteAuditRecordAsync(record).Ignore();
                }

                OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(Channel.Id));
            }
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            Trace.TraceInformation("{0} - Channel {1} state change '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id, e.State);
        }


        #endregion

        #region dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Trace.TraceInformation("MQTT Protocol Adapter disposing on Channel '{0}'.", Channel.Id);
                if (disposing)
                {

                    try
                    {
                        if (dispatcher != null)
                        {
                            dispatcher.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("{0} - CoAP adapter on channel '{1}' dispatcher dispose fault '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id, ex.Message);
                        Console.WriteLine("Fault disposing CoAP dispatcher in CoAP adapter - '{0}'", ex.Message);
                    }

                    try
                    {
                        if (Channel != null)
                        {
                            Channel.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("{0} - CoAP adapter channel dispose fault '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.Message);
                        Console.WriteLine("Fault disposing Channel in CoAP adapter - '{0}'", ex.Message);
                    }

                    try
                    {
                        if (session != null)
                        {
                            session.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("{0} - CoAP adapter session dispose fault '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.Message);
                        Console.WriteLine("Fault disposing Session in CoAP adapter - '{0}'", ex.Message);
                    }
                }
                disposed = true;
            }            
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
