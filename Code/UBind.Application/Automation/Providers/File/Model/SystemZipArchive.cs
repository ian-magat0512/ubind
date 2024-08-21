// <copyright file="SystemZipArchive.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// The SystemZipArchive supports listing contents, extracting files, and modifications,
    /// but does not support passwords as it relies on the .Net System.IO.Compression libraries.
    /// </summary>
    public class SystemZipArchive : IArchive, IDisposable
    {
        public SystemZipArchive(ZipArchive zipArchive, System.IO.MemoryStream memoryStream)
        {
            this.ZipArchive = zipArchive;
            this.MemoryStream = memoryStream;
        }

        public ZipArchive ZipArchive { get; }

        /// <summary>
        /// Gets the memory stream for reading the Zip archive's raw bytes.
        /// </summary>
        public System.IO.MemoryStream MemoryStream { get; }

        public Instant? CreatedTimestamp { get; set; }

        public Instant? LastModifiedTimestamp { get; set; }

        public static async Task<SystemZipArchive> Open(
            FileInfo sourceFile,
            IClock clock,
            Func<Task<JObject>> getErrorDataCallback,
            bool readOnly = false)
        {
            var memoryStream = new System.IO.MemoryStream();
            memoryStream.Write(sourceFile.Content, 0, sourceFile.Content.Length);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            var archiveAccessMode = readOnly ? ZipArchiveMode.Read : ZipArchiveMode.Update;
            try
            {
                var zipArchive = new ZipArchive(memoryStream, archiveAccessMode);
                var archive = new SystemZipArchive(zipArchive, memoryStream);
                archive.CreatedTimestamp = archive.CreatedTimestamp ?? sourceFile.CreatedTimestamp;
                archive.LastModifiedTimestamp = archive.LastModifiedTimestamp ?? sourceFile.LastModifiedTimestamp;
                return archive;
            }
            catch (System.IO.InvalidDataException ex)
            {
                var errorData = await getErrorDataCallback();
                throw new ErrorException(Errors.Automation.Archive.NotAValidZipArchive(ex.Message.WithDot(), errorData));
            }
        }

        public static SystemZipArchive Create(IClock clock)
        {
            var memoryStream = new System.IO.MemoryStream();
            var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Update);
            var archive = new SystemZipArchive(zipArchive, memoryStream);
            archive.CreatedTimestamp = archive.LastModifiedTimestamp = clock.Now();
            return archive;
        }

        public IArchiveEntry AddFile(
            string path,
            byte[] content,
            Instant? createdTimestamp = null,
            Instant? lastModifiedTimestamp = null,
            CompressionLevel compressionLevel = CompressionLevel.Optimal,
            string comment = null)
        {
            path = CleanPath(path, false);
            ZipArchiveEntry newEntry = this.ZipArchive.CreateEntry(path, compressionLevel);
            using (var writer = new System.IO.BinaryWriter(newEntry.Open()))
            {
                writer.Write(content, 0, content.Length);
            }

            return new SystemZipArchiveEntry(this, newEntry);
        }

        public IArchiveEntry CopyFile(IArchiveEntry sourceEntry, string targetPath)
        {
            targetPath = CleanPath(targetPath, false);
            var systemZipEntry = sourceEntry as SystemZipArchiveEntry;
            if (systemZipEntry == null)
            {
                throw new InvalidOperationException("When trying to copy a file from one location to another within "
                    + $"an archive, the source entry was of type {systemZipEntry.GetType().Name} however we were "
                    + $"expecting it to have the type {typeof(SystemZipArchiveEntry)}.");
            }

            if (sourceEntry.IsFolder)
            {
                throw new InvalidOperationException("You asked to copy a file from one location to another "
                    + "within an archive, but you have passed a folder instead of a file. If you wish to copy a folder "
                    + "then please use the CopyFolder method.");
            }

            SystemZipArchiveEntry existingEntry = (SystemZipArchiveEntry)this.GetEntry(targetPath);
            if (existingEntry != null)
            {
                existingEntry.ZipEntry.Delete();
            }

            var sourceSystemZipEntry = systemZipEntry.ZipEntry;
            ZipArchiveEntry newEntry = this.ZipArchive.CreateEntry(targetPath);

            using (var existingStream = sourceSystemZipEntry.Open())
            using (var newStream = newEntry.Open())
            {
                existingStream.CopyTo(newStream);
            }

            return new SystemZipArchiveEntry(this, newEntry);
        }

        public IArchiveEntry CopyFolder(IArchiveEntry sourceEntry, string targetPath)
        {
            targetPath = CleanPath(targetPath, true);
            var systemZipEntry = sourceEntry as SystemZipArchiveEntry;
            if (systemZipEntry == null)
            {
                throw new InvalidOperationException("When trying to copy a folder from one location to another within "
                    + $"an archive, the source entry was of type {systemZipEntry.GetType().Name} however we were "
                    + $"expecting it to have the type {typeof(SystemZipArchiveEntry)}.");
            }

            if (sourceEntry.IsFile)
            {
                throw new InvalidOperationException("You asked to copy a folder from one location to another "
                    + "within an archive, but you have passed a file instead of a folder. If you wish to copy a file "
                    + "then please use the CopyFile method.");
            }

            SystemZipArchiveEntry existingEntry = (SystemZipArchiveEntry)this.GetEntry(targetPath);
            if (existingEntry != null && existingEntry.IsFile)
            {
                existingEntry.ZipEntry.Delete();
            }

            var entries = this.GetEntriesMatchingPath(sourceEntry.Path).ToList();
            foreach (var entry in entries)
            {
                string newPath = this.ReplacePrefixInPath(entry.Path, sourceEntry.Path, targetPath);
                if (entry.IsFile)
                {
                    this.CopyFile(entry, newPath);
                }
                else
                {
                    this.AddFolder(newPath);
                }
            }

            return this.GetEntry(targetPath);
        }

        public IArchiveEntry CopyFolder(string sourcePath, string targetPath)
        {
            sourcePath = CleanPath(sourcePath, true);
            targetPath = CleanPath(targetPath, true);
            var existingEntry = this.GetEntry(sourcePath);
            if (existingEntry != null)
            {
                return this.CopyFolder(existingEntry, targetPath);
            }

            var entries = this.GetEntriesMatchingPath(sourcePath).ToList();
            foreach (var entry in entries)
            {
                string newPath = this.ReplacePrefixInPath(entry.Path, sourcePath, targetPath);
                if (entry.IsFile)
                {
                    this.CopyFile(entry, newPath);
                }
                else
                {
                    this.AddFolder(newPath);
                }
            }

            // create an entry for the target path
            return this.AddFolder(targetPath);
        }

        public IArchiveEntry AddFolder(string path)
        {
            path = CleanPath(path, true);
            if (!path.EndsWith("/"))
            {
                path += "/";
            }

            SystemZipArchiveEntry existingEntry = (SystemZipArchiveEntry)this.GetEntry(path);
            if (existingEntry != null && existingEntry.IsFile)
            {
                existingEntry.ZipEntry.Delete();
            }

            ZipArchiveEntry newEntry = this.CreateEntriesForFolderPath(path);
            return new SystemZipArchiveEntry(this, newEntry);
        }

        /// <summary>
        /// Gets an entry (file or folder) with the given path. Ignores leading an trailing slashes.
        /// </summary>
        public IArchiveEntry GetEntry(string path)
        {
            path = CleanPath(path);
            path = Path.RemoveTrailingDelimiter(path);

            // since we don't know if it's a file or a folder, we'll try for both:
            var zipEntry = this.ZipArchive.GetEntry(path) ?? this.ZipArchive.GetEntry(path + "/");
            return zipEntry != null
                ? new SystemZipArchiveEntry(this, zipEntry)
                : (IArchiveEntry)null;
        }

        public bool PathExists(string path)
        {
            string filePath = CleanPath(path, false);
            string folderPath = CleanPath(path, true);
            return this.ZipArchive.Entries
                .Any(e => e.FullName.StartsWith(folderPath) || e.FullName == filePath);
        }

        public IEnumerator<IArchiveEntry> GetEnumerator()
        {
            return new SystemZipArchiveEnumerator(this);
        }

        public IArchiveEntry MoveFile(IArchiveEntry entry, string targetPath)
        {
            targetPath = CleanPath(targetPath, false);
            var newEntry = this.CopyFile(entry, targetPath);
            this.RemoveEntry(entry);
            return newEntry;
        }

        public IArchiveEntry MoveFolder(IArchiveEntry sourceEntry, string targetPath)
        {
            targetPath = CleanPath(targetPath, true);
            var copiedFolderEntry = this.CopyFolder(sourceEntry, targetPath);
            var entries = this.GetEntriesMatchingPath(sourceEntry.Path).ToList();
            entries.ForEach(e => (e as SystemZipArchiveEntry).ZipEntry.Delete());
            return copiedFolderEntry;
        }

        public IArchiveEntry MoveFolder(string oldPath, string newPath)
        {
            oldPath = CleanPath(oldPath, true);
            newPath = CleanPath(newPath, true);
            this.CopyFolder(oldPath, newPath);
            var entries = this.GetEntriesMatchingPath(oldPath).ToList();
            entries.ForEach(e => (e as SystemZipArchiveEntry).ZipEntry.Delete());
            return this.GetEntry(newPath);
        }

        public byte[] ReadBytes(Func<Task<JObject>> getErrorDataCallback)
        {
            return this.MemoryStream.ToArray();
        }

        public void RemoveEntry(IArchiveEntry entry)
        {
            var systemZipEntry = entry as SystemZipArchiveEntry;
            if (systemZipEntry == null)
            {
                throw new InvalidOperationException("When trying to delete an entry within "
                    + $"an archive, the entry was of type {systemZipEntry.GetType().Name} however we were "
                    + $"expecting it to have the type {typeof(SystemZipArchiveEntry)}.");
            }

            systemZipEntry.ZipEntry.Delete();
        }

        public int RemoveEntries(string path)
        {
            path = CleanPath(path);
            var entries = this.GetEntriesMatchingPath(path).ToList();
            foreach (var entry in entries)
            {
                this.RemoveEntry(entry);
            }

            return entries.Count;
        }

        public void Dispose()
        {
            this.ZipArchive.Dispose();
            this.MemoryStream.Dispose();
        }

        /// <summary>
        /// Cleans the path by:
        /// - normalising it to use forward slashes
        /// - removing the leading slash, since in zip archives all paths are relative
        /// - if it's a folder, ensuring it has a trailing slash, otherwise no trailing slash.
        /// </summary>
        private static string CleanPath(string path, bool? isFolder = null)
        {
            path = path.Replace("\\", "/");
            if (isFolder.HasValue)
            {
                path = isFolder.Value
                    ? Path.AddTrailingDelimiter(path)
                    : Path.RemoveTrailingDelimiter(path);
            }

            return Path.RemoveLeadingDelimiter(path);
        }

        private IEnumerable<IArchiveEntry> GetEntriesMatchingPath(string path)
        {
            string filePath = CleanPath(path, false);
            string folderPath = CleanPath(path, true);
            return this.ZipArchive.Entries
                .Where(e => e.FullName.StartsWith(folderPath) || e.FullName == filePath)
                .Select(e => new SystemZipArchiveEntry(this, e));
        }

        /// <summary>
        /// Changes a path by replacing it's prefix with a given value.
        /// This is used during copy of move operations to substitute the new location.
        /// </summary>
        private string ReplacePrefixInPath(string path, string from, string to)
        {
            return string.Concat(to, path.AsSpan(from.Length));
        }

        /// <summary>
        /// Creates entries for a folder path, including for all of it's parents when they don't already exist.
        /// </summary>
        /// <returns>The topmost ZipArchiveEntry, representing the leaf node (or childmost segment).</returns>
        private ZipArchiveEntry CreateEntriesForFolderPath(string folderPath)
        {
            folderPath = CleanPath(folderPath, true);
            var segments = folderPath.Split('/');
            string createdPath = string.Empty;
            ZipArchiveEntry topEntry = null;
            foreach (var segment in segments)
            {
                if (segment == string.Empty)
                {
                    continue;
                }

                createdPath += segment + "/";
                topEntry = this.ZipArchive.GetEntry(createdPath);
                if (topEntry == null)
                {
                    topEntry = this.ZipArchive.CreateEntry(createdPath);
                }
            }

            return topEntry;
        }
    }
}
