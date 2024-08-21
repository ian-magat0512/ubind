// <copyright file="FileAttachedEvent.cs" company="uBind">
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
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when a file has been attached to a quote.
        /// </summary>
        public class FileAttachedEvent : Event<QuoteAggregate, Guid>
        {
            private Guid quoteId;

            /// <summary>
            /// Initializes a new instance of the <see cref="FileAttachedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="quoteId">The ID of the quote.</param>
            /// <param name="attachmentId">The quote file attachment ID.</param>
            /// <param name="name">The attachment filename.</param>
            /// <param name="type">The attachment file type.</param>
            /// <param name="fileContentId">The file content ID to refer to the actual file content.</param>
            /// <param name="fileSize">The attachment file size.</param>
            /// <param name="performingUserId">The userId who attached the file.</param>
            /// <param name="createdTimestamp">The created timestamp.</param>
            public FileAttachedEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid quoteId,
                Guid attachmentId,
                string name,
                string type,
                Guid fileContentId,
                long fileSize,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.AttachmentId = attachmentId;
                this.QuoteId = quoteId;
                this.Name = name;
                this.Type = type;
                this.FileContentId = fileContentId;
                this.FileSize = fileSize;
            }

            [JsonConstructor]
            private FileAttachedEvent()
            {
            }

            /// <summary>
            /// Gets an ID uniquely identifying the quote.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId
            {
                get
                {
                    // Quote ID will not exist for quotes created before introduction of renewals and updates, in which case the ID
                    // of the aggregate will be used.
                    return this.quoteId == default
                        ? this.AggregateId
                        : this.quoteId;
                }

                private set
                {
                    this.quoteId = value;
                }
            }

            /// <summary>
            /// Gets the attachment ID.
            /// </summary>
            [JsonProperty]
            public Guid AttachmentId { get; private set; }

            /// <summary>
            /// Gets the file name.
            /// </summary>
            [JsonProperty]
            public string Name { get; private set; }

            /// <summary>
            /// Gets the file type.
            /// </summary>
            [JsonProperty]
            public string Type { get; private set; }

            /// <summary>
            /// Gets the file content ID.
            /// </summary>
            [JsonProperty]
            public Guid FileContentId { get; private set; }

            /// <summary>
            /// Gets the file size.
            /// </summary>
            [JsonProperty]
            public long FileSize { get; private set; }
        }
    }
}
