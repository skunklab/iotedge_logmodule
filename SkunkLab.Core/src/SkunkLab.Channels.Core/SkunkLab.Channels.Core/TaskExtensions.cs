using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels
{
    public static class TaskExtensions
    {


        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(
                    s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            return await task;
        }

        public static void Ignore(this Task task)
        {
            if (task.IsCompleted)
            {
                var ignored = task.Exception;
            }
            else
            {
                IgnoreAsync(task);
            }

            async void IgnoreAsync(Task asyncTask)
            {
                try
                {
                    await asyncTask.ConfigureAwait(false);
                }
                catch
                {
                    // Ignored.
                }
            }
        }
    }
}
