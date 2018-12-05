using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class ClientIdentity
    {
        public ClientIdentity()
        {

        }

        public ClientIdentity(string identityClaimType, List<KeyValuePair<string, string>> indexes = null)
        {
            IdentityClaimType = identityClaimType;
            this.indexes = indexes;
            if(indexes != null)
            {
                KvpsToString();
            }
        }

        private string indexKeys;
        private string indexValues;
        private List<KeyValuePair<string, string>> indexes;

        [JsonProperty("identityClaimType")]
        public string IdentityClaimType { get; set; }

        [JsonIgnore]
        public List<KeyValuePair<string,string>> Indexes
        {
            get { return this.indexes; }
            set
            {
                this.indexes = value;
                KvpsToString();
            }
        }

        [JsonProperty("indexKeys")]
        public string IndexKeys
        {
            get { return this.indexKeys; }
            set
            {
                this.indexKeys = value;
                StringsToKvps();
            }
        }

        [JsonProperty("indexValues")]
        public string IndexValues
        {
            get { return this.indexValues; }
            set
            {
                this.indexValues = value;
                StringsToKvps();
            }
        }


        private void KvpsToString()
        {
            if(this.indexes != null)
            {
                StringBuilder keyBuilder = new StringBuilder();
                StringBuilder valueBuilder = new StringBuilder();
                foreach (var item in this.indexes)
                {
                    keyBuilder.Append(item.Key + ";");
                    valueBuilder.Append(item.Value + ";");
                }

                if (keyBuilder.ToString().Length > 0)
                {
                    string keyString = keyBuilder.ToString().TrimEnd(';');
                    string valueString = valueBuilder.ToString().TrimEnd(';');
                    this.indexKeys = keyString;
                    this.indexValues = valueString;
                }
            }
        }

        private void StringsToKvps()
        {
            if(!string.IsNullOrEmpty(this.indexKeys) && !string.IsNullOrEmpty(this.indexValues))
            {
                string[] keyParts = this.indexKeys.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string[] valueParts = this.indexValues.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if(keyParts.Length == valueParts.Length)
                {
                    List<KeyValuePair<string, string>> kvps = new List<KeyValuePair<string, string>>();
                    int index = 0;
                    while (index < keyParts.Length)
                    {
                        kvps.Add(new KeyValuePair<string, string>(keyParts[index], valueParts[index]));
                        index++;
                    }

                    this.indexes = kvps;
                }
            }
        }


    }
}
