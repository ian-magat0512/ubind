// <copyright file="HttpRequestType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.Enums
{
    using System.ComponentModel;

    /// <summary>
    /// The HTTP Request type used in a HTTP Request.
    /// </summary>
    public enum HttpRequestType
    {
        /// <summary>
        /// GET
        /// </summary>
        [Description("GET")]
        Get,

        /// <summary>
        /// PUT
        /// </summary>
        [Description("PUT")]
        Put,

        /// <summary>
        /// PATCH
        /// </summary>
        [Description("PATCH")]
        Patch,

        /// <summary>
        /// POST
        /// </summary>
        [Description("POST")]
        Post,

        /// <summary>
        /// DELETE
        /// </summary>
        [Description("DELETE")]
        Delete,
    }
}
