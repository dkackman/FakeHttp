using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace FakeHttp
{
    /// <summary>
    /// A <see cref="HttpMessageHandler"/> that retrieves http response messages from
    /// from local storage if they exist or if they do not, from the http endpoint and then stores them for future retrieval
    /// </summary>
    public sealed class AutomaticHttpClientHandler : HttpClientHandler
    {
        private readonly IResponseStore _store;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources"></param>
        public AutomaticHttpClientHandler(IResources resources)
            :this(new ResponseStore(resources))
        {

        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="store">The storage mechanism for responses</param>
        public AutomaticHttpClientHandler(IResponseStore store)
        {
            _store = store ?? throw new ArgumentNullException("store");
        }

        /// <summary>
        /// Override the base class to capture and store the response message
        /// if it doesn't already exist in storage
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The response message</returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // if the response exists in the store go get it from there
            if (_store.ResponseExists(request))
            {
                return _store.FindResponse(request);
            }

            // otherwise get it from the actual endpoint, store it and return it
            var response = await base.SendAsync(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            await _store.StoreResponse(response);

            return _store.FindResponse(request);
        }
    }
}
