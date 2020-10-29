using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace EmbeddedResourceBrowser.Tests
{
    public class EmbeddedDirectoryTests
    {
        [Fact]
        public void CreatingEmbeddedDirectory_WithNullAssembly_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("assembly", () => new EmbeddedDirectory((Assembly)null));

            Assert.Equal(new ArgumentNullException("assembly").Message, exception.Message);
        }

        [Fact]
        public void CreatingEmbeddedDirectory_WithNullAssemblies_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("assemblies", () => new EmbeddedDirectory((IEnumerable<Assembly>)null));

            Assert.Equal(new ArgumentException("Cannot be null or contain null values.", "assemblies").Message, exception.Message);
        }

        [Fact]
        public void CreatingEmbeddedDirectory_WithAssembliesCollectionContainingNull_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("assemblies", () => new EmbeddedDirectory(new Assembly[] { null }));

            Assert.Equal(new ArgumentException("Cannot be null or contain null values.", "assemblies").Message, exception.Message);
        }

        [Fact]
        public void EmbeddedDirectory_WhenInitiallyCreated_HasEmptyName()
        {
            var embeddedDirectory = new EmbeddedDirectory(new[] { typeof(EmbeddedDirectory).Assembly });

            Assert.Empty(embeddedDirectory.Name);
        }

        [Fact]
        public void EmbeddedDirectory_WhenInitiallyCreated_HasNullParentDirectory()
        {
            var embeddedDirectory = new EmbeddedDirectory(new[] { typeof(EmbeddedDirectory).Assembly });

            Assert.Null(embeddedDirectory.ParentDirectory);
        }

        [Fact]
        public void EmbeddedDirectory_WhenThereAreNoEmbeddedFiles_HasEmptySubdirectory()
        {
            var embeddedDirectory = new EmbeddedDirectory(new[] { typeof(EmbeddedDirectory).Assembly });

            var assemblyDirectory = Assert.Single(embeddedDirectory.Subdirectories);
            Assert.Equal("EmbeddedResourceBrowser", assemblyDirectory.Name);
            Assert.Same(embeddedDirectory, assemblyDirectory.ParentDirectory);
            Assert.Empty(assemblyDirectory.Subdirectories);
            Assert.Empty(assemblyDirectory.Files);
        }

        [Fact]
        public void EmbeddedDirectory_WithFilesInSubdirectories_HasSubdirectoryTreeSet()
        {
            var embeddedDirectory = new EmbeddedDirectory(new[] { typeof(EmbeddedDirectoryTests).Assembly });

            var assemblyDirectory = Assert.Single(embeddedDirectory.Subdirectories);
            Assert.Equal("EmbeddedResourceBrowser.Tests", assemblyDirectory.Name);
            Assert.Same(embeddedDirectory, assemblyDirectory.ParentDirectory);

            Assert.Equal(2, assemblyDirectory.Subdirectories.Count);

            var embeddedResources1Directory = assemblyDirectory.Subdirectories.ElementAt(0);
            Assert.Equal("EmbeddedResources1", embeddedResources1Directory.Name);
            Assert.Same(assemblyDirectory, embeddedResources1Directory.ParentDirectory);

            var embeddedResourceSubdirectoryDirectory = Assert.Single(embeddedResources1Directory.Subdirectories);
            Assert.Equal("EmbeddedResourceSubdirectory", embeddedResourceSubdirectoryDirectory.Name);
            Assert.Same(embeddedResources1Directory, embeddedResourceSubdirectoryDirectory.ParentDirectory);

            var embeddedResources2Directory = assemblyDirectory.Subdirectories.ElementAt(1);
            Assert.Equal("EmbeddedResources2", embeddedResources2Directory.Name);
            Assert.Same(assemblyDirectory, embeddedResources2Directory.ParentDirectory);

            Assert.Empty(embeddedResources2Directory.Subdirectories);
        }

        [Fact]
        public void EmbeddedDirectory_WithFilesInSubdirectories_ListsEmbeddedFilesForEachDirectory()
        {
            var embeddedDirectory = new EmbeddedDirectory(new[] { typeof(EmbeddedDirectoryTests).Assembly });

            Assert.Empty(embeddedDirectory.Files);

            var assemblyDirectory = Assert.Single(embeddedDirectory.Subdirectories);
            Assert.Equal("EmbeddedResourceBrowser.Tests", assemblyDirectory.Name);
            Assert.Empty(assemblyDirectory.Files);

            var embeddedResources1Directory = assemblyDirectory.Subdirectories.ElementAt(0);
            Assert.Equal("EmbeddedResources1", embeddedResources1Directory.Name);
            Assert.Equal(2, embeddedResources1Directory.Files.Count);
            Assert.Equal("test file 1.txt", embeddedResources1Directory.Files[0].Name);
            Assert.Equal("test file 2.txt", embeddedResources1Directory.Files[1].Name);

            var embeddedResourceSubdirectoryDirectory = Assert.Single(embeddedResources1Directory.Subdirectories);
            Assert.Equal("EmbeddedResourceSubdirectory", embeddedResourceSubdirectoryDirectory.Name);
            Assert.Single(embeddedResourceSubdirectoryDirectory.Files);
            Assert.Equal("test file 3.txt", embeddedResourceSubdirectoryDirectory.Files[0].Name);

            var embeddedResources2Directory = assemblyDirectory.Subdirectories.ElementAt(1);
            Assert.Equal("EmbeddedResources2", embeddedResources2Directory.Name);
            Assert.Single(embeddedResources2Directory.Files);
            Assert.Equal("test file 4.txt", embeddedResources2Directory.Files[0].Name);
        }

        [Fact]
        public void GetAllFiles_WithFilesInSubdirectories_ListsAllEmbeddedFiles()
        {
            var embeddedDirectory = new EmbeddedDirectory(new[] { typeof(EmbeddedDirectoryTests).Assembly });

            Assert.Equal(
                new[]
                {
                    "test file 1.txt",
                    "test file 2.txt",
                    "test file 3.txt",
                    "test file 4.txt"
                },
                embeddedDirectory.GetAllFiles().Select(file => file.Name)
            );
        }


        [Fact]
        public void GetAllSubdirectories_WithFilesInSubdirectories_ListsAllEmbeddedDirectories()
        {
            var embeddedDirectory = new EmbeddedDirectory(new[] { typeof(EmbeddedDirectoryTests).Assembly });

            Assert.Equal(
                new[]
                {
                    "EmbeddedResourceBrowser.Tests",
                    "EmbeddedResources1",
                    "EmbeddedResourceSubdirectory",
                    "EmbeddedResources2"
                },
                embeddedDirectory.GetAllSubdirectories().Select(directory => directory.Name)
            );
        }
    }
}