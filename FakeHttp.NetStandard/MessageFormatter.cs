using System;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;

using Microsoft.QueryStringDotNET;

namespace FakeHttp
{
    /// <summary>
    /// Base class that formats http request and response message data prior to serialization
    /// </summary>
    public sealed class MessageFormatter
    {
        private readonly IResponseCallbacks _responseCallbacks;
        public MessageFormatter()
            :this(new ResponseCallbacks())
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="callbacks">call back object to manage resposnes at runtime</param>
        public MessageFormatter(IResponseCallbacks callbacks)
        {
            _responseCallbacks = callbacks ?? throw new ArgumentNullException("callbacks");
        }

        /// <summary>
        /// A <see cref="IResponseCallbacks"/> to manage response handling at runtime
        /// </summary>
        public IResponseCallbacks RepsonseCallbacks => _responseCallbacks; 

        /// <summary>
        /// Convert the <see cref="System.Net.Http.HttpResponseMessage"/> into an object
        /// that is more serialization friendly
        /// </summary>
        /// <param name="response">The <see cref="System.Net.Http.HttpResponseMessage"/></param>
        /// <returns>A serializable object</returns>
        public ResponseInfo PackageResponse(HttpResponseMessage response)
        {
            var uri = response.RequestMessage.RequestUri;
            var fileName = ToFileName(response.RequestMessage);
            var fileExtension = MimeMap.GetFileExtension(response?.Content?.Headers?.ContentType?.MediaType);            

            // since HttpHeaders is not a creatable object, store the headers off to the side
            // also never put our FAKEHTTP header in the serializable object
            var headers = response.Headers.Where(h => h.Key != "FAKEHTTP").ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var contentHeaders = response.Content.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new ResponseInfo()
            {
                HttpVersion = response.Version,
                StatusCode = response.StatusCode,
                BaseUri = uri.GetComponents(UriComponents.NormalizedHost | UriComponents.Path, UriFormat.Unescaped),
                Query = NormalizeQuery(uri),
                // if file extension is null we have no content
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
        public string ToSha1Hash(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            using (var sha1 = SHA1.Create())
            {
                byte[] textData = Encoding.UTF8.GetBytes(text);
                byte[] hash = sha1.ComputeHash(textData);

                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
        /// <summary>
        /// Retrieve folder friendly representation of a Uri
        /// </summary>
        /// <param name="uri">The uri</param>
        /// <returns>Folder path</returns>
        public string ToFolderPath(Uri uri)
        {
            return Path.Combine(uri.Host, uri.LocalPath.TrimStart('/')).Replace("/", "\\");
        }

        /// <summary>
        /// Deterministically generated file name for a request message
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="query">Nomralized query string</param>
        /// <returns>Filename</returns>
        public string ToFileName(HttpRequestMessage request)
        {
            var query = NormalizeQuery(request.RequestUri);
            if (string.IsNullOrEmpty(query))
            {
                return ToShortFileName(request);
            }

            return string.Concat(request.Method.Method, ".", ToSha1Hash(query));
        }

        /// <summary>
        /// Deterministically generated file name for a request message
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
        private string NormalizeQuery(Uri uri)
        {
            // QueryString leaves the '?' on the first parameter - make sure to trim it out
            var sortedParams = from p in QueryString.Parse(uri.Query.TrimStart('?'))
                               where !_responseCallbacks.FilterParameter(p.Name, p.Value)
                               orderby p.Name
                               select p;

            var builder = new StringBuilder();
            var seperator = "";
            foreach (var param in sortedParams)
            {
                builder.Append($"{seperator}{param.Name}={param.Value}");
                seperator = "&";
            }

            return builder.ToString().ToLowerInvariant();
        }
    }
}
