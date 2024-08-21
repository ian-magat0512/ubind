// <copyright file="Frequency.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ValueTypes
{
    /// <summary>
    /// Enumerates frequencies.
    /// </summary>
    public enum Frequency
    {
        /// <summary>
        /// Once a day.
        /// </summary>
        Daily = 0,

        /// <summary>
        /// Once every seven days.
        /// </summary>
        Weekly,

        /// <summary>
        /// Once every fourteen days.
        /// </summary>
        Fortnightly,

        /// <summary>
        /// Once every month.
        /// </summary>
        Monthly,

        /// <summary>
        /// Once every three months.
        /// </summary>
        Quarterly,

        /// <summary>
        /// Once a year.
        /// </summary>
        Annually,
    }
}
