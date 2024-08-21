// <copyright file="ExceptionExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System;

    /// <summary>
    /// Extension methods for exceptions.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Get the innermost exception where the innermost exception is of a give type.
        /// </summary>
        /// <typeparam name="TException">The expected type of the innermost exception.</typeparam>
        /// <param name="ex">The exception whose innermost exception should be returned.</param>
        /// <returns>The innermost exception, if it is of type TException, otherwise null.</returns>
        public static TException InnermostException<TException>(this Exception ex)
            where TException : Exception
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            return ex as TException;
        }
    }
}
