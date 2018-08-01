using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttConnackHandler : MqttMessageHandler
    {
        public MqttConnackHandler(MqttSession session, MqttMessage message)
            : base(session, message)
        {

        }

        public override async  Task<MqttMessage> ProcessAsync()
        {
            //the connection has been accepted
            ConnectAckMessage msg = Message as ConnectAckMessage;            
            Session.IsConnected = msg.ReturnCode == ConnectAckCode.ConnectionAccepted;
            Session.Connect(msg.ReturnCode);
            Session.IncrementKeepAlive();
            return await Task.FromResult<MqttMessage>(null);
        }
    }
}
