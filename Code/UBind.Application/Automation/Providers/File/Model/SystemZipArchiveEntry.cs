// <copyright file="SystemZipArchiveEntry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.Model
{
    using System;
    using System.IO.Compression;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.SerialisedEntitySchemaObject;

    public class SystemZipArchiveEntry : ArchiveEntry
    {
        private ZipArchiveEntry zipEntry;
        private SystemZipArchive zipArchive;
        private DateTimeVariants lastModified;

        public SystemZipArchiveEntry(SystemZipArchive zipArchive, ZipArchiveEntry zipEntry)
        {
            this.zipArchive = zipArchive;
            this.zipEntry = zipEntry;
        }

        /// <summary>
        /// Gets the inner zip entry used by System.IO.Compression.
        /// </summary>
        public ZipArchiveEntry ZipEntry => this.zipEntry;

        public override bool IsFile => !this.IsFolder;

        public override bool IsFolder => this.Path.EndsWith("/");

        public override string Name
        {
            get
            {
                return this.IsFile
                    ? System.IO.Path.GetFileName(this.Path)
                    : this.GetDirectoryName(this.Path);
            }
        }

        public override string Path => this.zipEntry.FullName;

        public override long? SizeBytes => this.IsFile ? this.zipEntry.Length : (long?)null;

        public override long? CompressedSizeBytes => this.IsFile ? this.zipEntry.CompressedLength : (long?)null;

        /// <summary>
        /// Gets the comment associated with the archive entry.
        /// TODO: Comment support was added in .NET 7 so we can add this when we upgrade to it.
        /// </summary>
        public override string Comment => null;

        public override Instant? CreatedTimestamp => null;

        public override string CreatedDateTime => null;

        public override long? CreatedTicksSinceEpoch => null;

        public override string CreatedDate => null;

        public override string CreatedTime => null;

        public override Instant? LastModifiedTimestamp => this.LastModified.Timestamp;

        public override string LastModifiedDateTime => this.LastModified.DateTime;

        public override long? LastModifiedTicksSinceEpoch => this.LastModified.TicksSinceEpoch;

        public override string LastModifiedDate => this.LastModified.Date;

        public override string LastModifiedTime => this.LastModified.Time;

        private DateTimeVariants LastModified
        {
            get
            {
                if (this.lastModified == null)
                {
                    this.lastModified = DateTimeVariants.CreateFromDateTimeOffset(this.zipEntry.LastWriteTime);
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
                    using (var zipStream = this.zipEntry.Open())
                    {
                        zipStream.CopyTo(memoryStream);
                    }

                    return memoryStream.ToArray();
                }
            }
            catch (System.IO.InvalidDataException ex) when (ex.Message == "Block length does not match with its complement.")
            {
                var errorData = await getErrorDataCallback();
                throw new ErrorException(Errors.Automation.Archive.NoPasswordSupplied(errorData));
            }
        }
    }
}
