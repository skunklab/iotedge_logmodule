using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.GrainInterfaces;
using Piraeus.Grains;
using Piraeus.Grains.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Capl.Authorization;
using System.Security.Claims;
using System.Threading;
using System.Diagnostics;
using Piraeus.Core;

namespace Piraeus.Adapters
{
    public class OrleansAdapter
    {
        public OrleansAdapter(string identity, string channelType, string protocolType)
        {
            this.auditor = new Auditor();
            this.identity = identity;
            this.channelType = channelType;
            this.protocolType = protocolType;

            container = new Dictionary<string, Tuple<string, string>>();
            ephemeralObservers = new Dictionary<string, IMessageObserver>();
            durableObservers = new Dictionary<string, IMessageObserver>();
        }

        public event EventHandler<ObserveMessageEventArgs> OnObserve;   //signal protocol adapter

        private Auditor auditor;
        private string identity;
        private string channelType;
        private string protocolType;
        private Dictionary<string, Tuple<string, string>> container;  //resource, subscription + leaseKey
        private Dictionary<string, IMessageObserver> ephemeralObservers; //subscription, observer
        private Dictionary<string, IMessageObserver> durableObservers;   //subscription, observer
        private System.Timers.Timer leaseTimer; //timer for leases
        private bool disposedValue = false; // To detect redundant calls

        public string Identity
        {
            set { identity = value; }
        }

        public async Task<List<string>> LoadDurableSubscriptionsAsync(string identity)
        {
            List<string> list = new List<string>();

            IEnumerable<string> subscriptionUriStrings = await GraphManager.GetSubscriberSubscriptionsListAsync(identity);

            if (subscriptionUriStrings == null || subscriptionUriStrings.Count() == 0)
            {
                return null;
            }

            foreach (var item in subscriptionUriStrings)
            {
                if (!durableObservers.ContainsKey(item))
                {
                    MessageObserver observer = new MessageObserver();
                    observer.OnNotify += Observer_OnNotify;

                    //set the observer in the subscription with the lease lifetime
                    TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);

                    string leaseKey = await GraphManager.AddSubscriptionObserverAsync(item, leaseTime, observer);

                    //add the lease key to the list of ephemeral observers
                    durableObservers.Add(item, observer);


                    //get the resource from the subscription
                    Uri uri = new Uri(item);
                    string resourceUriString = item.Replace(uri.Segments[uri.Segments.Length - 1], "");

                    list.Add(resourceUriString); //add to list to return

                    //add the resource, subscription, and lease key the container

                    if (!container.ContainsKey(resourceUriString))
                    {
                        container.Add(resourceUriString, new Tuple<string, string>(item, leaseKey));
                    }
                }
            }

            if (subscriptionUriStrings.Count() > 0)
            {
                EnsureLeaseTimer();
            }

            return list.Count == 0 ? null : list;
        }

