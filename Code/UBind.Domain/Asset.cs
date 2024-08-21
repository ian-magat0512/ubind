// <copyright file="Asset.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Store the contents of asset in a Release.
    /// </summary>
    public class Asset : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Asset"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="filename">The name of the file.</param>
        /// <param name="fileContent">The file contents model.</param>
        /// <param name="createdTimestamp">The time this entity was created.</param>
        public Asset(Guid tenantId, string filename, Instant fileModifiedTimestamp, FileContent fileContent, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Name = filename;
            this.FileModifiedTimestamp = fileModifiedTimestamp;
            this.FileContentId = fileContent.Id;
            this.FileContent = fileContent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asset"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        public Asset()
            : base(default(Guid), default(Instant))
        {
        }

        public Asset(Asset other)
            : base(Guid.NewGuid(), other.CreatedTimestamp)
        {
            this.Name = other.Name;
            this.FileModifiedTimestamp = other.FileModifiedTimestamp;
            this.FileContentId = other.FileContentId;
            this.FileContent = other.FileContent;
        }

        /// <summary>
        /// Gets or sets the file content Id of an asset.
        /// </summary>
        public Guid FileContentId { get; set; }

        public Instant FileModifiedTimestamp
        {
            get { return Instant.FromUnixTimeTicks(this.FileModifiedTicksSinceEpoch); }
            protected set { this.FileModifiedTicksSinceEpoch = value.ToUnixTimeTicks(); }
        }

        [JsonProperty]
        public long FileModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets filename of an asset.
        /// </summary>
        public string Name { get; private set; }

        public FileContent FileContent { get; set; }
    }
}
