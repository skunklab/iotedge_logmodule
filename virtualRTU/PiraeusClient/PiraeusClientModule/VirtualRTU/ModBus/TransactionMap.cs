using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace VirtualRTU.ModBus
{
    public class TransactionMap
    {

        public TransactionMap()
        {
            MemoryCacheOptions options = new MemoryCacheOptions();
            options.ExpirationScanFrequency = TimeSpan.FromSeconds(120);
            options.SizeLimit = 1000;
            cache = new MemoryCache(options);
        }

      
        private MemoryCache cache;

        public void Add(ushort transactionId, ushort unitId)
        {
            string key = transactionId.ToString() + "-" + unitId.ToString();
            Tuple<ushort, ushort> tuple = new Tuple<ushort, ushort>(transactionId, unitId);
           
            if(cache.Get< Tuple<ushort, ushort>>(key) == null)
            {
                cache.Set<Tuple<ushort, ushort>>(key, tuple);
            }
        }

        public bool IsMatch(ushort transactionId, ushort unitId)
        {
            string key = transactionId.ToString() + "-" + unitId.ToString();
            return cache.Get<Tuple<ushort, ushort>>(key) != null;
        }

        public void Remove(ushort transactionId, ushort unitId)
        {
            string key = transactionId.ToString() + unitId.ToString();
            cache.Remove(key);
        }
    }
}
