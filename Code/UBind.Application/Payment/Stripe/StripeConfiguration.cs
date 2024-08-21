// <copyright file="StripeConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Stripe
{
    using Newtonsoft.Json;
    using UBind.Domain.Enums;

    /// <summary>
    /// Configuration for the stripe payment gateway.
    /// </summary>
    public class StripeConfiguration : IStripeConfiguration, IPaymentConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StripeConfiguration"/> class.
        /// </summary>
        /// <param name="defaults">Default values.</param>
        /// <param name="overrides">Environment-specific overrides.</param>
        public StripeConfiguration(StripeConfiguration defaults, StripeConfiguration overrides)
        {
            this.PrivateApiKey = overrides?.PrivateApiKey ?? defaults.PrivateApiKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StripeConfiguration"/> class.
        /// </summary>
        /// <param name="privateApiKey">The private API key for the stripe account to use.</param>
        protected StripeConfiguration(string privateApiKey)
        {
            this.PrivateApiKey = privateApiKey;
        }

        [JsonConstructor]
        private StripeConfiguration()
        {
        }

        /// <summary>
        /// Gets the private API key.
        /// </summary>
        [JsonProperty]
        public string PrivateApiKey { get; private set; }

        /// <inheritdoc/>
        public PaymentGatewayName GatewayName => PaymentGatewayName.Stripe;

        /// <inheritdoc/>
        public IPaymentGateway Create(PaymentGatewayFactory factory)
        {
            return factory.CreateStripePaymentGateway(this);
        }
    }
}
