using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Metadata;
using Piraeus.GrainInterfaces;
using System.Linq;
using Piraeus.Core.Messaging;
using Orleans.Providers;
using Orleans.Concurrency;
using System.Diagnostics;

namespace Piraeus.Grains
{
    [Reentrant]
    [StorageProvider(ProviderName = "store")]
    public class Resource : Grain<ResourceState>, IResource
    {
        [NonSerialized]
        IDisposable leaseTimer;


        #region Activation/Deactivation
        public override Task OnActivateAsync()
        {
            if(State.Subscriptions == null)
            {
                State.Subscriptions = new System.Collections.Generic.Dictionary<string, ISubscription>();
            }

            if(State.LeaseExpiry == null)
            {
                State.LeaseExpiry = new System.Collections.Generic.Dictionary<string, Tuple<DateTime, string>>();
            }

            if(State.MetricLeases == null)
            {
                State.MetricLeases = new Dictionary<string, IMetricObserver>();
            }

            if(State.ErrorLeases == null)
            {
                State.ErrorLeases = new Dictionary<string, IErrorObserver>();
            }

            return Task.CompletedTask;
        }

        public override async Task OnDeactivateAsync()
        {
            await WriteStateAsync();
        }
        #endregion

        #region Resource Metadata
        public async Task UpsertMetadataAsync(ResourceMetadata metadata)
        {
            if(metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            if(State.Metadata != null && metadata.ResourceUriString != this.GetPrimaryKeyString())
            {
                Trace.TraceWarning("Resource metadata identifier mismatch failed for resource metadata upsert.");
                Exception ex = new ResourceIdentityMismatchException(String.Format("Resource metadata {0} does not match grain identity {1}", State.Metadata.ResourceUriString, this.GetPrimaryKeyString()));
                await NotifyErrorAsync(ex);
                throw ex;
            }

            State.Metadata = metadata;

            IResourceList resourceList = GrainFactory.GetGrain<IResourceList>("resourcelist");
            await resourceList.AddAsync(metadata.ResourceUriString);

            await WriteStateAsync();
        }

        public async Task<CommunicationMetrics> GetMetricsAsync()
        {
            CommunicationMetrics metrics = new CommunicationMetrics(State.Metadata.ResourceUriString, State.MessageCount, State.ByteCount, State.ErrorCount, State.LastMessageTimestamp, State.LastErrorTimestamp);
            return await Task.FromResult<CommunicationMetrics>(metrics);
        }

        public async Task<ResourceMetadata> GetMetadataAsync()
        {
            return await Task.FromResult<ResourceMetadata>(State.Metadata);
        }
        #endregion

        #region Subscribe/Unsubscribe

        public async Task SubscribeAsync(ISubscription subscription)
        {
            if (subscription == null)
            {
                Exception ex = new ArgumentNullException("resource subscribe null");                
                //GetLogger().Log(1009, Orleans.Runtime.Severity.Error, "Resource subscribe null subscription on resource {0}", new object[] { State.Metadata.ResourceUriString }, ex);
                await NotifyErrorAsync(ex);
                return;
            }

            //test for invalid subscription uri string
            string id = await subscription.GetIdAsync();
            Uri uri = new Uri(id);

            //test for match with resource 
            if(id == null || State.Metadata.ResourceUriString != id.Replace("/" + uri.Segments[uri.Segments.Length - 1], ""))
            {
                Exception ex = new SubscriptionIdentityMismatchException(String.Format("Subscription identity is mismatched with resource. Subscription {0}, Resource {1}", id, State.Metadata.ResourceUriString));
                //GetLogger().Log(1010, Orleans.Runtime.Severity.Error, ex.Message, null, ex);
                await NotifyErrorAsync(ex);
                return;
            }

            //get the subscription into resource state
            if (State.Subscriptions.ContainsKey(id))
            {
                State.Subscriptions[id] = subscription;
            }
            else
            {
                State.Subscriptions.Add(id, subscription);
            }

            //determine if a durable subscriber
            SubscriptionMetadata metadata = await subscription.GetMetadataAsync();

            if(!metadata.IsEphemeral && !string.IsNullOrEmpty(metadata.Identity) && metadata.NotifyAddress == null)
            {
                //add as a durable active connection subscriber (no notify address)
                ISubscriber subscriber = GrainFactory.GetGrain<ISubscriber>(metadata.Identity.ToLowerInvariant());
                await subscriber.AddSubscriptionAsync(metadata.SubscriptionUriString);
            }

            await WriteStateAsync();
        }

        //private object GetLogger()
        //{
        //    throw new NotImplementedException();
        //}

        public async Task UnsubscribeAsync(string subscriptionUriString, string identity)
        {
            if (subscriptionUriString == null)
            {
                throw new ArgumentNullException("subscriptionUriString");
            }

            if(identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            await UnsubscribeAsync(subscriptionUriString);

            ISubscriber subscriber = GrainFactory.GetGrain<ISubscriber>(identity.ToLowerInvariant());
            await subscriber.RemoveSubscriptionAsync(subscriptionUriString);
            await WriteStateAsync();
        }

        public async Task UnsubscribeAsync(string subscriptionUriString)
        {
            if(subscriptionUriString == null)
            {
                throw new ArgumentNullException("subscriptionUriString");
            }

            if (State.Subscriptions.ContainsKey(subscriptionUriString))
            {
                State.Subscriptions.Remove(subscriptionUriString);
            }

            await WriteStateAsync();
            
        }

        public async Task<IEnumerable<string>> GetSubscriptionListAsync()
        {            
            return await Task.FromResult<IEnumerable<string>>(State.Subscriptions.Keys.ToArray());
        }

        #endregion

        #region Publish

        public async Task PublishAsync(EventMessage message)
        {
            if (message == null)
            {
                Trace.TraceWarning("Resource publish has null message");
                Exception ex = new ArgumentNullException("resource publish message");
                await NotifyErrorAsync(ex);
                return;
            }

            Exception error = null;

            try
            {
                State.MessageCount++;
                State.ByteCount += message.Message.LongLength;
                State.LastMessageTimestamp = DateTime.UtcNow;

                List<Task> taskList = new List<Task>();
                
                foreach (var item in State.Subscriptions.Values)
                {
                    taskList.Add(item.NotifyAsync(message));
                }

                await Task.WhenAll(taskList);

                await NotifyMetricsAsync();
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Resource publish failed to complete.");
                Trace.TraceError("Resource publish error {0}", ex.Message);
                error = ex;
                //GetLogger().Log(1006, Orleans.Runtime.Severity.Error, "Resource publish error {0}", new object[] { State.Metadata.ResourceUriString }, ex);
            }

            if(error != null)
            {
                await NotifyErrorAsync(error);
            }
        }

        public async Task PublishAsync(EventMessage message, List<KeyValuePair<string, string>> indexes)
        {
            if (message == null)
            {
                Trace.TraceWarning("Resource publish with indexes has null message");
                Exception ex = new ArgumentNullException("resource publish message");
                await NotifyErrorAsync(ex);
                return;
            }

            Exception error = null;

            try
            {
                State.MessageCount++;
                State.ByteCount += message.Message.LongLength;
                State.LastMessageTimestamp = DateTime.UtcNow;
                if (indexes == null)
                {
                    await PublishAsync(message);
                }
            }
            catch(AggregateException ae)
            {
                error = ae.Flatten().InnerException;
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Resource publish with indexes failed to complete.");
                Trace.TraceError("Resource publish with indexes error {0}", ex.Message);
                error = ex;                
            }

            if(error != null)
            {
                //GetLogger().Log(1008, Orleans.Runtime.Severity.Error, "Resource publish with index error {0}", new object[] { State.Metadata.ResourceUriString }, error);
                await NotifyErrorAsync(error);
            }
            

        }

        #endregion



        #region Clear

        public async Task ClearAsync()
        {
            foreach (ISubscription item in State.Subscriptions.Values)
            {
                string id = await item.GetIdAsync();
                if (id != null)
                {
                    await UnsubscribeAsync(id);
                }
            }

            if (State.Metadata != null)
            {
                IResourceList resourceList = GrainFactory.GetGrain<IResourceList>("resourcelist");
                await resourceList.RemoveAsync(State.Metadata.ResourceUriString);
            }

            await ClearStateAsync();
        }

        #endregion

        #region Observers
        public async Task<string> AddObserverAsync(TimeSpan lifetime, IMetricObserver observer)
        {
            if (observer == null)
            {
                Exception ex = new ArgumentNullException("resource metric observer");
                await NotifyErrorAsync(ex);
                return await Task.FromResult<string>(null);
            }


            Exception error = null;
            string leaseKey = null;

            try
            {
                leaseKey = Guid.NewGuid().ToString();
                State.MetricLeases.Add(leaseKey, observer);
                State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Metric"));

                if (leaseTimer == null)
                {
                    leaseTimer = RegisterTimer(CheckLeaseExpiryAsync, null, TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(60.0));
                }
            }
            catch(Exception ex)
            {
                error = ex;
                //GetLogger().Log(1001, Orleans.Runtime.Severity.Error, "Resource add metric observer {0}", new object[] { State.Metadata.ResourceUriString }, ex);                
            }

            if(error != null)
            {
                await NotifyErrorAsync(error);
            }

            return await Task.FromResult<string>(leaseKey);
        }

        public async Task<string> AddObserverAsync(TimeSpan lifetime, IErrorObserver observer)
        {
            if (observer == null)
            {
                Exception ex = new ArgumentNullException("resource error observer");
                await NotifyErrorAsync(ex);
                return await Task.FromResult<string>(null);
            }

            string leaseKey = null;
            Exception error = null;

            try
            {
                leaseKey = Guid.NewGuid().ToString();
                State.ErrorLeases.Add(leaseKey, observer);
                State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Error"));

                if (leaseTimer == null)
                {
                    leaseTimer = RegisterTimer(CheckLeaseExpiryAsync, null, TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(60.0));
                }
            }
            catch(Exception ex)
            {
                error = ex;
                //GetLogger().Log(1002, Orleans.Runtime.Severity.Error, "Resource add error observer {0}", new object[] { State.Metadata.ResourceUriString }, ex);                
            }

            if(error != null)
            {
                await NotifyErrorAsync(error);
            }

            return await Task.FromResult<string>(leaseKey);
        }

        public async Task RemoveObserverAsync(string leaseKey)
        {
            if (leaseKey == null)
            {
                Exception ex = new ArgumentNullException("resource remomove observer leasekey");
                await NotifyErrorAsync(ex);
                return;
            }

            Exception error = null;

            try
            {
                var metricQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Metric");
                var errorQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Error");

                State.LeaseExpiry.Remove(leaseKey);

                if (metricQuery.Count() == 1)
                {
                    State.MetricLeases.Remove(leaseKey);
                }

                if (errorQuery.Count() == 1)
                {
                    State.ErrorLeases.Remove(leaseKey);
                }
            }
            catch(Exception ex)
            {
                error = ex;
                //GetLogger().Log(1005, Orleans.Runtime.Severity.Error, "Resource remove observer {0}", new object[] { State.Metadata.ResourceUriString }, ex);
            }

            if(error != null)
            {
                await NotifyErrorAsync(error);
            }
        }

        public async Task<bool> RenewObserverLeaseAsync(string leaseKey, TimeSpan lifetime)
        {
            bool result = false;

            if (State.LeaseExpiry.ContainsKey(leaseKey))
            {
                Tuple<DateTime, string> value = State.LeaseExpiry[leaseKey];
                Tuple<DateTime, string> newValue = new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), value.Item2);
                State.LeaseExpiry[leaseKey] = newValue;
                result = true;
            }

            return await Task.FromResult<bool>(result);
        }

