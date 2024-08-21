// <copyright file="IArchiveEntry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.Model
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;

    /// <summary>
    /// Represents an entry in an archive.
    /// </summary>
    public interface IArchiveEntry
    {
        [JsonProperty(PropertyName = "isFile")]
        bool IsFile { get; }

        [JsonProperty(PropertyName = "isFolder")]
        bool IsFolder { get; }

        /// <summary>
        /// Gets the file name, including the file extension, but without any path information.
        /// If this ArchiveEntry represents a directory, this will be the directory name.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        string Name { get; }

        /// <summary>
        /// Gets the full file path, including the file name and extension.
        /// </summary>
        [JsonProperty(PropertyName = "path")]
        string Path { get; }

        /// <summary>
        /// Gets a human readable size.
        /// </summary>
        [JsonProperty(PropertyName = "size", NullValueHandling = NullValueHandling.Ignore)]
        string Size { get; }

        [JsonProperty(PropertyName = "sizeBytes", NullValueHandling = NullValueHandling.Ignore)]
        long? SizeBytes { get; }

        /// <summary>
        /// Gets a human readable size of the compressed file.
        /// </summary>
        [JsonProperty(PropertyName = "compressedSize", NullValueHandling = NullValueHandling.Ignore)]
        string CompressedSize { get; }

        [JsonProperty(PropertyName = "compressedSizeBytes", NullValueHandling = NullValueHandling.Ignore)]
        long? CompressedSizeBytes { get; }

        [JsonProperty(PropertyName = "comment", NullValueHandling = NullValueHandling.Ignore)]
        string Comment { get; }

        [JsonIgnore]
        Instant? CreatedTimestamp { get; }

        /// <summary>
        /// Gets the ISO-8601 formatted date time string representing the date and time the file was last
        /// modified.
        /// </summary>
        [JsonProperty(PropertyName = "createdDateTime", NullValueHandling = NullValueHandling.Ignore)]
        string CreatedDateTime { get; }

        /// <summary>
        /// Gets the number of ticks since the epoch representing the date and time the file was last
        /// modified.
        /// </summary>
        [JsonProperty(PropertyName = "createdTicksSinceEpoch", NullValueHandling = NullValueHandling.Ignore)]
        long? CreatedTicksSinceEpoch { get; }

        /// <summary>
        /// Gets the date the file was last modified.
        /// </summary>
        [JsonProperty(PropertyName = "createdDate", NullValueHandling = NullValueHandling.Ignore)]
        string CreatedDate { get; }

        /// <summary>
        /// Gets the time of day the file was last modified.
        /// </summary>
        [JsonProperty(PropertyName = "createdTime", NullValueHandling = NullValueHandling.Ignore)]
        string CreatedTime { get; }

        [JsonIgnore]
        Instant? LastModifiedTimestamp { get; }

        /// <summary>
        /// Gets the ISO-8601 formatted date time string representing the date and time the file was last
        /// modified.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedDateTime", NullValueHandling = NullValueHandling.Ignore)]
        string LastModifiedDateTime { get; }

        /// <summary>
        /// Gets the number of ticks since the epoch representing the date and time the file was last
        /// modified.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedTicksSinceEpoch", NullValueHandling = NullValueHandling.Ignore)]
        long? LastModifiedTicksSinceEpoch { get; }

        /// <summary>
        /// Gets the date the file was last modified.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedDate", NullValueHandling = NullValueHandling.Ignore)]
        string LastModifiedDate { get; }

        /// <summary>
        /// Gets the time of day the file was last modified.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedTime", NullValueHandling = NullValueHandling.Ignore)]
        string LastModifiedTime { get; }

        /// <summary>
        /// Reads the uncompressed data.
        /// </summary>
        /// <param name="getErrorDataCallback">A callback which returns a JObject of contextual error information
        /// should the operation fail.
        /// </param>
        /// <returns>A byte array of the uncompressed data.</returns>
        Task<byte[]> ReadBytes(Func<Task<JObject>> getErrorDataCallback);
    }
}
