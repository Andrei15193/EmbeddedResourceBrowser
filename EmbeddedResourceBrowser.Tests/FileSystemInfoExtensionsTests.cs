using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace EmbeddedResourceBrowser.Tests
{
    public class FileSystemInfoExtensionsTests
    {
        [Fact]
        public void CopyTo_DirectoryInfo_CopiesOnlyTopLevelFiles()
        {
            var embeddedDirectory = new EmbeddedDirectory(typeof(FileSystemInfoExtensionsTests).Assembly);

            _DirectoryTest(
                directoryInfo =>
                {
                    embeddedDirectory.Subdirectories["EmbeddedResources1"].CopyTo(directoryInfo);

                    Assert.Equal(new[] { "test file 2.txt" }, directoryInfo.GetFiles().Select(file => file.Name));
                    Assert.Equal("This is a second text file, just for testing.", File.ReadAllText(Path.Combine(directoryInfo.FullName, "test file 2.txt")));
                    Assert.Empty(directoryInfo.GetDirectories());
                }
            );
        }

        [Fact]
        public async Task CopyToAsync_DirectoryInfo_CopiesOnlyTopLevelFiles()
        {
            var embeddedDirectory = new EmbeddedDirectory(typeof(FileSystemInfoExtensionsTests).Assembly);

            await _DirectoryTestAsync(
                async directoryInfo =>
                {
                    await embeddedDirectory.Subdirectories["EmbeddedResources1"].CopyToAsync(directoryInfo);

                    Assert.Equal(new[] { "test file 2.txt" }, directoryInfo.GetFiles().Select(file => file.Name));
                    Assert.Equal("This is a second text file, just for testing.", File.ReadAllText(Path.Combine(directoryInfo.FullName, "test file 2.txt")));
                    Assert.Empty(directoryInfo.GetDirectories());
                }
            );
        }

        [Fact]
        public void CopyToRecursively_DirectoryInfo_CopiesAllFiles()
        {
            var embeddedDirectory = new EmbeddedDirectory(typeof(FileSystemInfoExtensionsTests).Assembly);

            _DirectoryTest(
                directoryInfo =>
                {
                    embeddedDirectory.Subdirectories["EmbeddedResources1"].CopyToRecursively(directoryInfo);

                    Assert.Equal(new[] { "test file 2.txt" }, directoryInfo.GetFiles().Select(file => file.Name));
                    Assert.Equal("This is a second text file, just for testing.", File.ReadAllText(Path.Combine(directoryInfo.FullName, "test file 2.txt")));

                    var embeddedResourceSubdirectory = Assert.Single(directoryInfo.GetDirectories(), directory => directory.Name == "EmbeddedResourceSubdirectory");
                    Assert.Equal("EmbeddedResourceSubdirectory", embeddedResourceSubdirectory.Name);
                    Assert.Equal(new[] { "test file 3.txt", }, embeddedResourceSubdirectory.GetFiles().Select(file => file.Name));
                    Assert.Equal("This is a third text file, just for testing.", File.ReadAllText(Path.Combine(embeddedResourceSubdirectory.FullName, "test file 3.txt")));
                }
            );
        }

        [Fact]
        public async Task CopyToRecursivelyAsync_DirectoryInfo_CopiesAllFiles()
        {
            var embeddedDirectory = new EmbeddedDirectory(typeof(FileSystemInfoExtensionsTests).Assembly);

            await _DirectoryTestAsync(
                async directoryInfo =>
                {
                    await embeddedDirectory.Subdirectories["EmbeddedResources1"].CopyToRecursivelyAsync(directoryInfo);

                    Assert.Equal(new[] { "test file 2.txt" }, directoryInfo.GetFiles().Select(file => file.Name));
                    Assert.Equal("This is a second text file, just for testing.", File.ReadAllText(Path.Combine(directoryInfo.FullName, "test file 2.txt")));

                    var embeddedResourceSubdirectory = Assert.Single(directoryInfo.GetDirectories(), directory => directory.Name == "EmbeddedResourceSubdirectory");
                    Assert.Equal("EmbeddedResourceSubdirectory", embeddedResourceSubdirectory.Name);
                    Assert.Equal(new[] { "test file 3.txt", }, embeddedResourceSubdirectory.GetFiles().Select(file => file.Name));
                    Assert.Equal("This is a third text file, just for testing.", File.ReadAllText(Path.Combine(embeddedResourceSubdirectory.FullName, "test file 3.txt")));
                }
            );
        }

        private static void _DirectoryTest(Action<DirectoryInfo> testCallback, [CallerMemberName] string testFolderName = null)
        {
            _DirectoryTestAsync(directoryInfo => { testCallback(directoryInfo); return Task.CompletedTask; }, testFolderName).Wait();
        }

        private static async Task _DirectoryTestAsync(Func<DirectoryInfo, Task> asyncTestCallback, [CallerMemberName] string testFolderName = null)
        {
            var targetDirectoryInfo = new DirectoryInfo(Environment.CurrentDirectory).CreateSubdirectory(testFolderName);
            try
            {
                await asyncTestCallback(targetDirectoryInfo);
            }
            finally
            {
                targetDirectoryInfo.Delete(true);
            }
        }
    }
}