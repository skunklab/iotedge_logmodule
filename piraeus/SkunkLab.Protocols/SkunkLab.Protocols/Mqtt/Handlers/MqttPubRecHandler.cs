using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttPubRecHandler : MqttMessageHandler
    {
        public MqttPubRecHandler(MqttSession session, MqttMessage message)
            : base(session, message)
        {

        }

        public override async  Task<MqttMessage> ProcessAsync()
        {
            Session.IncrementKeepAlive();
            return await Task.FromResult<MqttMessage>(new PublishAckMessage(PublishAckType.PUBREL, this.Message.MessageId));
        }
    }
}
