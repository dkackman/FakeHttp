using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace MockHttp
{
    static class Extensions
    {
        public static string ToSha1Hash(this string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha1 = new SHA1Managed())
            {
                byte[] textData = Encoding.UTF8.GetBytes(text);

                byte[] hash = sha1.ComputeHash(textData);

                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

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
            if (string.IsNullOrEmpty(query))
            {
                return request.ToShortFileName();
            }

            return string.Concat(request.Method.Method, ".", query.ToSha1Hash());
        }

        public static string ToShortFileName(this HttpRequestMessage request)
        {
            return request.Method.Method;
        }

        public static string NormalizeQuery(this Uri uri, Func<string, string, bool> paramFilter)
        {
            var sortedParams = from p in GetParameters(uri, paramFilter)
                               orderby p
                               select p;

            var builder = new StringBuilder();
            var seperator = "";
            foreach (var param in sortedParams)
            {
                builder.AppendFormat("{0}{1}", seperator, param);
                seperator = "&";
            }

            return builder.ToString();
        }

        private static IEnumerable<string> GetParameters(Uri uri, Func<string, string, bool> paramFilter)
        {
            foreach (var param in uri.ParseQueryString())
            {
                var name = param.Key.ToLowerInvariant();
                var value = param.Value != null ? param.Value.ToLowerInvariant() : "";

                if (!paramFilter(name, value))
                {
                    yield return string.Format("{0}={1}", name, value);
                }
            }
        }
    }
}
