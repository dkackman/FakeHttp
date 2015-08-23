using System;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage.Streams;
using Windows.Storage;

namespace FakeHttp.Store
{
    class StoreResponseLoader : ResponseLoader
    {
        private IStorageFolder _folder;

        public StoreResponseLoader(IStorageFolder folder, MessageFormatter formatter)
            : base(formatter)
        {
            _folder = folder;
        }

        protected override async Task<bool> Exists(string folder, string fileName)
        {
            try
            {
                var subFolder = await _folder.GetFolderAsync(folder);
                return await subFolder.GetFileAsync(fileName) != null;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        protected override async Task<string> LoadAsString(string folder, string fileName)
        {
            var subFolder = await _folder.GetFolderAsync(folder);
            var file = await subFolder.GetFileAsync(fileName);

            using (var stream = await file.OpenStreamForReadAsync())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        protected override async Task<Stream> LoadAsStream(string folder, string fileName)
        {
            var subFolder = await _folder.GetFolderAsync(folder);
            var file = await subFolder.GetFileAsync(fileName);

            var fileStream = await file.OpenReadAsync();
            return fileStream.AsStreamForRead();
        }
    }
}
