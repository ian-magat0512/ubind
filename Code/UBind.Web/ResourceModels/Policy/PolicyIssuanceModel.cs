// <copyright file="PolicyIssuanceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Policy
{
    using System;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// Resource model for issuance of Policy.
    /// </summary>
    public class PolicyIssuanceModel : IPolicyResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyIssuanceModel"/> class.
        /// </summary>
        /// <param name="quote">The quote binded.</param>
        /// <param name="policy">The policy issued.</param>
        public PolicyIssuanceModel(NewQuoteReadModel quote, PolicyReadModel policy)
        {
            this.QuoteId = quote.Id;
            this.PolicyId = policy.Id;
            this.QuoteId = quote.Id;
            this.Policy = new PolicyModel(policy);
            this.PolicyNumber = policy.PolicyNumber;
        }

        /// <inheritdoc/>
        public Guid QuoteId { get; }

        /// <inheritdoc/>
        public Guid PolicyId { get; }

        /// <inheritdoc/>
        public PolicyModel Policy { get; }

        public string PolicyNumber { get; }
    }
}
