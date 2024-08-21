// <copyright file="ClaimFileAttachment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// A document that can be associated with a claim or claim version.
    /// </summary>
    public class ClaimFileAttachment : DocumentEntity, IFileAttachment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimFileAttachment"/> class.
        /// </summary>
        /// <param name="id">The claim file attachment ID.</param>
        /// <param name="name">The document name.</param>
        /// <param name="type">The document type (e.g. PDF, text, etc.</param>
        /// <param name="fileSize">The document size in bytes.</param>
        /// <param name="fileContentId">The fileContent Id.</param>
        /// <param name="createdTimestamp">The time the document was created.</param>
        public ClaimFileAttachment(
            Guid id,
            string name,
            string type,
            long fileSize,
            Guid fileContentId,
            Instant createdTimestamp)
            : base(id, createdTimestamp)
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
            this.FileSize = fileSize;
            this.FileContentId = fileContentId;
            this.CreatedTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimFileAttachment"/> class.
        /// </summary>
        /// <param name="name">The document name.</param>
        /// <param name="type">The document type (e.g. PDF, text, etc.</param>
        /// <param name="fileSize">The document size in bytes.</param>
        /// <param name="fileContentId">The fileContent Id.</param>
        /// <param name="createdTimestamp">The time the document was created.</param>
        public ClaimFileAttachment(
            string name,
            string type,
            long fileSize,
            Guid fileContentId,
            Instant createdTimestamp)
            : this(Guid.NewGuid(), name, type, fileSize, fileContentId, createdTimestamp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimFileAttachment"/> class.
        /// Parameterless constructor for EF and JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ClaimFileAttachment()
        {
        }

        /// <summary>
        /// Gets or sets the document name.
        /// </summary>
        [JsonProperty]
        public string Name { get; protected set; }

        /// <summary>
        /// Gets or sets the document type.
        /// </summary>
        [JsonProperty]
        public string Type { get; protected set; }

        /// <summary>
        /// Gets or sets the document size.
        /// </summary>
        [JsonProperty]
        public long FileSize { get; protected set; }

        /// <summary>
        /// Gets or sets the ID of the file content.
        /// </summary>
        [JsonProperty]
        public Guid FileContentId { get; protected set; }
    }
}
