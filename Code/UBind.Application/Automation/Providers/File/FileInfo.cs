// <copyright file="FileInfo.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.JsonConverters;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// This class represents the information for a file being used by automations.
    /// </summary>
    public class FileInfo
    {
        public FileInfo(string fileName, byte[] content, Instant? createdTimestamp = null, Instant? lastModifiedTimestamp = null)
        {
            this.FileName = new FileName(fileName);
            this.Content = content;
            this.CreatedTimestamp = createdTimestamp;
            this.LastModifiedTimestamp = lastModifiedTimestamp;
        }

        public byte[] Content { get; }

        [JsonConverter(typeof(StringObjectConverter<FileName>))]
        public FileName FileName { get; }

        public Instant? CreatedTimestamp { get; }

        public Instant? LastModifiedTimestamp { get; }
    }
}
