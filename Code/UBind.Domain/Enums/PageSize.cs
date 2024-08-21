// <copyright file="PageSize.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Enums
{
    /// <summary>
    /// Enumeration of the page sizes for pagination purposes.
    /// </summary>
    public enum PageSize
    {
        /// <summary>
        /// No Page size specified
        /// </summary>
        Default = 1000,

        /// <summary>
        /// Twice Normal Page Size
        /// </summary>
        Twice = 100,

        /// <summary>
        /// Normal Page Size
        /// </summary>
        Normal = 50,

        /// <summary>
        /// Few page size
        /// </summary>
        Few = 20,
    }
}
