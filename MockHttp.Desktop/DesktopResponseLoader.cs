using System.IO;
using System.Threading.Tasks;

namespace MockHttp.Desktop
{
    class DesktopResponseLoader : ResponseLoader
    {
        public DesktopResponseLoader(MessageFormatter formatter)
            : base(formatter)
        {
        }

        protected override async Task<bool> Exists(string folder, string fileName)
        {
            return await Task.Run<bool>(() =>
                {
                    var path = Path.Combine(folder, fileName);
                    return File.Exists(path);
                });
        }

        protected override async Task<string> LoadAsString(string folder, string fileName)
        {
            var path = Path.Combine(folder, fileName);
            using (var reader = new StreamReader(path))
            {
                return await reader.ReadToEndAsync();
            }
        }

        protected override async Task<Stream> LoadAsStream(string folder, string fileName)
        {
            return await Task.Run(() =>
                 new FileStream(Path.Combine(folder, fileName), FileMode.Open, FileAccess.Read, FileShare.Read));
        }
    }
}
