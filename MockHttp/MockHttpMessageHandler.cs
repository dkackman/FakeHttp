using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace MockHttp
{
    public class MockHttpMessageHandler : HttpClientHandler
    {
        private readonly IResponseStore _store = new FileSystemResponseStore(@"D:\temp\MockResponses");

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _store.FindResponse(request);
        }
    }
}
