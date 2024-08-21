// <copyright file="IPaymentConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment
{
    using UBind.Domain.Enums;

    /// <summary>
    /// Configuration for payment gateways.
    /// </summary>
    public interface IPaymentConfiguration
    {
        /// <summary>
        /// Gets the name of the gateway to use.
        /// </summary>
        PaymentGatewayName GatewayName { get; }

        /// <summary>
        /// Create a payment gateway using this configuration.
        /// </summary>
        /// <param name="factory">A factory for creating payment gateways.</param>
        /// <returns>A payment gateway using this configuration.</returns>
        IPaymentGateway Create(PaymentGatewayFactory factory);
    }
}
