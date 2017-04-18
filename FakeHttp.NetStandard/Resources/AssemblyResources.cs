using System;
using System.IO;
using System.Reflection;
using System.Linq;

namespace FakeHttp.Resources
{
    /// <summary>
    /// Resources that reside in IsolatedStorage (used for store apps
    /// </summary>
    public sealed class AssemblyResources : IReadOnlyResources
    {
        private readonly Assembly _assembly;

        public AssemblyResources(Assembly assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException("assembly");
        }

        public bool Exists(string folder, string fileName)
        {
            var resource = ResourceName(folder, fileName);

            return _assembly.GetManifestResourceNames().Where(s => s == resource).Any();
        }

        private string ResourceName(string folder, string fileName)
        {
            // '.' is the separator for an embedded resource folder structure
            var name = _assembly.GetName().Name + "." + Path.Combine(folder, fileName).Replace('\\', '.');
            return name.Replace('-', '_'); // resource names replace dashes with underbars
        }

        public Stream LoadAsStream(string folder, string fileName)
        {
            var resource = ResourceName(folder, fileName);
            return _assembly.GetManifestResourceStream(resource);
        }

        public string LoadAsString(string folder, string fileName)
        {
            using (var reader = new StreamReader(LoadAsStream(folder, fileName)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
