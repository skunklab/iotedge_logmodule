using Microsoft.Azure.EventHubs;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;


namespace Piraeus.Grains.Notifications
{
    public class EventHubSink : EventSink
    {
        //private EventHubSender sender;
        //private EventHubClient client;
        private Uri uri;
        private Auditor auditor;
        private string keyName;
        private string partitionId;
        private string hubName;
        private string connectionString;
        private ConcurrentQueue<byte[]> queue;
        private int delay;
        private int clientCount;
        private EventHubClient[] storageArray;
        private int arrayIndex;
        private PartitionSender[] senderArray;



        public EventHubSink(SubscriptionMetadata metadata)
            : base(metadata)
        {

            queue = new ConcurrentQueue<byte[]>();
            auditor = new Auditor();
            uri = new Uri(metadata.NotifyAddress);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            keyName = nvc["keyname"];
            partitionId = nvc["partitionid"];
            hubName = nvc["hub"];
            connectionString = String.Format("Endpoint=sb://{0}/;SharedAccessKeyName={1};SharedAccessKey={2}", uri.Authority, keyName, metadata.SymmetricKey);

            if (!int.TryParse(nvc["clients"], out clientCount))
            {
                clientCount = 1;
            }

            if (!int.TryParse(nvc["delay"], out delay))
            {
                delay = 1000;
            }

            if (!String.IsNullOrEmpty(partitionId))
            {
                senderArray = new PartitionSender[clientCount];
            }



            storageArray = new EventHubClient[clientCount];
            for (int i = 0; i < clientCount; i++)
            {                
                storageArray[i] = EventHubClient.CreateFromConnectionString(connectionString);

                if (!String.IsNullOrEmpty(partitionId))
                {
                    senderArray[i] = storageArray[i].CreatePartitionSender(partitionId.ToString());
                }
            }

        }

        


        public override async Task SendAsync(EventMessage message)
        {
            AuditRecord record = null;
            byte[] payload = null;

            try
            {
                
                byte[] msg = GetPayload(message);
                queue.Enqueue(msg);

                while (!queue.IsEmpty)
                {
                    arrayIndex = arrayIndex.RangeIncrement(0, clientCount - 1);
                    queue.TryDequeue(out payload);


                    if (payload == null)
                    {
                        Trace.TraceWarning("Subscription {0} could not write to event hub sink because payload was either null or unknown protocol type.");
                        return;
                    }

                    EventData data = new EventData(payload);
                    data.Properties.Add("Content-Type", message.ContentType);

                    if (String.IsNullOrEmpty(partitionId))
                    {
                        await storageArray[arrayIndex].SendAsync(data);
                    }
                    else
                    {
                        await senderArray[arrayIndex].SendAsync(data);
                    }

                    if (auditor.CanAudit && message.Audit)
                    {
                        record = new AuditRecord(message.MessageId, String.Format("sb://{0}/{1}", uri.Authority, hubName), "EventHub", "EventHub", payload.Length, MessageDirectionType.Out, true, DateTime.UtcNow);
                    }
                }
            }
            catch(Exception ex)
            {
                record = new AuditRecord(message.MessageId, String.Format("sb://{0}", uri.Authority, hubName), "EventHub", "EventHub", payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
                throw;
            }
            finally
            {
                if(message.Audit && auditor.CanAudit && record != null)
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
