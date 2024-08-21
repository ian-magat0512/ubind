// <copyright file="DuplicateReleaseNumberException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception thrown when an attempt is made to inser a release with duplicate number into the database.
    /// </summary>
    [Serializable]
    public class DuplicateReleaseNumberException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateReleaseNumberException"/> class.
        /// </summary>
        public DuplicateReleaseNumberException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateReleaseNumberException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DuplicateReleaseNumberException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateReleaseNumberException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference
        /// if no inner exception is specified.</param>
        public DuplicateReleaseNumberException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateReleaseNumberException"/> class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information
        /// about the source or destination.</param>
        protected DuplicateReleaseNumberException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context)
        {
        }
    }
}
