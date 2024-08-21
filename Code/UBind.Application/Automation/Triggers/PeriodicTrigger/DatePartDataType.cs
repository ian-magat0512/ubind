// <copyright file="DatePartDataType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    /// <summary>
    /// Enumeration of data types that you can use for minute, hour, day, day of the month and month properties of periodic trigger.
    /// </summary>
    public enum DatePartDataType
    {
        /// <summary>
        /// The value of the property is a list.
        /// e.g.
        /// ['January', 'March', 'December']
        /// ['Monday', 'Wednesday', 'Sunday']
        /// </summary>
        List,

        /// <summary>
        /// The value of the property is a range.
        ///  e.g.
        /// ['March'-'December']
        /// ['Wednesday'-'Sunday']
        /// [0-25]
        /// </summary>
        Range,

        /// <summary>
        /// The value of the property is an specific interval.
        ///  e.g.
        /// [5] // every May of the year.
        /// [6] // every 6AM of the day
        /// [5] // every 5 minutes
        /// </summary>
        Every,

        /// <summary>
        /// The value of the property is an occurence.
        ///  e.g.
        /// Every second tuesday of the month.
        /// </summary>
        Occurence,
    }
}
