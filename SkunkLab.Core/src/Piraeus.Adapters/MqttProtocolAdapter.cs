using Piraeus.Configuration.Settings;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using Piraeus.Grains.Notifications;
using SkunkLab.Channels;
using SkunkLab.Channels.Udp;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Protocols.Mqtt.Handlers;
using SkunkLab.Security.Authentication;
using SkunkLab.Security.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Threading.Tasks;
using Piraeus.Core;

namespace Piraeus.Adapters
{
    public class MqttProtocolAdapter : ProtocolAdapter
    {
        public MqttProtocolAdapter(PiraeusConfig config, IAuthenticator authenticator, IChannel channel)
        {
            this.config = config;
            MqttConfig mqttConfig = new MqttConfig(config.Protocols.Mqtt.KeepAliveSeconds, config.Protocols.Mqtt.AckTimeoutSeconds,
                config.Protocols.Mqtt.AckRandomFactor, config.Protocols.Mqtt.MaxRetransmit, config.Protocols.Mqtt.MaxLatencySeconds,
                authenticator, config.Identity.Client.IdentityClaimType, config.Identity.Client.Indexes);
            
            session = new MqttSession(mqttConfig);
            userAuditor = new UserAuditor();

            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnStateChange += Channel_OnStateChange;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnOpen += Channel_OnOpen;
        }

        public override event System.EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event System.EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event System.EventHandler<ChannelObserverEventArgs> OnObserve;

        private Auditor auditor;
        private MqttSession session;
        private bool disposed;
        private OrleansAdapter adapter;
        private PiraeusConfig config;
        private bool forcePerReceiveAuthn;
        private UserAuditor userAuditor;
        private bool closing;


        public override IChannel Channel { get; set; }

        public override void Init()
        {
            Trace.TraceInformation("{0} - MQTT Protocol Adapter intialization on Channel '{1}'.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id);
            auditor = new Auditor();

            forcePerReceiveAuthn = Channel as UdpChannel != null;
            session.OnPublish += Session_OnPublish;
            session.OnSubscribe += Session_OnSubscribe;
            session.OnUnsubscribe += Session_OnUnsubscribe;
            session.OnDisconnect += Session_OnDisconnect; ;
            session.OnConnect += Session_OnConnect;
        }

        #region Orleans Adapter Events
        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {
            AuditRecord record = null;
            int length = 0;
            DateTime sendTime = DateTime.UtcNow;
            try
            {
                byte[] message = ProtocolTransition.ConvertToMqtt(session, e.Message);
                Send(message).LogExceptions();

                MqttMessage mm = MqttMessage.DecodeMessage(message);

                length = mm.Payload.Length;
                record = new AuditRecord(e.Message.MessageId, session.Identity, this.Channel.TypeId, "MQTT", length, MessageDirectionType.Out, true, sendTime);
            }
            catch(AggregateException ae)
            {
                string msg = String.Format("{0} - MQTT adapter observe error on channel '{1}' with '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id, ae.Flatten().InnerException.Message);
                Trace.TraceError(msg);
                record = new AuditRecord(e.Message.MessageId, session.Identity, this.Channel.TypeId, "MQTT", length, MessageDirectionType.Out, true, sendTime, msg);
            }
            catch(Exception ex)
            {
                string msg = String.Format("{0} - MQTT adapter observe error on channel '{1}' with '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id, ex.Message);                
                Trace.TraceError(msg);
                record = new AuditRecord(e.Message.MessageId, session.Identity, this.Channel.TypeId, "MQTT", length, MessageDirectionType.Out, true, sendTime, msg);
            }
            finally
            {
                if (auditor.CanAudit && e.Message.Audit)
                {
                    auditor.WriteAuditRecordAsync(record).Ignore();
                }
            }
        }

        private async Task Send(byte[] message)
        {
            try
            {
                await Channel.SendAsync(message);
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0} - Mqtt adapter on channel '{1}' send fault '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id, ex.Message);
            }
        }

        #endregion

        #region MQTT Session Events
        private void Session_OnConnect(object sender, MqttConnectionArgs args)
        {
            try
            {
                Task task = Task.Factory.StartNew(async () =>
                {
                    if (args.Code == ConnectAckCode.ConnectionAccepted)
                    {
                        await adapter.LoadDurableSubscriptionsAsync(session.Identity);
                    }

                    Trace.TraceInformation("{0} - MQTT session is connected on channel '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"),Channel.Id);
                });

                Task.WaitAll(task);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
        }

        private void Session_OnDisconnect(object sender, MqttMessageEventArgs args)
        {
            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, new DisconnectException(String.Format("MQTT adapter on channel {0} has been disconnected.", Channel.Id))));            
        }

