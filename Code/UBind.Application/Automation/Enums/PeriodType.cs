// <copyright file="PeriodType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Enums
{
    /// <summary>
    /// The Period type used in a Raise Event Action.
    /// </summary>
    public enum PeriodType
    {
        /// <summary>
        /// Year
        /// </summary>
        Year,

        /// <summary>
        /// Quarter
        /// </summary>
        Quarter,

        /// <summary>
        /// Month
        /// </summary>
        Month,

        /// <summary>
        /// Week
        /// </summary>
        Week,

        /// <summary>
        /// Day
        /// </summary>
        Day,

        /// <summary>
        /// Hour
        /// </summary>
        Hour,

        /// <summary>
        /// Minute
        /// </summary>
        Minute,

        /// <summary>
        /// Second
        /// </summary>
        Second,

        /// <summary>
        /// Millisecond
        /// </summary>
        Millisecond,
    }
}
