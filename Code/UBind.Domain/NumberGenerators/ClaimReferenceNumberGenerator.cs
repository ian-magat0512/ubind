// <copyright file="ClaimReferenceNumberGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.NumberGenerators
{
    using System;
    using UBind.Domain.Extensions;

    /// <summary>
    /// For generating quote numbers from a seed.
    /// </summary>
    public class ClaimReferenceNumberGenerator : IClaimReferenceNumberGenerator
    {
        private readonly IUniqueNumberSequenceGenerator sequenceGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimReferenceNumberGenerator"/> class.
        /// </summary>
        /// <param name="seedGenerator">A service for generating unqiue seed numbers for use in number generation.</param>
        public ClaimReferenceNumberGenerator(IUniqueNumberSequenceGenerator seedGenerator)
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
        private NumberObfuscationMethod Method => NumberObfuscationMethod.SixDigitAlphabetic;

        /// <summary>
        /// Gets the form type which is "claim" for this instance.
        /// </summary>
        private WebFormAppType FormType => WebFormAppType.Claim;

        /// <inheritdoc/>
        public string Generate()
        {
            if (this.Method == NumberObfuscationMethod.SixDigitAlphabetic)
            {
                var sequenceNumber = this.sequenceGenerator.Next(
                    this.TenantId, this.ProductId, this.Environment, UniqueNumberUseCase.ClaimNumber);
                var obfuscatedSequenceNumber = (int)SequenceObfuscator.SixLetterSequenceObfuscator.Obfuscate(sequenceNumber);
                return obfuscatedSequenceNumber.ToBase26().PadLeft(6, 'A');
            }

            throw new ArgumentException($"Cannot automatically generate a {this.FormType.ToString()} reference number when method is {this.Method.ToString()}");
        }

        /// <inheritdoc/>
        public void SetProperties(Guid tenantId, Guid productId, DeploymentEnvironment env)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = env;
        }
    }
}
