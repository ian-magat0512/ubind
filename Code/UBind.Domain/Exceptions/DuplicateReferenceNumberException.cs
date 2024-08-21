// <copyright file="DuplicateReferenceNumberException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Humanizer;

    /// <summary>
    /// Raised when there are duplicate reference numbers being entered for either invoice, policies or claims for the same product and environment.
    /// </summary>
    [Serializable]
    public class DuplicateReferenceNumberException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateReferenceNumberException"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the duplicate number is intended for.</param>
        /// <param name="productId">The ID of the product the duplicate number is intended for.</param>
        /// <param name="environment">The deployment environment for which the duplicate number is intended for.</param>
        /// <param name="number">The duplicate reference number text.</param>
        /// <param name="referenceType">The object for which the duplicate reference number is intended for.</param>
        /// <param name="innerException">The underlying exception that caused this exception.</param>
        public DuplicateReferenceNumberException(
            string tenantId, string productId, DeploymentEnvironment environment, string number, string referenceType, Exception innerException = null)
            : base(
                  $"Duplicate {referenceType} number for product {productId} under tenant {tenantId} in environment {environment.Humanize()} : {number}",
                  innerException)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = environment;
            this.Number = number;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateReferenceNumberException"/> class.
        /// </summary>
        public DuplicateReferenceNumberException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateReferenceNumberException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public DuplicateReferenceNumberException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateReferenceNumberException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The underlying exception that caused this exception.</param>
        public DuplicateReferenceNumberException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateReferenceNumberException"/> class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information
        /// about the source or destination.</param>
        protected DuplicateReferenceNumberException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.TenantId = info.GetString(nameof(this.TenantId));
            this.ProductId = info.GetString(nameof(this.ProductId));
            this.Environment = (DeploymentEnvironment)info.GetInt32(nameof(this.Environment));
            this.Number = info.GetString(nameof(this.Number));
        }

        /// <summary>
        /// Gets the ID of the tenant for which the duplicate reference number was intended.
        /// </summary>
        public string TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the product for which the duplicate reference number was intended.
        /// </summary>
        public string ProductId { get; private set; }

        /// <summary>
        /// Gets the deployment environment for which the duplicate reference number was intended.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the the duplicate reference number itself.
        /// </summary>
        public string Number { get; private set; }

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
            info.AddValue(nameof(this.Number), this.Number);

            base.GetObjectData(info, context);
        }
    }
}
