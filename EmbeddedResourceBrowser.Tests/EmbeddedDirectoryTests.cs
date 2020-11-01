using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EmbeddedResourceBrowser.MergeTests;
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
            Assert.Single(assemblyDirectory.Files);
            Assert.Equal("EmbeddedResources1/test file 1.txt", assemblyDirectory.Files[0].Name);

            var embeddedResources1Directory = assemblyDirectory.Subdirectories.ElementAt(0);
            Assert.Equal("EmbeddedResources1", embeddedResources1Directory.Name);
            Assert.Single(embeddedResources1Directory.Files);
            Assert.Equal("test file 2.txt", embeddedResources1Directory.Files[0].Name);

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
                    "EmbeddedResources1/test file 1.txt",
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

        [Fact]
        public void Merge_WithNullAssemblies_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("assemblies", () => EmbeddedDirectory.Merge((IEnumerable<Assembly>)null));

            Assert.Equal(new ArgumentException("Cannot be null or contain null values.", "assemblies").Message, exception.Message);
        }

        [Fact]
        public void Merge_WithAssembliesCollectionContainingNull_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("assemblies", () => EmbeddedDirectory.Merge(new Assembly[] { null }));

            Assert.Equal(new ArgumentException("Cannot be null or contain null values.", "assemblies").Message, exception.Message);
        }

        [Fact]
        public void Merge_WithFilesInSubdirectories_MergesDirectories()
        {
            var embeddedDirectory = EmbeddedDirectory.Merge(typeof(EmbeddedDirectoryTests).Assembly, typeof(SampleType).Assembly);

            Assert.Equal(
                new[]
                {
                    "EmbeddedResources1",
                    "EmbeddedResourceSubdirectory",
                    "EmbeddedResources2",
                    "EmbeddedResources3"
                },
                embeddedDirectory.GetAllSubdirectories().Select(directory => directory.Name)
            );
        }

        [Fact]
        public void Merge_WithFilesInSubdirectories_MergesFiles()
        {
            var embeddedDirectory = EmbeddedDirectory.Merge(typeof(EmbeddedDirectoryTests).Assembly, typeof(SampleType).Assembly);

            Assert.Equal(
                new[]
                {
                    new
                    {
                        Name = "EmbeddedResources1/test file 1.txt",
                        Content = "This is a text file. The content is overridden, just for testing the merge feature."
                    },
                    new
                    {
                        Name = "test file 2-1.txt",
                        Content = "This text file does not override any content, just for testing the merge feature."
                    },
                    new
                    {
                        Name = "test file 2.txt",
                        Content = "This is a second text file, just for testing."
                    },
                    new
                    {
                        Name = "test file 3.txt",
                        Content = "This is a third text file, just for testing."
                    },
                    new
                    {
                        Name = "test file 4.txt",
                        Content = "This is a fourth text file, just for testing."
                    },
                    new
                    {
                        Name = "test file 5.txt",
                        Content = "This is a fifth text file, just for testing the merge scenario."
                    }
                },
                embeddedDirectory
                    .GetAllFiles()
                    .Select(file =>
                    {
                        using var streamReader = new StreamReader(file.OpenRead());

                        return new
                        {
                            file.Name,
                            Content = streamReader.ReadToEnd()
                        };
                    })
            );
        }

        [Fact]
        public void EmbeddedDirectory_WhenCreatedWithDifferentSeparator_MapsTheStructureAccordingToIt()
        {
            var embeddedDirectory = new EmbeddedDirectory(typeof(EmbeddedDirectoryTests).Assembly, '/');

            Assert.Equal("EmbeddedResourceBrowser.Tests", embeddedDirectory.Name);
            Assert.Equal(
                new[]
                {
                    "EmbeddedResources1"
                },
                embeddedDirectory.GetAllSubdirectories().Select(directory => directory.Name)
            );
            Assert.Equal(
                new[]
                {
                    "EmbeddedResources1.EmbeddedResourceSubdirectory.test file 3.txt",
                    "EmbeddedResources1.test file 2.txt",
                    "EmbeddedResources2.test file 4.txt",
                    "test file 1.txt"
                },
                embeddedDirectory.GetAllFiles().Select(file => file.Name)
            );
        }
    }
}