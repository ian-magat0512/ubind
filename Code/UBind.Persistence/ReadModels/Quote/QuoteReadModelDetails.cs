// <copyright file="QuoteReadModelDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Quote
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// For projecting read model summaries from the database.
    /// </summary>
    internal class QuoteReadModelDetails : QuoteReadModelSummary, IQuoteReadModelDetails
    {
        /// <inheritdoc/>
        public string OwnerFullName { get; set; }

        /// <summary>
        /// Gets or sets the name of the organisation this quote was created under.
        /// </summary>
        public string OrganisationName { get; set; }

        /// <inheritdoc/>
        public IEnumerable<QuoteDocumentReadModel> Documents { get; set; }

        /// <inheritdoc/>
        public Guid LatestCalculationResultId { get; set; }

        /// <inheritdoc/>
        public Guid LatestCalculationResultFormDataId { get; set; }

        /// <inheritdoc/>
        public Guid? PolicyOwnerUserId { get; set; }

        /// <inheritdoc/>
        public Guid? CustomerOwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the last modified time by user.
        /// </summary>
        public Instant? LastModifiedByUserTimestamp
        {
            get => this.LastModifiedByUserTicksSinceEpoch.HasValue
               ? Instant.FromUnixTimeTicks(this.LastModifiedByUserTicksSinceEpoch.Value)
               : (Instant?)null;

            set => this.LastModifiedByUserTicksSinceEpoch = value.HasValue
                ? value.Value.ToUnixTimeTicks()
                : (long?)null;
        }
    }
}
