// <copyright file="State.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ValueTypes
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Australian States and Territories.
    /// </summary>
    public enum State
    {
        /// <summary>
        /// A value indicating that the state has not been specified.
        /// </summary>
        [Display(Name = "")]
        Unspecified = 0,

        /// <summary>
        /// Australian Capital Territory.
        /// </summary>
        [Display(Name = "Australian Capital Territory")]
        ACT,

        /// <summary>
        /// New South Wales.
        /// </summary>
        [Display(Name = "New South Wales")]
        NSW,

        /// <summary>
        /// Northern Territory.
        /// </summary>
        [Display(Name = "Northern Territory")]
        NT,

        /// <summary>
        /// Queensland.
        /// </summary>
        [Display(Name = "Queensland")]
        QLD,

        /// <summary>
        /// South Australia.
        /// </summary>
        [Display(Name = "South Australia")]
        SA,

        /// <summary>
        /// Tasmania.
        /// </summary>
        [Display(Name = "Tasmania")]
        TAS,

        /// <summary>
        /// Victoria.
        /// </summary>
        [Display(Name = "Victoria")]
        VIC,

        /// <summary>
        /// Western Australia.
        /// </summary>
        [Display(Name = "Western Australia")]
        WA,
    }
}