        public async Task<bool> CanPublishAsync(ResourceMetadata metadata, bool channelEncrypted)
        {
            if (metadata == null)
            {
                Trace.TraceWarning("{0} - Cannot publish to Orleans resource with null metadata.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
                return false;
            }

            if (!metadata.Enabled)
            {
                Trace.TraceWarning("{0} - Publish resource '{1}' is disabled.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.ResourceUriString);
                return false;
            }

            if (metadata.Expires.HasValue && metadata.Expires.Value < DateTime.UtcNow)
            {
                Trace.TraceWarning("{0} - Publish resource '{1}' has expired.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.ResourceUriString);
                return false;
            }

            if (metadata.RequireEncryptedChannel && !channelEncrypted)
            {
                Trace.TraceWarning("{0} - Publish resource '{1}' requires an encrypted channel.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.ResourceUriString);
                return false;
            }

            AuthorizationPolicy policy = await GraphManager.GetAccessControlPolicyAsync(metadata.PublishPolicyUriString);

            if (policy == null)
            {
                Trace.TraceWarning("{0} - Publish policy URI {1} did not return an authorization policy.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.PublishPolicyUriString);
                return false;
            }

            ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            bool authz = policy.Evaluate(identity);

            if (!authz)
            {
                Trace.TraceWarning("{0} - Identity '{1}' is not authorized to publish to resource '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), this.identity, metadata.ResourceUriString);
            }

            return authz;
        }

        public async Task<bool> CanSubscribeAsync(string resourceUriString, bool channelEncrypted)
        {
            ResourceMetadata metadata = await GraphManager.GetResourceMetadataAsync(resourceUriString);

            if (metadata == null)
            {
                Trace.TraceWarning("{0} - Cannot subscribe to Orleans resource will null metadata.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
                return false;
            }

            if (!metadata.Enabled)
            {
                Trace.TraceWarning("{0} - Subscribe resource '{1}' is disabled.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.ResourceUriString);
                return false;
            }

            if (metadata.Expires.HasValue && metadata.Expires.Value < DateTime.UtcNow)
            {
                Trace.TraceWarning("{0} - Subscribe resource '{1}' has expired.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.ResourceUriString);
                return false;
            }

            if (metadata.RequireEncryptedChannel && !channelEncrypted)
            {
                Trace.TraceWarning("{0} - Subscribe resource '{1}' requires an encrypted channel.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.ResourceUriString);
                return false;
            }

            AuthorizationPolicy policy = await GraphManager.GetAccessControlPolicyAsync(metadata.SubscribePolicyUriString);

            if (policy == null)
            {
                Trace.TraceWarning("{0} - Subscribe policy URI did not return an authorization policy", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), metadata.SubscribePolicyUriString);
                return false;
            }

            ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;

            bool authz = policy.Evaluate(identity);

            if (!authz)
            {
                Trace.TraceWarning("{0} - Identity '{1}' is not authorized to subscribe/unsubcribe to resource '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), this.identity, metadata.ResourceUriString);                
            }

            return authz;
        }

        public async Task PublishAsync(EventMessage message, List<KeyValuePair<string, string>> indexes = null)
        {
            AuditRecord record = null;
            DateTime receiveTime = DateTime.UtcNow;

            try
            {
                record = new AuditRecord(message.MessageId, identity, channelType, protocolType.ToUpperInvariant(), message.Message.Length, MessageDirectionType.In, true, receiveTime);

                if (indexes == null || indexes.Count == 0)
                {
                    await GraphManager.PublishAsync(message.ResourceUri, message);
                }
                else
                {
                    await GraphManager.PublishAsync(message.ResourceUri, message, indexes);
                }
            }
            catch (Exception ex)
            {
                record = new AuditRecord(message.MessageId, identity, channelType, protocolType.ToUpperInvariant(), message.Message.Length, MessageDirectionType.In, false, receiveTime, ex.Message);
            }
            finally
            {
                if (auditor.CanAudit && message.Audit)
                {
                    await auditor.WriteAuditRecordAsync(record);
                }
            }
        }

        public async Task<string> SubscribeAsync(string resourceUriString, SubscriptionMetadata metadata)
        {
            metadata.IsEphemeral = true;
            string subscriptionUriString = await GraphManager.SubscribeAsync(resourceUriString, metadata);

            //create and observer and wire up event to receive notifications
            MessageObserver observer = new MessageObserver();
            observer.OnNotify += Observer_OnNotify;

            //set the observer in the subscription with the lease lifetime
            TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);

            string leaseKey = await GraphManager.AddSubscriptionObserverAsync(subscriptionUriString, leaseTime, observer);

            //add the lease key to the list of ephemeral observers
            ephemeralObservers.Add(subscriptionUriString, observer);

            //add the resource, subscription, and lease key the container
            if (!container.ContainsKey(resourceUriString))
            {
                container.Add(resourceUriString, new Tuple<string, string>(subscriptionUriString, leaseKey));
            }

            //ensure the lease timer is running
            EnsureLeaseTimer();

            return subscriptionUriString;
        }

        public async Task UnsubscribeAsync(string resourceUriString)
        {
            //unsubscribe from resource
            if (container.ContainsKey(resourceUriString))
            {
                if (ephemeralObservers.ContainsKey(container[resourceUriString].Item1))
                {
                    await GraphManager.RemoveSubscriptionObserverAsync(container[resourceUriString].Item1, container[resourceUriString].Item2);
                    await GraphManager.UnsubscribeAsync(container[resourceUriString].Item1);
                    ephemeralObservers.Remove(container[resourceUriString].Item1);
                }

                container.Remove(resourceUriString);
            }
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Trace.TraceInformation("Orleans Adapter disposing on Protocol '{0}' with Channel Type '{1}' for identity '{2}'", protocolType, channelType, identity);
                if (disposing)
                {
                    if (leaseTimer != null)
                    {
                        leaseTimer.Stop();
                        leaseTimer.Dispose();
                    }

                    Task t0 = RemoveDurableObserversAsync();
                    Task.WaitAll(t0);

                    Task t1 = RemoveEphemeralObserversAsync();
                    Task.WaitAll(t1);
                }

                disposedValue = true;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion




        #region private methods
        private void Observer_OnNotify(object sender, MessageNotificationArgs e)
        {
            //observeCount++;
            //Trace.TraceInformation("Obsever {0}", observeCount);
            //signal the protocol adapter
            OnObserve?.Invoke(this, new ObserveMessageEventArgs(e.Message));
        }

        private void EnsureLeaseTimer()
        {
            if (leaseTimer == null)
            {
                leaseTimer = new System.Timers.Timer(30000);
                leaseTimer.Elapsed += LeaseTimer_Elapsed;
                leaseTimer.Start();
            }
        }

        private void LeaseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            KeyValuePair<string, Tuple<string, string>>[] kvps = container.ToArray();

            if (kvps == null || kvps.Length == 0)
            {
                leaseTimer.Stop();
                return;
            }

            Task leaseTask = Task.Factory.StartNew(async () =>
            {
                if (kvps != null && kvps.Length > 0)
                {
                    foreach (var kvp in kvps)
                    {
                        await GraphManager.RenewObserverLeaseAsync(kvp.Value.Item1, kvp.Value.Item2, TimeSpan.FromSeconds(60.0));
                    }
                }
            });

            leaseTask.LogExceptions();

        }

        private async Task RemoveDurableObserversAsync()
        {
            List<string> list = new List<string>();

            int cnt = durableObservers.Count;
            if (durableObservers.Count > 0)
            {
                List<Task> taskList = new List<Task>();
                KeyValuePair<string, IMessageObserver>[] kvps = durableObservers.ToArray();
                foreach (var item in kvps)
                {
                    IEnumerable<KeyValuePair<string, Tuple<string, string>>> items = container.Where((c) => c.Value.Item1 == item.Key);
                    foreach (var lease in items)
                    {
                        list.Add(lease.Value.Item1);

                        if (durableObservers.ContainsKey(lease.Value.Item1))
                        {
                            Task task = GraphManager.RemoveSubscriptionObserverAsync(lease.Value.Item1, lease.Value.Item2);
                            taskList.Add(task);
                        }
                    }
                }

                if (taskList.Count > 0)
                {
                    await Task.WhenAll(taskList);
                }

                durableObservers.Clear();
                RemoveFromContainer(list);
                Trace.TraceInformation("'{0}' - Durable observers removed by Orleans Adapter for identity '{1}'", cnt, identity);
            }
            else
            {
                Trace.TraceInformation("No Durable observers found by Orleans Adapter to be removed for identity '{0}'", identity);
            }
        }

        private async Task RemoveEphemeralObserversAsync()
        {
            List<string> list = new List<string>();
            int cnt = ephemeralObservers.Count;

            if (ephemeralObservers.Count > 0)
            {
                KeyValuePair<string, IMessageObserver>[] kvps = ephemeralObservers.ToArray();
                List<Task> unobserveTaskList = new List<Task>();
                foreach (var item in kvps)
                {
                    IEnumerable<KeyValuePair<string, Tuple<string, string>>> items = container.Where((c) => c.Value.Item1 == item.Key);

                    foreach (var lease in items)
                    {
                        list.Add(lease.Value.Item1);
                        if (ephemeralObservers.ContainsKey(lease.Value.Item1))
                        {
                            Task unobserveTask = GraphManager.RemoveSubscriptionObserverAsync(lease.Value.Item1, lease.Value.Item2);
                            unobserveTaskList.Add(unobserveTask);
                        }
                    }
                }

                if (unobserveTaskList.Count > 0)
                {
                    await Task.WhenAll(unobserveTaskList);
                }


                ephemeralObservers.Clear();
                RemoveFromContainer(list);
                Trace.TraceInformation("'{0}' - Ephemeral observers removed by Orleans Adapter for identity '{1}'", cnt, identity);
            }
            else
            {
                Trace.TraceInformation("No Ephemeral observers found by Orleans Adapter to be removed for identity '{0}'", identity);
            }

        }

        private void RemoveFromContainer(string subscriptionUriString)
        {
            List<string> list = new List<string>();
            var query = container.Where((c) => c.Value.Item1 == subscriptionUriString);

            foreach (var item in query)
            {
                list.Add(item.Key);
            }

            foreach (string item in list)
            {
                container.Remove(item);
            }
        }

        private void RemoveFromContainer(List<string> subscriptionUriStrings)
        {
            foreach (var item in subscriptionUriStrings)
            {
                RemoveFromContainer(item);
            }
        }




        #endregion
    }
}
