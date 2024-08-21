// <copyright file="QuoteReferenceNumberGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.NumberGenerators
{
    using System;

    /// <summary>
    /// For generating quote numbers from a seed.
    /// </summary>
    public class QuoteReferenceNumberGenerator : IQuoteReferenceNumberGenerator
    {
        private readonly IUniqueNumberSequenceGenerator sequenceGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteReferenceNumberGenerator"/> class.
        /// </summary>
        /// <param name="seedGenerator">A service for generating unqiue seed numbers for use in number generation.</param>
        public QuoteReferenceNumberGenerator(
            IUniqueNumberSequenceGenerator seedGenerator)
        {
            this.sequenceGenerator = seedGenerator;
        }

        /// <summary>
        /// Gets or sets the Tenant Id relevant to the current quote.
        /// </summary>
        private Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the Product Id relevant to the current quote.
        /// </summary>
        private Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the environment of the product relevant to the quote needing a quote reference number generated.
        /// </summary>
        private DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets the unique number generation method.
        /// </summary>
        private NumberObfuscationMethod ObfuscationMethod => NumberObfuscationMethod.SixDigitAlphabetic;

        /// <summary>
        /// Gets the form type which is "quote" for this instance.
        /// </summary>
        private WebFormAppType FormType => WebFormAppType.Quote;

        /// <inheritdoc/>
        public string Generate()
        {
            if (this.ObfuscationMethod != NumberObfuscationMethod.None)
            {
                var sequenceNumber = this.sequenceGenerator.Next(
                    this.TenantId, this.ProductId, this.Environment, UniqueNumberUseCase.QuoteNumber);
                int offset = this.GetUniqueProductReferenceValue(this.TenantId, this.ProductId);
                return new NumberObfuscator(this.ObfuscationMethod, sequenceNumber, offset).ObfuscatedResult;
            }

            throw new ArgumentException($"Cannot automatically generate a {this.FormType.ToString()} reference number when method is {this.ObfuscationMethod.ToString()}");
        }

        /// <summary>
        /// Sets the internal class properties related to the quote generation.
        /// </summary>
        /// <param name="tenantId">The Tenant's Id relevant to the quote generating the reference number.</param>
        /// <param name="productId">The product id of which the reference number is being generated for.</param>
        /// <param name="env">The product version environment of which the reference number is being generated for.</param>
        public void SetProperties(Guid tenantId, Guid productId, DeploymentEnvironment env)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = env;
        }

        private int GetUniqueProductReferenceValue(Guid tenantId, Guid productId)
        {
            var byteVal = System.Text.Encoding.ASCII.GetBytes(tenantId.ToString() + productId.ToString());
            int byteValTotal = 0;
            int index = 1;
            foreach (byte b in byteVal)
            {
                byteValTotal += Convert.ToInt32(b) * (26 * (index + 1));
                index++;
            }

            return byteValTotal;
        }
    }
}
