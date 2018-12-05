using System;
using System.Threading.Tasks;

namespace Piraeus.Core
{
    public static class TaskExtensions
    {
        public static void LogExceptions(this Task task)
        {
            task.ContinueWith(t =>
            {
                var aggException = t.Exception.Flatten();
                foreach (var exception in aggException.InnerExceptions)
                    Console.WriteLine(exception.Message);


            },
            TaskContinuationOptions.OnlyOnFaulted);
        }

        //public static void Ignore(this Task task)
        //{
        //    if (task.IsCompleted)
        //    {
        //        var ignored = task.Exception;
        //    }
        //    else
        //    {
        //        IgnoreAsync(task);
        //    }

        //    async void IgnoreAsync(Task asyncTask)
        //    {
        //        try
        //        {
        //            await asyncTask.ConfigureAwait(false);
        //        }
        //        catch
        //        {
        //            // Ignored.
        //        }
        //    }
        //}
    }
}