        #endregion


        #region private methods
        private async Task NotifyMetricsAsync()
        {
            if (State.MetricLeases.Count > 0)
            {
                foreach (var item in State.MetricLeases.Values)
                {
                    item.NotifyMetrics(new CommunicationMetrics(State.Metadata.ResourceUriString, State.MessageCount, State.ByteCount, State.ErrorCount, State.LastMessageTimestamp.Value, State.LastErrorTimestamp));
                }
            }

            await Task.CompletedTask;
        }

        private async Task NotifyErrorAsync(Exception ex)
        {
            if (State.ErrorLeases.Count > 0)
            {
                foreach (var item in State.ErrorLeases.Values)
                {
                    item.NotifyError(State.Metadata.ResourceUriString, ex);
                }
            }

            await Task.CompletedTask;
        }

        private async Task CheckLeaseExpiryAsync(object args)
        {
            var metricQuery = State.LeaseExpiry.Where((c) => c.Value.Item1 < DateTime.UtcNow && c.Value.Item2 == "Metric");
            var errorQuery = State.LeaseExpiry.Where((c) => c.Value.Item1 < DateTime.UtcNow && c.Value.Item2 == "Error");

            List<string> metricLeaseKeyList = new List<string>(metricQuery.Select((c) => c.Key));
            List<string> errorLeaseKeyList = new List<string>(errorQuery.Select((c) => c.Key));
                        
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

            if (State.LeaseExpiry.Count == 0)
            {
                leaseTimer.Dispose();
            }

            await Task.CompletedTask;
        }
        #endregion
    }
}
