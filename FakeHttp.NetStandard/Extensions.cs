using System;
using System.Text;
using System.Net.Http;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Linq;

using Microsoft.QueryStringDotNET;

namespace FakeHttp
{
    static class Extensions
    {
        public static HttpResponseMessage Prepare(this HttpResponseMessage response)
        {
            Debug.Assert(response != null);

            // this header value is never serialized it is 
            // used for debugging that the response was loaded from storage not retrieved from a live service
            response.Headers.TryAddWithoutValidation("FAKEHTTP", "1");

            return response;
        }

        public static string Hash(this string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            using (var sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(text));

                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
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
        /// <param name="filter"></param>
        /// <returns>The normalized query string</returns>
        public static string NormalizeQuery(this Uri uri, Func<string, string, bool> filter)
        {
            // QueryString leaves the '?' on the first parameter - make sure to trim it out
            var sortedParams = from p in QueryString.Parse(uri.Query.TrimStart('?').ToLowerInvariant())
                               where !filter(p.Name, p.Value)
                               orderby p.Name
                               select p;

            var builder = new StringBuilder();
            var seperator = "";
            foreach (var param in sortedParams)
            {
                builder.Append($"{seperator}{param.Name}={param.Value}");
                seperator = "&";
            }

            return builder.ToString();
        }
    }
}
