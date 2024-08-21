// <copyright file="ArchiveEntryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers.File.Model;

    public class ArchiveEntryModel
    {
        /// <summary>
        /// Constructs and archive entry from a real IArchiveEntry within a archive file (e.g. a zip file).
        /// This is needed so that we can return an model for automations to use after we close the zip file.
        /// If we didn't do this then we'd get an ObjectDisposed exception trying to access the IArchiveEntry.
        /// </summary>
        /// <param name="resource">The archive entry within the archvie file that will soon be disposed of.</param>
        public ArchiveEntryModel(IArchiveEntry resource)
        {
            this.IsFile = resource.IsFile;
            this.IsFolder = resource.IsFolder;
            this.Name = resource.Name;
            this.Path = resource.Path;
            this.Size = resource.Size;
            this.SizeBytes = resource.SizeBytes;
            this.CompressedSize = resource.CompressedSize;
            this.CompressedSizeBytes = resource.CompressedSizeBytes;
            this.Comment = resource.Comment;
            this.CreatedDateTime = resource.CreatedDateTime;
            this.CreatedTicksSinceEpoch = resource.CreatedTicksSinceEpoch;
            this.CreatedDate = resource.CreatedDate;
            this.CreatedTime = resource.CreatedTime;
            this.LastModifiedDateTime = resource.LastModifiedDateTime;
            this.LastModifiedTicksSinceEpoch = resource.LastModifiedTicksSinceEpoch;
            this.LastModifiedDate = resource.LastModifiedDate;
            this.LastModifiedTime = resource.LastModifiedTime;
        }

        [JsonProperty(PropertyName = "isFile")]
        public bool IsFile { get; }

        [JsonProperty(PropertyName = "isFolder")]
        public bool IsFolder { get; }

        /// <summary>
        /// Gets the file name, including the file extension, but without any path information.
        /// If this ArchiveEntry represents a directory, this will be the directory name.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; }

        /// <summary>
        /// Gets the full file path, including the file name and extension.
        /// </summary>
        [JsonProperty(PropertyName = "path")]
        public string Path { get; }

        /// <summary>
        /// Gets a human readable size.
        /// </summary>
        [JsonProperty(PropertyName = "size", NullValueHandling = NullValueHandling.Ignore)]
        public string Size { get; }

        [JsonProperty(PropertyName = "sizeBytes", NullValueHandling = NullValueHandling.Ignore)]
        public long? SizeBytes { get; }

        /// <summary>
        /// Gets a human readable size of the compressed file.
        /// </summary>
        [JsonProperty(PropertyName = "compressedSize", NullValueHandling = NullValueHandling.Ignore)]
        public string CompressedSize { get; }

        [JsonProperty(PropertyName = "compressedSizeBytes", NullValueHandling = NullValueHandling.Ignore)]
        public long? CompressedSizeBytes { get; }

        [JsonProperty(PropertyName = "comment", NullValueHandling = NullValueHandling.Ignore)]
        public string Comment { get; }

        /// <summary>
        /// Gets the ISO-8601 formatted date time string representing the date and time the file was last
        /// modified.
        /// </summary>
        [JsonProperty(PropertyName = "createdDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public string CreatedDateTime { get; }

        /// <summary>
        /// Gets the number of ticks since the epoch representing the date and time the file was last
        /// modified.
        /// </summary>
        [JsonProperty(PropertyName = "createdTicksSinceEpoch", NullValueHandling = NullValueHandling.Ignore)]
        public long? CreatedTicksSinceEpoch { get; }

        /// <summary>
        /// Gets the date the file was last modified.
        /// </summary>
        [JsonProperty(PropertyName = "createdDate", NullValueHandling = NullValueHandling.Ignore)]
        public string CreatedDate { get; }

        /// <summary>
        /// Gets the time of day the file was last modified.
        /// </summary>
        [JsonProperty(PropertyName = "createdTime", NullValueHandling = NullValueHandling.Ignore)]
        public string CreatedTime { get; }

        /// <summary>
        /// Gets the ISO-8601 formatted date time string representing the date and time the file was last
        /// modified.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public string LastModifiedDateTime { get; }

        /// <summary>
        /// Gets the number of ticks since the epoch representing the date and time the file was last
        /// modified.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedTicksSinceEpoch", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastModifiedTicksSinceEpoch { get; }

        /// <summary>
        /// Gets the date the file was last modified.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedDate", NullValueHandling = NullValueHandling.Ignore)]
        public string LastModifiedDate { get; }

        /// <summary>
        /// Gets the time of day the file was last modified.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedTime", NullValueHandling = NullValueHandling.Ignore)]
        public string LastModifiedTime { get; }
    }
}
