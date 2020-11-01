using System.IO;
using Xunit;

namespace EmbeddedResourceBrowser.Tests
{
    public class EmbeddedFileTests
    {
        [Fact]
        public void EmbeddedFile_TestFile1_HasNameExtensionAndParentDirectorySet()
        {
            var assemblyDirectory = new EmbeddedDirectory(typeof(EmbeddedFileTests).Assembly);

            var testFile1 = assemblyDirectory.Files["EmbeddedResources1/test file 1.txt"];
            Assert.Equal("EmbeddedResources1/test file 1.txt", testFile1.Name);
            Assert.Equal(".txt", testFile1.Extension);
            Assert.Same(assemblyDirectory, testFile1.ParentDirectory);
        }

        [Fact]
        public void EmbeddedFile_TestFile1_ReadContent()
        {
            var assemblyDirectory = new EmbeddedDirectory(typeof(EmbeddedFileTests).Assembly);

            var testFile1 = assemblyDirectory.Files["EmbeddedResources1/test file 1.txt"];

            using (var streamReader = new StreamReader(testFile1.OpenRead()))
                Assert.Equal("This is a text file, just for testing.", streamReader.ReadToEnd());
        }

        [Fact]
        public void EmbeddedFile_TestFile2_HasNameExtensionAndParentDirectorySet()
        {
            var assemblyDirectory = new EmbeddedDirectory(typeof(EmbeddedFileTests).Assembly);

            var embeddedResources1Directory = assemblyDirectory.Subdirectories["EmbeddedResources1"];
            var testFile2 = embeddedResources1Directory.Files["test file 2.txt"];
            Assert.Equal("test file 2.txt", testFile2.Name);
            Assert.Equal(".txt", testFile2.Extension);
            Assert.Same(embeddedResources1Directory, testFile2.ParentDirectory);
        }

        [Fact]
        public void EmbeddedFile_TestFile2_ReadContent()
        {
            var assemblyDirectory = new EmbeddedDirectory(typeof(EmbeddedFileTests).Assembly);

            var embeddedResources1Directory = assemblyDirectory.Subdirectories["EmbeddedResources1"];
            var testFile2 = embeddedResources1Directory.Files["test file 2.txt"];

            using (var streamReader = new StreamReader(testFile2.OpenRead()))
                Assert.Equal("This is a second text file, just for testing.", streamReader.ReadToEnd());
        }

        [Fact]
        public void EmbeddedFile_TestFile3_HasNameExtensionAndParentDirectorySet()
        {
            var assemblyDirectory = new EmbeddedDirectory(typeof(EmbeddedFileTests).Assembly);

            var embeddedResourceSubdirectory = assemblyDirectory.Subdirectories["EmbeddedResources1"].Subdirectories["EmbeddedResourceSubdirectory"];
            var testFile3 = embeddedResourceSubdirectory.Files["test file 3.txt"];
            Assert.Equal("test file 3.txt", testFile3.Name);
            Assert.Equal(".txt", testFile3.Extension);
            Assert.Same(embeddedResourceSubdirectory, testFile3.ParentDirectory);
        }

        [Fact]
        public void EmbeddedFile_TestFile3_ReadContent()
        {
            var assemblyDirectory = new EmbeddedDirectory(typeof(EmbeddedFileTests).Assembly);

            var embeddedResourceSubdirectory = assemblyDirectory.Subdirectories["EmbeddedResources1"].Subdirectories["EmbeddedResourceSubdirectory"];
            var testFile3 = embeddedResourceSubdirectory.Files["test file 3.txt"];

            using (var streamReader = new StreamReader(testFile3.OpenRead()))
                Assert.Equal("This is a third text file, just for testing.", streamReader.ReadToEnd());
        }

        [Fact]
        public void EmbeddedFile_TestFile4_HasNameExtensionAndParentDirectorySet()
        {
            var assemblyDirectory = new EmbeddedDirectory(typeof(EmbeddedFileTests).Assembly);

            var embeddedResources2Directory = assemblyDirectory.Subdirectories["EmbeddedResources2"];
            var testFile4 = embeddedResources2Directory.Files["test file 4.txt"];
            Assert.Equal("test file 4.txt", testFile4.Name);
            Assert.Equal(".txt", testFile4.Extension);
            Assert.Same(embeddedResources2Directory, testFile4.ParentDirectory);
        }

        [Fact]
        public void EmbeddedFile_TestFile4_ReadContent()
        {
            var assemblyDirectory = new EmbeddedDirectory(typeof(EmbeddedFileTests).Assembly);

            var embeddedResources2Directory = assemblyDirectory.Subdirectories["EmbeddedResources2"];
            var testFile4 = embeddedResources2Directory.Files["test file 4.txt"];

            using (var streamReader = new StreamReader(testFile4.OpenRead()))
                Assert.Equal("This is a fourth text file, just for testing.", streamReader.ReadToEnd());
        }
    }
}