using System.Configuration;

namespace Piraeus.Configuration.Protocols
{
    public class MqttProtocolElement : ConfigurationElement
    {
        [ConfigurationProperty("keepAliveSeconds", DefaultValue =180.0)]
        public double KeepAliveSeconds
        {
            get { return (double)base["keepAliveSeconds"]; }
            set { base["keepAliveSeconds"] = value; }
        }

        [ConfigurationProperty("ackTimeoutSeconds", DefaultValue =2.0)]
        public double AckTimeoutSeconds
        {
            get { return (double)base["ackTimeoutSeconds"]; }
            set { base["ackTimeoutSeconds"] = value; }
        }

        [ConfigurationProperty("ackRandomFactor", DefaultValue =1.5)]
        public double AckRandomFactor
        {
            get { return (double)base["ackRandomFactor"]; }
            set { base["ackRandomFactor"] = value; }
        }

        [ConfigurationProperty("maxRetransmit", DefaultValue =4)]
        public int MaxRetransmit
        {
            get { return (int)base["maxRetransmit"]; }
            set { base["maxRetransmit"] = value; }
        }

        [ConfigurationProperty("maxLatencySeconds", DefaultValue =100.0)]
        public double MaxLatencySeconds
        {
            get { return (double)base["maxLatencySeconds"]; }
            set { base["maxLatencySeconds"] = value; }
        }
    }
}
