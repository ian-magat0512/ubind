// <copyright file="IArchive.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using NodaTime;

    /// <summary>
    /// Represents an archive, such as a zip archive.
    /// </summary>
    public interface IArchive : IDisposable
    {
        Instant? CreatedTimestamp { get; }

        Instant? LastModifiedTimestamp { get; }

        /// <summary>
        /// Returns an IArchiveEntry if one exists with the given path.
        /// The path is the full path to the file, e.g. "/a/b/c.txt".
        /// </summary>
        /// <param name="path">The full path to the file or folder.</param>
        /// <returns>The archive entry, or null if no match is found.</returns>
        IArchiveEntry GetEntry(string path);

        /// <summary>
        /// Returns true if an entry exists at the given path, or an entry exists which is a descendent of the given
        /// path.
        /// </summary>
        bool PathExists(string path);

        /// <summary>
        /// Gets an enumerator for the archive entries in this archive.
        /// </summary>
        /// <returns>A System.Collections.IEnumerator for this archive.</returns>
        /// <exception cref="System.ObjectDisposedException">The Zip file has been closed.</exception>
        IEnumerator<IArchiveEntry> GetEnumerator();

        /// <summary>
        /// Reads the archive's raw bytes.
        /// </summary>
        /// <param name="getErrorDataCallback">A callback which returns a JObject of contextual error information
        /// should the operation fail.
        /// </param>
        /// <returns>A byte array of the archive's raw data.</returns>
        byte[] ReadBytes(Func<Task<JObject>> getErrorDataCallback);

        IArchiveEntry AddFolder(string path);

        /// <summary>
        /// Removes the entry. If the entry is a folder, it does not remove it's descendants.
        /// Also remove descendants, please use <c>RemoveEntries</c>.
        /// </summary>
        void RemoveEntry(IArchiveEntry entry);

        /// <summary>
        /// Removes the entry at the given path and descendents of the given path.
        /// </summary>
        /// <returns>The number of entries removed.</returns>
        int RemoveEntries(string path);

        IArchiveEntry AddFile(
            string path,
            byte[] content,
            Instant? createdTimestamp = null,
            Instant? lastModifiedTimestamp = null,
            CompressionLevel compressionLevel = CompressionLevel.Optimal,
            string comment = null);

        /// <summary>
        /// Moves a file to the newFullPath.
        /// </summary>
        /// <param name="newFullPath">The full path, including folders, filename and extension. E.g. "/my/folder/file.ext".</param>
        IArchiveEntry MoveFile(IArchiveEntry entry, string newFullPath);

        /// <summary>
        /// Copies a file from one location to another.
        /// </summary>
        IArchiveEntry CopyFile(IArchiveEntry entry, string destinationPath);

        /// <summary>
        /// Moves a folder to the new newFullPath, and any entries under it.
        /// If a folder already exists at the path, it will merge the contents with that folder.
        /// </summary>
        /// <param name="entry">The existing archive entry, which must be a folder.</param>
        /// <param name="newFullPath">The new location within the archive.</param>
        IArchiveEntry MoveFolder(IArchiveEntry entry, string newFullPath);

        /// <summary>
        /// Moves any entries at the old path (or which are descendants of the old path) to new path.
        /// E.g. /oldpath/a/b becomes /newpath/a/b.
        /// </summary>
        IArchiveEntry MoveFolder(string oldPath, string newPath);

        /// <summary>
        /// Copies a folder to the new newFullPath, and any entries under it.
        /// If a folder already exists at the path, it will merge the contents with that folder.
        /// </summary>
        /// <param name="entry">The existing archive entry, which must be a folder.</param>
        /// <param name="newFullPath">The new location within the archive.</param>
        IArchiveEntry CopyFolder(IArchiveEntry entry, string newFullPath);

        /// <summary>
        /// Copies any entries at the old path (or which are descendants of the old path) to new path.
        /// E.g. /oldpath/a/b is copied to /newpath/a/b.
        /// </summary>
        IArchiveEntry CopyFolder(string oldPath, string newPath);
    }
}
