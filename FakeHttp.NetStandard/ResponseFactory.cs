using System.IO;
using System.Net.Http;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FakeHttp.UnitTests")]

namespace FakeHttp
{
    static class ResponseFactory
    {
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

        public static HttpContent CreateContent(this ResponseInfo info, Stream stream)
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
