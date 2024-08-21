// <copyright file="IFilesystemService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories.FileSystem
{
    using System.IO.Abstractions;

    /// <summary>
    /// Provides a common interface for filesystem operations, which can be implemented by different providers
    /// including local disk.
    /// </summary>
    public interface IFileSystemService : IFileSystemFileCompressionService, IFileHashService
    {
        /// <summary>
        /// Gets directory methods for creating, moving, and enumerating through directories and subdirectories.
        /// </summary>
        IDirectory Directory { get; }

        /// <summary>
        /// Gets file methods for the creation, copying, deletion, moving, and opening of a single file, and aids in the creation of FileStream objects.
        /// </summary>
        IFile File { get; }
    }
}
