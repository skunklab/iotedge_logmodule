using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkunkLab.VirtualRtu.Adapters
{
    internal class MbapCache
    {
        public MbapCache()
        {
            container = new HashSet<string>();
        }

        public HashSet<string> container;

        public bool Set(ushort unitId, ushort transactionId)
        {
            string key = GetKey(unitId, transactionId);
            if (!container.Contains(key))
            {
                container.Add(key);
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool Remove(ushort unitId, ushort transactionId)
        {
            string key = GetKey(unitId, transactionId);
            return container.Remove(key);
        }

        public bool IsCached(ushort unitId, ushort transactionId)
        {
            string key = GetKey(unitId, transactionId);
            return container.Contains(key);
        }

        private string GetKey(ushort unitId, ushort transactionId)
        {
            return String.Format("{0}-{1}", unitId, transactionId);
        }
    }
      
}
