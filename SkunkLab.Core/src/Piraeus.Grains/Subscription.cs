using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.GrainInterfaces;
using System.Linq;
using Orleans.Providers;
using Piraeus.Grains.Notifications;
using Orleans.Concurrency;
using System.Diagnostics;

namespace Piraeus.Grains
{
    [Reentrant]
    [StorageProvider(ProviderName = "store")]
    public class Subscription : Grain<SubscriptionState>, ISubscription
    {
        [NonSerialized]
        private Queue<EventMessage> memoryMessageQueue;

        [NonSerialized]
        private IDisposable leaseTimer;

        [NonSerialized]
        private IDisposable messageQueueTimer;

        [NonSerialized]
        private EventSink sink;

        #region Activatio/Deactivation

        public override Task OnActivateAsync()
        {
            if(State.ErrorLeases == null)
            {
                State.ErrorLeases = new Dictionary<string, IErrorObserver>();
            }

            if(State.LeaseExpiry == null)
            {
                State.LeaseExpiry = new Dictionary<string, Tuple<DateTime, string>>();
            }

            if(State.MessageLeases == null)
            {
                State.MessageLeases = new Dictionary<string, IMessageObserver>();
            }

            if(State.MessageQueue == null)
            {
                State.MessageQueue = new Queue<EventMessage>();
            }

            memoryMessageQueue = new Queue<EventMessage>();

            if (State.MetricLeases == null)
            {
                State.MetricLeases = new Dictionary<string, IMetricObserver>();
            }

            DequeueAsync(State.MessageQueue).Ignore();

            return Task.CompletedTask;
        }

        public override async Task OnDeactivateAsync()
        {
            Trace.TraceInformation("Subscription deactivation '{0}'", State.Metadata.SubscriptionUriString);
            await WriteStateAsync();
        }
        #endregion

        #region Metrics

        public async Task<CommunicationMetrics> GetMetricsAsync()
        {
            CommunicationMetrics metrics = new CommunicationMetrics(State.Metadata.SubscriptionUriString, State.MessageCount, State.ByteCount, State.ErrorCount, State.LastMessageTimestamp, State.LastErrorTimestamp);
            return await Task.FromResult<CommunicationMetrics>(metrics);
        }
        #endregion

        #region ID
        public async Task<string> GetIdAsync()
        {
            if(State.Metadata == null)
            {
                return null;
            }

            return await Task.FromResult<string>(State.Metadata.SubscriptionUriString);
        }

        #endregion

        #region Clear
        public async Task ClearAsync()
        {
            await ClearStateAsync();
        }
        #endregion

        #region Metadata

        public async Task UpsertMetadataAsync(SubscriptionMetadata metadata)
        {
            State.Metadata = metadata;
            await WriteStateAsync();
        }

        public async Task<SubscriptionMetadata> GetMetadataAsync()
        {
            SubscriptionMetadata metadata = null;

            try
            {
                metadata = State.Metadata;

            }
            catch
            { }

            return await Task.FromResult<SubscriptionMetadata>(metadata);
        }

        #endregion

        #region Notification

