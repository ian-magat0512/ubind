// <copyright file="DuplicateProductNameException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Thrown when the given product name exists in the data source.
    /// </summary>
    [Serializable]
    public class DuplicateProductNameException : ErrorException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateProductNameException"/> class.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        public DuplicateProductNameException(Error error)
            : base(error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateProductNameException"/> class.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        /// <param name="innerException">The inner exception.</param>
        public DuplicateProductNameException(Error error, Exception innerException)
            : base(error, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateProductNameException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected DuplicateProductNameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
