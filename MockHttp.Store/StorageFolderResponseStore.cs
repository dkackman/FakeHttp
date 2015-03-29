using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Windows.Storage;

using MockHttp.Store;

namespace MockHttp
{
    public sealed class StorageFolderResponseStore : IReadonlyResponseStore
    {
        private readonly StoreRequestFormatter _formatter = new StoreRequestFormatter();
        private readonly ResponseDeserializer _deserializer = new ResponseDeserializer();
        private readonly IStorageFolder _storeFolder;

        private readonly Func<string, string, bool> _paramFilter;

        public StorageFolderResponseStore(IStorageFolder storeFolder)
            : this(storeFolder, (name, value) => false)
        {
        }

        public StorageFolderResponseStore(IStorageFolder storeFolder, Func<string, string, bool> paramFilter)
        {
            _storeFolder = storeFolder;
            _paramFilter = paramFilter;
        }

        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            var query = _formatter.NormalizeQuery(request.RequestUri, _paramFilter);
            var folder = await _storeFolder.GetFolderAsync(_formatter.ToFilePath(request.RequestUri));

            // first try to find a file keyed to the request method and query
            return await _deserializer.DeserializeResponse(folder, _formatter.ToFileName(request, query))
                // next just look for a default response based on just the http method
                ?? await _deserializer.DeserializeResponse(folder, _formatter.ToShortFileName(request))
                // otherwise return 404            
                ?? new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };
        }
    }
}
