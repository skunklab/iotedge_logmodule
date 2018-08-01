using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace SkunkLab.Protocols.Mqtt
{
    public class PublishContainer : IDictionary<ushort, MqttMessage>, IDisposable
    {
        public PublishContainer(MqttConfig config)
        {
            exchangeLifetime = config.MaxTransmitSpan;
            container = new Dictionary<ushort, MqttMessage>();
            timeContainer = new Dictionary<ushort, DateTime>();
        }

        private TimeSpan exchangeLifetime;
        private Dictionary<ushort, MqttMessage> container;
        private Dictionary<ushort, DateTime> timeContainer;
        private Timer timer;
        private bool disposed;

        public MqttMessage this[ushort key]
        { get { return container[key]; } set { container[key] = value; } }

        public ICollection<ushort> Keys
        {
            get { return container.Keys; }
        }

        public ICollection<MqttMessage> Values
        {
            get { return container.Values; }
        }

        public int Count
        {
            get { return container.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(ushort key, MqttMessage value)
        {
            if (!container.ContainsKey(key))
            {
                container.Add(key, value);
                timeContainer.Add(key, DateTime.UtcNow.AddMilliseconds(exchangeLifetime.TotalMilliseconds));

                if (timer == null)
                {
                    timer = new Timer(exchangeLifetime.TotalMilliseconds);
                    timer.Elapsed += Timer_Elapsed;
                    timer.Start();
                }
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (timeContainer.Count > 0)
            {
                IEnumerable<KeyValuePair<ushort, DateTime>> items = timeContainer.Where((c) => c.Value < DateTime.UtcNow);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        container.Remove(item.Key);
                    }
                }
            }

            if (container.Count == 0)
            {
                timer.Stop();
                timer = null;
            }
        }

        public void Add(KeyValuePair<ushort, MqttMessage> item)
        {
            if (!container.ContainsKey(item.Key))
            {
                container.Add(item.Key, item.Value);
            }
        }

        public void Clear()
        {
            container.Clear();
        }

        public bool Contains(KeyValuePair<ushort, MqttMessage> item)
        {
            return container.Contains(item);
        }

        public bool ContainsKey(ushort key)
        {
            return container.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<ushort, MqttMessage>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<ushort, MqttMessage>> GetEnumerator()
        {
            return container.GetEnumerator();
        }

        public bool Remove(ushort key)
        {
            bool result = container.Remove(key);
            timeContainer.Remove(key);

            if (container.Count == 0)
            {
                timer.Stop();
                timer = null;
            }

            return result;
        }

        public bool Remove(KeyValuePair<ushort, MqttMessage> item)
        {
            bool result = container.Remove(item.Key);
            timeContainer.Remove(item.Key);

            if (container.Count == 0)
            {
                timer.Stop();
                timer = null;
            }

            return result;
        }

        public bool TryGetValue(ushort key, out MqttMessage value)
        {
            return container.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return container.GetEnumerator();
        }

        public void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        protected void Disposing(bool dispose)
        {
            if (dispose && !disposed)
            {
                disposed = true;
                if(timer != null)
                {
                    timer.Dispose();
                }

                timeContainer.Clear();
                container.Clear();
                timeContainer = null;
                container = null;
            }
        }
    }
}
