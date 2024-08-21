// <copyright file="ClaimFileAttachedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Entities;

    /// <summary>
    /// Aggregate for claims.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// Event raised when a file has been attached to a claim.
        /// </summary>
        public class ClaimFileAttachedEvent : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimFileAttachedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="fileAttachment">The file attachment.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public ClaimFileAttachedEvent(
                Guid tenantId,
                Guid aggregateId,
                ClaimFileAttachment fileAttachment,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.Attachment = fileAttachment;
            }

            [JsonConstructor]
            private ClaimFileAttachedEvent()
            {
            }

            /// <summary>
            /// Gets attachment Id.
            /// </summary>
            [JsonProperty]
            public Guid AttachmentId { get; private set; }

            /// <summary>
            /// Gets the File attachment.
            /// </summary>
            [JsonProperty]
            public ClaimFileAttachment Attachment { get; private set; }
        }
    }
}
