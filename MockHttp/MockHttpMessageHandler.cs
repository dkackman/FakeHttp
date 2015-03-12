//#define CAPTURE_RESPONSE

using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace MockHttp
{
    public class MockHttpMessageHandler : HttpClientHandler
    {
        private readonly IResponseStore _store;

        public MockHttpMessageHandler(IResponseStore store)
        {
            _store = store;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
#if CAPTURE_RESPONSE
            var response = await base.SendAsync(request, cancellationToken);
            await _store.StoreResponse(response);

            return response;
#else
            cancellationToken.ThrowIfCancellationRequested();

            return await _store.FindResponse(request);
#endif
        }
    }
}
