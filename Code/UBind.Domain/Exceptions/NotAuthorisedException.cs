// <copyright file="NotAuthorisedException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception for when authorisation to access a resource is denied.
    /// </summary>
    /// <typeparam name="T">The type of the resource's ID.</typeparam>
    public class NotAuthorisedException<T> : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotAuthorisedException{T}"/> class.
        /// </summary>
        /// <param name="id">The ID of the resource to which access is not authorised.</param>
        /// <param name="message">Message.</param>
        public NotAuthorisedException(T id, string message)
            : base(message)
        {
            this.Id = id.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotAuthorisedException{T}"/> class.
        /// </summary>
        /// <param name="id">The ID of the resource to which access is not authorised.</param>
        public NotAuthorisedException(T id)
            : this(id, $"Access to resource with ID {id} is not authorised.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotAuthorisedException{T}"/> class.
        /// </summary>
        public NotAuthorisedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotAuthorisedException{T}"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public NotAuthorisedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotAuthorisedException{T}"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public NotAuthorisedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotAuthorisedException{T}"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected NotAuthorisedException(SerializationInfo info, StreamingContext context)
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
