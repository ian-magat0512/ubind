// <copyright file="UniqueIdentifierUnavailableException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Raised when there are no available identifiers for a given resource, product, and environment.
    /// </summary>
    public class UniqueIdentifierUnavailableException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueIdentifierUnavailableException"/> class.
        /// </summary>
        public UniqueIdentifierUnavailableException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueIdentifierUnavailableException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public UniqueIdentifierUnavailableException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueIdentifierUnavailableException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The underlying exception that caused this exception.</param>
        public UniqueIdentifierUnavailableException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueIdentifierUnavailableException"/> class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization Serialization info that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization StreamingContext that contains the contextual
        /// information about the source or destination.</param>
        public UniqueIdentifierUnavailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ////this.ProductId = info.GetString("ProductId");
        }

        /// <summary>
        /// Gets the ID of the product for which the quote numbers are needed.
        /// </summary>
        public string ProductId { get; private set; }

        /// <summary>
        /// Creates a new instance of the QuoteNumberUnavailableException for a particular product.
        /// </summary>
        /// <param name="type">The type of identifier that was requested.</param>
        /// <param name="tenantAlias">The alias of the tenant for which the identifier is needed.</param>
        /// <param name="productId">The ID of the product for which the identifier is needed.</param>
        /// <param name="environment">The environment in for which the identifier is needed.</param>
        /// <param name="innerException">The underlying exception that caused this exception.</param>
        /// <returns>A new instance of QuoteNumberUnavailableException.</returns>
        public static UniqueIdentifierUnavailableException Create(
            IdentifierType type,
            string tenantAlias,
            string productId,
            DeploymentEnvironment environment,
            Exception innerException = null)
        {
            return new UniqueIdentifierUnavailableException(
                $"There are no unique identifiers of type {type} avaialble for product {productId} under tenant {tenantAlias} in environment {environment}",
                innerException);
        }

        /////// <inheritdoc/>
        ////public override void GetObjectData(SerializationInfo info, StreamingContext context)
        ////{
        ////    if (info == null)
        ////    {
        ////        throw new ArgumentNullException("info");
        ////    }

        ////    info.AddValue("ProductId", this.ProductId);
        ////    base.GetObjectData(info, context);
        ////}
    }
}
