// <copyright file="ExternalServiceException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Exceptions
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Exception thrown when an unexpected error occurs while using an external service.
    /// </summary>
    [Serializable]
    public class ExternalServiceException : ErrorException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalServiceException"/> class with a specific error.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        public ExternalServiceException(Error error)
            : base(error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalServiceException"/> class with a specified error
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference
        /// if no inner exception is specified.</param>
        public ExternalServiceException(Error error, Exception innerException)
            : base(error, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalServiceException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        protected ExternalServiceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
