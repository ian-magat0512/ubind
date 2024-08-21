// <copyright file="IFilesystemFileCompressionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories.FileSystem
{
    /// <summary>
    /// Provides a common interface for file compression operations.
    /// </summary>
    public interface IFileSystemFileCompressionService
    {
        /// <summary>
        /// Extracts all the files in the specified zip archive to a directory on the file system.
        /// </summary>
        /// <param name="sourceArchiveFileName">The path to the archive that is to be extracted.</param>
        /// <param name="destinationDirectoryName">The path to the directory in which to place the extracted files, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName);
    }
}
