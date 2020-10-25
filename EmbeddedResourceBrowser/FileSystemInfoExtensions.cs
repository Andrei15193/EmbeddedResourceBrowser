using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmbeddedResourceBrowser
{
    /// <summary>
    /// Exposes extension methods for <see cref="EmbeddedDirectory"/> and <see cref="EmbeddedFile"/> objects for
    /// usage with <see cref="DirectoryInfo"/> and <see cref="FileInfo"/>.
    /// </summary>
    public static class FileSystemInfoExtensions
    {
        /// <summary>Copies the embedded files to the target <paramref name="directoryInfo"/> excepting subdirectories.</summary>
        /// <param name="embeddedDirectory">The <see cref="EmbeddedDirectory"/> to copy files from.</param>
        /// <param name="directoryInfo">The <see cref="DirectoryInfo"/> to copy files to.</param>
        public static void CopyTo(this EmbeddedDirectory embeddedDirectory, DirectoryInfo directoryInfo)
        {
            if (embeddedDirectory is null)
                throw new NullReferenceException();

            Task.Run(() => embeddedDirectory.CopyToAsync(directoryInfo)).Wait();
        }

        /// <summary>Copies the embedded files to the target <paramref name="directoryInfo"/> including subdirectories.</summary>
        /// <param name="embeddedDirectory">The <see cref="EmbeddedDirectory"/> to copy files from.</param>
        /// <param name="directoryInfo">The <see cref="DirectoryInfo"/> to copy files to.</param>
        public static void CopyToRecursively(this EmbeddedDirectory embeddedDirectory, DirectoryInfo directoryInfo)
        {
            if (embeddedDirectory is null)
                throw new NullReferenceException();

            Task.Run(() => embeddedDirectory.CopyToRecursivelyAsync(directoryInfo)).Wait();
        }

        /// <summary>Copies the embedded files to the target <paramref name="directoryInfo"/> excepting subdirectories.</summary>
        /// <param name="embeddedDirectory">The <see cref="EmbeddedDirectory"/> to copy files from.</param>
        /// <param name="directoryInfo">The <see cref="DirectoryInfo"/> to copy files to.</param>
        public static Task CopyToAsync(this EmbeddedDirectory embeddedDirectory, DirectoryInfo directoryInfo)
            => embeddedDirectory.CopyToAsync(directoryInfo, CancellationToken.None);

        /// <summary>Copies the embedded files to the target <paramref name="directoryInfo"/> excepting subdirectories.</summary>
        /// <param name="embeddedDirectory">The <see cref="EmbeddedDirectory"/> to copy files from.</param>
        /// <param name="directoryInfo">The <see cref="DirectoryInfo"/> to copy files to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to signal the intent to cancel the operation.</param>
        public static Task CopyToAsync(this EmbeddedDirectory embeddedDirectory, DirectoryInfo directoryInfo, CancellationToken cancellationToken)
        {
            if (embeddedDirectory is null)
                throw new NullReferenceException();

            return Task.WhenAll(
                embeddedDirectory.Files.Select(async embeddedFile =>
                {
                    using (var fileStream = new FileStream(Path.Combine(directoryInfo.FullName, embeddedFile.Name), FileMode.Create, FileAccess.Write, FileShare.Read))
                    using (var embeddedFileStream = embeddedFile.OpenRead())
                        await embeddedFileStream.CopyToAsync(fileStream, 81920, cancellationToken).ConfigureAwait(false);
                })
            );
        }

        /// <summary>Copies the embedded files to the target <paramref name="directoryInfo"/> excepting subdirectories.</summary>
        /// <param name="embeddedDirectory">The <see cref="EmbeddedDirectory"/> to copy files from.</param>
        /// <param name="directoryInfo">The <see cref="DirectoryInfo"/> to copy files to.</param>
        public static Task CopyToRecursivelyAsync(this EmbeddedDirectory embeddedDirectory, DirectoryInfo directoryInfo)
            => CopyToRecursivelyAsync(embeddedDirectory, directoryInfo, CancellationToken.None);

        /// <summary>Copies the embedded files to the target <paramref name="directoryInfo"/> excepting subdirectories.</summary>
        /// <param name="embeddedDirectory">The <see cref="EmbeddedDirectory"/> to copy files from.</param>
        /// <param name="directoryInfo">The <see cref="DirectoryInfo"/> to copy files to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to signal the intent to cancel the operation.</param>
        public static Task CopyToRecursivelyAsync(this EmbeddedDirectory embeddedDirectory, DirectoryInfo directoryInfo, CancellationToken cancellationToken)
        {
            if (embeddedDirectory is null)
                throw new NullReferenceException();

            return Task.WhenAll(
                _GetAllEmbeddedFilePaths(embeddedDirectory, directoryInfo).Select(async embeddedFilePath =>
                {
                    using (var fileStream = new FileStream(embeddedFilePath.FilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    using (var embeddedFileStream = embeddedFilePath.EmbeddedFile.OpenRead())
                        await embeddedFileStream.CopyToAsync(fileStream, 81920, cancellationToken).ConfigureAwait(false);
                })
            );

        }

        private static IEnumerable<EmbededFilePathPair> _GetAllEmbeddedFilePaths(EmbeddedDirectory embeddedDirectory, DirectoryInfo targetDirectory)
        {
            var directoriesToVisit = new Queue<EmbededDirectoryInfoPair>();
            directoriesToVisit.Enqueue(new EmbededDirectoryInfoPair(embeddedDirectory, targetDirectory));
            do
            {
                var current = directoriesToVisit.Dequeue();
                foreach (var embeddedFile in current.EmbeddedDirectory.Files)
                    yield return new EmbededFilePathPair(embeddedFile, Path.Combine(current.DirectoryInfo.FullName, embeddedFile.Name));
                foreach (var embeddedSubdirectory in current.EmbeddedDirectory.Subdirectories)
                    directoriesToVisit.Enqueue(new EmbededDirectoryInfoPair(embeddedSubdirectory, current.DirectoryInfo.CreateSubdirectory(embeddedSubdirectory.Name)));
            } while (directoriesToVisit.Count > 0);
        }

        private struct EmbededDirectoryInfoPair
        {
            public EmbededDirectoryInfoPair(EmbeddedDirectory embeddedDirectory, DirectoryInfo directoryInfo)
            {
                EmbeddedDirectory = embeddedDirectory;
                DirectoryInfo = directoryInfo;
            }

            public EmbeddedDirectory EmbeddedDirectory { get; }

            public DirectoryInfo DirectoryInfo { get; }
        }

        private struct EmbededFilePathPair
        {
            public EmbededFilePathPair(EmbeddedFile embeddedFile, string filePath)
            {
                EmbeddedFile = embeddedFile;
                FilePath = filePath;
            }

            public EmbeddedFile EmbeddedFile { get; }

            public string FilePath { get; }
        }
    }
}