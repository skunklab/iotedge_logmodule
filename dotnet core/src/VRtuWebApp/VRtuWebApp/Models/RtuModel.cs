using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VRtuWebApp.Models
{
    public class RtuModel
    {
        public RtuModel()
        {

        }

        public int UnitId { get; set; }

        public string VirtualRtuId { get; set; }

        public string DeviceId { get; set; }

        public string ModuleId { get; set; }

        public int ExpirationMinutes { get; set; }
    }
}
