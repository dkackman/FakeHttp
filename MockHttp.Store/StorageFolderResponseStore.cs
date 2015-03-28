using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Windows.Storage;

namespace MockHttp
{
    public sealed class StorageFolderResponseStore : IReadonlyResponseStore
    {
        private readonly IStorageFolder _folder;

        public StorageFolderResponseStore(IStorageFolder folder)
        {
            _folder = folder;
        }

        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            //_folder.GetFolderAsync(request.)
            return null;
        }
    }
}
