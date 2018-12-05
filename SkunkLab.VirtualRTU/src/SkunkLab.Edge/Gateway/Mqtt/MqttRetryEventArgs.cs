using VirtualRtu.Common.Configuration;

namespace SkunkLab.Edge.Gateway.Mqtt
{
    public class MqttRetryEventArgs
    {
        public MqttRetryEventArgs(IssuedConfig config, string errorMessage)
        {
            Config = config;
            Message = errorMessage;
        }

        public IssuedConfig Config { get; internal set; }

        public string Message { get; internal set; }
    }
}
