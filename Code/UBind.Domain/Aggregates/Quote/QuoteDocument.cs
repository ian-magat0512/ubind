// <copyright file="QuoteDocument.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// A document that can be associated with a quote or policy.
    /// </summary>
    public class QuoteDocument : DocumentEntity
    {
        [System.Text.Json.Serialization.JsonConstructor]
        public QuoteDocument(
            Guid id,
            string name,
            string type,
            long sizeInBytes,
            Guid fileContentId,
            long createdTicksSinceEpoch)
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
            this.SizeInBytes = sizeInBytes;
            this.CreatedTicksSinceEpoch = createdTicksSinceEpoch;
            this.FileContentId = fileContentId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDocument"/> class.
        /// </summary>
        /// <param name="name">The document name.</param>
        /// <param name="type">The document type (e.g. PDF, text, etc.</param>
        /// <param name="sizeInBytes">The document size in bytes.</param>
        /// <param name="fileContentId">The ID of the file content.</param>
        /// <param name="createdTimestamp">The time the document was created.</param>
        public QuoteDocument(string name, string type, long sizeInBytes, Guid fileContentId, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Name = name;
            this.Type = type;
            this.SizeInBytes = sizeInBytes;
            this.CreatedTimestamp = createdTimestamp;
            this.FileContentId = fileContentId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDocument"/> class.
        /// Parameterless constructor for EF and JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected QuoteDocument()
            : base(default(Guid), default(Instant))
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
        public long SizeInBytes { get; protected set; }

        /// <summary>
        /// Gets or sets the ID of the file content.
        /// </summary>
        [JsonProperty]
        public Guid FileContentId { get; protected set; }
    }
}
