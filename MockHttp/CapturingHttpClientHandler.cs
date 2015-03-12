using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace MockHttp
{
    public class CapturingHttpClientHandler : HttpClientHandler
    {
        private readonly IResponseStore _store;

        public CapturingHttpClientHandler(IResponseStore store)
        {
            _store = store;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            await _store.StoreResponse(response);

            return response;
        }
    }
}