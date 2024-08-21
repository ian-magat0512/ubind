// <copyright file="PremiumFundingRedirectViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ViewModels
{
    using System;

    /// <summary>
    /// View model for view generated in response to a redirect from a funding site.
    /// </summary>
    public class PremiumFundingRedirectViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PremiumFundingRedirectViewModel"/> class.
        /// </summary>
        /// <param name="quoteId">The ID of the quote the funding was for.</param>
        /// <param name="proposalId">The ID of the funding proposal.</param>
        public PremiumFundingRedirectViewModel(Guid quoteId, string proposalId)
        {
            this.QuoteId = quoteId;
            this.ProposalId = proposalId;
        }

        /// <summary>
        /// Gets the ID of the quote the funding was for.
        /// </summary>
        public Guid QuoteId { get; }

        /// <summary>
        /// Gets the ID of the funding proposal.
        /// </summary>
        public string ProposalId { get; }
    }
}
