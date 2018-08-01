using System;
using System.Globalization;

namespace SkunkLab.Channels.Core
{
    public static class UriExtensions
    {
        public static string ToLower(this Uri uri)
        {
            return uri.ToString().ToLower(CultureInfo.InvariantCulture);
        }
    }
}
