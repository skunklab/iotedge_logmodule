using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Core.Utilities
{
    public static class UriExtensions
    {
        public static string ToCanonicalString(this Uri uri, bool trailingWhack, bool removeLastSegment = false)
        {
            string uriString = uri.ToString().ToLowerInvariant();
            string result = null;

            if (string.IsNullOrEmpty(uri.Query))
            {
                result = GetBase(uriString, uri, trailingWhack);
            }
            else
            {
                result = GetFromQuery(uriString, uri);
            }

            if(!removeLastSegment)
            {
                return result;
            }
            else
            {
                Uri uri2 = new Uri(result);
                return result.Replace("/" + uri2.Segments[uri2.Segments.Length - 1], "");
            }


        }

        private static string GetFromQuery(string uriString, Uri uri)
        {
            string resourceString = uriString.Replace(uri.Query, "");
            return GetBase(resourceString, new Uri(resourceString), false);
            //string canonicalResourceString = GetBase(resourceString, new Uri(resourceString), false);
            //return canonicalResourceString + uri.Query;
        }


        private static string GetBase(string uriString, Uri uri, bool trailingWhack)
        {
            bool isTrailing = uri.Segments[uri.Segments.Length - 1] == "/";

            if (trailingWhack)
            {
                return isTrailing ? uriString : uriString + "/";
            }
            else
            {
                return !isTrailing ? uriString : uriString.Remove(uriString.Length - 1, 1);
            }
        }
    }
}
