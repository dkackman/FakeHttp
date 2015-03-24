using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace MockHttp
{
    static class Extensions
    {
        private static readonly Regex _regex = new Regex(@"[?|&]([\w\.]+)=([^?|^&]+)", RegexOptions.Compiled);

        public static IReadOnlyDictionary<string, string> ParseQueryString(this Uri uri)
        {
            var match = _regex.Match(uri.PathAndQuery);
            var paramaters = new Dictionary<string, string>();
            while (match.Success)
            {
                paramaters.Add(match.Groups[1].Value, match.Groups[2].Value);
                match = match.NextMatch();
            }
            return paramaters;
        }

        public static string ToFilePath(this Uri uri)
        {
            return Path.Combine(uri.Host, uri.LocalPath.TrimStart('/').Replace('/', '\\'));
        }

        public static string ToFileName(this HttpRequestMessage request, string query)
        {
            return string.Concat(request.Method.Method, ".", query.GetHashCode().ToString());
        }

        public static string ToShortFileName(this HttpRequestMessage request)
        {
            return request.Method.Method;
        }

        public static string NormalizeQuery(this Uri uri, Func<string, string, bool> paramFilter)
        {
            var query = uri.Query.TrimStart('?').ToLowerInvariant();

            var sortedParams = from p in GetParameters(query, paramFilter)
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

        private static IEnumerable<string> GetParameters(string query, Func<string, string, bool> paramFilter)
        {
            if (string.IsNullOrEmpty(query))
            {
                yield break;
            }
            
            foreach (var param in query.Split('&'))
            {
                var split = param.Split('=');
                if (!paramFilter(split[0], split[1]))
                {
                    yield return param;
                }
            }
        }
    }
}
