// <copyright file="NullableExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System;

    /// <summary>
    /// Extension methods for nullable types.
    /// </summary>
    public static class NullableExtensions
    {
        /// <summary>
        /// Get's the value of an instance of a nullable type, or throws an exception.
        /// </summary>
        /// <typeparam name="TValue">The type parameter of the nullable type.</typeparam>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="instance">The instance of the nullable type whose value should be returned.</param>
        /// <param name="exception">The exception to throw if the instance has no value.</param>
        /// <returns>The value of the instance of the nullable type, if not null.</returns>
        public static TValue GetValueOrThrow<TValue, TException>(this TValue? instance, TException exception)
            where TValue : struct
            where TException : Exception
        {
            if (instance.HasValue)
            {
                return instance.Value;
            }

            throw exception;
        }
    }
}
