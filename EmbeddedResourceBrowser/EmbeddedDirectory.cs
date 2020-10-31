using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        /// <summary>Creates an <see cref="EmbeddedDirectory"/> containing the resources from all provided <paramref name="assemblies"/> using a merge strategy.</summary>
        /// <param name="assemblies">The <see cref="Assembly"/> objects from where to map embedded resources.</param>
        /// <returns>
        /// Returns an <see cref="EmbeddedDirectory"/> containing the merged resources from all provided <paramref name="assemblies"/>. If multiple resources have the
        /// same name, then latest one is picked (regardless of assembly name).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para>Thrown when the provided <paramref name="assemblies"/> are <c>null</c> or contains <c>null</c> values.</para>
        /// <para>Thrown when multiple assembles or files have the same case-insensitive name, but different case-sensitive names.</para>
        /// </exception>
        /// <example>
        /// Given two assemblies with the following embedded resources:
        /// <code>
        /// Assembly1.directory1.resource-1.txt
        /// Assembly1.directory1.resource-2.txt
        /// Assembly1.directory2.resource-3.txt
        /// 
        /// Assembly2.directory1.resource-2.txt
        /// Assembly2.directory1.resource-3.txt
        /// Assembly2.directory3.resource-3.txt
        /// </code>
        /// The result of this method will be an <see cref="EmbeddedDirectory"/> having the following structure:
        /// <code>
        /// /
        ///     directory1/
        ///         resource-1.txt (from Assembly1)
        ///         resource-2.txt (from Assembly2, matching resources)
        ///         resource-3.txt (from Assembly2)
        ///     directory2/
        ///         resource-3.txt (from Assembly1)
        ///     directory3/
        ///         resource-3.txt (from Assembly2)
        /// </code>
        /// The matching is done by ignoring the case of each directory and file.
        /// </example>
        public static EmbeddedDirectory Merge(IEnumerable<Assembly> assemblies)
        {
            if (assemblies is null)
                throw new ArgumentException("Cannot be null or contain null values.", nameof(assemblies));

            return new EmbeddedDirectory(
                assemblies
                    .SelectMany(assembly =>  assembly is null
                        ? throw new ArgumentException("Cannot be null or contain null values.", nameof(assemblies))
                        : assembly.GetManifestResourceNames().Select(resourceName => new ResourcePair(assembly, resourceName)))
                    .GroupBy(resourcePair => resourcePair.ResourceName, (resourceName, resourcePairs) => resourcePairs.Last(), StringComparer.OrdinalIgnoreCase)
            );
        }

        /// <summary>Creates an <see cref="EmbeddedDirectory"/> containing the resources from all provided <paramref name="assemblies"/> using a merge strategy.</summary>
        /// <param name="assemblies">The <see cref="Assembly"/> objects from where to map embedded resources.</param>
        /// <returns>
        /// Returns an <see cref="EmbeddedDirectory"/> containing the merged resources from all provided <paramref name="assemblies"/>. If multiple resources have the
        /// same name, then latest one is picked (regardless of assembly name).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para>Thrown when the provided <paramref name="assemblies"/> are <c>null</c> or contains <c>null</c> values.</para>
        /// <para>Thrown when multiple assembles or files have the same case-insensitive name, but different case-sensitive names.</para>
        /// </exception>
        /// <example>
        /// Given two assemblies with the following embedded resources:
        /// <code>
        /// Assembly1.directory1.resource-1.txt
        /// Assembly1.directory1.resource-2.txt
        /// Assembly1.directory2.resource-3.txt
        /// 
        /// Assembly2.directory1.resource-2.txt
        /// Assembly2.directory1.resource-3.txt
        /// Assembly2.directory3.resource-3.txt
        /// </code>
        /// The result of this method will be an <see cref="EmbeddedDirectory"/> having the following structure:
        /// <code>
        /// /
        ///     directory1/
        ///         resource-1.txt (from Assembly1)
        ///         resource-2.txt (from Assembly2, matching resources)
        ///         resource-3.txt (from Assembly2)
        ///     directory2/
        ///         resource-3.txt (from Assembly1)
        ///     directory3/
        ///         resource-3.txt (from Assembly2)
        /// </code>
        /// The matching is done by ignoring the case of each directory and file.
        /// </example>
        public static EmbeddedDirectory Merge(params Assembly[] assemblies)
            => Merge((IEnumerable<Assembly>)assemblies);

        /// <summary>Initializes a new instance of the <see cref="EmbeddedDirectory"/> class.</summary>
        /// <param name="assembly">The <see cref="Assembly"/> from where to map embedded resources.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="assembly"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when multiple files have the same case-insensitive name, but different case-sensitive names.</exception>
        public EmbeddedDirectory(Assembly assembly)
            : this(null, string.Empty, assembly ?? throw new ArgumentNullException(nameof(assembly)))
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
                        : new EmbeddedDirectory(this, assembly.GetName().Name, assembly)
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

        private EmbeddedDirectory(EmbeddedDirectory parentDirectory, string name, Assembly assembly)
            : this(assembly.GetManifestResourceNames().Select(resourceFullName => new ResourcePair(assembly, resourceFullName)))
        {
            ParentDirectory = parentDirectory;
            Name = name;
        }

        private EmbeddedDirectory(IEnumerable<ResourcePair> resourcePairs)
            : this(null, string.Empty, resourcePairs, 0)
        {
        }

        private EmbeddedDirectory(EmbeddedDirectory parentDirectory, string name, IEnumerable<ResourcePair> resourcePairs, int skipCount)
        {
            ParentDirectory = parentDirectory;
            Name = name;
            Subdirectories = new NamedReadOnlyList<EmbeddedDirectory>(
                resourcePairs
                    .Where(resourcePair => resourcePair.DirectoryPath.Skip(skipCount).Any())
                    .GroupBy(
                        resourcePair => resourcePair.DirectoryPath.Skip(skipCount).First(),
                        (subdirectoryName, subdirectoryFilePairs) => new EmbeddedDirectory(this, subdirectoryName, subdirectoryFilePairs, skipCount + 1),
                        StringComparer.OrdinalIgnoreCase
                    ),
                direcotry => direcotry.Name
            );
            Files = new NamedReadOnlyList<EmbeddedFile>(
                resourcePairs
                    .Where(resourcePair => !resourcePair.DirectoryPath.Skip(skipCount).Any())
                    .Select(resourcePair => resourcePair.ToEmbeddedFile(this)),
                file => file.Name
            );
        }

        /// <summary>The name of the embedded directory.</summary>
        public string Name { get; }

        /// <summary>Gets the parent <see cref="EmbeddedDirectory"/>, the root directory has a <c>null</c> parent directory.</summary>
        public EmbeddedDirectory ParentDirectory { get; }

        /// <summary>Gets the list of the embedded subdirectories.</summary>
        public NamedReadOnlyList<EmbeddedDirectory> Subdirectories { get; }

        /// <summary>Gets the list of the embedded files.</summary>
        public NamedReadOnlyList<EmbeddedFile> Files { get; }

        /// <summary>Gets all subdirectires, from all levels, from the current embedded directory.</summary>
        /// <returns>Returns a collection containing all subdirectories from the current embedded directory tree.</returns>
        public IEnumerable<EmbeddedDirectory> GetAllSubdirectories()
        {
            var subdirectoriesToVisit = new Stack<EmbeddedDirectory>();
            for (var subdirectoryIndex = Subdirectories.Count - 1; subdirectoryIndex >= 0; subdirectoryIndex--)
                subdirectoriesToVisit.Push(Subdirectories[subdirectoryIndex]);

            while (subdirectoriesToVisit.Count > 0)
            {
                var currentSubdirectory = subdirectoriesToVisit.Pop();
                yield return currentSubdirectory;

                for (var subdirectoryIndex = currentSubdirectory.Subdirectories.Count - 1; subdirectoryIndex >= 0; subdirectoryIndex--)
                    subdirectoriesToVisit.Push(currentSubdirectory.Subdirectories[subdirectoryIndex]);
            }
        }

        /// <summary>Gets all files from the current embedded directory and all embedded subdirectories, from all levels.</summary>
        /// <returns>Returns a collection containing all files in the current embedded directory tree.</returns>
        public IEnumerable<EmbeddedFile> GetAllFiles()
            => Files.Concat(GetAllSubdirectories().SelectMany(directory => directory.Files));

        private struct ResourcePair
        {
            private readonly Assembly _assembly;
            private readonly string _resourceFullName;

            public ResourcePair(Assembly assembly, string resourceFullName)
            {
                _assembly = assembly;
                _resourceFullName = resourceFullName;
                ResourceName = resourceFullName.Substring(assembly.GetName().Name.Length + 1);
                DirectoryPath = ResourceName.Split(new[] { '.' }).Take(ResourceName.Count(@char => @char == '.') - 1).ToArray();
            }

            public string ResourceName { get; }

            public IReadOnlyList<string> DirectoryPath { get; }

            public EmbeddedFile ToEmbeddedFile(EmbeddedDirectory parentDirectory)
                => new EmbeddedFile(parentDirectory, ResourceName.Substring(_GetFileNameSeparatorIndex(ResourceName) + 1), _assembly, _resourceFullName);

            private static int _GetFileNameSeparatorIndex(string resourceName)
                => resourceName.LastIndexOf('.', resourceName.LastIndexOf('.') - 1);
        }
    }
}