

namespace Orleans.Storage.Redis
{
    using System;
    using System.Runtime.Serialization;

    public class AssemblyBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
        }
    }
}
