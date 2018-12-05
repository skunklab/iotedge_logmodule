using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Docker.Web.Server.Formatters
{
    public class CsvInputFormatter : TextInputFormatter
    {
        private const string mediaType = "text/csv";
        public CsvInputFormatter()
        {
            MediaTypeHeaderValue header = new MediaTypeHeaderValue(mediaType);
            SupportedMediaTypes.Add(header);
        }

        public override Boolean CanRead(InputFormatterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var contentType = context.HttpContext.Request.ContentType;
            if (string.IsNullOrEmpty(contentType) ||
                contentType == mediaType)
                return true;

            return false;
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var contentType = context.HttpContext.Request.ContentType;


            if (contentType == mediaType)
            {
                using (var ms = new MemoryStream())
                {
                    await request.Body.CopyToAsync(ms);
                    var content = Encoding.UTF8.GetString(ms.ToArray());
                    return await InputFormatterResult.SuccessAsync(content);
                }
            }

            return await InputFormatterResult.FailureAsync();
        }


    }
}
