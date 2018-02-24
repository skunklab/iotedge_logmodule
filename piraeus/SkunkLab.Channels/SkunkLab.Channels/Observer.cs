using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Channels
{
    public delegate void ObserverEventHandler(object sender, ObserverEventArgs args);
    public abstract class Observer
    {
        public abstract Uri ResourceUri { get; internal set; }
        public abstract event ObserverEventHandler OnNotify;
        public abstract void Update(Uri resourceUri, string contentType, byte[] messagee);
    }
}
