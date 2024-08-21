// <copyright file="SharpZipLibZipArchive.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.Model
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ICSharpCode.SharpZipLib.Zip;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// The SharpZipLibZipArchive supports passwords, listing contents and
    /// extracting files, but does not support modification.
    /// </summary>
    public class SharpZipLibZipArchive : IArchive, IDisposable
    {
        private readonly IClock clock;

        public SharpZipLibZipArchive(ZipFile zipFile, System.IO.MemoryStream memoryStream, IClock clock)
        {
            this.ZipFile = zipFile;
            this.MemoryStream = memoryStream;
            this.clock = clock;
        }

        public ZipFile ZipFile { get; }

        /// <summary>
        /// Gets the memory stream for reading the Zip archive's raw bytes.
        /// </summary>
        public System.IO.MemoryStream MemoryStream { get; }

        public Instant? CreatedTimestamp { get; set; }

        public Instant? LastModifiedTimestamp { get; set; }

        public static async Task<SharpZipLibZipArchive> Open(
            FileInfo sourceFile,
            string password,
            IClock clock,
            Func<Task<JObject>> getErrorDataCallback)
        {
            var memoryStream = new System.IO.MemoryStream();
            memoryStream.Write(sourceFile.Content, 0, sourceFile.Content.Length);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            try
            {
                var zipFile = new ZipFile(memoryStream);
                if (password != null)
                {
                    zipFile.Password = password;
                }

                var archive = new SharpZipLibZipArchive(zipFile, memoryStream, clock);
                archive.CreatedTimestamp = archive.CreatedTimestamp ?? sourceFile.CreatedTimestamp;
                archive.LastModifiedTimestamp = archive.LastModifiedTimestamp ?? sourceFile.LastModifiedTimestamp;
                return archive;
            }
            catch (ZipException ex) when (ex.Message.Equals("Cannot find central directory"))
            {
                var errorData = await getErrorDataCallback();
                throw new ErrorException(Errors.Automation.Archive.NotAValidZipArchive(ex.Message.WithDot(), errorData));
            }
        }

        public static SharpZipLibZipArchive Create(string password, IClock clock)
        {
            var memoryStream = new System.IO.MemoryStream();
            var zipFile = ZipFile.Create(memoryStream);
            if (password != null)
            {
                zipFile.Password = password;
            }

            var archive = new SharpZipLibZipArchive(zipFile, memoryStream, clock);
            archive.CreatedTimestamp = archive.LastModifiedTimestamp = clock.Now();
            return archive;
        }

        public IArchiveEntry GetEntry(string path)
        {
            var zipEntry = this.ZipFile.GetEntry(path);
            return zipEntry != null
                ? new SharpZipLibZipArchiveEntry(this, zipEntry)
                : (IArchiveEntry)null;
        }

        public IArchiveEntry FindEntry(string path, bool ignoreCase = false)
        {
            foreach (ZipEntry zipEntry in this.ZipFile)
            {
                string name = ZipEntry.CleanName(zipEntry.Name);
                if ((!ignoreCase && name == path) || (ignoreCase && name.EqualsIgnoreCase(path)))
                {
                    return new SharpZipLibZipArchiveEntry(this, zipEntry);
                }
            }

            return null;
        }

        public IEnumerator<IArchiveEntry> GetEnumerator()
        {
            return new SharpZipLibZipArchiveEnumerator(this);
        }

        public byte[] ReadBytes(Func<Task<JObject>> getErrorDataCallback)
        {
            return this.MemoryStream.ToArray();
        }

        public IArchiveEntry AddFolder(string path)
        {
            throw new NotImplementedException("The SharpZipLibZipArchive supports passwords, listing contents and "
                + "extracting files, but does not support modification.");
        }

        public void RemoveEntry(IArchiveEntry entry)
        {
            throw new NotImplementedException("The SharpZipLibZipArchive supports passwords, listing contents and "
                + "extracting files, but does not support modification.");
        }

        public IArchiveEntry MoveEntry(IArchiveEntry entry, string newFullPath)
        {
            throw new NotImplementedException("The SharpZipLibZipArchive supports passwords, listing contents and "
                + "extracting files, but does not support modification.");
        }

        public IArchiveEntry MoveFile(IArchiveEntry entry, string newFullPath)
        {
            throw new NotImplementedException("The SharpZipLibZipArchive supports passwords, listing contents and "
                + "extracting files, but does not support modification.");
        }

        public IArchiveEntry CopyFile(IArchiveEntry entry, string destinationPath)
        {
            throw new NotImplementedException("The SharpZipLibZipArchive supports passwords, listing contents and "
                + "extracting files, but does not support modification.");
        }

        public IArchiveEntry MoveFolder(IArchiveEntry entry, string newFullPath)
        {
            throw new NotImplementedException("The SharpZipLibZipArchive supports passwords, listing contents and "
                + "extracting files, but does not support modification.");
        }

        public IArchiveEntry CopyFolder(IArchiveEntry entry, string newFullPath)
        {
            throw new NotImplementedException("The SharpZipLibZipArchive supports passwords, listing contents and "
                + "extracting files, but does not support modification.");
        }

        public void Dispose()
        {
            this.ZipFile.IsStreamOwner = false;
            this.MemoryStream.Dispose();
        }

        public bool PathExists(string path)
        {
            foreach (ZipEntry zipEntry in this.ZipFile)
            {
                if (zipEntry.Name.StartsWith(path))
                {
                    return true;
                }
            }

            return false;
        }

        public int RemoveEntries(string path)
        {
            throw new NotImplementedException("The SharpZipLibZipArchive supports passwords, listing contents and "
                + "extracting files, but does not support modification.");
        }

        public IArchiveEntry AddFile(
            string path,
            byte[] content,
            Instant? createdTimestamp = null,
            Instant? lastModifiedTimestamp = null,
            System.IO.Compression.CompressionLevel compressionLevel = System.IO.Compression.CompressionLevel.Optimal,
            string comment = null)
        {
            throw new NotImplementedException("The SharpZipLibZipArchive supports passwords, listing contents and "
                + "extracting files, but does not support modification.");
        }

        public IArchiveEntry MoveFolder(string oldPath, string newPath)
        {
            throw new NotImplementedException("The SharpZipLibZipArchive supports passwords, listing contents and "
                + "extracting files, but does not support modification.");
        }

        public IArchiveEntry CopyFolder(string oldPath, string newPath)
        {
            throw new NotImplementedException("The SharpZipLibZipArchive supports passwords, listing contents and "
                + "extracting files, but does not support modification.");
        }
    }
}
