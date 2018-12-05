using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SkunkLab.Storage
{
    public static class Retry
    {

        public async static Task ExecuteAsync(Action retryOperation)
        {
            await ExecuteAsync(retryOperation, TimeSpan.FromMilliseconds(250), 3);
        }

        
        
        public async static Task ExecuteAsync(Action retryOperation, TimeSpan deltaBackoff, int maxRetries)
        {
            int delayMilliseconds = Convert.ToInt32(deltaBackoff.TotalMilliseconds);

            if (maxRetries < 1)
            {
                throw new ArgumentOutOfRangeException("Retry maxRetries must be >= 1.");
            }

            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    await Task.Run(retryOperation);
                    break;
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        Trace.TraceWarning("WARNING: Task failed all retries.");
                        Trace.TraceError("ERORR: Task retry error {0}", ex.Message);
                        Trace.TraceError("ERORR: Task retry stack trace {0}", ex.StackTrace);
                    }
                    else
                    {
                        Trace.TraceWarning("WARNING: Task in retry mode.");
                    }

                    await Task.Delay(delayMilliseconds);
                    attempt++;
                }
            }

            //throw new OperationCanceledException("Operation cancelled due to retry failure.");
        }

        public static void Execute(Action retryOperation)
        {
            Execute(retryOperation, TimeSpan.FromMilliseconds(250), 3);
        }

        public static void Execute(Action retryOperation, TimeSpan deltaBackoff, int maxRetries)
        {
            int delayMilliseconds = Convert.ToInt32(deltaBackoff.TotalMilliseconds);
            if (maxRetries < 1)
            {
                throw new ArgumentOutOfRangeException("Retry maxRetries must be >= 1.");
            }

            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    retryOperation();
                    return;
                }
                catch
                {
                    if (attempt == maxRetries)
                    {
                        throw;
                    }

                    Task task = Task.Delay(delayMilliseconds);
                    Task.WaitAll(task);
                    attempt++;
                }
            }

            throw new OperationCanceledException("Operation cancelled due to retry failure.");
        }

    }
}
