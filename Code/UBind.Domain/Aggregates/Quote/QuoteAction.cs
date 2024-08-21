// <copyright file="QuoteAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System.ComponentModel;

    /// <summary>
    /// The set of actions that can be performed on a quote.
    /// </summary>
    public enum QuoteAction
    {
        /// <summary>
        /// Updates the data of the quote.
        /// </summary>
        [Description("FormUpdate")]
        FormUpdate,

        /// <summary>
        /// Calculate the quote's premium.
        /// </summary>
        Calculation,

        /// <summary>
        /// Assign a unique identifier to the quote.
        /// </summary>
        /// <remarks>Previously called quote</remarks>
        Actualise,

        /// <summary>
        /// Saves a version of the quote.
        /// </summary>
        [Description("QuoteVersion")]
        QuoteVersion,

        /// <summary>
        /// Review the quote.
        /// </summary>
        [Description("ReviewReferral")]
        ReviewReferral,

        /// <summary>
        /// Approve the quote.
        /// </summary>
        [Description("ReviewApproval")]
        ReviewApproval,

        /// <summary>
        /// Approve the quote automatically.
        /// </summary>
        [Description("AutoApproval")]
        AutoApproval,

        /// <summary>
        /// Refer the quote to an approver.
        /// </summary>
        [Description("EndorsementReferral")]
        EndorsementReferral,

        /// <summary>
        /// Release the quote.
        /// </summary>
        [Description("EndorsementApproval")]
        EndorsementApproval,

        /// <summary>
        /// Returns the quote to Incomplete status.
        /// </summary>
        Return,

        /// <summary>
        /// Decline the quote.
        /// </summary>
        Decline,

        /// <summary>
        /// Bind the quote.
        /// </summary>
        Bind,

        /// <summary>
        /// Submit the quote.
        /// </summary>
        Submit,

        /// <summary>
        /// Issue a policy for the quote.
        /// </summary>
        Policy,

        /// <summary>
        /// Issue an invoice for the quote.
        /// </summary>
        Invoice,

        /// <summary>
        /// Fund the quote.
        /// </summary>
        Fund,

        /// <summary>
        /// Pay the premium of the quote.
        /// </summary>
        Payment,

        /// <summary>
        /// Issue a credit note for the quote.
        /// </summary>
        [Description("CreditNote")]
        CreditNote,

        /// <summary>
        /// Expires the quote
        /// </summary>
        Expire,

        /// <summary>
        /// Reverts the expiration of the quote.
        /// </summary>
        [Description("RevertExpiration")]
        RevertExpiry,

        Quote,
    }
}
