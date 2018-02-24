using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttConnectHandler : MqttMessageHandler
    {
        public MqttConnectHandler(MqttSession session, MqttMessage message)
            : base(session, message)
        {
           
        }

        public override async Task<MqttMessage> ProcessAsync()
        {
            ConnectMessage msg = Message as ConnectMessage;
            //authenticate
            string tokenType = msg.Username;
            string token = msg.Password;

            //wrong protocol version
            if(msg.ProtocolVersion != 4)
            {
                return await Task.FromResult<MqttMessage>(new ConnectAckMessage(false, ConnectAckCode.UnacceptableProtocolVersion));                
            }

            //0-byte client id and clean session = 0
            if(msg.ClientId == null && !msg.CleanSession)
            {
                return await Task.FromResult<MqttMessage>(new ConnectAckMessage(false, ConnectAckCode.IdentifierRejected));
            }

            if(msg.Username == null || msg.Password == null)
            {
                return await Task.FromResult<MqttMessage>(new ConnectAckMessage(false, ConnectAckCode.BadUsernameOrPassword));
            }

            //if not authn'd send back not authz'd

            return await Task.FromResult<MqttMessage>(new ConnectAckMessage(false, ConnectAckCode.ConnectionAccepted));






            //return new ConnectAckMessage(false, ConnectAckCode.ConnectionAccepted);
            //ConnectAckMessage msg = new ConnectAckMessage(false, ConnectAckCode.ConnectionAccepted);
            //return msg;
        }

       
    }
}
