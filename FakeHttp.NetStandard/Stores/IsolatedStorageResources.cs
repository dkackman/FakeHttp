using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace FakeHttp.Stores
{
    public sealed class IsolatedStorageResources : IResources
    {
        private IsolatedStorageFile _folder;

        public IsolatedStorageResources(IsolatedStorageFile storage)
        {        
            _folder = storage ?? throw new ArgumentNullException("storage");
        }

        public async Task<bool> Exists(string folder, string fileName)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(fileName)) return false;

            return await Task.Run<bool>(() =>
            {
                return _folder.FileExists(Path.Combine(folder, fileName));
            });
        }

        public async Task<string> LoadAsString(string folder, string fileName)
        {
            if (!await Exists(folder, fileName)) return null;

            using (var reader = new StreamReader(await LoadAsStream(folder, fileName)))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public async Task<Stream> LoadAsStream(string folder, string fileName)
        {
            if (!await Exists(folder, fileName)) return null;

            return await Task.Run(() =>
                _folder.OpenFile(Path.Combine(folder, fileName), FileMode.Open, FileAccess.Read, FileShare.Read)
            );
        }
    }
}
