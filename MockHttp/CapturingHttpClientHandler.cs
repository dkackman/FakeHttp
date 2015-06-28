using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace MockHttp
{
    /// <summary>
    /// A <see cref="System.Net.Http.HttpMessageHandler"/> that retrieves http resonse messages from
    /// from teh http endpoint and then stores them for future retrieval
    /// </summary>
    public sealed class CapturingHttpClientHandler : HttpClientHandler
    {
        private readonly IResponseStore _store;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="store">The storage meachansim for responses</param>
        public CapturingHttpClientHandler(IResponseStore store)
        {
            _store = store;
        }

        /// <summary>
        /// Override the base class to capture and store the response message
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The stored response message</returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            await _store.StoreResponse(response);

            return response;
        }
    }
}