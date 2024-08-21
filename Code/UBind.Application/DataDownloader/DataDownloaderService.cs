// <copyright file="DataDownloaderService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.DataDownloader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentFTP;
    using Flurl.Http;
    using Renci.SshNet;
    using Renci.SshNet.Sftp;
    using UBind.Domain.Repositories.FileSystem;

    /// <summary>
    /// Provides a common interface for filesystem operations, which can be implemented by different providers
    /// including local disk.
    /// </summary>
    public class DataDownloaderService : IDataDownloaderService
    {
        private readonly IFtpClientFactory ftpClientFactory;
        private readonly IFtpConfiguration ftpConfiguration;
        private readonly IFileSystemService fileSystemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataDownloaderService"/> class.
        /// </summary>
        /// <param name="ftpClientFactory">The FTP client factory.</param>
        /// <param name="ftpConfiguration">The ftp configuration.</param>
        /// <param name="fileSystemService">The file system service.</param>
        public DataDownloaderService(IFtpClientFactory ftpClientFactory, IFtpConfiguration ftpConfiguration, IFileSystemService fileSystemService)
        {
            this.ftpClientFactory = ftpClientFactory;
            this.ftpConfiguration = ftpConfiguration;
            this.fileSystemService = fileSystemService;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<(string Filename, string FileHash)>> DownloadFilesAsync(string downloadProfile, DataDownloaderProtocol dataDownloaderProtocol, string workingFolder, IReadOnlyList<(string Url, string FileHash, string fileName)> downloadUrls, int downloadBufferSize, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (dataDownloaderProtocol)
            {
                case DataDownloaderProtocol.Http:
                    {
                        return await this.HttpDownloadFilesAsync(workingFolder, downloadUrls, downloadBufferSize, cancellationToken);
                    }

                case DataDownloaderProtocol.Ftp:
                    {
                        return await this.FtpDownloadFilesAsync(downloadProfile, workingFolder, cancellationToken);
                    }

                case DataDownloaderProtocol.Sftp:
                    {
                        return await this.SftpDownloadFilesAsync(downloadProfile, workingFolder, cancellationToken);
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task<IReadOnlyList<(string Filename, string FileHash)>> FtpDownloadFilesAsync(
            string downloadProfile,
            string workingFolder,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var returnFileList = new List<(string Filename, string FileHash)>();

            this.ftpConfiguration.Connections.TryGetValue(downloadProfile, out var ftpConnectionConfig);

            if (ftpConnectionConfig == null)
            {
                return returnFileList;
            }

            var ftpClient = this.ftpClientFactory.GetNewFtpClient(ftpConnectionConfig);
            await ftpClient.ConnectAsync(cancellationToken);

            foreach (var item in await ftpClient.GetListingAsync(ftpConnectionConfig.DefaultRemoteDirectory, cancellationToken))
            {
                if (item.Type != FtpFileSystemObjectType.File)
                {
                    continue;
                }

                var destinationFile = Path.Combine(workingFolder, item.Name);

                if ((await ftpClient.DownloadFileAsync(destinationFile, item.FullName, token: cancellationToken)) !=
                    FtpStatus.Success)
                {
                    continue;
                }

                var md5Value = this.fileSystemService.GetFileMd5Hash(destinationFile);
                var md5FileName = Path.Combine(workingFolder, $"{item.Name}-md5-{md5Value}.txt").ToLower();
                this.fileSystemService.File.AppendAllText(md5FileName, $"MD5: {md5Value}");

                returnFileList.Add(($"ftp://{ftpConnectionConfig.Host}/{item.FullName}", md5FileName));
            }

            return returnFileList;
        }

        private async Task<IReadOnlyList<(string Filename, string FileHash)>> SftpDownloadFilesAsync(
            string downloadProfile,
            string workingFolder,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var returnFileList = new List<(string Filename, string FileHash)>();

            this.ftpConfiguration.Connections.TryGetValue(downloadProfile, out var ftpConnectionConfig);

            if (ftpConnectionConfig == null)
            {
                return returnFileList;
            }

            using SftpClient sFtpClient = new SftpClient(ftpConnectionConfig.Host, ftpConnectionConfig.Username, ftpConnectionConfig.Password);
            sFtpClient.Connect();

            var files = new List<SftpFile>();
            foreach (var item in sFtpClient.ListDirectory(ftpConnectionConfig.DefaultRemoteDirectory))
            {
                if (item.IsRegularFile)
                {
                    files.Add(item);
                }
            }

            files = files.OrderBy(f => f.LastWriteTime).ToList();
            var tasks = new List<Task>();
            if (files.Count > 0)
            {
                var latestFile = files.Last();
                var destinationFile = Path.Combine(workingFolder, latestFile.Name);
                tasks.Add(Task.Run(async () =>
                {
                    using (var stream = File.Create(destinationFile))
                    {
                        var task = Task.Factory.FromAsync(sFtpClient.BeginDownloadFile(latestFile.FullName, stream), sFtpClient.EndDownloadFile);
                        await task;
                    }

                    var md5Value = this.fileSystemService.GetFileMd5Hash(destinationFile);
                    var md5FileName = Path.Combine(workingFolder, $"{latestFile.Name}-md5-{md5Value}.txt").ToLower();
                    this.fileSystemService.File.AppendAllText(md5FileName, $"MD5: {md5Value}");

                    returnFileList.Add(($"ftp://{ftpConnectionConfig.Host}/{latestFile.FullName}", md5FileName));
                }));
            }

            await Task.WhenAll(tasks);
            return returnFileList;
        }

        private async Task<IReadOnlyList<(string Filename, string FileHash)>> HttpDownloadFilesAsync(
            string workingFolder,
            IEnumerable<(string Url, string FileHash, string fileName)> downloadUrls,
            int downloadBufferSize,
            CancellationToken cancellationToken)
        {
            var returnFileList = new List<(string fileName, string fileHash)>();

            foreach (var downloadManifest in downloadUrls)
            {
                var localPath = await downloadManifest.Url.DownloadFileAsync(workingFolder, null, downloadBufferSize, cancellationToken);

                var md5Value = this.fileSystemService.GetFileMd5Hash(localPath);
                var md5FileName = Path.Combine(workingFolder, $"{Path.GetFileName(localPath)}-md5-{md5Value}.txt").ToLower();
                this.fileSystemService.File.AppendAllText(md5FileName, $"MD5: {md5Value}");

                returnFileList.Add((localPath, md5FileName));
            }

            return returnFileList;
        }
    }
}
