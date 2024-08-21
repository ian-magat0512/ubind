// <copyright file="ListConditionMatchType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    /// <summary>
    /// Specifies how a list condition should be evaluated.
    /// </summary>
    public enum ListConditionMatchType
    {
        /// <summary>
        /// The condition should return true if any of the items in the list meet the predicate condition.
        /// </summary>
        Any = 0,

        /// <summary>
        /// The condition should return true if all of the items in the list meet the predicate condition.
        /// </summary>
        All,

        /// <summary>
        /// The condition should return true if exactly one of the items in the list meets the predicate condition.
        /// </summary>
        One,
    }
}
