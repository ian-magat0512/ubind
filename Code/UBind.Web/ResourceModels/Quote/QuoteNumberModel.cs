// <copyright file="QuoteNumberModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// Resource model for providing quote numbers currently available for a product.
    /// </summary>
    public class QuoteNumberModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteNumberModel"/> class.
        /// </summary>
        /// <param name="productId">The ID of the product the quote numbers are for.</param>
        /// <param name="environment">The environment the quote numbers are for.</param>
        /// <param name="numbers">The quote numbers.</param>
        public QuoteNumberModel(string productId, DeploymentEnvironment environment, IEnumerable<string> numbers)
        {
            this.ProductId = productId;
            this.Environment = environment;
            this.QuoteNumbers = numbers;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteNumberModel"/> class.
        /// </summary>
        [JsonConstructor]
        public QuoteNumberModel()
        {
        }

        /// <summary>
        /// Gets or sets the ID of the product the quote number set belongs to.
        /// </summary>
        /// <remarks>Public setter for deserializer.</remarks>
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the environment the policy number set belongs to.
        /// </summary>
        /// <remarks>Public setter for deserializer.</remarks>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the collection of the quote numbers for the specific product.
        /// </summary>
        /// <remarks>Public setter for deserializer.</remarks>
        public IEnumerable<string> QuoteNumbers { get; set; }
    }
}
