using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt
{
    public class LifetimeEventArgs : EventArgs
    {
        public LifetimeEventArgs(ushort[] ids)
        {
            Ids = ids;
        }

        public ushort[] Ids { get; internal set; }
    }
}
