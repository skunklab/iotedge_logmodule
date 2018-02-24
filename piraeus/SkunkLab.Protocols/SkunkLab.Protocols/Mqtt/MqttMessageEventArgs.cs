using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt
{
    public class MqttMessageEventArgs : EventArgs
    {
        public MqttMessageEventArgs(MqttMessage message)
        {
            Message = message;
        }

        public MqttMessage Message { get; internal set; }
    }
}
