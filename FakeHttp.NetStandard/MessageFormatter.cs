using System;
using System.Linq;
using System.IO;
using System.Net.Http;

namespace FakeHttp
{
    /// <summary>
    /// Class that formats http request and response message data prior for serialization and deserialization
    /// </summary>
    public sealed class MessageFormatter
    {
        /// <summary>
        /// Convert the <see cref="System.Net.Http.HttpResponseMessage"/> into an object that is more serialization friendly
        /// </summary>
        /// <param name="response">The <see cref="System.Net.Http.HttpResponseMessage"/></param>
        /// <param name="filter">Predicate to applt to each paramter that can be used to keep them out of the packaged response</param>
        /// <returns>A serializable object</returns>
        /// <exception cref="ArgumentNullException"/>
        public ResponseInfo PackageResponse(HttpResponseMessage response, Func<string, string, bool> filter)
        {
            if (response == null) throw new ArgumentNullException("response");
            if (filter == null) throw new ArgumentNullException("filter");

            var uri = response.RequestMessage.RequestUri;
            var name = ToName(response.RequestMessage, filter);
            var contentExtension = MimeMap.GetFileExtension(response.Content.Headers.ContentType?.MediaType);

            // since HttpHeaders is not a creatable object, store the headers off to the side
            // also never put our FAKEHTTP header in the serializable object
            var headers = response.Headers.Where(h => h.Key != "FAKEHTTP").ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var contentHeaders = response.Content.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new ResponseInfo()
            {
                HttpVersion = response.Version,
                StatusCode = response.StatusCode,
                BaseUri = uri.GetComponents(UriComponents.NormalizedHost | UriComponents.Path, UriFormat.Unescaped),
                Query = uri.NormalizeQuery(filter),
                // if file extension is null we have no content
                ContentFileName = !string.IsNullOrEmpty(contentExtension) ? string.Concat(name, ".content", contentExtension) : null,
                ResponseHeaders = headers,
                ContentHeaders = contentHeaders
            };
        }

        /// <summary>
        /// Retrieve folder friendly representation of a Uri
        /// </summary>
        /// <param name="uri">The uri</param>
        /// <returns>Resource path</returns>
        /// <exception cref="ArgumentNullException"/>
        public string ToResourcePath(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            return Path.Combine(uri.Host, uri.LocalPath.TrimStart('/')).Replace('/', '\\');
        }

        /// <summary>
        /// Deterministically generated file name for a request message
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="filter">Parameter filtering predicate</param>
        /// <returns>Filename</returns>
        /// <exception cref="ArgumentNullException"/>
        public string ToName(HttpRequestMessage request, Func<string, string, bool> filter)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (filter == null) throw new ArgumentNullException("filter");

            var query = request.RequestUri.NormalizeQuery(filter);
            if (string.IsNullOrEmpty(query))
            {
                return ToShortName(request);
            }

            return string.Concat(request.Method.Method, ".", query.Hash());
        }

        /// <summary>
        /// Deterministically generated file name for a request message
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>Filename</returns>
        /// <exception cref="ArgumentNullException"/>
        public string ToShortName(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            return request.Method.Method;
        }
    }
}
