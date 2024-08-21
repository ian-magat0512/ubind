// <copyright file="PatchRules.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Json
{
    using System;

    /// <summary>
    /// For specifying rules controlling data patching.
    /// </summary>
    [Flags]
    public enum PatchRules
    {
        /// <summary>
        /// No rules are specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// The property to be patched must exist.
        /// </summary>
        PropertyExists = 1,

        /// <summary>
        /// The property to be patched must not exist.
        /// </summary>
        PropertyDoesNotExist = 2,

        /// <summary>
        /// The property to be patched must be null or empty (or not exist).
        /// </summary>
        PropertyIsMissingOrNullOrEmpty = 4,

        /// <summary>
        /// The parent of the property to be patched must exist.
        /// </summary>
        ParentMustExist = 8,
    }
}
