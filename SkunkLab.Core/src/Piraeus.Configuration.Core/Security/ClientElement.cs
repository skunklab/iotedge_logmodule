using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Configuration.Security
{
    public class ClientElement : ConfigurationElement
    {
        [ConfigurationProperty("symmetricKey", IsRequired =false)]
        public SymmetricKeyElement SymmetricKey
        {
            get { return (SymmetricKeyElement)base["symmetricKey"]; }
            set { base["symmetricKey"] = value; }
        }

        //[ConfigurationProperty("asymmetricKey")]
        //public AsymmetricKeyElement AsymmetricKey
        //{
        //    get { return (AsymmetricKeyElement)base["asymmetricKey"]; }
        //    set { base["asymmetricKey"] = value; }
        //}


    }
}
