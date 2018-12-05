using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using VirtualRtu.Common.Configuration;

namespace FieldGatewayMicroservice.Communications
{
    public class ConfigFile
    {
        public ConfigFile()
        {
        }

        public bool HasDirectory
        {
            get
            {
                return Directory.Exists(Constants.CONFIG_DIRECTORY);
            }
        }

        public bool HasFile
        {
            get
            {
                return File.Exists(Constants.CONFIG_PATH);
            }
        }


        public IssuedConfig ReadConfig()
        {
            if (!HasDirectory || !HasFile)
            {
                throw new InvalidOperationException("Either config directory or file does not exist.");
            }

            byte[] buffer = File.ReadAllBytes(Constants.CONFIG_PATH);
            Console.WriteLine("Returning configuration from file.");
            return JsonConvert.DeserializeObject<IssuedConfig>(Encoding.UTF8.GetString(buffer));
        }

        public void WriteConfig(IssuedConfig config)
        {
            if(config == null)
            {
                throw new ArgumentNullException("config");
            }

            string jsonString = JsonConvert.SerializeObject(config);
            byte[] buffer = Encoding.UTF8.GetBytes(jsonString);

            Console.WriteLine("Writing configuration to file.");
            File.WriteAllBytes(Constants.CONFIG_PATH, buffer);
        }
    }
}
