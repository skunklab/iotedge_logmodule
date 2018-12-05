

namespace Orleans.Storage.Redis
{
    using Orleans.Runtime.Configuration;
    using System;
    using System.Collections.Generic;

    public static class ClusterConfigrationExtensions
    {
        public static void AddRedisStorageProvider(this ClusterConfiguration config,
           string providerName = "RedisStore",
           string connectionString = null)
        {
            if (string.IsNullOrEmpty(providerName)) throw new ArgumentNullException(nameof(providerName));
            connectionString = GetConnectionString(connectionString, config);

            var properties = new Dictionary<string, string>
            {
                { "DataConnectionString", connectionString }
            };

            config.Globals.RegisterStorageProvider<RedisStorageProvider>(providerName, properties);
        }

        private static string GetConnectionString(string connectionString, ClusterConfiguration config)
        {
            if (!string.IsNullOrWhiteSpace(connectionString)) return connectionString;
            if (!string.IsNullOrWhiteSpace(config.Globals.DataConnectionString)) return config.Globals.DataConnectionString;

            throw new ArgumentNullException(nameof(connectionString),
                "Parameter value and fallback value are both null or empty.");
        }

        
    }
}
