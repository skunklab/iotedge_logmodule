using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;




namespace EchoMicroservice
{
    public class BinaryOutputFormatter : OutputFormatter
    {
        public BinaryOutputFormatter()
        {
            MediaTypeHeaderValue header = new MediaTypeHeaderValue("application/octet-stream");
            SupportedMediaTypes.Add(header);
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var contentType = context.HttpContext.Request.ContentType;
            if (string.IsNullOrEmpty(contentType) ||
                contentType == "application/octet-stream")
                return true;

            return false;
        }

        public override Task WriteAsync(OutputFormatterWriteContext context)
        {
            return base.WriteAsync(context);
        }

        //public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        //{

        //    //return base.GetSupportedContentTypes(contentType, objectType);
        //}

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            await Task.CompletedTask;
            //if(context.Object == null)
            //{
            //    return;
            //}

            //HttpResponseMessage msg = context.Object as HttpResponseMessage;
            //if(msg == null)
            //{
            //    return;
            //}

            //byte[] data = await msg.Content.ReadAsByteArrayAsync();
            //if(data == null)
            //{
            //    Console.WriteLine("---------- No byte[] data ----------");
            //    return;
            //}

            //var response = context.HttpContext.Response;

            //await response.Body.WriteAsync(data, 0, data.Length);
            
            //byte[] data = MyLittleHelper.Queue.Dequeue();
            //return context.HttpContext.Response.Body.WriteAsync(data, 0, data.Length); 
        }

        private Exception InvalidOperationException(string v)
        {
            throw new NotImplementedException();
        }
    }
}
