using System;
using System.Net;
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
        private readonly StoreRequestFormatter _formatter;
        private readonly StoreResponseLoader _deserializer;
        private readonly IStorageFolder _storeFolder;

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
            _storeFolder = storeFolder;
            _formatter = new StoreRequestFormatter(paramFilter);
            _deserializer = new StoreResponseLoader(_storeFolder, _formatter);
        }

        /// <summary>
        /// Retreive response message from storage based on the a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response messsage</returns>
        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            var query = _formatter.NormalizeQuery(request.RequestUri);
            var folder = _formatter.ToFilePath(request.RequestUri);

            // first try to find a file keyed to the request method and query
            return await _deserializer.DeserializeResponse(folder, _formatter.ToFileName(request, query))
                // next just look for a default response based on just the http method
                ?? await _deserializer.DeserializeResponse(folder, _formatter.ToShortFileName(request))
                // otherwise return 404            
                ?? new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };
        }
    }
}
