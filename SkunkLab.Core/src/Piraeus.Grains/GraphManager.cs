using System;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Metadata;
using Piraeus.GrainInterfaces;
using Piraeus.Core.Utilities;
using System.Collections.Generic;
using Piraeus.Core.Messaging;
using Capl.Authorization;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace Piraeus.Grains
{
    public class GraphManager
    {
        
        #region Static Resource Operations



        /// <summary>
        /// Gets a resource from Orleans 
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <returns>Resource interface for grain.</returns>
        public static IResource GetResource(string resourceUriString)
        {
            Uri uri = new Uri(resourceUriString);
            string uriString = uri.ToCanonicalString(false);
            return GrainClient.GrainFactory.GetGrain<IResource>(uriString);
        }

        /// <summary>
        /// Adds or updates a resource's metadata.
        /// </summary>
        /// <param name="metadata">Metadata that describes the resource.</param>
        /// <returns></returns>
        public static async Task UpsertResourceMetadataAsync(ResourceMetadata metadata)
        {
            Uri uri = new Uri(metadata.ResourceUriString);
            metadata.ResourceUriString = uri.ToCanonicalString(false);
            IResource resource = GetResource(metadata.ResourceUriString);
            await resource.UpsertMetadataAsync(metadata);
        }

        /// <summary>
        /// Get a resource's metadata.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <returns>Resource's metadata.</returns>
        public static async Task<ResourceMetadata> GetResourceMetadataAsync(string resourceUriString)
        {
            IResource resource = GetResource(resourceUriString);
            return await resource.GetMetadataAsync();
        }

        


        public static async Task<CommunicationMetrics> GetResourceMetricsAsync(string resourceUriString)
        {
            IResource resource = GetResource(resourceUriString);
            return await resource.GetMetricsAsync();
        }
        /// <summary>
        /// Subscribes to a resource by creating a subscription and adding it to the resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="metadata">Metadata that describes the subscription.</param>
        /// <returns>Unique URI for the subscription.</returns>
        /// <remarks>The function creates a subscription, adds the subscription to the resource, then return the URI that identifies the subscription.</remarks>
        public static async Task<string> SubscribeAsync(string resourceUriString, SubscriptionMetadata metadata)
        {
            Uri uri = new Uri(resourceUriString);
            string subscriptionUriString = uri.ToCanonicalString(true) + Guid.NewGuid().ToString();
            metadata.SubscriptionUriString = subscriptionUriString;


            //Add the metadata to the subscription
            ISubscription subscription = GetSubscription(subscriptionUriString);
            await subscription.UpsertMetadataAsync(metadata);

            //Add the subscription to the resource
            IResource resource = GetResource(uri.ToCanonicalString(false));
            await resource.SubscribeAsync(subscription);            

            return subscriptionUriString;
        }

        /// <summary>
        /// Unsubscribes a subscription from a resource.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI for the subscription.</param>
        /// <returns></returns>
        public static async Task UnsubscribeAsync(string subscriptionUriString)
        {
            //get the resource to unsubscribe
            Uri uri = new Uri(subscriptionUriString);
            string resourceUriString = uri.ToCanonicalString(false, true);
            IResource resource = GetResource(resourceUriString);

            //unsubscribe from the resource
            await resource.UnsubscribeAsync(subscriptionUriString);
        }

        public static async Task UnsubscribeAsync(string subscriptionUriString, string identity)
        {
            Uri uri = new Uri(subscriptionUriString);
            string resourceUriString = uri.ToCanonicalString(false, true);
            IResource resource = GetResource(resourceUriString);

            //unsubscribe from the resource
            await resource.UnsubscribeAsync(subscriptionUriString, identity);
        }

        /// <summary>
        /// Publishes a message to a resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="message">Message to publish.</param>
        /// <returns></returns>
        public static async Task PublishAsync(string resourceUriString, EventMessage message)
        {
            IResource resource = GetResource(resourceUriString);
            await resource.PublishAsync(message);
        }

        /// <summary>
        /// Publishes a message to a resources with indexes.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="message">Message to publishes.</param>
        /// <param name="indexes">Indexes used to filter subscriptions to subset.</param>
        /// <returns></returns>
        public static async Task PublishAsync(string resourceUriString, EventMessage message, List<KeyValuePair<string, string>> indexes)
        {
            IResource resource = GetResource(resourceUriString);
            await resource.PublishAsync(message, indexes);
        }

        /// <summary>
        /// Gets a list of subscription URIs subscribed to the resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <returns>Array of subscription URIs subscribed to the resource.</returns>
        public static async Task<IEnumerable<string>> GetResourceSubscriptionListAsync(string resourceUriString)
        {
            IResource resource = GetResource(resourceUriString);
            return await resource.GetSubscriptionListAsync();
        }

        /// <summary>
        /// Adds a metric observer to a resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="lifetime">The lifetime of the lease.</param>
        /// <param name="observer">Metric observer to receive events.</param>
        /// <returns>A unique string for the lease key, which is used to refresh the lease for the observer.</returns>
        public static async Task<string> AddResourceObserverAsync(string resourceUriString, TimeSpan lifetime, MetricObserver observer)
        {
            IMetricObserver objRef = await GrainClient.GrainFactory.CreateObjectReference<IMetricObserver>(observer);
            IResource resource = GetResource(resourceUriString);
            return await resource.AddObserverAsync(lifetime, objRef);
        }

        /// <summary>
        /// Add an error observer to a resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="lifetime">The lifetime of the lease.</param>
        /// <param name="observer">Error observer to receive events.</param>
        /// <returns>A unique string for the lease key, whic is used to refresh the lease for the observer.</returns>
        public static async Task<string> AddResourceObserverAsync(string resourceUriString, TimeSpan lifetime, ErrorObserver observer)
        {
            IErrorObserver objRef = await GrainClient.GrainFactory.CreateObjectReference<IErrorObserver>(observer);
            IResource resource = GetResource(resourceUriString);
            return await resource.AddObserverAsync(lifetime, objRef);
        }

        /// <summary>
        /// Renews the lease for a resource's observer.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="leaseKey">Unique string of the observer's lease.</param>
        /// <param name="lifetime">The lifetime of the renewed lease.</param>
        /// <returns></returns>
        public static async Task<bool> RenewResourceObserverLeaseAsync(string resourceUriString, string leaseKey, TimeSpan lifetime)
        {
            IResource resource = GetResource(resourceUriString);
            return await resource.RenewObserverLeaseAsync(leaseKey, lifetime);
        }

        /// <summary>
        /// Removes an observer from a resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <param name="leaseKey">Unqiue string of the observer's lease.</param>
        /// <returns></returns>
        public static async Task RemoveResourceObserverAsync(string resourceUriString, string leaseKey)
        {
            IResource resource = GetResource(resourceUriString);
            await resource.RemoveObserverAsync(leaseKey);
        }

        /// <summary>
        /// Clears the resource; equivalent to deleting the resource.
        /// </summary>
        /// <param name="resourceUriString">Unique URI that identifies the resource.</param>
        /// <returns></returns>
        public static async Task ClearResourceAsync(string resourceUriString)
        {
            IResource resource = GetResource(resourceUriString);
            await resource.ClearAsync();
        }

        #endregion

        #region Static Subscription Operations

        /// <summary>
        /// Get a subscription from Orleans.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <returns>Subscription interface for grain.</returns>
        public static ISubscription GetSubscription(string subscriptionUriString)
        {
            Uri uri = new Uri(subscriptionUriString);
            return GrainClient.GrainFactory.GetGrain<ISubscription>(uri.ToCanonicalString(false));
        }

        /// <summary>
        /// Adds or updates a subscription's metadata.
        /// </summary>
        /// <param name="metadata">Metadata that describes the subscription.</param>
        /// <returns></returns>
        public static async Task UpsertSubscriptionMetadataAsync(SubscriptionMetadata metadata)
        {
            ISubscription subscription = GetSubscription(metadata.SubscriptionUriString);
            await subscription.UpsertMetadataAsync(metadata);
        }

        /// <summary>
        /// Gets a subscription's metadata.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <returns>Subscription metadata.</returns>
        public static async Task<SubscriptionMetadata> GetSubscriptionMetadataAsync(string subscriptionUriString)
        {
            Uri uri = new Uri(subscriptionUriString);
            ISubscription subscription = GetSubscription(uri.ToCanonicalString(false));
            return await subscription.GetMetadataAsync();
        }

        public static async Task<CommunicationMetrics> GetSubscriptionMetricsAsync(string subscriptionUriString)
        {
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.GetMetricsAsync();
        }

        /// <summary>
        /// Adds a message observer to the subscription.  Used to observe messages received by the subscription.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <param name="lifetime">Lifetime of the lease.</param>
        /// <param name="observer">Observer to receive events.</param>
        /// <returns>A unique string for the lease key, which is used to renew or delete the observer's lease.</returns>
        public static async Task<string> AddSubscriptionObserverAsync(string subscriptionUriString, TimeSpan lifetime, MessageObserver observer)
        {
            IMessageObserver observerRef = await GrainClient.GrainFactory.CreateObjectReference<IMessageObserver>(observer);
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.AddObserverAsync(lifetime, observerRef);
        }

        /// <summary>
        /// Adds a metric observer to the subscription.  Used to observe metrics in the subscription.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <param name="lifetime">Lifetime of the lease.</param>
        /// <param name="observer">Observer to receive events.</param>
        /// <returns>A unqiue string for the lease key, which is used to renew or delete the observer's lease.</returns>
        public static async Task<string> AddSubscriptionObserverAsync(string subscriptionUriString, TimeSpan lifetime, MetricObserver observer)
        {
            IMetricObserver observerRef = await GrainClient.GrainFactory.CreateObjectReference<IMetricObserver>(observer);
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.AddObserverAsync(lifetime, observerRef);
        }

        /// <summary>
        /// Adds an error observer to the subscription. Used to observe errors in the subscription.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <param name="lifetime">Lifetime of the lease.</param>
        /// <param name="observer">Observer to receive events.</param>
        /// <returns>A unique string for the lease key, which is used to renew or delete the observer's lease.</returns>
        public static async Task<string> AddSubscriptionObserverAsync(string subscriptionUriString, TimeSpan lifetime, ErrorObserver observer)
        {
            IErrorObserver observerRef = await GrainClient.GrainFactory.CreateObjectReference<IErrorObserver>(observer);
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.AddObserverAsync(lifetime, observerRef);
        }

        /// <summary>
        /// Removes an observer from a subscription.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <param name="leaseKey">Unqiue string of the lease to remove.</param>
        /// <returns></returns>
        public static async Task RemoveSubscriptionObserverAsync(string subscriptionUriString, string leaseKey)
        {
            ISubscription subscription = GetSubscription(subscriptionUriString);
            await subscription.RemoveObserverAsync(leaseKey);
        }

        /// <summary>
        /// Renews an observers lease for a subscription.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <param name="leaseKey">Unique string of the lease to renew.</param>
        /// <param name="lifetime">Lifetime of the renewed lease.</param>
        /// <returns>True if the lease is renewed; otherwise False.</returns>
        public static async Task<bool> RenewObserverLeaseAsync(string subscriptionUriString, string leaseKey, TimeSpan lifetime)
        {
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.RenewObserverLeaseAsync(leaseKey, lifetime);
        }

        /// <summary>
        /// Deletes a subscription.
        /// </summary>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <returns></returns>
        public static async Task SubscriptionClearAsync(string subscriptionUriString)
        {
            ISubscription subscription = GetSubscription(subscriptionUriString);
            await subscription.ClearAsync();
        }

        #endregion

        #region Static Subscriber Operations
        /// <summary>
        /// Gets subscriber from Orleans.
        /// </summary>
        /// <param name="identity">The identity of the subscriber.</param>
        /// <returns>Subscriber interface for grain.</returns>
        public static ISubscriber GetSubscriber(string identity)
        {            
            if(string.IsNullOrEmpty(identity))
            {
                return null;
            }

            return GrainClient.GrainFactory.GetGrain<ISubscriber>(identity.ToLowerInvariant());
        }

        /// <summary>
        /// Adds a subscription reference to the subscriber.
        /// </summary>
        /// <param name="identity">Identity of the subscriber.</param>
        /// <param name="subscriptionUriString">Unique URI that identifies the subscription.</param>
        /// <returns></returns>
        public static async Task AddSubscriberSubscriptionAsync(string identity, string subscriptionUriString)
        {
            ISubscriber subscriber = GetSubscriber(identity);

            if (subscriber != null)
            {
                await subscriber.AddSubscriptionAsync(subscriptionUriString);
            }
        }

        /// <summary>
        /// Removes a subscription reference from a subscriber.
        /// </summary>
        /// <param name="identity">Identity of the subscriber.</param>
        /// <param name="subscriptionUriString">Unique URI that identifies the subsription.</param>
        /// <returns></returns>
        public static async Task RemoveSubscriberSubscriptionAsync(string identity, string subscriptionUriString)
        {
            ISubscriber subscriber = GetSubscriber(identity);

            if (subscriber != null)
            {
                await subscriber.RemoveSubscriptionAsync(subscriptionUriString);
            }
        }

        /// <summary>
        /// Returns a list of subscription URIs for the subscriber.
        /// </summary>
        /// <param name="identity">Identity of the subscriber.</param>
        /// <returns>Array of subscription URIs for the subscriber.</returns>
        public static async Task<IEnumerable<string>> GetSubscriberSubscriptionsListAsync(string identity)
        {
            ISubscriber subscriber = GetSubscriber(identity);

            if (subscriber != null)
            {
                return await subscriber.GetSubscriptionsAsync();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes a subscriber.
        /// </summary>
        /// <param name="identity">Identity of the subscriber.</param>
        /// <returns></returns>
        public static async Task ClearSubscriberSubscriptionsAsync(string identity)
        {
            ISubscriber subscriber = GetSubscriber(identity);

            if (subscriber != null)
            {
                await subscriber.ClearAsync();
            }
        }

        #endregion

        #region Static ResourceList
        
        /// <summary>
        /// Returns of list of resources in Orleans.
        /// </summary>
        /// <returns>Array of resource URIs.</returns>
        public static async Task<IEnumerable<string>> GetResourceListAsync()
        {
            IResourceList resourceList = GrainClient.GrainFactory.GetGrain<IResourceList>("resourcelist");
            return await resourceList.GetListAsync();
        }

        #endregion

        #region Static Access Control

        /// <summary>
        /// Gets access control grain from Orleans.
        /// </summary>
        /// <param name="policyUriString">Unique URI that identifies the access control policy.</param>
        /// <returns>AccessControl Interface from grain.</returns>
        public static IAccessControl GetAccessControlPolicy(string policyUriString)
        {
            Uri uri = new Uri(policyUriString);
            string uriString = uri.ToCanonicalString(false);
            return GrainClient.GrainFactory.GetGrain<IAccessControl>(uriString);
        }

        /// <summary>
        /// Adds or updates an CAPL access control policy.
        /// </summary>
        /// <param name="policyUriString">Unique URI that identifies the policy.</param>
        /// <param name="policy">CAPL access control policy.</param>
        /// <returns></returns>
        public static async Task UpsertAcessControlPolicyAsync(string policyUriString, AuthorizationPolicy policy)
        {
            IAccessControl accessControl = GetAccessControlPolicy(policyUriString);
            await accessControl.UpsertPolicyAsync(policy);
        }

        /// <summary>
        /// Deletes an access control policy.
        /// </summary>
        /// <param name="policyUriString">Unique URI that identifies the policy.</param>
        /// <returns></returns>
        public static async Task ClearAccessControlPolicyAsync(string policyUriString)
        {
            IAccessControl accessControl = GetAccessControlPolicy(policyUriString);
            await accessControl.ClearAsync();
        }

        /// <summary>
        /// Gets an access control policy.
        /// </summary>
        /// <param name="policyUriString">Unqiue URI that identifies the policy.</param>
        /// <returns>CAPL access control policy.</returns>
        public static async Task<AuthorizationPolicy> GetAccessControlPolicyAsync(string policyUriString)
        {
            IAccessControl accessControl = GetAccessControlPolicy(policyUriString);
            return await accessControl.GetPolicyAsync();
        }

        #endregion

        #region Static Service Identity

        public static IServiceIdentity GetServiceIdentity()
        {
            return GrainClient.GrainFactory.GetGrain<IServiceIdentity>("serviceidentity");
        }

        public static async Task SetServiceIdentityAsync(List<Claim> claims = null, X509Certificate2 certificate = null)
        {
            IServiceIdentity identity = GrainClient.GrainFactory.GetGrain<IServiceIdentity>("serviceidentity");

            if(claims != null)
            {
                List<KeyValuePair<string, string>> claimList = new List<KeyValuePair<string, string>>();
                foreach(var item in claims)
                {
                    claimList.Add(new KeyValuePair<string, string>(item.Type, item.Value));
                }

                await identity.AddClaimsAsync(claimList);
            }

            if (certificate != null)
            {
                await identity.AddCertificateAsync(certificate.RawData);
            }
        }

        public static async Task<List<Claim>> GetServiceIdentityClaimsAsync()
        {
            IServiceIdentity identity = GrainClient.GrainFactory.GetGrain<IServiceIdentity>("serviceidentity");
            List<KeyValuePair<string,string>> list = await identity.GetClaimsAsync();

            if (list == null)
            {
                return null;
            }

            List<Claim> claims = new List<Claim>();
            foreach(var item in list)
            {
                claims.Add(new Claim(item.Key, item.Value));
            }

            return claims;
        }

        public static async Task<X509Certificate2> GetServiceIdentityCertificateAsync()
        {
            IServiceIdentity identity = GrainClient.GrainFactory.GetGrain<IServiceIdentity>("serviceidentity");
            byte[] certBytes = await identity.GetCertificateAsync();

            if(certBytes != null)
            {
                return new X509Certificate2(certBytes);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Static Audit

        public static async Task SetAuditConfigAsync(string connectionstring, string tablename)
        {
            IAuditConfig config = GrainClient.GrainFactory.GetGrain<IAuditConfig>("auditconfig");
            await config.SetAzureStorageConnectionAsync(connectionstring, tablename);

        }

        public static async Task<string> GetAuditConfigConnectionstringAsync()
        {
            IAuditConfig config = GrainClient.GrainFactory.GetGrain<IAuditConfig>("auditconfig");
            return await config.GetConnectionstringAsync();
        }

        public static async Task<string> GetAuditConfigTablenameAsync()
        {
            IAuditConfig config = GrainClient.GrainFactory.GetGrain<IAuditConfig>("auditconfig");
            return await config.GetTableNameAsync();
        }
        #endregion


    }
}
