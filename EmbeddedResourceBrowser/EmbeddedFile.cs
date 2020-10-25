using System.IO;
using System.Reflection;

namespace EmbeddedResourceBrowser
{
    /// <summary>Represents an embedded file.</summary>
    public class EmbeddedFile
    {
        private readonly Assembly _assembly;
        private readonly string _resourceName;

        internal EmbeddedFile(EmbeddedDirectory parentDirectory, string name, Assembly assembly, string resourceName)
        {
            ParentDirectory = parentDirectory;
            Name = name;
            _assembly = assembly;
            _resourceName = resourceName;
        }

        /// <summary>Gets the <see cref="EmbeddedDirectory"/> to which this file belongs to.</summary>
        public EmbeddedDirectory ParentDirectory { get; }

        /// <summary>Gets the name of the embedded file.</summary>
        public string Name { get; }

        /// <summary>Gets a <see cref="Stream"/> for reading the contents of the embedded file.</summary>
        /// <returns>Returns a <see cref="Stream"/> that can be used for reading the contents of the embedded file.</returns>
        public Stream OpenRead()
            => _assembly.GetManifestResourceStream(_resourceName);
    }
}