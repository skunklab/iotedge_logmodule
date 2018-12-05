using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttPubAckHandler : MqttMessageHandler
    {
        public MqttPubAckHandler(MqttSession session, MqttMessage message)
            : base(session, message)
        {

        }

        public override async Task<MqttMessage> ProcessAsync()
        {
            Session.Unquarantine(Message.MessageId);
            return await Task.FromResult<MqttMessage>(null);
        }
    }
}
