using System;
using System.Linq;
using System.IO;
using System.IO.Compression;

namespace FakeHttp.Resources
{
    /// <summary>
    /// Resources stored ina <see cref="ZipArchive"/>
    /// </summary>
    public sealed class ZipResources : IReadOnlyResources, IDisposable
    {
        private readonly ZipArchive _archive;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="archiveFilePath">full or relative path to the zip archive file</param>
        public ZipResources(string archiveFilePath)
        {
            if (string.IsNullOrEmpty(archiveFilePath)) throw new ArgumentException("archiveFilePath cannot be empty", "archiveFilePath");

            _archive = ZipFile.OpenRead(archiveFilePath);
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            _archive.Dispose();
        }

        /// <summary>
        /// Checks whether the specified file exists
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>Flag indicating whether file exists</returns>
        public bool Exists(string folder, string fileName)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(fileName)) return false;

            return GetEntry(folder, fileName) != null;
        }

        /// <summary>
        /// Loads a given file as a string
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>The file's contents as a string</returns>
        public string LoadAsString(string folder, string fileName)
        {
            var stream = LoadAsStream(folder, fileName) ?? throw new InvalidOperationException($"Archive entry {folder}, {fileName} not found - Check Exists first");
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Loads a given file as a stream
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>File's contents as a stream</returns>
        public Stream LoadAsStream(string folder, string fileName)
        {
            var entry = GetEntry(folder, fileName) ?? throw new InvalidOperationException($"Archive entry {folder}, {fileName} not found - Check Exists first");

            return entry.Open();
        }

        private ZipArchiveEntry GetEntry(string folder, string fileName)
        {
            // TODO - this might be a performance hotspot
            // we use this instead of ZipArchive.GetEntry in order to do case insensitve searching - because DNS is not case sensistive
            return _archive.Entries.Where(e => e.FullName.Equals(FullPath(folder, fileName), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        private static string FullPath(string folder, string fileName)
        {
            return Path.Combine(folder, fileName).Replace("\\", "/");
        }
    }
}
