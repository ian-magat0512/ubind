// <copyright file="ApiError.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Middleware
{
    /// <summary>
    /// For indicating API error types in error response bodies.
    /// </summary>
    public enum ApiError
    {
        /// <summary>
        /// Default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Unspecified server rorr.
        /// </summary>
        UBindServerError,

        /// <summary>
        /// Error using MS Graph.
        /// </summary>
        GraphError,

        /// <summary>
        /// Timeout using MS Graph.
        /// </summary>
        GraphTimeout,

        /// <summary>
        /// Error using Excel workbook.
        /// </summary>
        WorkbookError,

        /// <summary>
        ///  name was not unique across all un-deleted tenants.
        /// </summary>
        DuplicateNameError,

        /// <summary>
        ///  alias was not unique across all un-delete tenants.
        /// </summary>
        DuplicateAliasError,

        /// <summary>
        /// Tenant alias was not unique across all un-delete tenants.
        /// </summary>
        InvalidFormDataError,
    }
}
