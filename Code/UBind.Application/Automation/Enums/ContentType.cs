// <copyright file="ContentType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Enums
{
    /// <summary>
    /// Represents the different types of content applicable.
    /// </summary>
    public enum ContentType
    {
        /// <summary>
        /// Text content defined by a text provider.
        /// </summary>
        StringContent = 0,

        /// <summary>
        /// Binary data content defined by a binary data provider.
        /// </summary>
        BinaryContent,

        /// <summary>
        /// List of mime-type and content pairs.
        /// </summary>
        EnumerableContent,
    }
}
