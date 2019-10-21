﻿using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LogModule
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

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            try
            {
                HttpResponseMessage response = (HttpResponseMessage)context.Object;
                if (response.Content != null)
                {
                    byte[] content = await response.Content.ReadAsByteArrayAsync();
                    context.HttpContext.Response.ContentLength = content.Length;
                    await context.HttpContext.Response.Body.WriteAsync(content);
                }
                else
                {
                    return;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"WriteResponseBodyAsync error - {ex.Message}");
                throw ex;
            }
        }

        private Exception InvalidOperationException(string v)
        {
            throw new NotImplementedException();
        }
    }
}
