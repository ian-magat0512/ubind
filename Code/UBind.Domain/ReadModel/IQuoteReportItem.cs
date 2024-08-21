// <copyright file="IQuoteReportItem.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    /// <summary>
    /// Data transfer object for quote report items for use for reports only.
    /// </summary>
    public interface IQuoteReportItem : IQuoteReadModelSummary
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
    }
}
