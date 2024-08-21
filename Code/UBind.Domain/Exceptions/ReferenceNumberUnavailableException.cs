// <copyright file="ReferenceNumberUnavailableException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Thrown when there are no policy, invoice or claim numbers available for a particular product in a specific environment.
    /// </summary>
    [Serializable]
    public class ReferenceNumberUnavailableException : ErrorException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceNumberUnavailableException"/> class.
        /// </summary>
        /// <param name="error">The error instance.</param>
        public ReferenceNumberUnavailableException(Error error)
            : base(error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceNumberUnavailableException"/> class.
        /// </summary>
        /// <param name="error">The error instance.</param>
        /// <param name="inner">The underlying exception that caused this exception.</param>
        public ReferenceNumberUnavailableException(Error error, Exception inner)
            : base(error, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceNumberUnavailableException"/> class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information
        /// about the source or destination.</param>
        protected ReferenceNumberUnavailableException(SerializationInfo info, StreamingContext context)
             : base(info, context)
        {
            this.TenantId = info.GetString(nameof(this.TenantId));
            this.ProductId = info.GetString(nameof(this.ProductId));
            this.Environment = (DeploymentEnvironment)info.GetInt32(nameof(this.Environment));
        }

        /// <summary>
        /// Gets the ID of the tenant for which policy, invoice or claim numbers are not available.
        /// </summary>
        public string TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the product for which policy, invoice or claim numbers are not available.
        /// </summary>
        public string ProductId { get; private set; }

        /// <summary>
        /// Gets the deployment environment for which policy, invoice or claim numbers are not available.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue(nameof(this.TenantId), this.TenantId);
            info.AddValue(nameof(this.ProductId), this.ProductId);
            info.AddValue(nameof(this.Environment), this.Environment);

            base.GetObjectData(info, context);
        }
    }
}
