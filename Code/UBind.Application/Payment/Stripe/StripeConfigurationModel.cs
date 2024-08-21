// <copyright file="StripeConfigurationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment
{
    using UBind.Application.Payment.Stripe;

    /// <summary>
    /// Factory for generating Eway payment gateway configuration for a given environment.
    /// </summary>
    public class StripeConfigurationModel : PerEnvironmentConfigurationModel<StripeConfiguration, IPaymentConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StripeConfigurationModel"/> class.
        /// </summary>
        public StripeConfigurationModel()
            : base((defaults, overrides) => new StripeConfiguration(defaults, overrides))
        {
        }
    }
}
