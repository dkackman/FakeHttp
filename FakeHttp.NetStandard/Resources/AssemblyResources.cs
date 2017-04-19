using System;
using System.IO;
using System.Reflection;
using System.Linq;

namespace FakeHttp.Resources
{
    /// <summary>
    /// Resources that reside as embedded resources in a <see cref="Assembly"/> 
    /// </summary>
    public sealed class AssemblyResources : IReadOnlyResources
    {
        private readonly Assembly _assembly;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> with response files stored as embedded resources</param>
        /// <exception cref="ArgumentNullException"/>
        public AssemblyResources(Assembly assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException("assembly");
        }

        /// <summary>
        /// Checks whether the specified file exists
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>Flag indicating whether file exists</returns>
        public bool Exists(string folder, string fileName)
        {
            var resource = ResourceName(folder, fileName);

            return _assembly.GetManifestResourceNames().Where(s => s == resource).Any();
        }

        /// <summary>
        /// Loads a given file as a stream
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>File's contents as a stream</returns>
        public Stream LoadAsStream(string folder, string fileName)
        {
            var resource = ResourceName(folder, fileName);
            return _assembly.GetManifestResourceStream(resource);
        }

        /// <summary>
        /// Loads a given file as a string
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>The file's contents as a string</returns>
        public string LoadAsString(string folder, string fileName)
        {
            using (var reader = new StreamReader(LoadAsStream(folder, fileName)))
            {
                return reader.ReadToEnd();
            }
        }

        private string ResourceName(string folder, string fileName)
        {
            // '.' is the separator for an embedded resource folder structure
            var name = _assembly.GetName().Name + "." + Path.Combine(folder, fileName).Replace('\\', '.');
            return name.Replace('-', '_'); // resource names replace dashes with underbars
        }

    }
}
