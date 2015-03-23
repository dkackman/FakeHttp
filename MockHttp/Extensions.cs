using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;

namespace MockHttp
{
    static class Extensions
    {
        public static string ToFilePath(this Uri uri)
        {
            return Path.Combine(uri.Host, uri.LocalPath.TrimStart('/').Replace('/', '\\'));
        }

        private readonly static string[] _ignoreNames = new string[] { "key" };

        public static string ToFileName(this HttpRequestMessage request)
        {
            var query = request.RequestUri.NormalizeQuery();

            return string.Concat(request.Method.Method, ".", query.GetHashCode().ToString());
        }
        public static string ToShortFileName(this HttpRequestMessage request)
        {
            return request.Method.Method;
        }

        public static string NormalizeQuery(this Uri uri)
        {
            var query = uri.Query.TrimStart('?').ToLowerInvariant();

            var sortedParams = from p in GetParameters(query)
                               orderby p
                               select p;

            var builder = new StringBuilder();
            var seperator = "";
            foreach (var param in sortedParams)
            {
                builder.AppendFormat("{0}{1}", seperator, param);
                seperator = "&";
            }

            return string.Concat(builder.ToString());
        }

        private static IEnumerable<string> GetParameters(string query)
        {
            foreach (var param in query.Split('&'))
            {
                var split = param.Split('=');
                if (!_ignoreNames.Contains(split[0]))
                {
                    yield return param;
                }
            }
        }
    }
}
