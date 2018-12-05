using Orleans;
using Piraeus.Core.Messaging;

namespace Piraeus.GrainInterfaces
{
    public interface IMessageObserver : IGrainObserver
    {
        /// <summary>
        /// Notifies the observers of the subscription.
        /// </summary>
        /// <param name="message">Notification message.</param>
        void Notify(EventMessage message);

    }
}
