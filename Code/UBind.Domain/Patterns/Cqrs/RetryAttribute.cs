// <copyright file="RetryAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Patterns.Cqrs
{
    using System;

    /// <summary>
    /// Attribute for transparently retrying a failed operation.
    /// </summary>
    public class RetryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class.
        /// </summary>
        /// <param name="maxRetries">The maximum retry.</param>
        public RetryAttribute(
            int maxRetries = 8,
            int maxDelayMilliseconds = 8000,
            int medianFirstRetryDelayMilliseconds = 1000)
        {
            this.MaxRetries = maxRetries;
            this.MaxDelayMilliseconds = maxDelayMilliseconds;
            this.MedianFirstRetryDelayMilliseconds = medianFirstRetryDelayMilliseconds;
        }

        /// <summary>
        /// Gets the maximum retry.
        /// </summary>
        public int MaxRetries { get; }

        public int MaxDelayMilliseconds { get; }

        public int MedianFirstRetryDelayMilliseconds { get; }
    }
}
