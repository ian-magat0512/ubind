// <copyright file="IdenitiferType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// The type of entity an instance of <see cref="UniqueIdentifier"/> is to be used for.
    /// </summary>
    public enum IdentifierType
    {
        /// <summary>
        /// Quotes.
        /// </summary>
        Quote = 0,

        /// <summary>
        /// Policies.
        /// </summary>
        Policy = 1,

        /// <summary>
        /// Invoices.
        /// </summary>
        Invoice = 2,
    }
}
