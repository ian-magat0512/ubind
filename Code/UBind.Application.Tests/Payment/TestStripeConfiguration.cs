// <copyright file="TestStripeConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Payment
{
    using UBind.Application.Payment.Stripe;

    /// <summary>
    /// Configuration for test Stripe account.
    /// </summary>
    public class TestStripeConfiguration : StripeConfiguration
    {
        public TestStripeConfiguration()
            : base("sk_test_uJCBiwAjntkm4OXTAqUOG8N0")
        {
        }
    }
}