        public async Task NotifyAsync(EventMessage message)
        {
            Exception error = null;

            State.MessageCount++;
            State.ByteCount += message.Message.LongLength;
            State.LastMessageTimestamp = DateTime.UtcNow;

            try
            {
                if (!string.IsNullOrEmpty(State.Metadata.NotifyAddress))
                {
                    if (sink == null)
                    {
                        sink = EventSinkFactory.Create(State.Metadata);
                    }

                    await sink.SendAsync(message);
                }
                else if (State.MessageLeases.Count > 0)
                {
                    //send to actively connected subsystem
                    foreach (var observer in State.MessageLeases.Values)
                    {
                        observer.Notify(message);
                    }
                }
                else
                {
                    if (State.Metadata.DurableMessaging && State.Metadata.TTL.HasValue) //durable message queue
                    {
                        await QueueDurableMessageAsync(message);
                    }
                    else //in-memory message queue
                    {
                        await QueueInMemoryMessageAsync(message);
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Subscription publish failed to complete.");
                Trace.TraceError("Subscription publish error {0}", ex.Message);
                error = ex;
                //GetLogger().Log(2, Orleans.Runtime.Severity.Error, "Subscription notification exception {0}", new object[] { State.Metadata.SubscriptionUriString }, ex);                
            }

            await NotifyMetricsAsync();

            if(error != null)
            {
                await NotifyErrorAsync(error);
            }
        }

        public async Task NotifyAsync(EventMessage message, List<KeyValuePair<string, string>> indexes)
        {
            try
            {
                if (indexes == null)
                {
                    await NotifyAsync(message);
                    return;
                }

                var query = indexes.Where((c) => State.Metadata.Indexes.Contains(new KeyValuePair<string, string>(c.Key, c.Value)));

                if (indexes.Count == query.Count())
                {
                    await NotifyAsync(message);
                }
                else
                {
                    State.MessageCount++;
                    State.ByteCount += message.Message.LongLength;
                    State.LastMessageTimestamp = DateTime.UtcNow;
                }
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Subscription publish with indexes failed to complete.");
                Trace.TraceError("Subscription publish with indexes error {0}", ex.Message);
                //GetLogger().Log(2, Orleans.Runtime.Severity.Error, "Subscription notification exception {0}", new object[] { State.Metadata.SubscriptionUriString }, ex);
            }
        }

        #endregion

        #region Observers

        public async Task<string> AddObserverAsync(TimeSpan lifetime, IMessageObserver observer)
        {
            if (observer == null)
            {
                Exception ex = new ArgumentNullException("subscription message observer");
                await NotifyErrorAsync(ex);
                return await Task.FromResult<string>(null);
            }

            string leaseKey = Guid.NewGuid().ToString();
            State.MessageLeases.Add(leaseKey, observer);
            State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Message"));

            if (leaseTimer == null)
            {
                leaseTimer = RegisterTimer(CheckLeaseExpiryAsync, null, TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(60.0));
            }

            await DequeueAsync(State.MessageQueue);
            await WriteStateAsync();

            return await Task.FromResult<string>(leaseKey);
        }
        public async Task<string> AddObserverAsync(TimeSpan lifetime, IMetricObserver observer)
        {
            if (observer == null)
            {
                Exception ex = new ArgumentNullException("subscription metric observer");
                await NotifyErrorAsync(ex);
                return await Task.FromResult<string>(null);
            }

            string leaseKey = Guid.NewGuid().ToString();
            State.MetricLeases.Add(leaseKey, observer);
            State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Metric"));

            if (leaseTimer == null)
            {
                leaseTimer = RegisterTimer(CheckLeaseExpiryAsync, null, TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(60.0));
            }

            await WriteStateAsync();
            return await Task.FromResult<string>(leaseKey);

        }
        public async Task<string> AddObserverAsync(TimeSpan lifetime, IErrorObserver observer)
        {
            if(observer == null)
            {
                Exception ex = new ArgumentNullException("subscription error observer");
                await NotifyErrorAsync(ex);
                return await Task.FromResult<string>(null);
            }

            string leaseKey = Guid.NewGuid().ToString();
            State.ErrorLeases.Add(leaseKey, observer);
            State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Error"));

            if (leaseTimer == null)
            {
                leaseTimer = RegisterTimer(CheckLeaseExpiryAsync, null, TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(60.0));
            }

            await WriteStateAsync();
            return await Task.FromResult<string>(leaseKey);
        }
        public async Task RemoveObserverAsync(string leaseKey)
        {
            var messageQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Message");
            var metricQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Metric");
            var errorQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Error");

            //State.LeaseExpiry.Remove(leaseKey);

            if (messageQuery.Count() == 1)
            {
                State.MessageLeases.Remove(leaseKey);
                
               
                if (State.MessageLeases.Count == 0 && State.Metadata.IsEphemeral)
                {
                    //leaseTimer.Dispose();
                    await UnsubscribeFromResourceAsync();
                }
            }

            if (metricQuery.Count() == 1)
            {
                State.MetricLeases.Remove(leaseKey);
            }

            if (errorQuery.Count() == 1)
            {
                State.ErrorLeases.Remove(leaseKey);
            }

            State.LeaseExpiry.Remove(leaseKey);

            await WriteStateAsync();
        }

        public async Task<bool> RenewObserverLeaseAsync(string leaseKey, TimeSpan lifetime)
        {
            if (State.LeaseExpiry.ContainsKey(leaseKey))
            {
                Tuple<DateTime, string> value = State.LeaseExpiry[leaseKey];
                Tuple<DateTime, string> newValue = new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), value.Item2);
                State.LeaseExpiry[leaseKey] = newValue;
                await WriteStateAsync();
                return await Task.FromResult<bool>(true);
            }

            return await Task.FromResult<bool>(false);
        }

        #endregion


        #region private methods

        private async Task UnsubscribeFromResourceAsync()
        {
            //unsubscribe from resource
            string uriString = State.Metadata.SubscriptionUriString;
            Uri uri = new Uri(uriString);

            string resourceUriString = uriString.Replace("/" + uri.Segments[uri.Segments.Length - 1], "");
            IResource resource = GrainFactory.GetGrain<IResource>(resourceUriString);

            if (State.Metadata != null && !string.IsNullOrEmpty(State.Metadata.SubscriptionUriString))
            {
                await resource.UnsubscribeAsync(State.Metadata.SubscriptionUriString);
            }
        }

        private async Task NotifyErrorAsync(Exception ex)
        {
            try
            {
                if (State.ErrorLeases.Count == 0)
                {
                    return;
                }

                foreach (var item in State.ErrorLeases.Values)
                {
                    item.NotifyError(State.Metadata.SubscriptionUriString, ex);
                }
            }
            catch(Exception ex1)
            {
                Trace.TraceWarning("Subscription notify error failed to complete.");
                Trace.TraceError("Subscription notify error with error {0}", ex1.Message);
            }
            await Task.CompletedTask;
        }

