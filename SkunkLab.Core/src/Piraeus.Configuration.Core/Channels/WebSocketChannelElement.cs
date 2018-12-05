using System.Configuration;

namespace Piraeus.Configuration.Channels
{
    public class WebSocketChannelElement : ConfigurationElement
    {

        [ConfigurationProperty("maxIncomingMessageSize", DefaultValue = 0x400000)]
        public int MaxIncomingMessageSize
        {
            get { return (int)base["maxIncomingMessageSize"]; }
            set { base["maxIncomingMessageSize"] = value; }
        }

        [ConfigurationProperty("receiveLoopBufferSize", DefaultValue = 0x2000)]
        public int ReceiveLoopBufferSize
        {
            get { return (int)base["receiveLoopBufferSize"]; }
            set { base["receiveLoopBufferSize"] = value; }
        }

        [ConfigurationProperty("sendBufferSize", DefaultValue = 0x2000)]
        public int SendBufferSize
        {
            get { return (int)base["sendBufferSize"]; }
            set { base["sendBufferSize"] = value; }
        }

        [ConfigurationProperty("closeTimeoutMilliseconds", DefaultValue =250.0)]
        public double CloseTimeoutMilliseconds
        {
            get { return (double)base["closeTimeoutMilliseconds"]; }
            set { base["closeTimeoutMilliseconds"] = value; }
        }

    }
}
