// <copyright file="IFilesystemStorageConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    /// <summary>
    /// Holds configuration properties for the use of filesystem type storage for UBind.
    /// </summary>
    public interface IFilesystemStorageConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the storage provider to use, e.g. "Microsoft Graph" or "Local".
        /// </summary>
        string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the folder within the filesystem storage that Ubind should use to store it's files.
        /// This folder name is not a full path, and the storage configuration may have it's own base path.
        /// </summary>
        string UBindFolderName { get; set; }
    }
}
