// <copyright file="FailedUserUpdateException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Thrown when the system fails to update a user.
    /// </summary>
    [Serializable]
    public class FailedUserUpdateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailedUserUpdateException"/> class.
        /// </summary>
        public FailedUserUpdateException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedUserUpdateException"/> class.
        /// </summary>
        /// <param name="message">The exception message to display.</param>
        public FailedUserUpdateException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedUserUpdateException"/> class.
        /// </summary>
        /// <param name="message">The exception message to display.</param>
        /// <param name="inner">The inner exception.</param>
        public FailedUserUpdateException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedUserUpdateException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected FailedUserUpdateException(
          SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
