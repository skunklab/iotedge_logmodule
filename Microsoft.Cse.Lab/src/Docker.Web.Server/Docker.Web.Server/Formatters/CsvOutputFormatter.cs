using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Docker.Web.Server.Formatters
{
    public class CsvOutputFormatter : TextOutputFormatter
    {
        public CsvOutputFormatter()
        {
            MediaTypeHeaderValue header = new MediaTypeHeaderValue(mediaType);
            SupportedMediaTypes.Add(header);
        }
    }
}
