// <copyright file="PolicyPeriodCategory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Enumeration of policy period category.
    /// </summary>
    public enum PolicyPeriodCategory
    {
        /// <summary>
        /// The current policy period
        /// </summary>
        CurrentPolicyPeriod = 0,

        /// <summary>
        /// The lifetime of the policy
        /// </summary>
        LifeTimeOfThePolicy = 1,

        /// <summary>
        /// The last last number of years
        /// </summary>
        LastNumberOfYears = 2,

        /// <summary>
        /// At any time
        /// </summary>
        AtAnyTime = 3,
    }
}
