// <copyright file="FundingNotConfiguredException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Exceptions
{
    using System;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// Thrown when funding has not been configured for a given product.
    /// </summary>
    [Serializable]
    public class FundingNotConfiguredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingNotConfiguredException"/> class.
        /// </summary>
        /// <param name="quoteAggregate">The quote funding was trying to be obtained for.</param>
        public FundingNotConfiguredException(QuoteAggregate quoteAggregate)
            : base($"Funding has not been configured for product {quoteAggregate.ProductId}.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingNotConfiguredException"/> class.
        /// </summary>
        /// <param name="message">A message explaining the reason for the exception.</param>
        public FundingNotConfiguredException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingNotConfiguredException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Serialization context.</param>
        protected FundingNotConfiguredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
