// <copyright file="ClaimVersionFileAttachedEvent.cs" company="uBind">
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
        public class ClaimVersionFileAttachedEvent : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimVersionFileAttachedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="claimVersionId">The ID of the claim version.</param>
            /// <param name="fileAttachment">The file attachment.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public ClaimVersionFileAttachedEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid claimVersionId,
                ClaimFileAttachment fileAttachment,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.VersionId = claimVersionId;
                this.Attachment = fileAttachment;
            }

            [JsonConstructor]
            private ClaimVersionFileAttachedEvent()
            {
            }

            /// <summary>
            /// Gets claim version Id.
            /// </summary>
            [JsonProperty]
            public Guid VersionId { get; private set; }

            /// <summary>
            /// Gets the File attachment.
            /// </summary>
            [JsonProperty]
            public ClaimFileAttachment Attachment { get; private set; }
        }
    }
}
