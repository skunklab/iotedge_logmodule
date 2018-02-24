using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttSubscribeHandler : MqttMessageHandler
    {
        public MqttSubscribeHandler(MqttSession session, MqttMessage message)
            : base(session, message)
        {

        }

        public override async Task<MqttMessage> ProcessAsync()
        {
            Session.IncrementKeepAlive();
            SubscribeMessage msg = Message as SubscribeMessage;
            IEnumerator<KeyValuePair<string, QualityOfServiceLevelType>> en = msg.Topics.GetEnumerator();
            while(en.MoveNext())
            {
                MqttUri uri = new MqttUri(en.Current.Key);
                Session.AddQosLevel(uri.Resource, en.Current.Value);
            }

            Session.Subscribe(Message);
            return await Task.FromResult<MqttMessage>(new UnsubscribeAckMessage(Message.MessageId));
        }
    }
}
