using System.Configuration;

namespace Piraeus.Configuration.Protocols
{
    public class ProtocolsElement : ConfigurationElement
    {
        [ConfigurationProperty("mqtt", IsRequired =false)]
        public MqttProtocolElement Mqtt
        {
            get { return (MqttProtocolElement)base["mqtt"]; }
            set { base["mqtt"] = value; }
        }

        [ConfigurationProperty("coap", IsRequired =false)]
        public CoapProtocolElement Coap
        {
            get { return (CoapProtocolElement)base["coap"]; }
            set { base["coap"] = value; }
        }


    }
}
