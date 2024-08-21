// <copyright file="ApplicationMode.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.ComponentModel;

    /// <summary>
    /// Represents various types of quote editing sessions done by different users.
    /// </summary>
    public enum ApplicationMode
    {
        // Quotes and Claims
        [Description("create")]
        Create,

        [Description("edit")]
        Edit,

        [Description("review")]
        Review,

        // Quotes only
        [Description("endorse")]
        Endorse,

        // Claims only
        [Description("assess")]
        Assess,

        [Description("settle")]
        Settle,
    }
}
