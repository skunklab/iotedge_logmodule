using System;

namespace SkunkLab.Protocols.Utilities
{
    public interface IDispatch
    {
        void Register(string key, Action<string,string,byte[]> action);
        void Unregister(string key);
        void Dispatch(string key, string contentType, byte[] data);
    }
}
