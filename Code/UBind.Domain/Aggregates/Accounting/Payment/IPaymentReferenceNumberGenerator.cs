// <copyright file="IPaymentReferenceNumberGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Accounting
{
    using System;

    /// <summary>
    /// Service for generating a unique sequence of contiguous integers for payments.
    /// </summary>
    public interface IPaymentReferenceNumberGenerator
    {
        /// <summary>
        /// Generates payment reference number for payments for accounting purposes.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>The payment reference number.</returns>
        int GeneratePaymentReferenceNumber(Guid tenantId, DeploymentEnvironment environment);
    }
}
