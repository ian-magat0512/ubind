// <copyright file="ReportSourceDataType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Represents the different report sources.
    /// </summary>
    public enum ReportSourceDataType
    {
        /// <summary>
        /// The source data is new business transactions.
        /// </summary>
        NewBusiness = 0,

        /// <summary>
        /// The source data is for renewal transactions.
        /// </summary>
        Renewal = 1,

        /// <summary>
        /// The source data is for adjusment transactions.
        /// </summary>
        Adjustment = 2,

        /// <summary>
        /// The source data is for cancellation transactions.
        /// </summary>
        Cancellation = 3,

        /// <summary>
        /// The source data is for quotes.
        /// </summary>
        Quote = 4,

        /// <summary>
        /// The source data is for system email.
        /// </summary>
        SystemEmail = 5,

        /// <summary>
        /// The source data is for product email.
        /// </summary>
        ProductEmail = 6,

        /// <summary>
        /// The source data is for claims.
        /// </summary>
        Claim = 7,
    }
}
