using Newtonsoft.Json;

namespace ProvisioningService
{
    [JsonObject]
    public class ServiceConfig
    {
        public ServiceConfig()
        {

        }

        public string PiraeusHostname { get; set; }

        public int Port { get; set; }
        public string NameClaimType { get; set; }

        public string RoleClaimType { get; set; }

        public string SymmetricKey { get; set; }

        public int LifetimeMinutes { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string PiraeusApiHostname { get; set; }

        public string PiraeusApiToken { get; set; }

        public string[] PskIdentities { get; set; }

        public string[] Psks { get; set; }

        public string RtuMapContainerName { get; set; }

        public string RtuMapFilename { get; set; }

        public string LussTableName { get; set; }
    }
}
