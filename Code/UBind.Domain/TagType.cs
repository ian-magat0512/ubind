// <copyright file="TagType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// The tag types to specify what the type of data the tag contains.
    /// </summary>
    public enum TagType
    {
        /// <summary>
        /// Can be used to associate an entity with a particular data environment (e.g. "Producting", "Staging" or "Development"). This can then be used in queries for filtering.
        /// </summary>
        Environment = 0,

        /// <summary>
        /// For indicating that a tag was defined by a user (e.g. in an automation) rather than being system-defined.
        /// </summary>
        UserDefined = 1,

        /// <summary>
        /// The tag type used by the system for classifying emails. This can then be used in queries for filtering.
        /// </summary>
        EmailType = 2,
    }
}
