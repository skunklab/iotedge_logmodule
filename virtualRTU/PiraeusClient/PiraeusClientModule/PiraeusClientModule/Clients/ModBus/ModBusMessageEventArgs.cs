using System;
using System.Collections.Generic;
using System.Text;

namespace PiraeusClientModule.Clients.ModBus
{
    public class ModBusMessageEventArgs : EventArgs
    {
        public ModBusMessageEventArgs(byte[] payload)
        {
            Payload = payload;
        }

        public byte[] Payload { get; internal set; }
    }
}