        private void Session_OnUnsubscribe(object sender, MqttMessageEventArgs args)
        {
            try
            {
                UnsubscribeMessage msg = (UnsubscribeMessage)args.Message;
                foreach (var item in msg.Topics)
                {
                    Task task = Task.Factory.StartNew(async () =>
                    {
                        MqttUri uri = new MqttUri(item.ToLowerInvariant());
                        if (await adapter.CanSubscribeAsync(uri.Resource, Channel.IsEncrypted))
                        {
                            await adapter.UnsubscribeAsync(uri.Resource);
                        }
                    });

                    Task.WaitAll(task);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
        }

        private List<string> Session_OnSubscribe(object sender, MqttMessageEventArgs args)
        {
            List<string> list = new List<string>();

            try
            {
                SubscribeMessage message = args.Message as SubscribeMessage;

                SubscriptionMetadata metadata = new SubscriptionMetadata()
                {
                    Identity = session.Identity,
                    Indexes = session.Indexes,
                    IsEphemeral = true
                };

                foreach (var item in message.Topics)
                {
                    MqttUri uri = new MqttUri(item.Key);
                    string resourceUriString = uri.Resource;

                    Task<bool> t = CanSubscribe(resourceUriString);
                    bool subscribe = t.Result;

                    if (subscribe)
                    {
                        Task<string> subTask = Subscribe(resourceUriString, metadata);
                        string subscriptionUriString = subTask.Result;
                        list.Add(resourceUriString);
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }

            return list;
        }

        private Task<string> Subscribe(string resourceUriString, SubscriptionMetadata metadata)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            Task t = Task.Factory.StartNew(async () =>
            {
                string id = await adapter.SubscribeAsync(resourceUriString, metadata);
                tcs.SetResult(id);
            });

            return tcs.Task;
        }

        private Task<bool> CanSubscribe(string resourceUriString)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            Task t = Task.Factory.StartNew(async () =>
            {
                bool r = await adapter.CanSubscribeAsync(resourceUriString, Channel.IsEncrypted);
                tcs.SetResult(r);
            });

            return tcs.Task;
        }

        private void Session_OnPublish(object sender, MqttMessageEventArgs args)
        {
            try
            {
                PublishMessage message = args.Message as PublishMessage;
                Task task = PublishAsync(message);
                Task.WaitAll(task);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
        }

        private async Task PublishAsync(PublishMessage message)
        {
            AuditRecord record = null;
            ResourceMetadata metadata = null;

            try
            {
                MqttUri mqttUri = new MqttUri(message.Topic);
                metadata = await GraphManager.GetResourceMetadataAsync(mqttUri.Resource);
                if (await adapter.CanPublishAsync(metadata, Channel.IsEncrypted))
                {
                    EventMessage msg = new EventMessage(mqttUri.ContentType, mqttUri.Resource, ProtocolType.MQTT, message.Encode(), DateTime.UtcNow, metadata.Audit);
                    if (!string.IsNullOrEmpty(mqttUri.CacheKey))
                    {
                        msg.CacheKey = mqttUri.CacheKey;
                    }

                    await adapter.PublishAsync(msg, null);
                }
                else
                {
                    if (metadata.Audit && auditor.CanAudit)
                    {
                        record = new AuditRecord("XXXXXXXXXXXX", session.Identity, this.Channel.TypeId, "MQTT", message.Payload.Length, MessageDirectionType.In, false, DateTime.UtcNow, "Not authorized, missing resource metadata, or channel encryption requirements");
                    }

                    throw new SecurityException(String.Format("'{0}' not authorized to publish to '{1}'", session.Identity, metadata.ResourceUriString));
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
            finally
            {
                if (metadata != null && metadata.Audit && auditor.CanAudit && record != null)
                {
                    await auditor.WriteAuditRecordAsync(record);
                }
            }

        }

        #endregion

        #region Channel Events
        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            try
            {
                session.IsAuthenticated = Channel.IsAuthenticated;

                if (session.IsAuthenticated)
                {
                    IdentityDecoder decoder = new IdentityDecoder(session.Config.IdentityClaimType, session.Config.Indexes);
                    session.Identity = decoder.Id;
                    session.Indexes = decoder.Indexes;

                    if (userAuditor.CanAudit)
                    {
                        UserLogRecord record = new UserLogRecord(Channel.Id, session.Identity, session.Config.IdentityClaimType, Channel.TypeId, "MQTT", "Granted", DateTime.UtcNow);
                        userAuditor.WriteAuditRecordAsync(record).Ignore();
                    }
                }

                adapter = new OrleansAdapter(session.Identity, Channel.TypeId, "MQTT");
                adapter.OnObserve += Adapter_OnObserve;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            try
            {
                MqttMessage msg = MqttMessage.DecodeMessage(e.Message);
                OnObserve?.Invoke(this, new ChannelObserverEventArgs(null, null, e.Message));

                if (!session.IsAuthenticated)
                {
                    ConnectMessage message = msg as ConnectMessage;
                    if (message == null)
                    {
                        throw new SecurityException("Connect message not first message");
                    }

                    if (session.Authenticate(message.Username, message.Password))
                    {
                        IdentityDecoder decoder = new IdentityDecoder(session.Config.IdentityClaimType, session.Config.Indexes);
                        session.Identity = decoder.Id;
                        session.Indexes = decoder.Indexes;
                        adapter.Identity = decoder.Id;

                        if (userAuditor.CanAudit)
                        {
                            UserLogRecord record = new UserLogRecord(Channel.Id, session.Identity, session.Config.IdentityClaimType, Channel.TypeId, "MQTT", "Granted", DateTime.UtcNow);
                            userAuditor.WriteAuditRecordAsync(record).Ignore();
                        }
                    }
                    else
                    {
                        throw new SecurityException("Session could not be authenticated.");
                    }
                }
                else if (forcePerReceiveAuthn)
                {
                    if (!session.Authenticate())
                    {
                        throw new SecurityException("Per receive authentication failed.");
                    }
                }

                Task task = ProcessMessageAsync(msg);
                Task.WaitAll(task);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
        }

        private async Task ProcessMessageAsync(MqttMessage message)
        {
            try
            {
                MqttMessageHandler handler = MqttMessageHandler.Create(session, message);
                MqttMessage msg = await handler.ProcessAsync();

                if (msg != null)
                {
                    await Channel.SendAsync(msg.Encode());
                }
            }
            catch (Exception ex)
            {
                OnError.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            Trace.TraceInformation("{0} - Channel {1} state change '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id, e.State);            
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, e.Error));
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            try
            {                
                if (!closing)
                {
                    closing = true;

                    if (userAuditor.CanAudit)
                    {

                        UserLogRecord record = userAuditor.GetAuditRecord(Channel.Id, session.Identity);
                        if (record != null)
                        {
                            record.LogoutTime = DateTime.UtcNow;
                            Task t = userAuditor.WriteAuditRecordAsync(record);
                            Task.WaitAll(t);
                        }

                    }
                }

                OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(e.ChannelId));
            }
            catch
            {

            }
        }

        #endregion

        #region Dispose 
        protected void Disposing(bool disposing)
        {
            if (!disposed)
            {
                Trace.TraceInformation("MQTT Protocol Adapter disposing on Channel '{0}'.", Channel.Id);
                if (disposing)
                {

                    try
                    {
                        if (adapter != null)
                        {
                            adapter.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("{0} - Mqtt adapter on channel '{1}' Orleans adapter dispose fault '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id, ex.Message);
                        Console.WriteLine("Fault disposing Orleans adapter in MQTT adapter - '{0}'", ex.Message);
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
                        Trace.TraceError("{0} - Mqtt adapter channel dispose fault '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.Message);
                        Console.WriteLine("Fault disposing Channel in MQTT adapter - '{0}'", ex.Message);
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
                        Trace.TraceError("{0} - Mqtt adapter session dispose fault '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.Message);
                        Console.WriteLine("Fault disposing Session in MQTT adapter - '{0}'", ex.Message);
                    }
                }
                disposed = true;
            }
        }

        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
