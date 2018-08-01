
namespace SkunkLab.Channels.Core.WebSocket
{
   
        using System;
        using System.Threading.Tasks;

        internal sealed class TaskQueue
        {
            private Task _lastQueuedTask = Task.FromResult<int>(0);
            private readonly object _lockObj = new object();

            public Task Enqueue(Func<Task> taskFunc)
            {
                Func<Task, Task> continuationFunction = null;
                lock (this._lockObj)
                {
                    if (continuationFunction == null)
                    {
                        continuationFunction = _ => taskFunc();
                    }
                    Task task = this._lastQueuedTask.ContinueWith<Task>(continuationFunction, TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap();
                    this._lastQueuedTask = task;
                    return task;
                }
            }
        }
    

}
