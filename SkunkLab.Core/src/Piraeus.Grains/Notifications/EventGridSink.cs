using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Rest;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;

namespace Piraeus.Grains.Notifications
{
    public class EventGridSink : EventSink
    {

        private string topicKey;
        private string topicHostname;
        private int clientCount;
        private EventGridClient[] clients;
        private string resourceUriString;
        private int arrayIndex;
        private Auditor auditor;
        private Uri uri;

        public EventGridSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            auditor = new Auditor();
            uri = new Uri(metadata.NotifyAddress);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            topicHostname = uri.Authority;
            topicKey = metadata.SymmetricKey;
            string uriString = new Uri(metadata.SubscriptionUriString).ToString();
            resourceUriString = uriString.Replace("/" + uri.Segments[uri.Segments.Length - 1], "");
            if (!int.TryParse(nvc["clients"], out clientCount))
            {
                clientCount = 1;
            }

            ServiceClientCredentials credentials = new TopicCredentials(topicKey);

            clients = new EventGridClient[clientCount];
            for (int i=0;i<clientCount;i++)
            {
                clients[i] = new EventGridClient(credentials);
            }
        }

        


        public override async Task SendAsync(EventMessage message)
        {
            AuditRecord record = null;
            byte[] payload = null;


            try
            {
                arrayIndex = arrayIndex.RangeIncrement(0, clientCount - 1);
                payload = GetPayload(message);
                if (payload == null)
                {
                    Trace.TraceWarning("Subscription {0} could not write to blob storage sink because payload was either null or unknown protocol type.");
                    return;
                }

                EventGridEvent gridEvent = new EventGridEvent(message.MessageId, resourceUriString, payload, resourceUriString, DateTime.UtcNow, "1.0");
                IList<EventGridEvent> events = new List<EventGridEvent>(new EventGridEvent[] { gridEvent });
                Task task =  clients[arrayIndex].PublishEventsAsync(topicHostname, events);
                Task innerTask = task.ContinueWith(async (a) => { await FaultTask(message.MessageId, payload, message.ContentType, auditor.CanAudit && message.Audit); }, TaskContinuationOptions.OnlyOnFaulted);
                await Task.WhenAll(task);

                record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "EventGrid", "EventGrid", payload.Length, MessageDirectionType.Out, true, DateTime.UtcNow);

            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Initial event grid write error {0}", ex.Message);
                record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "EventGrid", "EventGrid", payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
            }
            finally
            {
                if (auditor.CanAudit && message.Audit && record != null)
                {
                    await auditor.WriteAuditRecordAsync(record);
                }
            }
        }

        private async Task FaultTask(string id,  byte[] payload, string contentType, bool canAudit)
        {
            AuditRecord record = null;
            try
            {
                ServiceClientCredentials credentials = new TopicCredentials(topicKey);
                EventGridClient client = new EventGridClient(credentials);
                EventGridEvent gridEvent = new EventGridEvent(id, resourceUriString, payload, resourceUriString, DateTime.UtcNow, "1.0");
                IList<EventGridEvent> events = new List<EventGridEvent>(new EventGridEvent[] { gridEvent });
                await clients[arrayIndex].PublishEventsAsync(topicHostname, events);
                record = new AuditRecord(id, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "EventGrid", "EventGrid", payload.Length, MessageDirectionType.Out, true, DateTime.UtcNow);
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Retry EventGrid failed.");
                Trace.TraceError(ex.Message);
                record = new AuditRecord(id, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "EventGrid", "EventGrid", payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
            }
            finally
            {
                if (canAudit)
                {
                    await auditor.WriteAuditRecordAsync(record);
                }
            }
        }

        private byte[] GetPayload(EventMessage message)
        {
            switch (message.Protocol)
            {
                case ProtocolType.COAP:
                    CoapMessage coap = CoapMessage.DecodeMessage(message.Message);
                    return coap.Payload;
                case ProtocolType.MQTT:
                    MqttMessage mqtt = MqttMessage.DecodeMessage(message.Message);
                    return mqtt.Payload;
                case ProtocolType.REST:
                    return message.Message;
                case ProtocolType.WSN:
                    return message.Message;
                default:
                    return null;
            }
        }
    }
}
