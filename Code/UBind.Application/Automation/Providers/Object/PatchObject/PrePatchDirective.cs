// <copyright file="PrePatchDirective.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object.PatchObject
{
    /// <summary>
    /// Enumeration of action that the operation will do when they encounter an validation error.
    /// </summary>
    public enum PrePatchDirective
    {
        None,

        /// <summary>
        /// Add the value when the property does not exists.
        /// </summary>
        Add,

        /// <summary>
        /// Do not perform the operation and continue on the next operation.
        /// </summary>
        Continue,

        /// <summary>
        /// Stop the operation. Do not perform the next operation.
        /// </summary>
        End,

        /// <summary>
        /// Replace the value when the property already exists.
        /// </summary>
        Replace,

        /// <summary>
        /// Throw an exception.
        /// </summary>
        RaiseError,
    }
}
