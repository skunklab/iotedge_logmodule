using System;
using System.Globalization;

namespace SkunkLab.Channels
{
    public static class UriExtensions
    {
        public static string ToLower(this Uri uri)
        {
            return uri.ToString().ToLower(CultureInfo.InvariantCulture);
        }
    }
}
