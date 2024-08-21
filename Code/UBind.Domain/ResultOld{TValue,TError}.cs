// <copyright file="ResultOld{TValue,TError}.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Generic class for holding the result of a method that may succeed or fail.
    /// </summary>
    /// <typeparam name="TValue">The type of the value returned by the method when it succeeds.</typeparam>
    /// <typeparam name="TError">The type of the error returned by the method when it fails.</typeparam>
    public class ResultOld<TValue, TError>
    {
        private readonly TValue value;
        private readonly TError error;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultOld{TValue,TError}"/> class for a successful result.
        /// </summary>
        /// <param name="value">The value sucessfully returned.</param>
        protected ResultOld(TValue value)
        {
            value.ThrowIfArgumentNull(nameof(value));
            this.value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultOld{TValue,TError}"/> class for a failure result.
        /// </summary>
        /// <param name="error">Errors reported.</param>
        protected ResultOld(TError error)
        {
            error.ThrowIfArgumentNull(nameof(error));
            this.error = error;
        }

        /// <summary>
        /// Gets a value indicating whether the result was a success or not.
        /// </summary>
        public bool Succeeded => this.error == null;

        /// <summary>
        /// Gets the value returned in the case of success, otherwise throws an InvalidOperationException.
        /// </summary>
        public TValue Value
        {
            get
            {
                if (!this.Succeeded)
                {
                    throw new InvalidOperationException("Cannot obtain value when result was not a success.");
                }

                return this.value;
            }
        }

        /// <summary>
        /// Gets the error reported in the case of failure, otherwise throws an InvalidOperationException.
        /// </summary>
        public TError Error
        {
            get
            {
                if (this.Succeeded)
                {
                    throw new InvalidOperationException("Cannot get error when result was a success.");
                }

                return this.error;
            }
        }

        /// <summary>
        /// Create a new instance of <see cref="ResultOld{TValue, TError}"/> representing a failure.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>A new instance of <see cref="ResultOld{TValue, TError}"/> representing a failure.</returns>
        public static ResultOld<TValue, TError> Failure(TError error)
        {
            return new ResultOld<TValue, TError>(error);
        }

        /// <summary>
        /// Create a new instance of <see cref="ResultOld{TValue, TError}"/> representing a success.
        /// </summary>
        /// <param name="value">The result's value.</param>
        /// <returns>A new instance of <see cref="ResultOld{TValue, TError}"/> representing a success.</returns>
        public static ResultOld<TValue, TError> Success(TValue value)
        {
            return new ResultOld<TValue, TError>(value);
        }
    }
}
