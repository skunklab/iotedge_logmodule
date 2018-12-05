using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FieldGatewayMicroservice
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
    }
}
