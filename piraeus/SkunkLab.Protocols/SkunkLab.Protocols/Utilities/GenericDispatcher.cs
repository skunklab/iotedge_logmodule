using System;
using System.Collections.Generic;

namespace SkunkLab.Protocols.Utilities
{
    public class GenericDispatcher : IDispatch
    {
        public GenericDispatcher()
        {
            container = new Dictionary<string, Action<string, string, byte[]>>();
        }

        private Dictionary<string, Action<string, string, byte[]>> container;

        public void Dispatch(string key, string contentType, byte[] data)
        {
            Action<string, string, byte[]> action = container[key];
            action(key, contentType, data);
        }

        public void Register(string key, Action<string, string, byte[]> action)
        {
            container.Add(key, action);
        }

        public void Unregister(string key)
        {
            container.Remove(key);
        }
    }
}
