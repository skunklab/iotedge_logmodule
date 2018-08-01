using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttPublishHandler : MqttMessageHandler
    {
        public MqttPublishHandler(MqttSession session, MqttMessage message, IMqttDispatch dispatcher)
            : base(session, message, dispatcher)
        {
        }


        

        public override async Task<MqttMessage> ProcessAsync()
        {
            if (!Session.IsConnected)
            {
                Session.Disconnect(Message);
                return null;
            }

            PublishMessage msg = Message as PublishMessage;
            Session.IncrementKeepAlive();

            if(msg.QualityOfServiceLevel == QualityOfServiceLevelType.AtMostOnce)
            {
                Dispatch(msg);
                return await Task.FromResult<MqttMessage>(null);
            }



            MqttMessage response = GetAck(msg);

            //if(!msg.Dup || (msg.Dup && !Session.IsQuarantined(msg.MessageId)))
            if (!Session.IsQuarantined(msg.MessageId))
            {
                Session.Quarantine(Message, DirectionType.In);
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
            else
            {
                Session.Unquarantine(msg.MessageId);
            }

            if (msg.QualityOfServiceLevel == QualityOfServiceLevelType.AtMostOnce)
            {
                return null;
            }
            else
            {
                return new PublishAckMessage(ackType, msg.MessageId);
            }
        }
    }
}
