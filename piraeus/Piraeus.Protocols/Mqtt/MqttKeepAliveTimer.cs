using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.Protocols.Mqtt
{
    public class MqttKeepAliveTimer : IDisposable
    {
        public event EventHandler OnExpired;

        public MqttKeepAliveTimer(int periodMilliseconds)
        {
            this.period = periodMilliseconds;
        }


        private Timer timer;
        private int period;

        public void Start()
        {
            this.timer = new Timer(new TimerCallback(Callback), null, this.period, this.period);
        }

        public void Stop()
        {
            this.timer.Dispose();
        }

        public void Reset()
        {
            this.timer.Dispose();
            this.timer = new Timer(new TimerCallback(Callback), null, this.period, period);
        }

        public void Callback(object state)
        {

            if (this.OnExpired != null)
            {
                OnExpired(this, new EventArgs());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
