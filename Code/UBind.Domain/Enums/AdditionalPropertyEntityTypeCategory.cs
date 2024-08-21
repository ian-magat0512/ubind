// <copyright file="AdditionalPropertyEntityTypeCategory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Enums
{
    /// <summary>
    /// Additional property entity type category.
    /// </summary>
    public enum AdditionalPropertyEntityTypeCategory
    {
        /// <summary>
        /// Quote.
        /// </summary>
        Quote,

        /// <summary>
        /// Quote Version.
        /// </summary>
        QuoteVersion,

        /// <summary>
        /// Policy.
        /// </summary>
        Policy,

        /// <summary>
        /// Policy transaction.
        /// </summary>
        PolicyTransaction,

        /// <summary>
        /// Claim.
        /// </summary>
        Claim,

        /// <summary>
        /// Claim Version.
        /// </summary>
        ClaimVersion,
    }
}
