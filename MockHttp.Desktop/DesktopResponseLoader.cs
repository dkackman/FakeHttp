using System.IO;
using System.Threading.Tasks;

namespace MockHttp.Desktop
{
    class DesktopResponseLoader : ResponseLoader
    {
        private readonly string _storeFolder;

        public DesktopResponseLoader(string storeFolder, MessageFormatter formatter)
            : base(formatter)
        {
            _storeFolder = storeFolder;

        }

        protected override async Task<bool> Exists(string folder, string fileName)
        {
            return await Task.Run<bool>(() =>
                {
                    return File.Exists(FullPath(folder, fileName));
                });
        }

        protected override async Task<string> LoadAsString(string folder, string fileName)
        {
            using (var reader = new StreamReader(FullPath(folder, fileName)))
            {
                return await reader.ReadToEndAsync();
            }
        }

        protected override async Task<Stream> LoadAsStream(string folder, string fileName)
        {
            return await Task.Run(() =>
                 new FileStream(FullPath(folder, fileName), FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        private string FullPath(string folder, string fileName)
        {
            return Path.Combine(_storeFolder, Path.Combine(folder, fileName));
        }
    }
}
