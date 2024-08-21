// <copyright file="NumberPool.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReferenceNumbers
{
    /// <summary>
    /// An enum for number pool that serves as a selection for number repository.
    /// </summary>
    public enum NumberPool
    {
        /// <summary>
        /// invoice numbers
        /// </summary>
        Invoice = 0,

        /// <summary>
        /// credit note numbers
        /// </summary>
        CreditNote = 1,

        /// <summary>
        /// policy numbers
        /// </summary>
        Policy = 2,

        /// <summary>
        /// claim numbers
        /// </summary>
        Claim = 3,
    }
}
