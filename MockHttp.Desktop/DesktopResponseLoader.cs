using System.IO;
using System.Threading.Tasks;

namespace MockHttp.Desktop
{
    class DesktopResponseLoader : ResponseLoader
    {
        protected override async Task<bool> Exists(string folder, string fileName)
        {
            return await Task.Run<bool>(() =>
                {
                    var path = Path.Combine(folder, fileName);
                    return File.Exists(path);
                });
        }

        protected override async Task<string> Load(string folder, string fileName)
        {
            var path = Path.Combine(folder, fileName);
            using (var reader = new StreamReader(path))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
