using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SkunkLab.WebGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectController : ControllerBase
    {
        public ConnectController()
        {

        }

        /// <summary>
        /// HTTP message POST'd to Web gateway
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] message)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Web Socket connection
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> Get([FromBody] message)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        }
    }
}