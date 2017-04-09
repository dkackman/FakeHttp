using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace FakeHttp.Stores
{
    public sealed class ZipResources : IResources, IDisposable
    {
        private readonly ZipArchive _archive;

        public ZipResources(string archiveFilePath)
        {
            if (string.IsNullOrEmpty(archiveFilePath)) throw new ArgumentException("storeFolder cannot be empty", "storeFolder");

            _archive = ZipFile.OpenRead(archiveFilePath);
        }

        public void Dispose()
        {
            _archive.Dispose();
        }

        public async Task<bool> Exists(string folder, string fileName)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(fileName)) return false;

            return await Task.Run<bool>(() =>
                {
                    return GetEntry(folder, fileName) != null;
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

            return await Task.Run(() => GetEntry(folder, fileName).Open());
        }


        private ZipArchiveEntry GetEntry(string folder, string fileName)
        {
            // we use this instead of ZipArchive.GetEntry in order to do case insensitve searching
            return _archive.Entries.Where(e => e.FullName.Equals(FullPath(folder, fileName), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        private static string FullPath(string folder, string fileName)
        {
            return Path.Combine(folder, fileName).Replace("\\", "/");
        }
    }
}
