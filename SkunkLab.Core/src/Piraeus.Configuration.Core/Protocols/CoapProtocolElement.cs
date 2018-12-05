using System.Configuration;

namespace Piraeus.Configuration.Protocols
{
    public class CoapProtocolElement : MqttProtocolElement
    {
        [ConfigurationProperty("hostname")]
        public string HostName
        {
            get { return (string)base["hostname"]; }
            set { base["hostname"] = value; }
        }

        [ConfigurationProperty("autoRetry", DefaultValue =false)]
        public bool AutoRetry
        {
            get { return (bool)base["autoRetry"]; }
            set { base["autoRetry"] = value; }
        }

        [ConfigurationProperty("observeOption", DefaultValue =true)]
        public bool ObserveOption
        {
            get { return (bool)base["observeOption"]; }
            set { base["observeOption"] = value; }
        }

        [ConfigurationProperty("noresponseOption", DefaultValue =true)]
        public bool NoResponseOption
        {
            get { return (bool)base["noresponseOption"]; }
            set { base["noresponseOption"] = value; }
        }

        [ConfigurationProperty("nstart", DefaultValue =1)]
        public int NStart
        {
            get { return (int)base["nstart"]; }
            set { base["nstart"] = value; }
        }

        [ConfigurationProperty("defaultLeisure", DefaultValue =4.0)]
        public double DefaultLeisure
        {
            get { return (double)base["defaultLeisure"]; }
            set { base["defaultLeisure"] = value; }
        }

        [ConfigurationProperty("probingRate", DefaultValue =1.0)]
        public double ProbingRate
        {
            get { return (double)base["probingRate"]; }
            set { base["probingRate"] = value; }
        }
    }
}
