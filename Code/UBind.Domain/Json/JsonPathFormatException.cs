// <copyright file="JsonPathFormatException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Json
{
    using System;

    /// <summary>
    /// Thrown when attempting to construct a Json path from an invalid string.
    /// </summary>
    [Serializable]
    public class JsonPathFormatException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPathFormatException"/> class.
        /// </summary>
        /// <param name="jsonPath">The invalid json path.</param>
        public JsonPathFormatException(string jsonPath)
            : base($@"""{jsonPath}""is not a valid valid JSON path.")
        {
        }
    }
}
