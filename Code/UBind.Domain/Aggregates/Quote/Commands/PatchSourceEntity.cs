// <copyright file="PatchSourceEntity.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Commands
{
    /// <summary>
    /// The entity within a policy to get the property data.
    /// </summary>
    public enum PatchSourceEntity
    {
        /// <summary>
        /// No source entity is specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Get the data from the form data in the first transaction in the policy.
        /// </summary>
        FirstPolicyTransactionFormData = 1,

        /// <summary>
        /// Get the data from the calculation result in the first transaction in the policy.
        /// </summary>
        FirstPolicyTransactionCalculationResult = 2,

        /// <summary>
        /// Get the data from the first quote.
        /// </summary>
        FirstQuoteFormData = 3,

        /// <summary>
        /// Get the data from a particular policy transaction's form data.
        /// </summary>
        SpecificPolicyTransactionFormData = 4,

        /// <summary>
        /// Get the data from a particular policy transaction's calculation result.
        /// </summary>
        SpecificPolicyTransactionCalculationResult = 5,

        /// <summary>
        /// Get the data from a particular quote.
        /// </summary>
        SpecificQuoteFormData = 6,
    }
}
