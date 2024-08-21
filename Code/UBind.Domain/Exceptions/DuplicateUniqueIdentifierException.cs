// <copyright file="DuplicateUniqueIdentifierException.cs" company="uBind">
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
    /// Raised when there are duplicate unique identifiers of a given type being entered for a product and environment.
    /// </summary>
    [Serializable]
    public class DuplicateUniqueIdentifierException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateUniqueIdentifierException"/> class.
        /// </summary>
        public DuplicateUniqueIdentifierException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateUniqueIdentifierException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public DuplicateUniqueIdentifierException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateUniqueIdentifierException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The underlying exception that caused this exception.</param>
        public DuplicateUniqueIdentifierException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateUniqueIdentifierException"/> class.
        /// </summary>
        /// <param name="info">THe System.Runtime.Serialization SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization StreamingContext that contains contextual information
        /// about the source or destination.</param>
        public DuplicateUniqueIdentifierException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Type = (IdentifierType)Enum.Parse(typeof(IdentifierType), info.GetString("Category"));
            this.TenantAlias = info.GetString("Tenant");
            this.ProductAlias = info.GetString("Product");
            this.Environment = (DeploymentEnvironment)info.GetInt32("Environment");
            this.Identifier = info.GetString("Number");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateUniqueIdentifierException"/> class.
        /// </summary>
        /// <param name="type">The type of identifier.</param>
        /// <param name="tenantAlias">The Alias for the tenant.</param>
        /// <param name="productAlias">The Alias of the product the duplicate quote number is intended for.</param>
        /// <param name="environment">The deployment environment the duplicate quote number is for.</param>
        /// <param name="number">The duplicate quote number text.</param>
        /// <param name="innerException">The underlying exception that caused this exception.</param>
        public DuplicateUniqueIdentifierException(
            IdentifierType type, string tenantAlias, string productAlias, DeploymentEnvironment environment, string number, Exception innerException = null)
            : base(
                  $"Duplicate identifier of type {type} for product {productAlias} under {tenantAlias} in environment {environment.Humanize()}: {number}",
                  innerException)
        {
            this.Type = type;
            this.TenantAlias = tenantAlias;
            this.ProductAlias = productAlias;
            this.Environment = environment;
            this.Identifier = number;
        }

        /// <summary>
        /// Gets the type of the duplicate identifier.
        /// </summary>
        public IdentifierType Type { get; private set; }

        /// <summary>
        /// Gets the Alias of the tenant the duplicate identifier was intended for.
        /// </summary>
        public string TenantAlias { get; private set; }

        /// <summary>
        /// Gets the Alias of the product the duplicate identifier was intended for.
        /// </summary>
        public string ProductAlias { get; private set; }

        /// <summary>
        /// Gets the deployment environment the duplicate identifier was intended for.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the duplicate identifier.
        /// </summary>
        public string Identifier { get; private set; }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("Category", this.Type.ToString());
            info.AddValue("Tenant", this.TenantAlias);
            info.AddValue("Product", this.ProductAlias);
            info.AddValue("Environment", this.Environment);
            info.AddValue("Number", this.Identifier);

            base.GetObjectData(info, context);
        }
    }
}
