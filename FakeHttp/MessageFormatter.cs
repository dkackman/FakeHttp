using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace FakeHttp
{
    /// <summary>
    /// Base class that formats http request and response message data prior to serialization
    /// </summary>
    public abstract class MessageFormatter
    {
        private readonly Func<string, string, bool> _paramFilter;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="paramFilter">call back used to determine if a given query paramters should be excluded from serialziation</param>
        protected MessageFormatter(Func<string, string, bool> paramFilter)
        {
            if (paramFilter == null)
            {
                throw new ArgumentNullException("paramFilter");
            }

            _paramFilter = paramFilter;
        }

        /// <summary>
        /// Convert the <see cref="System.Net.Http.HttpResponseMessage"/> into an object
        /// that is more serialization friendly
        /// </summary>
        /// <param name="response">The <see cref="System.Net.Http.HttpResponseMessage"/></param>
        /// <returns>A serializable object</returns>
        public ResponseInfo PackageResponse(HttpResponseMessage response)
        {
            var query = NormalizeQuery(response.RequestMessage.RequestUri);
            var fileName = ToFileName(response.RequestMessage, query);
            var fileExtension = "";
            if (response.Content.Headers.ContentType != null)
            {
                fileExtension = MimeMap.GetFileExtension(response.Content.Headers.ContentType.MediaType);
            }

            // since HttpHeaders is not a creatable object, store the headers off to the side
            var headers = response.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var contentHeaders = response.Content.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new ResponseInfo()
            {
                StatusCode = response.StatusCode,
                Query = query,
                ContentFileName = !string.IsNullOrEmpty(fileExtension) ? string.Concat(fileName, ".content", fileExtension) : null,
                ResponseHeaders = headers,
                ContentHeaders = contentHeaders
            };
        }

        /// <summary>
        /// Genearate a SHA1 hash of a given text. Allows for platform specific implementation
        /// </summary>
        /// <param name="text">The string to hash</param>
        /// <returns>The hash</returns>
        public abstract string ToSha1Hash(string text);

        /// <summary>
        /// Retreive folder friendly representation of a Uri
        /// </summary>
        /// <param name="uri">The uri</param>
        /// <returns>Folder path</returns>
        public string ToFolderPath(Uri uri)
        {
            return Path.Combine(uri.Host, uri.LocalPath.TrimStart('/').Replace('/', '\\'));
        }

        /// <summary>
        /// Determinaisatally generated file name for a request message
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="query">Nomralized query string</param>
        /// <returns>Filename</returns>
        public string ToFileName(HttpRequestMessage request, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return ToShortFileName(request);
            }

            return string.Concat(request.Method.Method, ".", ToSha1Hash(query));
        }

        /// <summary>
        /// Determinaisatally generated file name for a request message
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>Filename</returns>
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
        /// <returns>The normalized query string</returns>
        public string NormalizeQuery(Uri uri)
        {
            var sortedParams = from p in GetParameters(uri, _paramFilter)
                               orderby p
                               select p;

            var builder = new StringBuilder();
            var seperator = "";
            foreach (var param in sortedParams)
            {
                builder.AppendFormat("{0}{1}", seperator, param);
                seperator = "&";
            }

            return builder.ToString().ToLowerInvariant();
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
                var name = param.Key;
                var value = param.Value != null ? param.Value : "";

                if (!paramFilter(name, value))
                {
                    yield return string.Format("{0}={1}", name, value);
                }
            }
        }
    }
}
