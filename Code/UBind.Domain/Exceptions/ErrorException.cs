// <copyright file="ErrorException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// A class for converting and Error into an exception.
    /// This is used for things like hangfire which need tasks that fail to throw an exception so that they can be retried.
    /// </summary>
    [Serializable]
    public class ErrorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorException"/> class.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        public ErrorException(Error error)
            : base(error.ToString())
        {
            this.Error = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorException"/> class.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        /// <param name="innerException">The inner exception.</param>
        public ErrorException(Error error, Exception innerException)
            : base(error.ToString(), innerException)
        {
            this.Error = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // TODO: Deserialise the error?????
        }

        /// <summary>
        /// Gets the error which the exception was created from.
        /// </summary>
        /// <returns>The error which this exception was created from.</returns>
        public Error Error { get; }
    }
}
