using System;
using System.Collections.Generic;
using System.Text;

namespace SkunkLab.Edge.Gateway.Modules
{ 
    public class TwinMessageEventArgs : EventArgs
    {
        public TwinMessageEventArgs(string luss, string serviceUrl)
        {
            Luss = luss;
            ServiceUrl = serviceUrl;
            Timestamp = DateTime.Now;
        }

        public string Luss { get; internal set; }

        public string ServiceUrl { get; internal set; }

        public DateTime Timestamp { get; internal set; }
    }
}
