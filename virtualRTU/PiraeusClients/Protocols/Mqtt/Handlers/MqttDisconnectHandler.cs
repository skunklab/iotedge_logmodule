using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttDisconnectHandler : MqttMessageHandler
    {
        public MqttDisconnectHandler(MqttSession session, MqttMessage message)
         : base(session, message)
        {

        }

        public override async Task<MqttMessage> ProcessAsync()
        {
            Session.Disconnect(Message);
            return await Task.FromResult<MqttMessage>(null);
        }
    }
}
