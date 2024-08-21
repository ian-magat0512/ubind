// <copyright file="QuoteReportItem.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Quote
{
    using UBind.Domain.ReadModel;

    /// <summary>
    /// For projecting report items from the database for use of reporting only.
    /// </summary>
    internal class QuoteReportItem : QuoteReadModelSummary, IQuoteReportItem
    {
        /// <inheritdoc/>
        public string CustomerEmail { get; internal set; }

        /// <inheritdoc/>
        public string CustomerAlternativeEmail { get; internal set; }

        /// <inheritdoc/>
        public string CustomerMobilePhone { get; internal set; }

        /// <inheritdoc/>
        public string CustomerHomePhone { get; internal set; }

        /// <inheritdoc/>
        public string CustomerWorkPhone { get; internal set; }

        /// <inheritdoc/>
        public string CreditNoteNumber { get; internal set; }

        /// <inheritdoc/>
        public string OrganisationName { get; internal set; }

        /// <inheritdoc/>
        public string OrganisationAlias { get; internal set; }

        /// <inheritdoc/>
        public string AgentName { get; internal set; }
    }
}
