using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace MockHttp
{
    public abstract class RequestFormatter
    {
        public abstract string ToSha1Hash(string text);

        public string ToFilePath(Uri uri)
        {
            return Path.Combine(uri.Host, uri.LocalPath.TrimStart('/').Replace('/', '\\'));
        }

        public string ToFileName(HttpRequestMessage request, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return ToShortFileName(request);
            }

            return string.Concat(request.Method.Method, ".", ToSha1Hash(query));
        }

        public string ToShortFileName(HttpRequestMessage request)
        {
            return request.Method.Method;
        }

        /// <summary>
        /// The normalization algorthm is logically as follows
        /// - ToLowerInvariant the query string
        /// - split out each name value pair
        /// - filter out unwanted paramaters
        /// - order the remaining parameters alphabetically
        /// - reassemble them into a query string (without leading '?')
        /// </summary>
        /// <param name="uri">The <see cref="System.Uri"/></param>
        /// <param name="paramFilter">A callback to indicate which paramters to filter from the normalized query string</param>
        /// <returns>The normalized query string</returns>
        public string NormalizeQuery(Uri uri, Func<string, string, bool> paramFilter)
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

        private static readonly Regex _regex = new Regex(@"[?|&]([\w\.]+)=([^?|^&]+)");

        private static IReadOnlyDictionary<string, string> ParseQueryString(Uri uri)
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

        private static IEnumerable<string> GetParameters(Uri uri, Func<string, string, bool> paramFilter)
        {
            foreach (var param in ParseQueryString(uri))
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
