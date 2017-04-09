using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace FakeHttp.Stores
{
    public class ReadonlyResponseStore : IReadonlyResponseStore
    {
        private readonly ResponseAdapter _responseAdapter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources"></param>
        public ReadonlyResponseStore(IResources resources)
            :this(resources, new ResponseCallbacks())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="callbacks"></param>
        public ReadonlyResponseStore(IResources resources, IResponseCallbacks callbacks)
        {
            _responseAdapter = new ResponseAdapter(resources, callbacks);
        }

        protected internal ResponseAdapter Adapter => _responseAdapter;

        /// <summary>
        /// Determines if a <see cref="HttpResponseMessage"/> exists for the 
        /// <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <returns>True if a response exists for the request. Otherwise false</returns>
        public async Task<bool> ResponseExists(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            return await _responseAdapter.Exists(request);
        }

        /// <summary>
        /// Retrieve response message from storage based on a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response messsage</returns>
        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            return await _responseAdapter.FindResponse(request);
        }
    }
}
