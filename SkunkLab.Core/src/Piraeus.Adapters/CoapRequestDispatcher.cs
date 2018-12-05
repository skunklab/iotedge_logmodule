using Piraeus.Core;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using Piraeus.Grains.Notifications;
using SkunkLab.Channels;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Piraeus.Adapters
{
    public class CoapRequestDispatcher : ICoapRequestDispatch
    {
        public CoapRequestDispatcher(CoapSession session, IChannel channel)
        {
            this.channel = channel;
            this.session = session;
            auditor = new Auditor();
            coapObserved = new Dictionary<string, byte[]>();
            coapUnobserved = new HashSet<string>();
            adapter = new OrleansAdapter(session.Identity, channel.TypeId, "CoAP");
            adapter.OnObserve += Adapter_OnObserve;
            LoadDurablesAsync().LogExceptions();
        }

        private Auditor auditor;
        private OrleansAdapter adapter;
        private IChannel channel;
        private CoapSession session;
        private HashSet<string> coapUnobserved;
        private Dictionary<string, byte[]> coapObserved;
        private bool disposedValue = false; // To detect redundant calls

        public string Identity
        {
            set { adapter.Identity = value; }
        }

        public async Task<CoapMessage> DeleteAsync(CoapMessage message)
        {
            Exception error = null;

            CoapUri uri = new CoapUri(message.ResourceUri.ToString());
            try
            {
                await adapter.UnsubscribeAsync(uri.Resource);
                coapObserved.Remove(uri.Resource);
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0} - CoAP Delete fault '{1}' ", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.Message);
                error = ex;
            }

            if (error == null)
            {
                ResponseMessageType rmt = message.MessageType == CoapMessageType.Confirmable ? ResponseMessageType.Acknowledgement : ResponseMessageType.NonConfirmable;
                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Deleted, message.Token);
            }
            else
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.EmptyMessage);
            }
        }

       
        public Task<CoapMessage> GetAsync(CoapMessage message)
        {
            TaskCompletionSource<CoapMessage> tcs = new TaskCompletionSource<CoapMessage>();
            CoapMessage msg = new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.EmptyMessage, message.Token);
            tcs.SetResult(msg);
            return tcs.Task;

        }

        public async Task<CoapMessage> ObserveAsync(CoapMessage message)
        {
            if (!message.Observe.HasValue)
            {
                //RST because GET needs to be observe/unobserve
                Trace.TraceWarning("{0} - CoAP observe received without Observe flag set on channel '{1}', returning RST", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), channel.Id);
                return new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.EmptyMessage);
            }

            CoapUri uri = new CoapUri(message.ResourceUri.ToString());
            ResponseMessageType rmt = message.MessageType == CoapMessageType.Confirmable ? ResponseMessageType.Acknowledgement : ResponseMessageType.NonConfirmable;

            if (!await adapter.CanSubscribeAsync(uri.Resource, channel.IsEncrypted))
            {
                //not authorized
                Trace.TraceWarning("{0} - CoAP observe not authorized on channel '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), channel.Id);                
                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Unauthorized, message.Token);
            }

            if (!message.Observe.Value)
            {
                //unsubscribe
                Trace.TraceWarning("{0} - CoAP observe with value on channel '{1}', unsubscribing.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), channel.Id);                
                await adapter.UnsubscribeAsync(uri.Resource);
                coapObserved.Remove(uri.Resource);
            }
            else
            {
                //subscribe
                SubscriptionMetadata metadata = new SubscriptionMetadata()
                {
                    IsEphemeral = true,
                    Identity = session.Identity,
                    Indexes = session.Indexes
                };

                string subscriptionUriString = await adapter.SubscribeAsync(uri.Resource, metadata);


                if (!coapObserved.ContainsKey(uri.Resource)) //add resource to observed list
                {
                    coapObserved.Add(uri.Resource, message.Token);
                }
            }

            return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Valid, message.Token);
        }

        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {

            byte[] message = null;

            if (coapObserved.ContainsKey(e.Message.ResourceUri))
            {
                message = ProtocolTransition.ConvertToCoap(session, e.Message, coapObserved[e.Message.ResourceUri]);
            }
            else
            {
                message = ProtocolTransition.ConvertToCoap(session, e.Message);
            }

            Send(message, e).LogExceptions();
        }

        private async Task Send(byte[] message, ObserveMessageEventArgs e)
        {
            AuditRecord record = null;
            try
            {
                await channel.SendAsync(message);
                record = new AuditRecord(e.Message.MessageId, session.Identity, this.channel.TypeId, "COAP", e.Message.Message.Length, MessageDirectionType.Out, true, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                record = new AuditRecord(e.Message.MessageId, session.Identity, this.channel.TypeId, "COAP", e.Message.Message.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
            }
            finally
            {
                if (e.Message.Audit && auditor.CanAudit)
                {
                    await auditor.WriteAuditRecordAsync(record);
                }
            }



        }

     
        public async Task<CoapMessage> PostAsync(CoapMessage message)
        {
            try
            {
                CoapUri uri = new CoapUri(message.ResourceUri.ToString());
                ResponseMessageType rmt = message.MessageType == CoapMessageType.Confirmable ? ResponseMessageType.Acknowledgement : ResponseMessageType.NonConfirmable;
                ResourceMetadata metadata = await GraphManager.GetResourceMetadataAsync(uri.Resource);

                if (!await adapter.CanPublishAsync(metadata, channel.IsEncrypted))
                {
                    if (metadata.Audit && auditor.CanAudit)
                    {
                        await auditor.WriteAuditRecordAsync(new AuditRecord("XXXXXXXXXXXX", session.Identity, this.channel.TypeId, "COAP", message.Payload.Length, MessageDirectionType.In, false, DateTime.UtcNow, "Not authorized, missing resource metadata, or channel encryption requirements"));
                    }

                    return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Unauthorized, message.Token);
                }

                string contentType = message.ContentType.HasValue ? message.ContentType.Value.ConvertToContentType() : "application/octet-stream";

                EventMessage msg = new EventMessage(contentType, uri.Resource, ProtocolType.COAP, message.Encode(), DateTime.UtcNow, metadata.Audit);

                if (!string.IsNullOrEmpty(uri.CacheKey))
                {
                    msg.CacheKey = uri.CacheKey;
                }

                if (uri.Indexes == null)
                {
                    await adapter.PublishAsync(msg);
                }
                else
                {
                    List<KeyValuePair<string, string>> indexes = new List<KeyValuePair<string, string>>(uri.Indexes);

                    Task task = Retry.ExecuteAsync(async () =>
                    {
                        await adapter.PublishAsync(msg, indexes);
                    });

                    task.LogExceptions();
                }


                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Created, message.Token);
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0} - CoAP publish error on channel '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), channel.Id);                
                throw ex;
            }
        }
        
        public async Task<CoapMessage> PutAsync(CoapMessage message)
        {
            CoapUri uri = new CoapUri(message.ResourceUri.ToString());
            ResponseMessageType rmt = message.MessageType == CoapMessageType.Confirmable ? ResponseMessageType.Acknowledgement : ResponseMessageType.NonConfirmable;

            if (!await adapter.CanSubscribeAsync(uri.Resource, channel.IsEncrypted))
            {
                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Unauthorized, message.Token);
            }

            if (coapObserved.ContainsKey(uri.Resource) || coapUnobserved.Contains(uri.Resource))
            {
                //resource previously subscribed 
                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.NotAcceptable, message.Token);
            }

            //this point the resource is not being observed, so we can
            // #1 subscribe to it
            // #2 add to unobserved resources (means not coap observed)

            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                IsEphemeral = true,
                Identity = session.Identity,
                Indexes = session.Indexes
            };

            string subscriptionUriString = await adapter.SubscribeAsync(uri.Resource, metadata);

            coapUnobserved.Add(uri.Resource);

            return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Created, message.Token);
        }
        
        private async Task LoadDurablesAsync()
        {
            List<string> list = await adapter.LoadDurableSubscriptionsAsync(session.Identity);

            if (list != null)
            {
                coapUnobserved = new HashSet<string>(list);
            }
        }

        #region IDisposable Support


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    adapter.OnObserve -= Adapter_OnObserve;
                    adapter.Dispose();
                    coapObserved.Clear();
                    coapUnobserved.Clear();
                    coapObserved = null;
                    coapUnobserved = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
