using System.IO;
using System.Threading.Tasks;

namespace FakeHttp.Desktop
{
    sealed class Resources : IResources
    {
        private readonly string _storeFolder;

        public Resources(string storeFolder)
        {
            _storeFolder = storeFolder;
        }

        public async Task<bool> Exists(string folder, string fileName)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(fileName)) return false;

            return await Task.Run<bool>(() =>
                {
                    return File.Exists(FullPath(folder, fileName));
                });
        }

        public async Task<string> LoadAsString(string folder, string fileName)
        {
            if (!await Exists(folder, fileName)) return null;

            using (var reader = new StreamReader(FullPath(folder, fileName)))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public async Task<Stream> LoadAsStream(string folder, string fileName)
        {
            if (!await Exists(folder, fileName)) return null;

            return await Task.Run(() =>
                 new FileStream(FullPath(folder, fileName), FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        private string FullPath(string folder, string fileName)
        {
            return Path.Combine(_storeFolder, Path.Combine(folder, fileName));
        }
    }
}
