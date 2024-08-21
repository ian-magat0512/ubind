// <copyright file="GenericNumberGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.NumberGenerators
{
    using System;
    using UBind.Domain;

    /// <inheritdoc/>
    public class GenericNumberGenerator : IGenericNumberGenerator
    {
        private readonly IUniqueNumberSequenceGenerator sequenceGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericNumberGenerator"/> class.
        /// </summary>
        /// <param name="sequenceGenerator">A service for generating unique seed numbers for use in number generation.</param>
        public GenericNumberGenerator(IUniqueNumberSequenceGenerator sequenceGenerator)
        {
            this.sequenceGenerator = sequenceGenerator;
        }

        public string Generate(
            Guid tenantId,
            DeploymentEnvironment environment,
            UniqueNumberUseCase useCase,
            NumberObfuscationMethod method)
        {
            var sequenceNumber = this.sequenceGenerator.Next(tenantId, environment, useCase);
            if (method != NumberObfuscationMethod.None)
            {
                return new NumberObfuscator(method, sequenceNumber).ObfuscatedResult;
            }

            return sequenceNumber.ToString();
        }
    }
}
