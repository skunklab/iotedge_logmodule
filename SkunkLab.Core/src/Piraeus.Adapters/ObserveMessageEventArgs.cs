using Piraeus.Core.Messaging;
using System;

namespace Piraeus.Adapters
{
    public class ObserveMessageEventArgs : EventArgs
    {
        public ObserveMessageEventArgs(EventMessage message)
        {
            Message = message;
        }

        public EventMessage Message { get; internal set; }
    }
}
