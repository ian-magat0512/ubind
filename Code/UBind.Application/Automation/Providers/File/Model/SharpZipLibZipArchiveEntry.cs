// <copyright file="SharpZipLibZipArchiveEntry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.Model
{
    using System;
    using System.Threading.Tasks;
    using ICSharpCode.SharpZipLib.Zip;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// The SharpZipLibZipArchiveEntry is an entry for the SharpZipLibZipArchive, which supports listing contents and
    /// extracting files, but does not support modification. It also has password support.
    /// </summary>
    public class SharpZipLibZipArchiveEntry : ArchiveEntry
    {
        private ZipEntry zipEntry;
        private SharpZipLibZipArchive zipArchive;
        private DateTimeVariants lastModified;

        public SharpZipLibZipArchiveEntry(SharpZipLibZipArchive zipArchive, ZipEntry zipEntry)
        {
            this.zipArchive = zipArchive;
            this.zipEntry = zipEntry;
        }

        /// <summary>
        /// Gets the inner zip entry used by SharpZipLib.
        /// </summary>
        public ZipEntry ZipEntry => this.zipEntry;

        public override bool IsFile => this.zipEntry.IsFile;

        public override bool IsFolder => this.zipEntry.IsDirectory;

        public override string Name
        {
            get
            {
                return this.IsFile
                    ? System.IO.Path.GetFileName(this.Path)
                    : this.GetDirectoryName(this.Path);
            }
        }

        public override string Path => ZipEntry.CleanName(this.zipEntry.Name);

        public override long? SizeBytes => this.IsFile ? this.zipEntry.Size : (long?)null;

        public override long? CompressedSizeBytes => this.IsFile ? this.zipEntry.CompressedSize : (long?)null;

        public override string Comment => this.zipEntry.Comment;

        public override Instant? LastModifiedTimestamp => this.LastModified.Timestamp;

        public override string LastModifiedDateTime => this.LastModified.DateTime;

        public override long? LastModifiedTicksSinceEpoch => this.LastModified.TicksSinceEpoch.Value;

        public override string LastModifiedDate => this.LastModified.Date;

        public override string LastModifiedTime => this.LastModified.Time;

        public override Instant? CreatedTimestamp => null;

        public override string CreatedDateTime => null;

        public override long? CreatedTicksSinceEpoch => null;

        public override string CreatedDate => null;

        public override string CreatedTime => null;

        private DateTimeVariants LastModified
        {
            get
            {
                if (this.lastModified == null)
                {
                    this.lastModified = DateTimeVariants.CreateFromDateTime(this.zipEntry.DateTime);
                }

                return this.lastModified;
            }
        }

        public override async Task<byte[]> ReadBytes(Func<Task<JObject>> getErrorDataCallback)
        {
            // read the file into memory
            int capacity = this.SizeBytes > -1
                ? unchecked((int)this.SizeBytes)
                : -1;
            System.IO.MemoryStream memoryStream = capacity > -1
                ? new System.IO.MemoryStream(capacity)
                : new System.IO.MemoryStream();

            try
            {
                using (memoryStream)
                {
                    using (var zipStream = this.zipArchive.ZipFile.GetInputStream(this.zipEntry))
                    {
                        zipStream.CopyTo(memoryStream);
                    }

                    return memoryStream.ToArray();
                }
            }
            catch (ZipException ex) when (ex.Message == "No password available for encrypted stream")
            {
                var errorData = await getErrorDataCallback();
                throw new ErrorException(Errors.Automation.Archive.NoPasswordSupplied(errorData));
            }
            catch (ZipException ex) when (ex.Message == "Invalid password")
            {
                var errorData = await getErrorDataCallback();
                throw new ErrorException(Errors.Automation.Archive.WrongPasswordSupplied(errorData));
            }
        }
    }
}
