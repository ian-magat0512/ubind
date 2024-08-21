// <copyright file="RefundRule.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Enumeration of the refund rule.
    /// </summary>
    public enum RefundRule
    {
        /// <summary>
        /// Refunds are always provided.
        /// </summary>
        RefundsAreAlwaysProvided = 0,

        /// <summary>
        /// Refunds are never provided
        /// </summary>
        RefundsAreNeverProvided = 1,

        /// <summary>
        /// Refunds are provided if no claims were made during a specific period.
        /// </summary>
        RefundsAreProvidedIfNoClaimsWereMade = 2,

        /// <summary>
        /// Refunds can optionally be provided at the discretion of a person performing a review or endorsement
        /// </summary>
        RefundsCanOptionallyBeProvided = 3,
    }
}
