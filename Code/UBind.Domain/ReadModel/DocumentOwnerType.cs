// <copyright file="DocumentOwnerType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    /// <summary>
    /// Specifies which type of entity a document belongs to.
    /// </summary>
    public enum DocumentOwnerType
    {
        /// <summary>
        /// For quote documents.
        /// </summary>
        Quote = 0,

        /// <summary>
        /// For policy documents.
        /// </summary>
        Policy = 1,

        /// <summary>
        /// For documents associated with a quote version.
        /// </summary>
        QuoteVersion = 2,

        /// <summary>
        /// For documents associated with a claim.
        /// </summary>
        Claim = 3,

        /// <summary>
        /// For documents associated with a claim version.
        /// </summary>
        ClaimVersion = 4,
    }
}
