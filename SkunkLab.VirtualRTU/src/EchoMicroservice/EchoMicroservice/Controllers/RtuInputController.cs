using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace EchoMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RtuInputController : ControllerBase
    {
        private string requestUrl; 

        public RtuInputController()
        {
            

            //string url = System.Environment.GetEnvironmentVariable("RTU_OutputUrl");
            //if(!string.IsNullOrEmpty(url))
            //{
            //    requestUrl = url;
            //}
        }
        //[HttpGet]
        //public HttpResponseMessage Get()
        //{
        //    HttpResponseMessage response = new HttpResponseMessage();
        //    response.Content = new StringContent("{ pie: \"hello\" }");
        //    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        //    response.StatusCode = HttpStatusCode.OK;
        //    return response;
        //}

        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] byte[] value)
        {
            Console.WriteLine("{0} - Received message to echo of '{1}' length", DateTime.Now.ToString("hh:MM:ss.ffff"), value.Length);
            try
            {
                await MyLittleHelper.EchoAsync(value);
                Console.WriteLine("{0} - Echo completed", DateTime.Now.ToString("hh:MM:ss.ffff"));
                //MyLittleHelper.Queue.Enqueue(value);

                //Task task =MyLittleHelper.ForwardAsync(value);
                //Task.WhenAll(task);

                return new HttpResponseMessage(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fault in http forward '{0}'", ex.Message);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }


        

     
    }
}
