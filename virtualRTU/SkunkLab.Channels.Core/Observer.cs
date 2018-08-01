using System;

namespace SkunkLab.Channels.Core
{
    public delegate void ObserverEventHandler(object sender, ObserverEventArgs args);
    public abstract class Observer
    {
        public abstract Uri ResourceUri { get; set; }
        public abstract event ObserverEventHandler OnNotify;
        public abstract void Update(Uri resourceUri, string contentType, byte[] message);
    }
}
