using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VirtualRtuWebApp.Pages
{
    public class ProvisionModel : PageModel
    {
        public void OnGet()
        {

        }

        public string DeviceId { get; set; }

        public string ModuleId { get; set; }

        public int UnitId { get; set; }

        public string VirtualRtuId { get; set; }
        
        public int ExpirationMinutes { get; set; }
    }
}