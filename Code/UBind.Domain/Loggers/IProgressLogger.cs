// <copyright file="IProgressLogger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Loggers
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Hangfire console context class.
    /// </summary>
    public interface IProgressLogger
    {
        /// <summary>
        /// Adds a message to the console.
        /// </summary>
        /// <param name="logLevel">The logging severity level.</param>
        /// <param name="message">The text to report.</param>
        void Log(LogLevel logLevel, string message);

        /// <summary>
        /// Updates a value of a progress bar.
        /// </summary>
        /// <param name="value">The new value.</param>
        void UpdateProgress(double value);
    }
}
