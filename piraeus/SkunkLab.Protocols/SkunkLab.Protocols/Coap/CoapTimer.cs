
namespace SkunkLab.Protocols.Coap
{
    using System;
    using System.Timers;

    public delegate void CoapTimerEventHandler(object sender, CoapTimerArgs args);
    public class CoapTimer : IDisposable
    {
        public CoapTimer(CoapMessage message, string internalMessageId)
        {
            this.message = message;
            this.internalMessageId = internalMessageId;
            this.interval = Convert.ToDouble(CoapConstants.Timeouts.AckTimeout.Milliseconds) * Convert.ToDouble(CoapConstants.Timeouts.AckRandomFactor);
            this.timer = new Timer(interval);
            this.timer.Elapsed += timer_Elapsed;
            this.startTime = DateTime.Now;
            this.timer.Start();
        }

        public event CoapTimerEventHandler Timeout;
        private double interval;
        private Timer timer;
        private int retryAttempt;
        private CoapMessage message;
        private DateTime startTime;
        private string internalMessageId;
        public void Decrement()
        {
            retryAttempt++;
            if (retryAttempt < CoapConstants.Timeouts.MaxRetransmit)
            {
                this.interval = this.interval * 2;
                this.timer.Interval = this.interval;
            }
        }
        public void Stop()
        {
            this.timer.Stop();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Timeout != null)
            {
                Timeout(this, new CoapTimerArgs(this.retryAttempt, this.startTime, this.message, this.internalMessageId));
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
