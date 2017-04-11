using System;
using System.Net.Http;

namespace FakeHttp
{
    /// <summary>
    /// A store that can serve responses from an <see cref="IReadOnlyResources"/> instance
    /// </summary>
    public class ReadOnlyResponseStore : IReadOnlyResponseStore
    {
        private readonly ResponseAdapter _responseAdapter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources"></param>
        public ReadOnlyResponseStore(IReadOnlyResources resources)
            :this(resources, new ResponseCallbacks())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="callbacks"></param>
        public ReadOnlyResponseStore(IReadOnlyResources resources, IResponseCallbacks callbacks)
        {
            _responseAdapter = new ResponseAdapter(resources, callbacks);
        }

        /// <summary>
        /// Used by derived classes to adapt <see cref="HttpResponseMessage"/>s to <see cref="IReadOnlyResources"/>
        /// </summary>
        protected internal ResponseAdapter Adapter => _responseAdapter;

        /// <summary>
        /// Determines if a <see cref="HttpResponseMessage"/> exists for the 
        /// <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <returns>True if a response exists for the request. Otherwise false</returns>
        public bool ResponseExists(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            return _responseAdapter.Exists(request);
        }

        /// <summary>
        /// Retrieve response message from storage based on a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response messsage</returns>
        public HttpResponseMessage FindResponse(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            return _responseAdapter.FindResponse(request);
        }
    }
}
