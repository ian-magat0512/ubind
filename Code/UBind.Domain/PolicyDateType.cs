// <copyright file="PolicyDateType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.ComponentModel;

    /// <summary>
    /// Represents the different type of date properties of a policy
    /// </summary>
    public enum PolicyDateType
    {
        /// <summary>
        /// Invalid Value.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Inception Date.
        /// </summary>
        [Description("Inception Date")]
        InceptionDate = 1,

        /// <summary>
        /// Effective Date.
        /// </summary>
        [Description("Effective Date")]
        EffectiveDate = 2,

        /// <summary>
        /// Expiry Date.
        /// </summary>
        [Description("Expiry Date")]
        ExpiryDate = 3,
    }
}
