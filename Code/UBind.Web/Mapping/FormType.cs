// <copyright file="FormType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Mapping
{
    using System.ComponentModel;

    /// <summary>
    /// Specifies the type of web form app being used.
    /// </summary>
    public enum FormType
    {
        /// <summary>
        /// Gets the type of form used for quotes transactions.
        /// </summary>
        [Description("quote")]
        Quote = 0,

        /// <summary>
        /// Gets the type of form used for claims transactions.
        /// </summary>
        [Description("claim")]
        Claim = 1,
    }
}
