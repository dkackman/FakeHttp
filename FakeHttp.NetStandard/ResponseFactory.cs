using System.IO;
using System.Net.Http;

[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("FakeHttp.UnitTests")]

namespace FakeHttp
{
    static class ResponseFactory
    {
        /// <summary>
        /// Create an <see cref="System.Net.Http.HttpResponseMessage"/> from the object's state
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> the initiates the response</param>
        /// <param name="content">The content stream</param>
        /// <returns>The <see cref="System.Net.Http.HttpResponseMessage"/></returns>
        public static HttpResponseMessage CreateResponse(this ResponseInfo info, HttpRequestMessage request, Stream content)
        {
            var response = new HttpResponseMessage(info.StatusCode)
            {
                Version = info.HttpVersion,
                RequestMessage = request
            };

            foreach (var kvp in info.ResponseHeaders)
            {
                response.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }

            if (content != null)
            {
                response.Content = info.CreateContent(content);
            }

            return response;
        }

        /// <summary>
        /// Creates an <see cref="System.Net.Http.HttpContent"/> object from a stream, setting content headers
        /// </summary>
        /// <param name="stream">The content stream</param>
        /// <returns>The conent object</returns>
        public static HttpContent CreateContent(this ResponseInfo info,Stream stream)
        {
            var content = new StreamContent(stream);
            foreach (var kvp in info.ContentHeaders)
            {
                content.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }

            return content;
        }
    }
}
