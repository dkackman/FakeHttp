using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace MockHttp
{
    public class MockHttpClientHandler : HttpClientHandler
    {
        private readonly IResponseStore _store;

        public MockHttpClientHandler(IResponseStore store)
        {
            _store = store;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _store.FindResponse(request);
        }
    }
}
