using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Protocols;

namespace Piraeus.Clients.Mqtt
{
    public class GenericMqttDispatcher : IMqttDispatch
    {
        public GenericMqttDispatcher()
        {
            register = new Dictionary<string, Action<string, string, byte[]>>();
        }

        private Dictionary<string, Action<string, string, byte[]>> register;

        public void Register(string key, Action<string, string, byte[]> action)
        {
            if(!register.ContainsKey(key))
            {
                register.Add(key, action);
            }
        }

        public void Unregister(string key)
        {
            register.Remove(key);
        }

        public void Dispatch(string key, string contentType, byte[] data)
        {
            if(register.ContainsKey(key))
            {
                register[key](key, contentType, data);
            }
        }
    }
}
