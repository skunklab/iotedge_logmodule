using System;

namespace SkunkLab.Protocols
{
    public interface IMqttDispatch
    {
        void Register(string key, Action<string,string,byte[]> action);
        void Unregister(string key);
        void Dispatch(string key, string contentType, byte[] data);
    }
}
