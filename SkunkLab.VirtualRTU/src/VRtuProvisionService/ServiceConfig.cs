using Newtonsoft.Json;
using System;

namespace VRtuProvisionService
{
    [JsonObject]
    public class ServiceConfig
    {
        public ServiceConfig()
        {

        }


        public static ServiceConfig LoadEnvironmentVariables()
        {
            ServiceConfig config = new ServiceConfig();
            config.PiraeusHostname = System.Environment.GetEnvironmentVariable("PiraeusHostname");
            config.Port = Convert.ToInt32(System.Environment.GetEnvironmentVariable("Port"));
            config.NameClaimType = System.Environment.GetEnvironmentVariable("NameClaimType");
            config.RoleClaimType = System.Environment.GetEnvironmentVariable("RoleClaimType");
            config.SymmetricKey = System.Environment.GetEnvironmentVariable("SymmetricKey");
            config.LifetimeMinutes = Convert.ToInt32(System.Environment.GetEnvironmentVariable("LifetimeMinutes"));
            config.Issuer = System.Environment.GetEnvironmentVariable("Issuer");
            config.Audience = System.Environment.GetEnvironmentVariable("Audience");
            config.PiraeusApiHostname = System.Environment.GetEnvironmentVariable("PiraeusApiHostname");
            config.PiraeusApiToken = System.Environment.GetEnvironmentVariable("PiraeusApiToken"); 
            config.RtuMapContainerName = System.Environment.GetEnvironmentVariable("RtuMapContainerName");
            config.RtuMapFilename = System.Environment.GetEnvironmentVariable("RtuMapFilename");
            config.LussTableName = System.Environment.GetEnvironmentVariable("LussTableName");
            config.PskIdentities = System.Environment.GetEnvironmentVariable("PskIdentities").Split(new char[] { ';' });
            config.Psks = System.Environment.GetEnvironmentVariable("Psks").Split(new char[] { ';' });

            return config;

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
