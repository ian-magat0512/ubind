// <copyright file="SentryExtrasConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Configuration
{
    /// <summary>
    /// Represents a configuration class for exceptions that are to be excluded from monitoring by Sentry.
    /// This configuration can be used to define specific exception types or criteria that should be ignored by the Sentry monitoring system.
    /// </summary>
    public class SentryExtrasConfiguration
    {
        /// <summary>
        /// Gets or Sets exception items via a full namespace Exception.
        /// </summary>
        public List<string> ExcludedExceptions { get; set; }

        /// <summary>
        /// Gets or Sets exception items via an error code.
        /// </summary>
        public List<string> ExcludedExceptionErrorCodes { get; set; }
    }
}
