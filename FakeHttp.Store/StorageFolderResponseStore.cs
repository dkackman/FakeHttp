using System;
using System.Net.Http;
using System.Threading.Tasks;

using Windows.Storage;

using FakeHttp.Store;

namespace FakeHttp
{
    /// <summary>
    /// Class that can retrieve stored response messages in a windows store app runtime environment
    /// </summary>
    public sealed class StorageFolderResponseStore : IReadonlyResponseStore
    {
        private readonly MessageFormatter _formatter;
        private readonly ResponseLoader _responseLoader;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">Root folder where message are kept</param>
        public StorageFolderResponseStore(IStorageFolder storeFolder)
            : this(storeFolder, new ResponseCallbacks())
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">Root folder where messages are kept</param>
        /// <param name="paramFilter">Call back used to control serialization</param>
        public StorageFolderResponseStore(IStorageFolder storeFolder, IResponseCallbacks callbacks)
        {
            _formatter = new StoreMessageFormatter(callbacks);
            _responseLoader = new StoreResponseLoader(storeFolder, _formatter);
        }

        /// <summary>
        /// Determines if a <see cref="HttpResponseMessage"/> exists for the 
        /// <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <returns>True if a response exists for the request. Otherwise false</returns>
        public async Task<bool> ResponseExists(HttpRequestMessage request)
        {
            return await _responseLoader.Exists(request);
        }

        /// <summary>
        /// Retrieve response message from storage based on a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response messsage</returns>
        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            return await _responseLoader.FindResponse(request);
        }
    }
}
