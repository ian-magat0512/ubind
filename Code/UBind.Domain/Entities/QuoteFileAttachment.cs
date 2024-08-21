// <copyright file="QuoteFileAttachment.cs" company="uBind">
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
    /// A document that can be associated with a Quote or Quote version.
    /// </summary>
    public class QuoteFileAttachment : Entity<Guid>, IFileAttachment, IReadModel<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteFileAttachment"/> class.
        /// </summary>
        /// <param name="id">The quote file attachment Id.</param>
        /// <param name="quoteId">The Quote ID.</param>
        /// <param name="fileContentId">The fileContent Id.</param>
        /// <param name="name">The document name.</param>
        /// <param name="type">The document type (e.g. PDF, text, etc.</param>
        /// <param name="fileSize">The document size in bytes.</param>
        /// <param name="createdTimestamp">The time the document was created.</param>
        public QuoteFileAttachment(
            Guid tenantId,
            Guid id,
            Guid quoteId,
            Guid fileContentId,
            string name,
            string type,
            long fileSize,
            Instant createdTimestamp)
            : base(id, createdTimestamp)
        {
            this.TenantId = tenantId;
            this.QuoteId = quoteId;
            this.FileContentId = fileContentId;
            this.Name = name;
            this.Type = type;
            this.FileSize = fileSize;
        }

        [System.Text.Json.Serialization.JsonConstructor]
        public QuoteFileAttachment(
            Guid tenantId,
            Guid id,
            Guid quoteId,
            Guid fileContentId,
            string name,
            string type,
            long fileSize,
            long createdTicksSinceEpoch)
        {
            this.Id = id;
            this.TenantId = tenantId;
            this.QuoteId = quoteId;
            this.FileContentId = fileContentId;
            this.Name = name;
            this.Type = type;
            this.FileSize = fileSize;
            this.CreatedTicksSinceEpoch = createdTicksSinceEpoch;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteFileAttachment"/> class.
        /// Parameterless constructor for EF and JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected QuoteFileAttachment()
        {
        }

        /// <summary>
        /// Gets or sets the tenant Id.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the quote ID.
        /// </summary>
        [JsonProperty]
        public Guid QuoteId { get; protected set; }

        /// <summary>
        /// Gets or sets the ID of the file content.
        /// </summary>
        [JsonProperty]
        public Guid FileContentId { get; protected set; }

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
    }
}
