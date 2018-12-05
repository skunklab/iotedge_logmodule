using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace SkunkLab.Protocols.Coap
{
    public class Receiver : IDisposable
    {
        public Receiver(double lifetimeMilliseconds)
        {
            this.lifetimeMilliseconds = lifetimeMilliseconds;
            container = new Dictionary<ushort, DateTime>();
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
        }

        

        private double lifetimeMilliseconds;
        private Dictionary<ushort, DateTime> container;
        private Timer timer;
        private bool disposedValue;

        public void CacheId(ushort id)
        {
            if(!container.ContainsKey(id))
            {
                container.Add(id, DateTime.UtcNow.AddMilliseconds(lifetimeMilliseconds));
            }

            timer.Enabled = container.Count() > 0;
        }

        public bool IsDup(ushort id)
        {
            return container.ContainsKey(id);
        }

        public void Remove(ushort id)
        {
            container.Remove(id);
            timer.Enabled = container.Count() > 0;
        }

        public void Clear()
        {
            container.Clear();
            timer.Enabled = false;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var query = container.Where((c) => c.Value < DateTime.UtcNow);

            List<ushort> list = new List<ushort>();
            if(query != null && query.Count() > 0)
            {
                foreach(var item in query)
                {
                    list.Add(item.Key);
                }
            }

            foreach(var item in list)
            {
                container.Remove(item);
            }

            timer.Enabled = container.Count() > 0;

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (timer != null)
                    {
                        timer.Stop();
                        timer.Dispose();
                    }
                    
                    container.Clear();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
    }
}
