using System;
using System.IO;

namespace UnitTestHelpers
{
    public sealed class TempFolder : IDisposable
    {
        private readonly string _root;

        public TempFolder(string root)
        {
            // since we're about to recursively delete a folder lets make sure it's in the temp folder
            if (string.IsNullOrEmpty(root)) throw new InvalidOperationException("Cannot create folder with empty name");
            if (root.Contains("..")) throw new InvalidOperationException("root cannot contain relative portion");
            if (Path.IsPathRooted(root)) throw new InvalidOperationException("root cannot be an absolute path");

            _root = Path.Combine(Path.GetTempPath(), root);

            // make sure the target folder doesn't exist when we start
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, true);
            }

            Directory.CreateDirectory(_root);
        }

        public string RootPath { get { return _root; } }

        public void Dispose()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, true);
            }
        }
    }
}
