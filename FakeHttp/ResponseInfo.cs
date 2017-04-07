using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace FakeHttp
{
    /// <summary>
    /// A serialization friendly wrapper around <see cref="System.Net.Http.HttpResponseMessage"/>
    /// </summary>
    public sealed class ResponseInfo
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ResponseInfo()
        {
        }

        /// <summary>
        /// <see cref="HttpResponseMessage.Version"/>
        /// </summary>
        public Version HttpVersion { get; set; } = new Version(1, 1);

        /// <summary>
        /// The response status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// The host and path of the request that originally created this response
        /// </summary>
        public string BaseUri { get; set; }

        /// <summary>
        /// The query string from the request that generated the response (used to key the response for future reference)
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// The name of the associated serialized content file
        /// </summary>
        public string ContentFileName { get; set; }

        /// <summary>
        /// The response headers
        /// </summary>
        public Dictionary<string, IEnumerable<string>> ResponseHeaders { get; set; } = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The content headers
        /// </summary>
        public Dictionary<string, IEnumerable<string>> ContentHeaders { get; set; } = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Create an <see cref="System.Net.Http.HttpResponseMessage"/> from the object's state
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> the initiates the response</param>
        /// <param name="content">The content stream</param>
        /// <returns>The <see cref="System.Net.Http.HttpResponseMessage"/></returns>
        public HttpResponseMessage CreateResponse(HttpRequestMessage request, Stream content)
        {
            var response = new HttpResponseMessage(StatusCode)
            {
                Version = HttpVersion,
                RequestMessage = request
            };

            foreach (var kvp in ResponseHeaders)
            {
                response.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }

            if (content != null)
            {
                response.Content = CreateContent(content);
            }

            return response;
        }

        /// <summary>
        /// Creates an <see cref="System.Net.Http.HttpContent"/> object from a stream, setting content headers
        /// </summary>
        /// <param name="stream">The content stream</param>
        /// <returns>The conent object</returns>
        public HttpContent CreateContent(Stream stream)
        {
            var content = new StreamContent(stream);
            foreach (var kvp in ContentHeaders)
            {
                content.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }

            return content;
        }
    }
}
