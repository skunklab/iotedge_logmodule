﻿using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LogModule
{
    public class BinaryInputFormatter : InputFormatter
    {

        public BinaryInputFormatter()
        {
            MediaTypeHeaderValue header = new MediaTypeHeaderValue("application/octet-stream");
            SupportedMediaTypes.Add(header);
        }

        public override Boolean CanRead(InputFormatterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var contentType = context.HttpContext.Request.ContentType;
            if (string.IsNullOrEmpty(contentType) ||
                contentType == "application/octet-stream")
                return true;

            return false;
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            try
            {
                var request = context.HttpContext.Request;
                var contentType = context.HttpContext.Request.ContentType;


                if (contentType == "application/octet-stream")
                {
                    using (var ms = new MemoryStream(2048))
                    {
                        await request.Body.CopyToAsync(ms);
                        var content = ms.ToArray();
                        return await InputFormatterResult.SuccessAsync(content);
                    }
                }

                return await InputFormatterResult.FailureAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"ReadRequestBodyAsync error - {ex.Message}");
                throw ex;
            }
        }


    }
}
