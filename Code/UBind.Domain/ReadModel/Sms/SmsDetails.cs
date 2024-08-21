// <copyright file="SmsDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Sms
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.ReadModel.Email;
    using UBind.Domain.ReadWriteModel;

    /// <inheritdoc/>
    public class SmsDetails : ISmsDetails
    {
        /// <inheritdoc/>
        public Guid TenantId { get; set; }

        /// <inheritdoc/>
        public Guid OrganisationId { get; set; }

        /// <inheritdoc/>
        public Guid ProductId { get; set; }

        /// <inheritdoc/>
        public Guid Id { get; set; }

        /// <inheritdoc/>
        public string To { get; set; }

        /// <inheritdoc/>
        public string From { get; set; }

        /// <inheritdoc/>
        public string Message { get; set; }

        /// <inheritdoc/>
        public CustomerData Customer { get; set; }

        /// <inheritdoc/>
        public PolicyData Policy { get; set; }

        /// <inheritdoc/>
        public QuoteData Quote { get; set; }

        /// <inheritdoc/>
        public ClaimData Claim { get; set; }

        /// <inheritdoc/>
        public PolicyTransactionData PolicyTransaction { get; set; }

        /// <inheritdoc/>
        public UserData User { get; set; }

        /// <inheritdoc/>
        public OrganisationData Organisation { get; set; }

        /// <inheritdoc/>
        public Instant CreatedTimestamp => Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch);

        /// <summary>
        /// Gets or sets CreatedTicksSinceEpoch.
        /// </summary>
        public long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the tags of the email.
        /// </summary>
        public IEnumerable<Tag> Tags { get; set; }

        /// <summary>
        /// Gets or sets the relationships.
        /// </summary>
        public IEnumerable<Relationship> Relationships { get; set; }
    }
}
