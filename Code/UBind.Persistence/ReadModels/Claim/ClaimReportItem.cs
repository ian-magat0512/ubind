// <copyright file="ClaimReportItem.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.ReadModels.Claim
{
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Data transfer object for claim report items for use for reports only.
    /// </summary>
    public class ClaimReportItem : ClaimReadModelSummary, IClaimReportItem
    {
        /// <inheritdoc/>
        public string? CustomerEmail { get; internal set; }

        /// <inheritdoc/>
        public string? CustomerAlternativeEmail { get; internal set; }

        /// <inheritdoc/>
        public string? CustomerMobilePhone { get; internal set; }

        /// <inheritdoc/>
        public string? CustomerHomePhone { get; internal set; }

        /// <inheritdoc/>
        public string? CustomerWorkPhone { get; internal set; }

        /// <inheritdoc/>
        public string? CreditNoteNumber { get; internal set; }

        /// <inheritdoc/>
        public string? OrganisationName { get; internal set; }

        /// <inheritdoc/>
        public string? OrganisationAlias { get; internal set; }

        /// <inheritdoc/>
        public string? AgentName { get; internal set; }

        public string? PaymentGateway { get; internal set; }

        public string? PaymentResponseJson { get; internal set; }

        public string? CreatedDate { get; internal set; }

        public string? CreatedTime { get; internal set; }

        public string? LastModifiedDate { get; internal set; }

        public string? LastModifiedTime { get; internal set; }

        public string? TestData { get; internal set; }

        public string? ProductEnvironment { get; internal set; }

        public string? InvoiceNumber { get; internal set; }
    }
}
