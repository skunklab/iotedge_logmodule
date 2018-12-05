using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Configuration.Core
{
    public abstract class WebConfig
    {
        #region Encrypted Channel
        public virtual string HttpsCertficateFilename { get; set; }

        public virtual string HttpsCertificatePassword { get; set; }

        #endregion

        
    }
}
