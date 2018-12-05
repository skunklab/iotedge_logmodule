using Piraeus.Core.Metadata;
using System;
namespace Piraeus.Grains.Notifications
{
    public class EventSinkFactory
    {
        public static EventSink Create(SubscriptionMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            if (String.IsNullOrEmpty(metadata.NotifyAddress))
            {
                throw new NullReferenceException("Subscription metadata has no NotifyAddress for passive event sink.");
            }

            Uri uri = new Uri(metadata.NotifyAddress);

            //if (uri.Scheme == "http" || uri.Scheme == "https")
            //{
            //    if (uri.Authority.Contains("blob.core.windows.net"))
            //    {
            //        return new AzureBlobStorageSink(metadata);
            //    }
            //    else if (uri.Authority.Contains("queue.core.windows.net"))
            //    {
            //        return new AzureQueueStorageSink(metadata);
            //    }
            //    else if (uri.Authority.Contains("documents.azure.com"))
            //    {
            //        return new CosmosDBSink(metadata);
            //    }
            //    else
            //    {
            //        return new RestWebServiceSink(metadata);
            //    }
            //}
            //else if (uri.Scheme == "iothub")
            //{
            //    return new IoTHubSink(metadata);
            //}
            //else if (uri.Scheme == "eh")
            //{
            //    return new EventHubSink(metadata);
            //}
            //else if (uri.Scheme == "sb")
            //{
            //    return new ServiceBusTopicSink(metadata);
            //}
            //else if (uri.Scheme == "adl")
            //{
            //    return new DataLakeSink(metadata);
            //}
            //else if (uri.Scheme == "eventgrid")
            //{
            //    return new EventGridSink(metadata);
            //}
            //else if(uri.Scheme == "redis")
            //{
            //    return new RedisSink(metadata);
            //}
            //else
            //{
            //    throw new InvalidOperationException(String.Format("EventSinkFactory cannot find concrete type for {0}", metadata.NotifyAddress));
            //}

            throw new Exception("ouch!");
        }
    }
}
