// <copyright file="ArchiveEntry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.Model
{
    using System;
    using System.Threading.Tasks;
    using Humanizer;
    using Newtonsoft.Json.Linq;
    using NodaTime;

    public abstract class ArchiveEntry : IArchiveEntry
    {
        public abstract bool IsFile { get; }

        public abstract bool IsFolder { get; }

        public abstract string Name { get; }

        public abstract string Path { get; }

        public string Size => this.SizeBytes?.Bytes().Humanize("0.0");

        public abstract long? SizeBytes { get; }

        public string CompressedSize => this.CompressedSizeBytes?.Bytes().Humanize("0.0");

        public abstract long? CompressedSizeBytes { get; }

        public abstract string Comment { get; }

        public abstract Instant? CreatedTimestamp { get; }

        public abstract string CreatedDateTime { get; }

        public abstract long? CreatedTicksSinceEpoch { get; }

        public abstract string CreatedDate { get; }

        public abstract string CreatedTime { get; }

        public abstract Instant? LastModifiedTimestamp { get; }

        public abstract string LastModifiedDateTime { get; }

        public abstract long? LastModifiedTicksSinceEpoch { get; }

        public abstract string LastModifiedDate { get; }

        public abstract string LastModifiedTime { get; }

        /// <inheritdoc/>
        public abstract Task<byte[]> ReadBytes(Func<Task<JObject>> getErrorDataCallback);

        protected string GetDirectoryName(string fullPath)
        {
            if (fullPath.EndsWith("/"))
            {
                fullPath = fullPath.Substring(0, fullPath.Length - 1);
            }

            return System.IO.Path.GetFileName(fullPath);
        }
    }
}
