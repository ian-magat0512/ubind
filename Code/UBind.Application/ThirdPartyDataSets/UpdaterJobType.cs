// <copyright file="UpdaterJobType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets
{
    using System.ComponentModel;

    /// <summary>
    /// Provides the applicable values for Updater Job Type.
    /// </summary>
    public enum UpdaterJobType
    {
        /// <summary>
        /// RedBook updater job type.
        /// </summary>
        [Description("RedBook")]
        RedBook,

        /// <summary>
        /// Gnaf updater job type.
        /// </summary>
        [Description("Gnaf")]
        Gnaf,

        /// <summary>
        /// Nfid updater job type.
        /// </summary>
        [Description("Nfid")]
        Nfid,

        /// <summary>
        /// Glass's Guide updater job type.
        /// </summary>
        [Description("GlassGuide")]
        GlassGuide,
    }
}
