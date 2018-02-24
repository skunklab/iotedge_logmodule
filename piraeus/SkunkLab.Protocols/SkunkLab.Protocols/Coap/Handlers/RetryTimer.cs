//using System;
//using System.Timers;

//namespace SkunkLab.Protocols.Coap.Handlers
//{
//    public class RetryTimer<TMessage> : IDisposable
//    {
//        public RetryTimer(string timerId, int maxAttempts, TimeSpan interval, TMessage message)
//        {
//            id = timerId;
//            this.maxAttempts = maxAttempts;
//            msg = message;
//            timer = new Timer(interval.TotalMilliseconds);
//            timer.Enabled = true;
//            timer.Elapsed += RetryTimer_Elapsed;
//            timer.Start();
//        }

//        private TMessage msg;
//        private int maxAttempts;
//        private int attemptCount;
//        private string id;
//        private Timer timer;
//        private bool disposed;

//        private void RetryTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
//        {
//           if(attemptCount + 1 == maxAttempts)
//            {
//                //retry limit
//                //signal no more retries
//            }
//           else
//            {
//                attemptCount++;
//                //signal a retry
//            }
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (disposing)
//            {                
//                timer.Dispose();
//            }

//            disposed = true;
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}
