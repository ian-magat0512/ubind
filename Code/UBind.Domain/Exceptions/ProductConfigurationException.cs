// <copyright file="ProductConfigurationException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Exceptions
{
    using System;

    /// <summary>
    /// Thrown when there is an error in the product configuration.
    /// </summary>
    [Serializable]
    public class ProductConfigurationException : ErrorException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductConfigurationException"/> class.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        public ProductConfigurationException(Error error)
            : base(error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductConfigurationException"/> class.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        /// <param name="innerException">The underlying exception that triggered this one.</param>
        public ProductConfigurationException(Error error, Exception innerException)
            : base(error, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductConfigurationException"/> class.
        /// </summary>
        /// <param name="info">Serialized exception information.</param>
        /// <param name="context">Error source context.</param>
        protected ProductConfigurationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
