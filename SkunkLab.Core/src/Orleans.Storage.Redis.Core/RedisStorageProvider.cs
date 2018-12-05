

namespace Orleans.Storage.Redis
{
    using Microsoft.Extensions.DependencyInjection;
    using Orleans;
    using Orleans.Providers;
    using Orleans.Runtime;
    using Orleans.Serialization;
    using Orleans.Storage;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RedisStorageProvider : IStorageProvider
    {
        public RedisStorageProvider()
        {
           
        }

        private string connectionString;
        private ConnectionMultiplexer connection;
        private IDatabase database;
        private SerializationManager serializationManager;

        //public Logger Log
        //{
        //    get;
        //    protected set;
        //}

        public string Name { get; protected set; }
        
        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Log = providerRuntime.GetLogger(this.GetType().FullName + "." + providerRuntime.ServiceId.ToString());
            this.serializationManager = providerRuntime.ServiceProvider.GetRequiredService<SerializationManager>();

            if (string.IsNullOrWhiteSpace(config.Properties["DataConnectionString"]))
            {
                throw new ArgumentException("Redis DataConnectionString property not set");
            }

            connectionString = config.Properties["DataConnectionString"];
            await ConnectAsync();

            Name = name;
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if (!connection.IsConnected)
            {
                Task task = ConnectAsync();
                Task.WaitAll(task);
            }

            string key = grainReference.ToKeyString();
            RedisValue value = await database.StringGetAsync(key);

            if (value.HasValue)

            {
                grainState.State = serializationManager.DeserializeFromByteArray<object>(value);
            }
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if (!connection.IsConnected)
            {
                Task task = ConnectAsync();
                Task.WaitAll(task);
            }

            var key = grainReference.ToKeyString();
            var data = grainState.State;

            byte[] payload = serializationManager.SerializeToByteArray(data);

            await database.StringSetAsync(key, payload);
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if (!connection.IsConnected)
            {
                Task task = ConnectAsync();
                Task.WaitAll(task);
            }

            string key = grainReference.ToKeyString();
            await database.KeyDeleteAsync(key);
        }

        public Task Close()
        {
            connection.Dispose();
            return Task.CompletedTask;
        }

        private async Task  ConnectAsync()
        {
            if(connection == null || !connection.IsConnected)
            {
                connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
                database = connection.GetDatabase();
            }
        }

        
    }
}
