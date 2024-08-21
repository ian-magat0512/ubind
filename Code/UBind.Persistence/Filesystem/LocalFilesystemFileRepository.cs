// <copyright file="LocalFilesystemFileRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Filesystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBind.Domain.Repositories;
    using UBindFileInfo = UBind.Domain.Models.FileInfo;

    /// <summary>
    /// A repository for storing and retreiving files locally from the filesystem.
    /// </summary>
    public class LocalFilesystemFileRepository : IFilesystemFileRepository
    {
        private ILocalFilesystemStorageConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFilesystemFileRepository"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public LocalFilesystemFileRepository(ILocalFilesystemStorageConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public bool IsConfigured()
        {
            return this.configuration.BasePath != null;
        }

        /// <inheritdoc/>
        public Task<string> GetAuthenticationToken()
        {
            // No authentication token is needed for this type of filesystem repository
            return Task.FromResult(string.Empty);
        }

        /// <inheritdoc/>
        public Task CopyItem(string path, string newParentPath, string newName, string authenticationToken, TimeSpan timeout = default)
        {
            var sourceFilePath = Path.Combine(this.configuration.BasePath, path);
            var targetFilePath = Path.Combine(new[] { this.configuration.BasePath, newParentPath, newName });
            File.Copy(sourceFilePath, targetFilePath);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task CreateFolder(string parentPath, string folderName, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(new[] { this.configuration.BasePath, parentPath, folderName });
            Directory.CreateDirectory(targetPath);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task CreateFolder(string path, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(new[] { this.configuration.BasePath, path });
            Directory.CreateDirectory(targetPath);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task DeleteFolder(string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, folderPath);
            Directory.Delete(targetPath);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task DeleteItem(string path, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, path);
            File.Delete(targetPath);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<bool> FolderExists(string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, folderPath);
            return Task.FromResult(Directory.Exists(targetPath));
        }

        /// <inheritdoc/>
        public Task<bool> FileExists(string filePath, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, filePath);
            return Task.FromResult(File.Exists(targetPath));
        }

        /// <inheritdoc/>
        public Task<IEnumerable<ConfigurationFileDto>> GetConfigurationFilesInFolder(string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, folderPath);
            var filePaths = Directory.GetFiles(targetPath).ToList();
            IEnumerable<ConfigurationFileDto> configFileDtos = filePaths.Select(filePath =>
            {
                var configFileDto = new ConfigurationFileDto();
                var filename = Path.GetFileName(filePath);
                var relativePath = Path.GetRelativePath(this.configuration.BasePath, filePath);
                configFileDto.Id = relativePath;
                configFileDto.Path = filename;
                return configFileDto;
            });

            return Task.FromResult(configFileDtos);
        }

        /// <inheritdoc/>
        public Task<byte[]> GetFileContents(string filePath, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, filePath);
            byte[] contents;
            using (FileStream fileStream = File.Open(targetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    contents = memoryStream.ToArray();
                }
            }

            return Task.FromResult(contents);
        }

        public Task<UBindFileInfo?> GetFileInfo(string filePath, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, filePath);
            var fileInfo = new FileInfo(targetPath);
            if (!fileInfo.Exists)
            {
                return Task.FromResult<UBindFileInfo?>(null);
            }

            var uBindFileInfo = new UBindFileInfo
            {
                Path = filePath,
                LastModifiedTimestamp = Instant.FromDateTimeUtc(fileInfo.LastWriteTimeUtc),
                CreatedTimestamp = Instant.FromDateTimeUtc(fileInfo.CreationTimeUtc),
            };
            return Task.FromResult(uBindFileInfo);
        }

        /// <inheritdoc/>
        public Task<string> GetFilesHashStringInFolders(List<string> folderPaths, string authenticationToken, TimeSpan timeout = default)
        {
            string filesHashString = string.Empty;
            Dictionary<string, string> filesAndModifiedDate = new Dictionary<string, string>();
            folderPaths.ForEach(folderPath =>
            {
                var fullFolderPath = Path.Combine(this.configuration.BasePath, folderPath);
                if (Directory.Exists(fullFolderPath))
                {
                    var filePaths = Directory.GetFiles(fullFolderPath).ToList();
                    filePaths.ForEach(filePath =>
                    {
                        filesAndModifiedDate.Add(filePath, File.GetLastWriteTimeUtc(filePath).ToString());
                    });
                }
            });

            filesAndModifiedDate = filesAndModifiedDate.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            var jsonBatchResponseFiles = JsonConvert.SerializeObject(filesAndModifiedDate);
            filesHashString = jsonBatchResponseFiles.GetHashString();
            return Task.FromResult(filesHashString);
        }

        /// <inheritdoc/>
        public Task<string> GetFileStringContents(string path, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, path);
            string contents;
            using (FileStream fileStream = File.Open(targetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    contents = reader.ReadToEnd();
                }
            }

            return Task.FromResult(contents);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<string>> ListFilesInFolder(
            string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, folderPath);
            var files = new DirectoryInfo(targetPath).GetFiles()
                                                     .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
            var relativePaths = files.Select(path => Path.GetRelativePath(targetPath, path.FullName));
            return Task.FromResult(relativePaths);
        }

        public Task<IEnumerable<UBindFileInfo>> ListFilesInfoInFolder(
            string folderPath,
            string authenticationToken,
            TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, folderPath);
            var files = new DirectoryInfo(targetPath).GetFiles()
                                                     .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
            var result = files.Select(f => new UBindFileInfo
            {
                Path = f.FullName,
                LastModifiedTimestamp = Instant.FromDateTimeUtc(f.LastWriteTimeUtc),
                CreatedTimestamp = Instant.FromDateTimeUtc(f.CreationTimeUtc),
            });
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<string>> ListItemsInFolder(string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, folderPath);
            string[] filePaths = Directory.GetFileSystemEntries(targetPath);
            var relativePaths = filePaths.AsEnumerable()
                .Select(path => Path.GetRelativePath(targetPath, path));
            return Task.FromResult(relativePaths);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<string>> ListOfFilesLastModifiedDateTimeInFolder(string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            // TODO: Change this so it returns DateTimes instead, e.g. NodaDate.
            var targetPath = Path.Combine(this.configuration.BasePath, folderPath);
            IEnumerable<string> fileLastWriteTimes = Directory.GetFiles(targetPath)
                .Select(filePath => Directory.GetLastWriteTimeUtc(filePath).ToString());
            return Task.FromResult(fileLastWriteTimes);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<string>> ListSubfoldersInFolder(string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, folderPath);
            string[] folderPaths = Directory.GetDirectories(targetPath);
            var relativePaths = folderPaths.AsEnumerable()
                .Select(path => Path.GetRelativePath(targetPath, path));
            return Task.FromResult(relativePaths);
        }

        /// <inheritdoc/>
        public Task MoveFolder(
            string sourcePath,
            string destinationPath,
            bool createTargetPathIfSourceDoesntExist,
            string authenticationToken,
            TimeSpan timeout = default)
        {
            string fullSourcePath = Path.Combine(this.configuration.BasePath, sourcePath);
            string fullTargetPath = Path.Combine(this.configuration.BasePath, destinationPath);

            if (Directory.Exists(fullTargetPath))
            {
                Directory.Delete(fullTargetPath, true);
            }

            if (Directory.Exists(fullSourcePath))
            {
                Directory.Move(fullSourcePath, fullTargetPath);
            }
            else if (createTargetPathIfSourceDoesntExist)
            {
                Directory.CreateDirectory(fullTargetPath);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task RenameItem(string itemPath, string newName, string authenticationToken, TimeSpan timeout = default)
        {
            var sourceFilePath = Path.Combine(this.configuration.BasePath, itemPath);
            var sourceFileFolder = Path.GetDirectoryName(sourceFilePath);
            var targetFilePath = Path.Combine(sourceFileFolder, newName);
            File.Move(sourceFilePath, targetFilePath);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<Maybe<string>> TryGetFileStringContents(string path, string authenticationToken, TimeSpan timeout = default)
        {
            var sourceFilePath = Path.Combine(this.configuration.BasePath, path);
            if (File.Exists(sourceFilePath))
            {
                string contents;
                using (FileStream fileStream = File.Open(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        contents = reader.ReadToEnd();
                    }
                }

                var result = Maybe<string>.From(contents);
                return Task.FromResult(result);
            }
            else
            {
                return Task.FromResult(Maybe<string>.None);
            }
        }

        /// <inheritdoc/>
        public Task WriteFileContents(
            byte[] fileContent, string destinationPath, string authenticationToken, TimeSpan timeout = default)
        {
            var targetPath = Path.Combine(this.configuration.BasePath, destinationPath);
            File.WriteAllBytes(targetPath, fileContent);
            return Task.CompletedTask;
        }
    }
}
