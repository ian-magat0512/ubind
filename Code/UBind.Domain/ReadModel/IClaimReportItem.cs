// <copyright file="IClaimReportItem.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ReadModel
{
    using UBind.Domain.ReadModel.Claim;

    public interface IClaimReportItem : IClaimReadModelSummary, IBaseReportReadModel
    {
        /// <summary>
        /// Gets customer email.
        /// </summary>
        string CustomerEmail { get; }

        /// <summary>
        /// Gets customer alternative email.
        /// </summary>
        string CustomerAlternativeEmail { get; }

        /// <summary>
        /// Gets customer mobile phone.
        /// </summary>
        string CustomerMobilePhone { get; }

        /// <summary>
        /// Gets customer home phone.
        /// </summary>
        string CustomerHomePhone { get; }

        /// <summary>
        /// Gets customer work phone.
        /// </summary>
        string CustomerWorkPhone { get; }

        /// <summary>
        /// Gets customer credit note.
        /// </summary>
        string CreditNoteNumber { get; }

        /// <summary>
        /// Gets the organisation name.
        /// </summary>
        string OrganisationName { get; }

        /// <summary>
        /// Gets the organisation alias.
        /// </summary>
        string OrganisationAlias { get; }

        /// <summary>
        /// Gets the agent name.
        /// </summary>
        string AgentName { get; }

        /// <summary>
        /// Get the invoice number
        /// </summary>
        string? InvoiceNumber { get; }
    }
}
