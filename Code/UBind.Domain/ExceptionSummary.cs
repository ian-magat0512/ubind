// <copyright file="ExceptionSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;

    /// <summary>
    /// For capturing summary details of an exception.
    /// </summary>
    public class ExceptionSummary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionSummary"/> class.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        /// <param name="stackTrace">The exception's stack trace.</param>
        public ExceptionSummary(string message, string stackTrace)
        {
            this.Message = message;
            this.StackTrace = stackTrace;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionSummary"/> class.
        /// </summary>
        /// <param name="exception">The exception to summarize.</param>
        public ExceptionSummary(Exception exception)
        {
            this.Message = exception.Message;
            this.StackTrace = exception.StackTrace;
        }

        /// <summary>
        /// Gets the exception's message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the exception's stack trace.
        /// </summary>
        public string StackTrace { get; }
    }
}
