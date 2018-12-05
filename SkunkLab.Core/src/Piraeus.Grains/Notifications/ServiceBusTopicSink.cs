using Microsoft.Azure.ServiceBus;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;

namespace Piraeus.Grains.Notifications
{
    public class ServiceBusTopicSink : EventSink
    {
        public ServiceBusTopicSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            auditor = new Auditor();
            uri = new Uri(metadata.NotifyAddress);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            keyName = nvc["keyname"];
            topic = nvc["topic"];
            string symmetricKey = metadata.SymmetricKey;
            connectionString = String.Format("Endpoint=sb://{0}/;SharedAccessKeyName={1};SharedAccessKey={2}", uri.Authority, keyName, symmetricKey);
        }

        private string keyName;
        private string topic;
        private string connectionString;
        private TopicClient client;
        private Auditor auditor;
        private Uri uri;


        public override async Task SendAsync(EventMessage message)
        {
            AuditRecord record = null;

            try
            {
                byte[] payload = GetPayload(message);
                if (payload == null)
                {
                    Trace.TraceWarning("Subscription {0} could not write to service bus sink because payload was either null or unknown protocol type.");
                    return;
                }

                if (client == null)
                {
                    //client = TopicClient.CreateFromConnectionString(connectionString, topic);
                    client = new TopicClient(connectionString, topic);
                }

                Message brokerMessage = new Message(payload);
                brokerMessage.ContentType = message.ContentType;
                brokerMessage.MessageId = message.MessageId;
                await client.SendAsync(brokerMessage);
                record = new AuditRecord(message.MessageId, String.Format("sb://{0}/{1}",uri.Authority, topic), "ServiceBus", "ServiceBus", message.Message.Length, MessageDirectionType.Out, true, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Service bus failed to send to topic with error {0}",ex.Message);
                record = new AuditRecord(message.MessageId, String.Format("sb://{0}/{1}", uri.Authority, topic), "ServiceBus", "ServiceBus", message.Message.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
            }
            finally
            {
                if(message.Audit && record != null && auditor.CanAudit)
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
