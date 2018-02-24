using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace SkunkLab.Protocols.Mqtt
{
    

    public class MqttQuarantineTimer
    {
        public MqttQuarantineTimer(MqttConfig config)
        {
            this.config = config;
            container = new Dictionary<ushort, RetryMessageData>();

            timer = new Timer(config.AckTimeout.TotalMilliseconds);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        public event EventHandler<MqttMessageEventArgs> OnRetry;
        private MqttConfig config;
        private Dictionary<ushort, RetryMessageData> container;
        private Timer timer;
        private ushort currentId;

        public ushort NewId()
        {
            currentId++;
            currentId = currentId == ushort.MaxValue ? (ushort)1 : currentId;

            while (container.ContainsKey(currentId))
            {
                currentId++;
                currentId = currentId == ushort.MaxValue ? (ushort)1 : currentId;
            }
            return currentId;
        }

        public bool ContainsKey(ushort id)
        {
            return container.ContainsKey(id);
        }

        public void Add(MqttMessage message)
        {
            if(message.QualityOfService == QualityOfServiceLevelType.AtMostOnce)
            {
                return;
            }

            if(!container.ContainsKey(message.MessageId))
            {
                DateTime timeout = DateTime.UtcNow.AddMilliseconds(config.AckTimeout.TotalMilliseconds);
                RetryMessageData amd = new RetryMessageData(message, timeout, 0);
                container.Add(message.MessageId, amd);
            }
        }

        public void Remove(ushort id)
        {
            container.Remove(id);            
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<ushort> list = new List<ushort>();

            IEnumerable<KeyValuePair<ushort, RetryMessageData>> items = container.Where((c) => c.Value.NextRetryTime > DateTime.UtcNow);
            if (items != null)
            {
                foreach (var item in items)
                {
                    item.Value.Increment(config.AckTimeout);
                    container[item.Key] = item.Value;

                    if (item.Value.AttemptCount >= config.MaxRetransmit)
                    {
                        //add expired items to list to be removed
                        list.Add(item.Key);
                    }
                    else
                    {
                        //signal retransmit
                        OnRetry?.Invoke(this, new MqttMessageEventArgs(item.Value.Message));
                    }
                }
            }

            //remove expired items
            foreach (var item in list)
            {
                Remove(item);
            }
        }
    }
}
