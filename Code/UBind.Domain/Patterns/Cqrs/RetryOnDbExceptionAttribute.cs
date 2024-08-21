// <copyright file="RetryOnDbExceptionAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Patterns.Cqrs
{
    /// <summary>
    /// Attribute for transparently retrying a failed operation.
    /// Note that CqrsMediator will now use retries for db exceptions by default, so you only
    /// need to specify this if you want to tweak the retry parameters.
    /// </summary>
    public class RetryOnDbExceptionAttribute : RetryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryOnDbExceptionAttribute "/> class.
        /// </summary>
        /// <param name="maxRetries">The maximum retry.</param>
        public RetryOnDbExceptionAttribute(
            int maxRetries = 10,
            int maxDelayMilliseconds = 10000,
            int medianFirstRetryDelayMilliseconds = 1000)
            : base(maxRetries, maxDelayMilliseconds, medianFirstRetryDelayMilliseconds)
        {
        }
    }
}
