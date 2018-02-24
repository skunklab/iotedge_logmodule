using System.Threading.Tasks;
using SkunkLab.Protocols.Utilities;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttPublishHandler : MqttMessageHandler
    {
        public MqttPublishHandler(MqttSession session, MqttMessage message, IDispatch dispatcher)
            : base(session, message, dispatcher)
        {
        }


        

        public override async Task<MqttMessage> ProcessAsync()
        {
            PublishMessage msg = Message as PublishMessage;
            Session.IncrementKeepAlive();

            if(msg.QualityOfServiceLevel == QualityOfServiceLevelType.AtMostOnce)
            {
                Dispatch(msg);
                return await Task.FromResult<MqttMessage>(null);
            }

            MqttMessage response = GetAck(msg);

            if(!msg.Dup || (msg.Dup && !Session.IsQuarantined(msg.MessageId)))
            {
                Session.Quarantine(Message);
                Dispatch(msg);               
            }

            return await Task.FromResult<MqttMessage>(response);
            
        }

        private void Dispatch(PublishMessage msg)
        {
            MqttUri uri = new MqttUri(msg.Topic);
            if (Dispatcher != null)
            {
                Dispatcher.Dispatch(uri.Resource, uri.ContentType, msg.Payload);
            }
            else
            {
                Session.Publish(msg);
            }
        }

        private MqttMessage GetAck(PublishMessage msg)
        {
            PublishAckType ackType = msg.QualityOfServiceLevel == QualityOfServiceLevelType.AtLeastOnce ? PublishAckType.PUBACK : PublishAckType.PUBREC;

            if (ackType == PublishAckType.PUBREC)
            {
                Session.HoldMessage(msg);
            }

            return new PublishAckMessage(ackType, msg.MessageId);
        }
    }
}
