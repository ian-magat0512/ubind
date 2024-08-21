// <copyright file="PrincipalFinanceProposalResultModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    /// <summary>
    /// Resource model for delivering funding proposl acceptance response.
    /// </summary>
    public class PrincipalFinanceProposalResultModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrincipalFinanceProposalResultModel"/> class.
        /// </summary>
        /// <param name="quote">The quote.</param>
        public PrincipalFinanceProposalResultModel(Domain.Aggregates.Quote.Quote quote)
        {
            this.AcceptanceConfirmation = quote.IsFunded;
        }

        /// <summary>
        /// Gets a value indicating whether the quote has a funding proposal.
        /// </summary>
        public bool AcceptanceConfirmation { get; private set; }
    }
}
