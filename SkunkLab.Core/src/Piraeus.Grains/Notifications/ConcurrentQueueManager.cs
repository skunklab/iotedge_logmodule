using Piraeus.Core.Messaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Grains.Notifications
{
    public class ConcurrentQueueManager
    {
        private ConcurrentQueue<EventMessage> queue;

        public ConcurrentQueueManager()
        {
            queue = new ConcurrentQueue<EventMessage>();
        }

        public bool IsEmpty
        {
            get { return queue.IsEmpty; }
        }


        public Task EnqueueAsync(EventMessage message)
        {
            TaskCompletionSource<Task> tcs = new TaskCompletionSource<Task>();
            queue.Enqueue(message);
            tcs.SetResult(null);
            return tcs.Task;
        }

        public Task<EventMessage> DequeueAsync()
        {
            TaskCompletionSource<EventMessage> tcs = new TaskCompletionSource<EventMessage>();
            if (!queue.IsEmpty)
            {
                EventMessage message = null;
                bool result = queue.TryDequeue(out message);

                tcs.SetResult(result ? message : null);
            }

            return tcs.Task;
        }


    }
}
