using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FieldGatewayMicroservice.Communications;
using FieldGatewayMicroservice.Connections;
using Microsoft.AspNetCore.Mvc;

namespace FieldGatewayMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RtuOutputController : ControllerBase
    {
        //private string requestUrl; // = "http://echomodule:8889/api/rtuinput";
        

        public RtuOutputController()
        {
           
            //string url = System.Environment.GetEnvironmentVariable("RTU_InputUrl");
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
            try
            {
                Console.WriteLine("Recieved msg from PA");
                MqttClient client = MqttClient.Create();
                await client.PublishAsync(value);
                Console.WriteLine("Published msg to Piraeus");
                //Console.WriteLine("{0} - Start echo forward to Piraeus", DateTime.Now.ToString("hh:MM:ss.ffff"));
                //await EdgeClient.Client.PublishAsync(EdgeClient.Config.Resources.RtuOutputResource, value);
                //Console.WriteLine("{0} - End echo forward to Piraeus", DateTime.Now.ToString("hh:MM:ss.ffff"));
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Fault publishing MQTT message '{0}'", ex.Message);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }


        }


        //private async Task<HttpStatusCode> ForwardAsync(byte[] payload)
        //{
        //    BinaryOutputFormatter2 formatter = new BinaryOutputFormatter2();
        //    HttpClient client = new HttpClient();
        //    HttpContent content = new ByteArrayContent(payload);
        //    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        //    content.Headers.ContentLength = payload.Length;
        //    HttpResponseMessage response = await client.PostAsync(requestUrl, content);
        //    return response.StatusCode;
        //}


    }
}
