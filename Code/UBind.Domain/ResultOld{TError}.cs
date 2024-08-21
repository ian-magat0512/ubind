// <copyright file="ResultOld{TError}.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Generic class for holding the outcome of a method that may succeed or fail with a given error.
    /// </summary>
    /// <typeparam name="TError">Type type of the error that can be returned.</typeparam>
    public class ResultOld<TError>
    {
        private readonly TError error;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultOld{TError}"/> class for a successful result.
        /// </summary>
        private ResultOld()
        {
            this.Succeeded = true;
        }

        private ResultOld(TError error)
        {
            this.Succeeded = false;
            error.ThrowIfArgumentNull(nameof(error));
            this.error = error;
        }

        /// <summary>
        /// Gets a value indicating whether the result was a success or not.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// Gets the errors reported in the case of failure, otherwise throws an InvalidOperationException.
        /// </summary>
        public TError Error
        {
            get
            {
                if (this.Succeeded)
                {
                    throw new InvalidOperationException("Cannot get errors when result was a success.");
                }

                return this.error;
            }
        }

        /// <summary>
        /// Create a new instance of the <see cref="ResultOld"/> class for a successful result.
        /// </summary>
        /// <returns>A new instance of the <see cref="ResultOld"/> class for a successful result.</returns>
        public static ResultOld<TError> Success()
        {
            return new ResultOld<TError>();
        }

        /// <summary>
        /// Create a new instance of the <see cref="ResultOld"/> class for a failure result.
        /// </summary>
        /// <param name="errors">Errors reported.</param>
        /// <returns>A new instance of the <see cref="ResultOld"/> class for a failure result with the supplied errrors.</returns>
        public static ResultOld<TError> Failure(TError errors)
        {
            return new ResultOld<TError>(errors);
        }
    }
}
