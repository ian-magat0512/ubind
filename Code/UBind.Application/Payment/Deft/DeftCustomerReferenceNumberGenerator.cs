// <copyright file="DeftCustomerReferenceNumberGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Deft
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.NumberGenerators;

    /// <inheritdoc/>
    public class DeftCustomerReferenceNumberGenerator : IDeftCustomerReferenceNumberGenerator
    {
        private readonly IUniqueNumberSequenceGenerator sequenceGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeftCustomerReferenceNumberGenerator"/> class.
        /// </summary>
        /// <param name="seedGenerator">A service for generating unqiue seed numbers for use in number generation.</param>
        public DeftCustomerReferenceNumberGenerator(IUniqueNumberSequenceGenerator seedGenerator)
        {
            this.sequenceGenerator = seedGenerator;
        }

        /// <inheritdoc/>
        public string GenerateDeftCrnNumber(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            CrnGenerationConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration("DEFT configuration must include CRN generation configuration."));
            }

            if (configuration.IsUniqueAcrossTenant)
            {
                productId = Guid.Empty;
            }

            if (configuration.Method == CrnGenerationMethod.Unique10DigitNumber)
            {
                var sequenceNumber = this.sequenceGenerator.Next(
                    tenantId,
                    productId,
                    environment,
                    UniqueNumberUseCase.DeftCrn);
                var obfuscatedSequenceNumber = SequenceObfuscator.TenDigitSequenceObfuscator.Obfuscate(sequenceNumber);

                return obfuscatedSequenceNumber.ToString("D6");
            }

            if (configuration.Method == CrnGenerationMethod.Fixed4DigitPrefxWithUniqueSixDigitSuffix)
            {
                var sequenceNumber = this.sequenceGenerator.Next(
                    tenantId,
                    productId,
                    environment,
                    UniqueNumberUseCase.DeftCrn);
                var ofuscatedequenceNumber = SequenceObfuscator.SixDigitSequenceObfuscator.Obfuscate(sequenceNumber);
                return configuration.Prefix + ofuscatedequenceNumber.ToString("D6");
            }

            throw new ProductConfigurationException(
                Errors.Product.MisConfiguration($"CRN generation method {configuration.Method} is not supported."));
        }
    }
}
