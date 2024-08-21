// <copyright file="ResultOld.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Generic class for holding the result of a method that may succeed or fail.
    /// </summary>
    public class ResultOld
    {
        private readonly IEnumerable<string> errors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultOld"/> class for a successful result.
        /// </summary>
        private ResultOld()
        {
        }

        private ResultOld(IEnumerable<string> errors)
        {
            errors.ThrowIfArgumentNull(nameof(errors));
            this.errors = errors;
        }

        /// <summary>
        /// Gets a value indicating whether the result was a success or not.
        /// </summary>
        public bool Succeeded => this.errors == null;

        /// <summary>
        /// Gets the errors reported in the case of failure, otherwise throws an InvalidOperationException.
        /// </summary>
        public IEnumerable<string> Errors
        {
            get
            {
                if (this.Succeeded)
                {
                    throw new InvalidOperationException("Cannot get errors when result was a success.");
                }

                return this.errors;
            }
        }

        /// <summary>
        /// Create a new instance of the <see cref="ResultOld"/> class for a successful result.
        /// </summary>
        /// <returns>A new instance of the <see cref="ResultOld"/> class for a successful result.</returns>
        public static ResultOld Success()
        {
            return new ResultOld();
        }

        /// <summary>
        /// Create a new instance of the <see cref="ResultOld"/> class for a failure result.
        /// </summary>
        /// <param name="errors">Errors reported.</param>
        /// <returns>A new instance of the <see cref="ResultOld"/> class for a failure result with the supplied errrors.</returns>
        public static ResultOld Failure(IEnumerable<string> errors)
        {
            return new ResultOld(errors);
        }

        /// <summary>
        /// Create a new instance of the <see cref="ResultOld"/> class for a failure result.
        /// </summary>
        /// <param name="errors">Errors reported.</param>
        /// <returns>A new instance of the <see cref="ResultOld"/> class for a failure result with the supplied errrors.</returns>
        public static ResultOld Failure(params string[] errors)
        {
            return new ResultOld(errors);
        }
    }
}
