using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttUnsubscribeHandler : MqttMessageHandler
    {
        public MqttUnsubscribeHandler(MqttSession session, MqttMessage message)
            : base(session, message)
        {

        }

        public override async Task<MqttMessage> ProcessAsync()
        {
            Session.IncrementKeepAlive();
            Session.Unsubscribe(Message);
            return await Task.FromResult<MqttMessage>(new UnsubscribeAckMessage(Message.MessageId));
        }
    }
}
