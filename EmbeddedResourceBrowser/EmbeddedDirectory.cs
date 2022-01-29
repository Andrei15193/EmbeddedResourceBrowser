using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EmbeddedResourceBrowser
{
    /// <summary>Represents a directory of embedded resources.</summary>
    /// <remarks>
    /// <para>
    /// The embedded resources are mapped using case-insensitive matching, if multiple assemblies or files (in the same directory) only differ
    /// in casing (e.g.: <c>TEST FILE.txt</c> is the same as <c>test file.txt</c>) then an <see cref="ArgumentException"/> is thrown.
    /// </para>
    /// <para>
    /// By default, embedded resources use the dot (<c>.</c>) as a separator for directory paths and the assembly name is placed at the beginning of the name.
    /// <c>&lt;EmbeddedResource Include="MyDirectory/MySubdirectory/MyFile.ext" /&gt;</c> is mapped to
    /// <c>MyAssemblyName.MyDirectory.MySubdirectory.MyFile.ext</c>. When processing files, it is expected that files have extensions, thus the last dot
    /// (<c>.</c>) is ignored from the file path. When processing files, the assembly name is expected to be at the beginning of an embedded resource and the
    /// character immediately afterwards is used to determine the path separator. This will make the resource be mapped to the following structure
    /// <c>MyDirectory/MySubdirectory/MyFile.ext</c>.
    /// </para>
    /// <para>
    /// To embed the file under a different name use the <c>LogicalName</c> attribute (or specify it as a child element), this allows the use of different
    /// path separators which can be useful if the embedded resources have dots (<c>.</c>) in their name (e.g.: <c>MyFileWithVersion@1.2.3.txt</c>) or when
    /// files or directories start with a number (e.g.: <c>MyDirectory/1.2.3/File.txt</c>). By default, the file will be embedded under
    /// <c>MyAssemblyName.MyDirectory._1._2._3.File.txt</c>.
    /// </para>
    /// <para>
    /// When specifying the <c>LogicalName</c> make sure to specify the assembly name followed by the path separator otherwise the library will not pick
    /// up these files. The library is made so that it works with the default naming structure.
    /// </para>
    /// <para>
    /// Specifying a different path separator can be done individually on each file.
    /// </para>
    /// <code lang="xml">
    /// &lt;!-- This uses '/' as the path separator --&gt;
    /// &lt;EmbeddedResource Include="MyDirectory/MySubdirectory/MyFile.ext" LogicalName="MyAssemblyName/MyDirectory/MySubdirectory/MyFile.ext" /&gt;
    /// 
    /// &lt;!-- This uses '!' as the path separator --&gt;
    /// &lt;EmbeddedResource Include="MyDirectory/MySubdirectory/MyFile.ext" LogicalName="MyAssemblyName!MyDirectory!MySubdirectory!MyFile.ext" /&gt;
    /// </code>
    /// <para>
    /// The path separator is determined per file, there is no global setting for path separators allowing for mixed settings. An alternative would be to
    /// specify all the embedded files and afterwards update all the matching files. The following element will update all embedded resources that have
    /// been specified up to that element in the <c>.csproj</c> (or any other project file type) to have the assembly name at the beginning and normalize
    /// the paths to use forward slash as a separator. This will cause all included embedded resources to that point to have the same structure as in the
    /// file system. See <a href="https://docs.microsoft.com/visualstudio/msbuild/msbuild-well-known-item-metadata">MSBuild well-known item metadata</a>
    /// and <a href="https://docs.microsoft.com/visualstudio/msbuild/common-msbuild-project-properties">Common MSBuild project properties</a> for more
    /// information, they work with <c>dotnet build</c> as well.
    /// </para>
    /// <code lang="xml">
    /// &lt;EmbeddedResource Update="**/*" LogicalName="$(AssemblyName)/$([System.String]::Copy('%(Identity)').Replace('\', '/'))" /&gt;
    /// </code>
    /// </remarks>
    public class EmbeddedDirectory
    {
        /// <summary>The default char separator used for directory paths.</summary>
        public const char DefaultDirectoryPathSeparator = '.';

        private static readonly NamedReadOnlyList<EmbeddedFile> _emptyEmbeddedFilesList = new NamedReadOnlyList<EmbeddedFile>(Enumerable.Empty<EmbeddedFile>(), file => file.Name);

        /// <summary>Creates an <see cref="EmbeddedDirectory"/> containing the resources from all provided <paramref name="assemblies"/> using a merge strategy.</summary>
        /// <param name="assemblies">The <see cref="Assembly"/> objects from where to map embedded resources.</param>
        /// <param name="directoryPathSeparator">The character separator to use for directory paths, the default is <see cref="DefaultDirectoryPathSeparator"/> (<c>.</c>).</param>
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
        public static EmbeddedDirectory Merge(IEnumerable<Assembly> assemblies, char directoryPathSeparator)
        {
            if (assemblies is null)
                throw new ArgumentException("Cannot be null or contain null values.", nameof(assemblies));

            return new EmbeddedDirectory(
                assemblies
                    .SelectMany(assembly => assembly is null
                        ? throw new ArgumentException("Cannot be null or contain null values.", nameof(assemblies))
                        : assembly
                            .GetManifestResourceNames()
                            .Where(resourceName => resourceName.Length > assembly.GetName().Name.Length + 2 && resourceName.StartsWith(assembly.GetName().Name, StringComparison.OrdinalIgnoreCase))
                            .Select(resourceName => new ResourcePair(assembly, resourceName, directoryPathSeparator)))
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
        public static EmbeddedDirectory Merge(IEnumerable<Assembly> assemblies)
        {
            if (assemblies is null)
                throw new ArgumentException("Cannot be null or contain null values.", nameof(assemblies));

            return new EmbeddedDirectory(
                assemblies
                    .SelectMany(assembly => assembly is null
                        ? throw new ArgumentException("Cannot be null or contain null values.", nameof(assemblies))
                        : assembly
                            .GetManifestResourceNames()
                            .Where(resourceName => resourceName.StartsWith(assembly.GetName().Name, StringComparison.OrdinalIgnoreCase))
                            .Select(resourceName => new ResourcePair(assembly, resourceName)))
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
        /// <param name="directoryPathSeparator">The character separator to use for directory paths, the default is <see cref="DefaultDirectoryPathSeparator"/> (<c>.</c>).</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="assembly"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when multiple files have the same case-insensitive name, but different case-sensitive names.</exception>
        public EmbeddedDirectory(Assembly assembly, char directoryPathSeparator)
            : this(null, assembly is null ? throw new ArgumentNullException(nameof(assembly)) : assembly.GetName().Name, assembly, directoryPathSeparator)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EmbeddedDirectory"/> class.</summary>
        /// <param name="assembly">The <see cref="Assembly"/> from where to map embedded resources.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="assembly"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when multiple files have the same case-insensitive name, but different case-sensitive names.</exception>
        public EmbeddedDirectory(Assembly assembly)
            : this(null, assembly is null ? throw new ArgumentNullException(nameof(assembly)) : assembly.GetName().Name, assembly)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EmbeddedDirectory"/> class.</summary>
        /// <param name="assemblies">The <see cref="Assembly"/> objects from where to map embedded resources.</param>
        /// <param name="directoryPathSeparator">The character separator to use for directory paths, the default is <see cref="DefaultDirectoryPathSeparator"/> (<c>.</c>).</param>
        /// <exception cref="ArgumentException">
        /// <para>Thrown when the provided <paramref name="assemblies"/> are <c>null</c> or contains <c>null</c> values.</para>
        /// <para>Thrown when multiple assembles or files have the same case-insensitive name, but different case-sensitive names.</para>
        /// </exception>
        public EmbeddedDirectory(IEnumerable<Assembly> assemblies, char directoryPathSeparator)
        {
            if (assemblies is null)
                throw new ArgumentException("Cannot be null or contain null values.", nameof(assemblies));

            Name = string.Empty;
            ParentDirectory = null;
            Subdirectories = new NamedReadOnlyList<EmbeddedDirectory>(
                assemblies
                    .Select(assembly => assembly is null
                        ? throw new ArgumentException("Cannot be null or contain null values.", nameof(assemblies))
                        : new EmbeddedDirectory(this, assembly.GetName().Name, assembly, directoryPathSeparator)
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
            : this(assembly.GetManifestResourceNames().Where(resourceName => resourceName.StartsWith(assembly.GetName().Name, StringComparison.OrdinalIgnoreCase)).Select(resourceFullName => new ResourcePair(assembly, resourceFullName)))
        {
            ParentDirectory = parentDirectory;
            Name = name;
        }

        private EmbeddedDirectory(EmbeddedDirectory parentDirectory, string name, Assembly assembly, char directoryPathSeparator)
            : this(assembly.GetManifestResourceNames().Where(resourceName => resourceName.StartsWith(assembly.GetName().Name, StringComparison.OrdinalIgnoreCase)).Select(resourceFullName => new ResourcePair(assembly, resourceFullName, directoryPathSeparator)))
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
            private readonly string _fileName;

            public ResourcePair(Assembly assembly, string resourceFullName)
            {
                var assemblyName = assembly.GetName().Name;
                var directorySeparator = resourceFullName[assemblyName.Length];

                _assembly = assembly;
                _resourceFullName = resourceFullName;
                ResourceName = resourceFullName.Substring(assemblyName.Length + 1);
                _fileName = ResourceName.Substring(ResourceName.LastIndexOf(directorySeparator, ResourceName.LastIndexOf('.') - 1) + 1);
                DirectoryPath = ResourceName.Length == _fileName.Length
                    ? Enumerable.Empty<string>()
                    : ResourceName.Substring(0, ResourceName.Length - _fileName.Length - 1).Split(directorySeparator);
            }

            public ResourcePair(Assembly assembly, string resourceFullName, char directoryPathSeparator)
            {
                _assembly = assembly;
                _resourceFullName = resourceFullName;
                ResourceName = resourceFullName.Substring(assembly.GetName().Name.Length + 1);
                _fileName = ResourceName.Substring(ResourceName.LastIndexOf(directoryPathSeparator, ResourceName.LastIndexOf('.') - 1) + 1);
                DirectoryPath = ResourceName.Length == _fileName.Length
                    ? Enumerable.Empty<string>()
                    : ResourceName.Substring(0, ResourceName.Length - _fileName.Length - 1).Split(directoryPathSeparator);
            }

            public string ResourceName { get; }

            public IEnumerable<string> DirectoryPath { get; }

            public EmbeddedFile ToEmbeddedFile(EmbeddedDirectory parentDirectory)
                => new EmbeddedFile(parentDirectory, _fileName, _assembly, _resourceFullName);
        }
    }
}