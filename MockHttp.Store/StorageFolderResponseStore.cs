using System;
using System.Net.Http;
using System.Threading.Tasks;

using Windows.Storage;

using MockHttp.Store;

namespace MockHttp
{
    /// <summary>
    /// Class that can retreive stored response messages in a windows store app runtime environment
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
            : this(storeFolder, (name, value) => false)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">Root folder where message are kept</param>
        /// <param name="paramFilter">call back used to determine if a given query paramters should be excluded from serialziation</param>
        public StorageFolderResponseStore(IStorageFolder storeFolder, Func<string, string, bool> paramFilter)
        {
            _formatter = new StoreMessageFormatter(paramFilter);
            _responseLoader = new StoreResponseLoader(storeFolder, _formatter);
        }

        /// <summary>
        /// Retreive response message from storage based on the a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response messsage</returns>
        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            return await _responseLoader.FindResponse(request);
        }
    }
}
