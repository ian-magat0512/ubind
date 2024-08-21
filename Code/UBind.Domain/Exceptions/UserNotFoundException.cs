// <copyright file="UserNotFoundException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Thrown when the system fails to find the user.
    /// </summary>
    [Serializable]
    public class UserNotFoundException : NotFoundException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotFoundException"/> class.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        public UserNotFoundException(Error error)
            : base(error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotFoundException"/> class.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        /// <param name="inner">The inner exception.</param>
        public UserNotFoundException(Error error, Exception inner)
            : base(error, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected UserNotFoundException(
          SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
