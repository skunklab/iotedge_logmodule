using System;
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
            if (!Session.IsConnected)
            {
                Session.Disconnect(Message);
                return null;
            }

            Session.IncrementKeepAlive();
            List<QualityOfServiceLevelType> list = new List<QualityOfServiceLevelType>();
            SubscribeMessage msg = Message as SubscribeMessage;
            
            List<string> validSubs = Session.Subscribe(Message); 
            IEnumerator<KeyValuePair<string, QualityOfServiceLevelType>> en = msg.Topics.GetEnumerator();            
            while(en.MoveNext())
            {
                MqttUri uri = new MqttUri(en.Current.Key);
                QualityOfServiceLevelType qos = validSubs.Contains(uri.ToString()) ? en.Current.Value : QualityOfServiceLevelType.Failure;
                list.Add(qos);
                Session.AddQosLevel(uri.Resource, qos);
            }
            
            return await Task.FromResult<MqttMessage>(new SubscriptionAckMessage(Message.MessageId,list));
        }
    }
}
