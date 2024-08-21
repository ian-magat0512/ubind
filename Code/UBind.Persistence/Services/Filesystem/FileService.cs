// <copyright file="FileService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Services.Filesystem
{
    using System;
    using System.IO.Abstractions;
    using System.Security.Cryptography;
    using UBind.Domain.Repositories.FileSystem;

    /// <inheritdoc/>
    public class FileService : IFileSystemService
    {
        private readonly IFileSystem fileSystem;
        private readonly IFileSystemFileCompressionService filesystemFileCompressionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        /// <param name="fileSystem">The FileSystem abstraction.</param>
        /// <param name="filesystemFileCompressionService">The filesystem file compression service.</param>
        public FileService(IFileSystem fileSystem, IFileSystemFileCompressionService filesystemFileCompressionService)
        {
            this.fileSystem = fileSystem;
            this.filesystemFileCompressionService = filesystemFileCompressionService;
        }

        /// <inheritdoc/>
        public IDirectory Directory => this.fileSystem.Directory;

        /// <inheritdoc/>
        public IFile File => this.fileSystem.File;

        /// <inheritdoc/>
        public void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName)
        {
            this.filesystemFileCompressionService.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName);
        }

        /// <inheritdoc/>
        public string GetFileMd5Hash(string path)
        {
            using (var md5Instance = MD5.Create())
            {
                using (var stream = this.File.OpenRead(path))
                {
                    var hashResult = md5Instance.ComputeHash(stream);
                    return BitConverter.ToString(hashResult).Replace("-", string.Empty);
                }
            }
        }
    }
}
