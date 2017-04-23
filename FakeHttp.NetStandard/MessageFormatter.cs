using System;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace FakeHttp
{
    /// <summary>
    /// Class that formats http request and response message data prior for serialization and deserialization.
    /// You shouldn't need to use this type.
    /// </summary>
    public sealed class MessageFormatter
    {
        /// <summary>
        /// object used to allow client code to modify responses during load and storage
        /// </summary>
        private readonly IResponseCallbacks _callbacks;

        /// <summary>
        /// ctor
        /// </summary>
        internal MessageFormatter()
            : this(new ResponseCallbacks())
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="callbacks">object used to allow client code to modify responses during load and storage</param>
        internal MessageFormatter(IResponseCallbacks callbacks)
        {
            _callbacks = callbacks ?? throw new ArgumentNullException("callbacks");
        }

        /// <summary>
        /// object used to allow client code to modify responses during load and storage
        /// </summary>
        internal IResponseCallbacks Callbacks => _callbacks;

        /// <summary>
        /// Convert the <see cref="HttpResponseMessage"/> into an object that is more serialization friendly
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/></param>
        /// <returns>A serializable object</returns>
        /// <exception cref="ArgumentNullException"/>
        internal ResponseInfo PackageResponse(HttpResponseMessage response)
        {
            if (response == null) throw new ArgumentNullException("response");

            var uri = response.RequestMessage.RequestUri;
            var name = ToName(response.RequestMessage);
            var contentExtension = MimeMap.GetFileExtension(response.Content.Headers.ContentType?.MediaType);

            // since HttpHeaders is not a creatable object, store the headers off to the side
            // sensitive header values are filtered here
            var headers = response.Headers.Where(h => !_callbacks.FilteredHeaderNames.Contains(h.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var contentHeaders = response.Content.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new ResponseInfo()
            {
                HttpVersion = response.Version,
                StatusCode = response.StatusCode,
                BaseUri = uri.GetComponents(UriComponents.NormalizedHost | UriComponents.Path, UriFormat.Unescaped),
                Query = uri.NormalizeQuery(_callbacks.FilterParameter),
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
        internal string ToResourcePath(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            return Path.Combine(uri.Host, uri.LocalPath.TrimStart('/')).Replace('/', '\\');
        }

        /// <summary>
        /// Deterministically generated file name for a request message
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>Filename</returns>
        /// <exception cref="ArgumentNullException"/>
        internal string ToName(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var query = request.RequestUri.NormalizeQuery(_callbacks.FilterParameter);
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
        internal string ToShortName(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            return request.Method.Method;
        }
    }
}
