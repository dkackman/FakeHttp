using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace FakeHttp
{
    /// <summary>
    /// A <see cref="HttpMessageHandler"/> that retrieves http response messages from
    /// from the http endpoint and then stores them for future retrieval
    /// </summary>
    public sealed class CapturingHttpClientHandler : HttpClientHandler
    {
        private readonly IResponseStore _store;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources">An object that can access stored response</param>
        /// <exception cref="ArgumentNullException"/>
        public CapturingHttpClientHandler(IResources resources)
            : this(new ResponseStore(resources))
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="store">The storage mechanism for responses</param>
        /// <exception cref="ArgumentNullException"/>
        public CapturingHttpClientHandler(IResponseStore store)
        {
            _store = store ?? throw new ArgumentNullException("store");
        }

        /// <summary>
        /// Override the base class to capture and store the response message
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to capture and store</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The stored response message</returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            await _store.StoreResponse(response);

            return _store.FindResponse(request);
        }
    }
}