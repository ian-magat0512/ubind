// <copyright file="IFileHashService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories.FileSystem
{
    /// <summary>
    /// Provides the contract to calculate file hashes.
    /// </summary>
    public interface IFileHashService
    {
        /// <summary>
        /// Get the MD5 hash value from the specified file path on the file system.
        /// </summary>
        /// <param name="path">The file path of the file to be calculated.</param>
        /// <returns>Return the computed MD5 hash value.</returns>
        string GetFileMd5Hash(string path);
    }
}
