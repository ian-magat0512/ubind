// <copyright file="Enum.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides strongly typed Enum value enumeration.
    /// </summary>
    /// <typeparam name="T">The type of the Enum.</typeparam>
    public static class Enum<T>
        where T : Enum
    {
        /// <summary>
        /// Retrieves an enumeration of the values of the constants in a specified enumeration.
        /// </summary>
        /// <returns>An enumeration of the values of the constants in a specified enumeration.</returns>
        public static IEnumerable<T> GetValues()
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}
