// <copyright file="PolicyDetailsType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Represents the different types of Policy Details.
    /// </summary>
    public enum PolicyDetailsType
    {
        /// <summary>
        /// The Policy Basic details.
        /// </summary>
        Base,

        /// <summary>
        /// The Policy Premium details.
        /// </summary>
        Premium,

        /// <summary>
        /// The Policy Questions.
        /// </summary>
        Questions,

        /// <summary>
        /// The Policy Documents.
        /// </summary>
        Documents,

        /// <summary>
        /// The Policy Claims.
        /// </summary>
        Claims,

        /// <summary>
        /// The Policy Price.
        /// </summary>
        Price,

        /// <summary>
        /// The Policy Refund.
        /// </summary>
        Refund,
    }
}
