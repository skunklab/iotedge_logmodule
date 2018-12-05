using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Piraeus.Configuration.Settings
{

    [Serializable]
    [JsonObject]
    public class ServiceIdentity
    {
        public ServiceIdentity()
        {
        }

        public ServiceIdentity(List<KeyValuePair<string,string>> claims)
        {
            this.claims = claims;

            if (claims != null)
            {
                KvpsToString();
            }
        }

        private string claimTypes;
        private string claimValues;
        private List<KeyValuePair<string, string>> claims;

     

        [JsonIgnore]
        public List<KeyValuePair<string,string>> Claims
        {
            get { return this.claims; }
            set
            {
                this.claims = value;
                KvpsToString();
            }
        }

        [JsonProperty("claimTypes")]
        public string ClaimTypes
        {
            get { return this.claimTypes; }
            set
            {
                this.claimTypes = value;
                StringsToKvps();
            }
        }

        [JsonProperty("claimValues")]
        public string ClaimValues
        {
            get { return this.claimValues; }
            set
            {
                this.claimValues = value;
                StringsToKvps();
            }
        }

        private void KvpsToString()
        {
            if (this.claims != null)
            {
                StringBuilder keyBuilder = new StringBuilder();
                StringBuilder valueBuilder = new StringBuilder();
                foreach (var item in this.claims)
                {
                    keyBuilder.Append(item.Key + ";");
                    valueBuilder.Append(item.Value + ";");
                }

                if (keyBuilder.ToString().Length > 0)
                {
                    string keyString = keyBuilder.ToString().TrimEnd(';');
                    string valueString = valueBuilder.ToString().TrimEnd(';');
                    this.claimTypes = keyString;
                    this.claimValues = valueString;
                }
            }
        }

        private void StringsToKvps()
        {
            if (!string.IsNullOrEmpty(this.claimTypes) && !string.IsNullOrEmpty(this.claimValues))
            {
                string[] keyParts = this.claimTypes.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string[] valueParts = this.claimValues.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (keyParts.Length == valueParts.Length)
                {
                    List<KeyValuePair<string, string>> kvps = new List<KeyValuePair<string, string>>();
                    int index = 0;
                    while (index < keyParts.Length)
                    {
                        kvps.Add(new KeyValuePair<string, string>(keyParts[index], valueParts[index]));
                        index++;
                    }

                    this.claims = kvps;
                }
            }
        }
    }
}
