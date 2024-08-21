// <copyright file="NotFoundException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception for when an entity cannot be found.
    /// </summary>
    public class NotFoundException : ErrorException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        public NotFoundException(Error error)
            : base(error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        /// <param name="innerException">Inner exception.</param>
        public NotFoundException(Error error, Exception innerException)
            : base(error, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected NotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Id = info.GetString(nameof(this.Id));
        }

        /// <summary>
        /// Gets the ID off the entity that could not be found.
        /// </summary>
        public string Id { get; }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue(nameof(this.Id), this.Id);
            base.GetObjectData(info, context);
        }
    }
}
