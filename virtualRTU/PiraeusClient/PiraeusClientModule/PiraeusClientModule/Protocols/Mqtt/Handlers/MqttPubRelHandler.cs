using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Protocols.Utilities;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttPubRelHandler : MqttMessageHandler
    {
        public MqttPubRelHandler(MqttSession session, MqttMessage message, IMqttDispatch dispatcher)
            : base(session, message, dispatcher)
        {

        }
        public override async Task<MqttMessage> ProcessAsync()
        {
            Session.IncrementKeepAlive();
            MqttMessage message = Session.GetHeldMessage(Message.MessageId);

            if(message != null)
            {
                PublishMessage msg = message as PublishMessage;
                MqttUri uri = new MqttUri(msg.Topic);
                if (Dispatcher != null)
                {
                    Dispatcher.Dispatch(uri.Resource, uri.ContentType, msg.Payload);
                }
                else
                {
                    Session.Publish(msg, true);
                }
            }
            
            return await Task.FromResult<MqttMessage>(new PublishAckMessage(PublishAckType.PUBCOMP, Message.MessageId));
        }
    }
}
