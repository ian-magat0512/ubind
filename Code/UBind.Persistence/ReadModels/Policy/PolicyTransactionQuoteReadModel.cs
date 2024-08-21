// <copyright file="PolicyTransactionQuoteReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// For queries join transaction, policy and quote.
    /// </summary>
    internal class PolicyTransactionQuoteReadModel : PolicyQuoteCustomerProductReadModel
    {
        /// <summary>
        /// Gets or sets the policy transaction.
        /// </summary>
        public PolicyTransaction PolicyTransaction { get; set; }

        /// <summary>
        /// Gets or sets the organisation.
        /// </summary>
        public OrganisationReadModel Organisation { get; set; }
    }
}