        private async Task NotifyMetricsAsync()
        {
            try
            {
                if (State.MetricLeases.Count == 0)
                {
                    return;
                }

                foreach (var item in State.MetricLeases.Values)
                {
                    item.NotifyMetrics(new CommunicationMetrics(State.Metadata.SubscriptionUriString, State.MessageCount, State.ByteCount, State.ErrorCount, State.LastMessageTimestamp.Value, State.LastErrorTimestamp));
                }
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Subscription notify metrics failed to complete.");
                Trace.TraceError("Subscription notify metrics with error {0}", ex.Message);
            }
            await Task.CompletedTask;
        }

        private async Task CheckLeaseExpiryAsync(object args)
        {
            try
            {
                var messageQuery = State.LeaseExpiry.Where((c) => c.Value.Item1 < DateTime.UtcNow && c.Value.Item2 == "Message");
                var metricQuery = State.LeaseExpiry.Where((c) => c.Value.Item1 < DateTime.UtcNow && c.Value.Item2 == "Metric");
                var errorQuery = State.LeaseExpiry.Where((c) => c.Value.Item1 < DateTime.UtcNow && c.Value.Item2 == "Error");

                List<string> messageLeaseKeyList = new List<string>(messageQuery.Select((c) => c.Key));
                List<string> metricLeaseKeyList = new List<string>(metricQuery.Select((c) => c.Key));
                List<string> errorLeaseKeyList = new List<string>(errorQuery.Select((c) => c.Key));

                foreach (var item in messageLeaseKeyList)
                {
                    State.MessageLeases.Remove(item);
                    State.LeaseExpiry.Remove(item);
                    //GetLogger().Log(3, Orleans.Runtime.Severity.Warning, "Subscription {0} message lease expired", new object[] { State.Metadata.SubscriptionUriString }, null);

                    if (State.Metadata.IsEphemeral)
                    {
                        await UnsubscribeFromResourceAsync();
                    }
                }

                foreach (var item in metricLeaseKeyList)
                {
                    State.MetricLeases.Remove(item);
                    State.LeaseExpiry.Remove(item);
                }

                foreach (var item in errorLeaseKeyList)
                {
                    State.ErrorLeases.Remove(item);
                    State.LeaseExpiry.Remove(item);
                }
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Subscription check lease expiry failed.");
                Trace.TraceError("Subscription check lease expiry with error {0}", ex.Message);
            }
        }
        private async Task QueueDurableMessageAsync(EventMessage message)
        {
            try
            {
                if (State.MessageQueue.Count > 0)
                {
                    //remove expired messages
                    while (State.MessageQueue.Peek().Timestamp.Add(State.Metadata.TTL.Value) < DateTime.UtcNow)
                    {
                        State.MessageQueue.Dequeue();
                    }
                }

                //add the new message
                State.MessageQueue.Enqueue(message);

                //start the timer if not already started
                if (messageQueueTimer == null)
                {
                    messageQueueTimer = RegisterTimer(CheckQueueAsync, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(5.0));
                }
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Subscription queue durable message failed.");
                Trace.TraceError("Subscription queue durable message with error {0}", ex.Message);
            }

            await Task.CompletedTask;
        }

        private async Task QueueInMemoryMessageAsync(EventMessage message)
        {
            try
            {
                memoryMessageQueue.Enqueue(message);

                if (messageQueueTimer == null)
                {
                    messageQueueTimer = RegisterTimer(CheckQueueAsync, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(5.0));
                }

                DelayDeactivation(TimeSpan.FromSeconds(60.0));
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Subscription queue in-memorty message failed.");
                Trace.TraceError("Subscription queue in-memory message with error {0}", ex.Message);
            }
            await Task.CompletedTask;
        }

        private async Task CheckQueueAsync(object args)
        {
            //timer firing for queued messages

            try
            {
                if (State.MessageLeases.Count > 0)
                {
                    if (memoryMessageQueue != null)
                    {
                        await DequeueAsync(memoryMessageQueue);

                        if (memoryMessageQueue.Count > 0)
                        {
                            DelayDeactivation(State.Metadata.TTL.Value);
                        }
                    }

                    if (State.MessageQueue != null)
                    {
                        await DequeueAsync(State.MessageQueue);
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Subscription check queue failed.");
                Trace.TraceError("Subscription check queue with error {0}", ex.Message);
            }
        }


        private async Task DequeueAsync(Queue<EventMessage> queue)
        {
            try
            {
                EventMessage[] msgs = queue != null && queue.Count > 0 ? queue.ToArray() : null;
                queue.Clear();

                if (msgs != null)
                {
                    foreach (EventMessage msg in msgs)
                    {
                        if (msg.Timestamp.Add(State.Metadata.TTL.Value) > DateTime.UtcNow)
                        {
                            await NotifyAsync(msg);

                            if (State.Metadata.SpoolRate.HasValue)
                            {
                                await Task.Delay(State.Metadata.SpoolRate.Value);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Subscription dequeue failed.");
                Trace.TraceError("Subscription dequeue with error {0}", ex.Message);
            }
        }
        #endregion

    }
}
