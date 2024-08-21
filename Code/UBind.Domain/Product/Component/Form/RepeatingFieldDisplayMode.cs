// <copyright file="RepeatingFieldDisplayMode.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.ComponentModel;

    /// <summary>
    /// Specifies how to display repeating fields, i.e. whether you want to display all or one instance at a time.
    /// </summary>
    public enum RepeatingFieldDisplayMode
    {
        /// <summary>
        /// Displays the full list of repeated instances one after the other on the screen at once.
        /// </summary>
        [Description("list")]
        List,

        /// <summary>
        /// Displays a single instance on the screen at once.
        /// </summary>
        [Description("instance")]
        Instance,
    }
}
