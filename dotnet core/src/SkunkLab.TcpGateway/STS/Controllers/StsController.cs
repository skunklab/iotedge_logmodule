using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace STS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StsController : ControllerBase
    {
        public StsController(IConfiguration config)
        {
            this.config = config;
            //vrtusts_AzureStorageConnectionString
            acct = CloudStorageAccount.Parse(this.config.GetValue<string>("ConnectionStrings:/vrtusts_AzureStorageConnectionString"));
            tableClient = acct.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("rtu");

            

        }

        private IConfiguration config;
        private CloudTableClient tableClient;

        private CloudStorageAccount acct;
        [HttpGet]
        public string Get(string luss)
        {

        }
    }
}