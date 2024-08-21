// <copyright file="IFilesystemFileRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using UBind.Domain.Dto;
    using UBindFileInfo = UBind.Domain.Models.FileInfo;

    /// <summary>
    /// Provides a common interface for filesystem operations, which can be implemented by different providers
    /// including MS Graph API, or using a local disk.
    /// </summary>
    public interface IFilesystemFileRepository : IOptionallyConfigurable
    {
        /// <summary>
        /// Gets an authentication token which can be used in future calls.
        /// </summary>
        /// <returns>An authentication token.</returns>
        Task<string> GetAuthenticationToken();

        /// <summary>
        /// Download contents of a file from OneDrive synchronously.
        /// </summary>
        /// <param name="path">The file path relative to OneDrive root.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task whose result is a string containing the file contents.</returns>
        Task<string> GetFileStringContents(string path, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Try to download contents of a file from OneDrive.
        /// </summary>
        /// <param name="path">The file path relative to OneDrive root.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A maybe monad that holds a string containing the file contents if found, or none.</returns>
        Task<Maybe<string>> TryGetFileStringContents(string path, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Download contents of a file as data buffer from OneDrive synchronously.
        /// </summary>
        /// <param name="filePath">The file path relative to OneDrive root.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task whose result is a byte array containing the file contents.</returns>
        Task<byte[]> GetFileContents(string filePath, string authenticationToken, TimeSpan timeout = default);

        Task<UBindFileInfo?> GetFileInfo(string filePath, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Move a folder.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="createTargetPathIfSourceDoesntExist">Create the path if source doesnt exist.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that cna be awaited.</returns>
        Task MoveFolder(
            string sourcePath,
            string destinationPath,
            bool createTargetPathIfSourceDoesntExist,
            string authenticationToken,
            TimeSpan timeout = default);

        /// <summary>
        /// Rename an item in One Drive.
        /// </summary>
        /// <param name="itemPath">The path of the item.</param>
        /// <param name="newName">The new name for the item.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that cna be awaited.</returns>
        Task RenameItem(string itemPath, string newName, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Create a folder in OneDrive.
        /// </summary>
        /// <param name="parentPath">The path to the parent folder to create the folder in, or null to create the folder in the root.</param>
        /// <param name="folderName">The name for the new folder.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that can be awaited.</returns>
        Task CreateFolder(string parentPath, string folderName, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Create the folder path in OneDrive.
        /// </summary>
        /// <param name="path">The path to be created.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that can be awaited.</returns>
        Task CreateFolder(string path, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// List items in a given folder in OneDrive (files and folders).
        /// </summary>
        /// <param name="folderPath">The path to the folder.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that will return the list of item names.</returns>
        Task<IEnumerable<string>> ListItemsInFolder(string folderPath, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Delete a folder in One Drive.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteFolder(string folderPath, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// List files in a given folder in OneDrive.
        /// </summary>
        /// <param name="folderPath">The path to the folder.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that will return the list of file names.</returns>
        Task<IEnumerable<string>> ListFilesInFolder(string folderPath, string authenticationToken, TimeSpan timeout = default);

        Task<IEnumerable<UBindFileInfo>> ListFilesInfoInFolder(string folderPath, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// List sub-folders in a given folder in OneDrive.
        /// </summary>
        /// <param name="folderPath">The path to the folder.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that will return the list of sub-folder names.</returns>
        Task<IEnumerable<string>> ListSubfoldersInFolder(string folderPath, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Write a file from bytes.
        /// </summary>
        /// <param name="fileContent">The contents of the file to upload.</param>
        /// <param name="destinationPath">The path relative to the One Drive root to create the file at.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that can be awaited.</returns>
        Task WriteFileContents(byte[] fileContent, string destinationPath, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// List of file last modified datetimes in a given folder in OneDrive.
        /// </summary>
        /// <param name="folderPath">The path to the folder.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A <see cref="Task"/> that will result to a list of last modified time string value.</returns>
        Task<IEnumerable<string>> ListOfFilesLastModifiedDateTimeInFolder(
            string folderPath, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Get the files last modified datetimes in a given folders in OneDrive.
        /// </summary>
        /// <param name="folderPaths">List of path to the in one drive.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A <see cref="Task"/> that will result to hash string of the folder paths.</returns>
        Task<string> GetFilesHashStringInFolders(
            List<string> folderPaths, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Copy a One Drive item to a new location.
        /// </summary>
        /// <param name="path">The path of the item to copy.</param>
        /// <param name="newParentPath">The path of the destination folder.</param>
        /// <param name="newName">The name for the created copy.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that can be awaited.</returns>
        Task CopyItem(string path, string newParentPath, string newName, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Delete a One Drive item.
        /// </summary>
        /// <param name="path">The path of the item to delete.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that can be awaited.</returns>
        Task DeleteItem(string path, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Retrieves a link to the file in OneDrive.
        /// </summary>
        /// <param name="path">The path to the folder the file is in.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that contains a collection of the configuration source files.</returns>
        Task<IEnumerable<ConfigurationFileDto>> GetConfigurationFilesInFolder(string path, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Check the existence a folder in OneDrive.
        /// </summary>
        /// <param name="folderPath">The path of the specified folder.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that can be awaited.</returns>
        Task<bool> FolderExists(string folderPath, string authenticationToken, TimeSpan timeout = default);

        /// <summary>
        /// Check the existence a file in OneDrive.
        /// </summary>
        /// <param name="filePath">The path of the specified file.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task that can be awaited.</returns>
        Task<bool> FileExists(string filePath, string authenticationToken, TimeSpan timeout = default);
    }
}
