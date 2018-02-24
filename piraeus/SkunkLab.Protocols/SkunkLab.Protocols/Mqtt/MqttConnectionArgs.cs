using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt
{
    public class MqttConnectionArgs : EventArgs
    {
        public MqttConnectionArgs(ConnectAckCode code)
        {
            Code = code;
        }

        public ConnectAckCode Code { get; internal set; }
    }
}
