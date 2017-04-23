using System;
using System.Linq;
using System.IO;
using System.IO.Compression;

namespace FakeHttp.Resources
{
    /// <summary>
    /// Resources stored in a <see cref="ZipArchive"/>
    /// </summary>
    public sealed class ZipResources : IReadOnlyResources, IDisposable
    {
        private readonly ZipArchive _archive;

        /// <summary>
        /// Opens the <see cref="ZipArchive"/> located at archiveFilePath.
        /// The archive will remain opened until <see cref="Dispose"/> is called
        /// </summary>
        /// <param name="archiveFilePath">full or relative path to the zip archive file</param>
        /// <exception cref="ArgumentException">If archiveFilePath is null or empty</exception>
        /// <exception cref="FileLoadException">If archiveFilePath is null or empty</exception>
        public ZipResources(string archiveFilePath)
        {
            if (string.IsNullOrEmpty(archiveFilePath)) throw new ArgumentException("archiveFilePath cannot be empty", "archiveFilePath");

            try
            {
                _archive = ZipFile.OpenRead(archiveFilePath);
            }
            catch (Exception e)
            {
                throw new FileLoadException(archiveFilePath + " could not be opened", e);
            }
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// Disposes the underlying <see cref="ZipArchive"/>
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
        /// <exception cref="FileLoadException"/>
        public string LoadAsString(string folder, string fileName)
        {
            using (var reader = new StreamReader(LoadFromEntry(folder, fileName)))
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
        /// <exception cref="FileLoadException"/>
        public Stream LoadAsStream(string folder, string fileName)
        {
            // since we are passing the stream out of our control
            // disconnect it from the lifetime of the ZipArchive
            using (var entry = LoadFromEntry(folder, fileName))
            {
                var memoryStream = new MemoryStream();
                entry.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
        }

        private Stream LoadFromEntry(string folder, string fileName)
        {
            var entry = GetEntry(folder, fileName) ?? throw new FileLoadException($"Archive entry {FullPath(folder, fileName)} not found - Check Exists first");

            try
            {
                return entry.Open();
            }
            catch (Exception e)
            {
                throw new FileLoadException(FullPath(folder, fileName) + " could not be opened", e);
            }
        }

        private ZipArchiveEntry GetEntry(string folder, string fileName)
        {
            // TODO - this might be a performance hotspot
            // we use this instead of ZipArchive.GetEntry in order to do case insensitve searching - because DNS is not case sensistive
            return _archive.Entries.Where(e => e.FullName.Equals(FullPath(folder, fileName), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        private static string FullPath(string folder, string fileName)
        {
            return Path.Combine(folder, fileName).Replace('\\', '/');
        }
    }
}
