using System;
using System.Collections.Generic;
using System.Timers;

namespace SkunkLab.Protocols.Utilities
{
    public delegate void RetryMessageEventHandler(object sender, RetryMessageEventArgs args);
    public delegate void RetryMaxAttemptsExceededEventHanlder(object sender, RetryMaxAttemptExceededEventArgs args);

    public class RetryTimer : IDisposable
    {
        public RetryTimer(ushort timerId, int maxAttempts, TimeSpan interval)
        {
            id = timerId;
            this.maxAttempts = maxAttempts;
            timer = new Timer(interval.TotalMilliseconds);
            timer.Enabled = true;
            timer.Elapsed += RetryTimer_Elapsed;
            timer.Start();

            container = new Dictionary<ushort, DateTime>();
        }

        public event RetryMessageEventHandler OnRetry;
        public event RetryMaxAttemptsExceededEventHanlder OnMaxAttemptsExceeded;
        private Dictionary<ushort, DateTime> container;

        private int maxAttempts;
        private int attemptCount;
        private ushort id;
        private Timer timer;
        private bool disposed;

        private void RetryTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (attemptCount + 1 == maxAttempts)
            {
                //retry limit
                //signal no more retries
                OnMaxAttemptsExceeded?.Invoke(this, new RetryMaxAttemptExceededEventArgs(id));
            }
            else
            {
                attemptCount++;
                //signal a retry
                OnRetry?.Invoke(this, new RetryMessageEventArgs(id));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Stop();
                timer.Dispose();
            }

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

