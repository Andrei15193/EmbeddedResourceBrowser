using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EmbeddedResourceBrowser
{
    /// <summary>Represents a directory of embedded resources.</summary>
    /// <remarks>
    /// The embedded resources are mapped using case-insensitive matching, if multiple assemblies or files (in the same directory) only differ
    /// in casing (e.g.: <c>TEST FILE.txt</c> is the same as <c>test file.txt</c>) then an <see cref="ArgumentException"/> is thrown.
    /// </remarks>
    public class EmbeddedDirectory
    {
        private static readonly NamedReadOnlyList<EmbeddedFile> _emptyEmbeddedFilesList = new NamedReadOnlyList<EmbeddedFile>(Enumerable.Empty<EmbeddedFile>(), file => file.Name);

        /// <summary>Initializes a new instance of the <see cref="EmbeddedDirectory"/> class.</summary>
        /// <param name="assembly">The <see cref="Assembly"/> from where to map embedded resources.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="assembly"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when multiple files have the same case-insensitive name, but different case-sensitive names.</exception>
        public EmbeddedDirectory(Assembly assembly)
            : this(null, null, assembly is null ? throw new ArgumentNullException(nameof(assembly)) : assembly.GetName().Name, assembly)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EmbeddedDirectory"/> class.</summary>
        /// <param name="assemblies">The <see cref="Assembly"/> objects from where to map embedded resources.</param>
        /// <exception cref="ArgumentException">
        /// <para>Thrown when the provided <paramref name="assemblies"/> are <c>null</c> or contains <c>null</c> values.</para>
        /// <para>Thrown when multiple assembles or files have the same case-insensitive name, but different case-sensitive names.</para>
        /// </exception>
        public EmbeddedDirectory(IEnumerable<Assembly> assemblies)
        {
            if (assemblies is null)
                throw new ArgumentException("Cannot be null or contain null values.", nameof(assemblies));

            Name = string.Empty;
            ParentDirectory = null;
            Subdirectories = new NamedReadOnlyList<EmbeddedDirectory>(
                assemblies
                    .Select(assembly => assembly is null
                        ? throw new ArgumentException("Cannot be null or contain null values.", nameof(assemblies))
                        : new EmbeddedDirectory(this, null, assembly.GetName().Name, assembly)
                    ),
                directory => directory.Name
            );
            Files = _emptyEmbeddedFilesList;
        }

        /// <summary>Initializes a new instance of the <see cref="EmbeddedDirectory"/> class.</summary>
        /// <param name="assemblies">The <see cref="Assembly"/> objects from where to map embedded resources.</param>
        /// <exception cref="ArgumentException">
        /// <para>Thrown when the provided <paramref name="assemblies"/> are <c>null</c> or contains <c>null</c> values.</para>
        /// <para>Thrown when multiple assembles or files have the same case-insensitive name, but different case-sensitive names.</para>
        /// </exception>
        public EmbeddedDirectory(params Assembly[] assemblies)
            : this((IEnumerable<Assembly>)assemblies)
        {
        }

        internal EmbeddedDirectory(EmbeddedDirectory parentDirectory, string prefix, string name, Assembly assembly)
        {
            Name = name;
            ParentDirectory = parentDirectory;
            Subdirectories = _GetSubdirectories(this, prefix, name, assembly);
            Files = _GetFiles(this, prefix, name, assembly);
        }

        private static NamedReadOnlyList<EmbeddedDirectory> _GetSubdirectories(EmbeddedDirectory parentDirectory, string prefix, string name, Assembly assembly)
        {
            var subdirectoryRegex = new Regex($@"^{prefix}{name}\.(?<subdirectoryName>[^\.]+)(\.[^\.]+){{2,}}$", RegexOptions.IgnoreCase);
            var subdirectoriesPrefix = $"{prefix}{name}.";
            return new NamedReadOnlyList<EmbeddedDirectory>(
                assembly
                    .GetManifestResourceNames()
                    .Select(new Func<string, Match>(subdirectoryRegex.Match))
                    .Where(match => match.Success)
                    .GroupBy(
                        match => match.Groups["subdirectoryName"].Value,
                        (subdirectoryName, matches) => new EmbeddedDirectory(parentDirectory, subdirectoriesPrefix, subdirectoryName, assembly),
                        StringComparer.OrdinalIgnoreCase
                    ),
                directory => directory.Name
            );
        }

        private static NamedReadOnlyList<EmbeddedFile> _GetFiles(EmbeddedDirectory parentDirectory, string prefix, string name, Assembly assembly)
        {
            var filesRegex = new Regex($@"^{prefix}{name}\.(?<fileName>[^\.]+\.[^\.]+)$", RegexOptions.IgnoreCase);
            return new NamedReadOnlyList<EmbeddedFile>(
                assembly
                    .GetManifestResourceNames()
                    .Select(new Func<string, Match>(filesRegex.Match))
                    .Where(match => match.Success)
                    .Select(match => new EmbeddedFile(parentDirectory, match.Groups["fileName"].Value, assembly, match.Value)),
                file => file.Name
            );
        }

        /// <summary>The name of the embedded directory, the root directory has an empty string (<see cref="string.Empty"/>) as its name.</summary>
        public string Name { get; }

        /// <summary>Gets the parent <see cref="EmbeddedDirectory"/>, the root directory has a <c>null</c> parent directory.</summary>
        public EmbeddedDirectory ParentDirectory { get; }

        /// <summary>Gets the list of embedded subdirectories.</summary>
        public NamedReadOnlyList<EmbeddedDirectory> Subdirectories { get; }

        /// <summary>Gets the list of embedded files.</summary>
        public NamedReadOnlyList<EmbeddedFile> Files { get; }
    }
}