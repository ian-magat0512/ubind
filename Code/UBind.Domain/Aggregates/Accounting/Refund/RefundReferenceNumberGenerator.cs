// <copyright file="RefundReferenceNumberGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Accounting
{
    using System;
    using UBind.Domain;
    using UBind.Domain.NumberGenerators;

    /// <inheritdoc/>
    public class RefundReferenceNumberGenerator : IRefundReferenceNumberGenerator
    {
        private readonly IUniqueNumberSequenceGenerator sequenceGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefundReferenceNumberGenerator"/> class.
        /// </summary>
        /// <param name="seedGenerator">A service for generating unqiue seed numbers for use in number generation.</param>
        public RefundReferenceNumberGenerator(IUniqueNumberSequenceGenerator seedGenerator)
        {
            this.sequenceGenerator = seedGenerator;
        }

        /// <inheritdoc/>
        public int GenerateRefundReferenceNumber(Guid tenantId, DeploymentEnvironment environment)
        {
            var sequenceNumber = this.sequenceGenerator.Next(
                   tenantId,
                   environment,
                   UniqueNumberUseCase.RefundReferenceNumber);
            return sequenceNumber;
        }
    }
}
