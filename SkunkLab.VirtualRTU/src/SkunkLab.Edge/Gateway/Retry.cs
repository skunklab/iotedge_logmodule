using System;
using System.Threading.Tasks;

namespace SkunkLab.Edge.Gateway
{

    

    public class Retry
    {
        public event System.EventHandler<RetryEventArgs> OnTimeout;
        public event System.EventHandler<RetryEventArgs> OnComplete;

        /// <summary>
        /// Executes an async function with a return type with retries.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="sigmoidTime"></param>
        /// <param name="maxRetries"></param>
        /// <param name="configureAwait"></param>
        /// <returns></returns>
        /// <remarks>Should be in the form Func&lt;Task&lt;T&gt;&gt; func = (async () => /*code here with await */ });</remarks>
        private async Task<T> ExecuteAsync<T>(Func<Task<T>> func, TimeSpan sigmoidTime, int maxRetries, SigmoidType sigmoidFactor = SigmoidType.FullSigmoid, bool configureAwait = true)
        {
            if (maxRetries < 1)
            {
                throw new ArgumentOutOfRangeException("Retry maxRetries must be >= 1.");
            }

            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    return await func.Invoke().ConfigureAwait(configureAwait);
                }
                catch
                {
                    if (attempt == maxRetries)
                    {
                        break;
                    }
                    else
                    {
                        await Task.Delay(GetSigmoid(attempt, maxRetries, sigmoidTime, sigmoidFactor));
                        attempt++;
                    }
                }
            }

            OnTimeout?.Invoke(typeof(Retry), new RetryEventArgs(attempt, maxRetries, sigmoidFactor, sigmoidTime));
            throw new OperationCanceledException("Operation cancelled due to retry failure.");
        }
       
        
        public async Task ExecuteAsync(Action retryOperation, TimeSpan sigmoidTime, int maxRetries, SigmoidType sigmoidFactor = SigmoidType.FullSigmoid, bool configurawait = true)
        {
            if (maxRetries < 1)
            {
                throw new ArgumentOutOfRangeException("Retry maxRetries must be >= 1.");
            }

            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {                    
                    await Task.Run(retryOperation).ConfigureAwait(configurawait);
                    OnComplete?.Invoke(typeof(Retry), new RetryEventArgs(attempt, maxRetries, sigmoidFactor, sigmoidTime));
                    return;
                }
                catch 
                {
                    if (attempt != maxRetries)
                    {
                        await Task.Delay(GetSigmoid(attempt, maxRetries, sigmoidTime, sigmoidFactor));
                        attempt++;
                    }
                }
            }

            if (OnTimeout == null)
            {
                throw new OperationCanceledException("Operation cancelled due to retry failure.");
            }
            else
            {
                OnTimeout?.Invoke(typeof(Retry), new RetryEventArgs(attempt, maxRetries, sigmoidFactor, sigmoidTime));
            }
        }

        /// <summary>
        /// Retries an sync action without parameters
        /// </summary>
        /// <param name="action"></param>
        /// <param name="sigmoidTime">The time divided by the sigmoid to determine the delay.</param>
        /// <param name="maxRetries">Number of possible retries.</param>
        /// <remarks>Throws OperationCancelledException if the operation is not completed.</remarks>
        public void Execute(Action action, TimeSpan sigmoidTime, int maxRetries, SigmoidType sigmoidFactor = SigmoidType.FullSigmoid)
        {
            if (maxRetries < 1)
            {
                throw new ArgumentOutOfRangeException("Retry maxRetries must be >= 1.");
            }

            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    action();
                    OnComplete?.Invoke(typeof(Retry), new RetryEventArgs(attempt, maxRetries, sigmoidFactor, sigmoidTime));
                    return;
                }
                catch
                {
                    if (attempt != maxRetries)
                    {
                        Task task = Task.Delay(GetSigmoid(attempt, maxRetries, sigmoidTime, sigmoidFactor));
                        Task.WaitAll(task);
                        attempt++;
                    }
                }
            }

            if (OnTimeout == null)
            {
                throw new OperationCanceledException("Operation cancelled due to retry failure.");
            }
            else
            {
                OnTimeout?.Invoke(typeof(Retry), new RetryEventArgs(attempt, maxRetries, sigmoidFactor, sigmoidTime));
            }
        }

        public T Execute<T>(Func<T> func, TimeSpan sigmoidTime, int maxRetries, SigmoidType sigmoidFactor = SigmoidType.FullSigmoid)
        {
            if (maxRetries < 1)
            {
                throw new ArgumentOutOfRangeException("Retry maxRetries must be >= 1.");
            }

            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    return func();
                }
                catch
                {
                    if (attempt != maxRetries)
                    {
                        Task task = Task.Delay(GetSigmoid(attempt, maxRetries, sigmoidTime, sigmoidFactor));
                        Task.WaitAll(task);
                        attempt++;
                    }
                }
            }

            OnTimeout?.Invoke(typeof(Retry), new RetryEventArgs(attempt, maxRetries, sigmoidFactor, sigmoidTime));
            throw new OperationCanceledException("Operation cancelled due to retry failure.");
        }


        private int GetSigmoid(int attempt, int maxRetries, TimeSpan deltaBackoff, SigmoidType sigmoidFactor = SigmoidType.FullSigmoid)
        {
            int divisor = (int)sigmoidFactor;
            double factorValue = Convert.ToDouble(divisor / 2.0);
            double factor = Convert.ToDouble(attempt / factorValue);
            
            return Convert.ToInt32(deltaBackoff.TotalMilliseconds / (Math.Exp(-1.0 * factor) / (Math.Exp(-1.0 * factor) + 1)));
        }
    }


    public enum SigmoidType
    {
        QuarterSigmoid = 8,
        HalfSigmoid = 4,
        FullSigmoid = 2,
        DoubleSigmoid = 1
    }

}
