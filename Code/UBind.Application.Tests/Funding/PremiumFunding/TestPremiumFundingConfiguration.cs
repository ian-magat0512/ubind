// <copyright file="TestPremiumFundingConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Funding.PremiumFunding
{
    using UBind.Application.Funding.PremiumFunding;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Defines the <see cref="TestPremiumFundingConfiguration" />.
    /// </summary>
    public class TestPremiumFundingConfiguration : PremiumFundingConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestPremiumFundingConfiguration"/> class.
        /// </summary>
        public TestPremiumFundingConfiguration()
            : base(
                  "aptiture-dev",
                  "hwyRrL4hU1nU",
                  "1.4.9",
                  Application.Funding.PremiumFunding.ContractType.Domestic,
                  Frequency.Monthly,
                  15,
                  12,
                  "Foo Corp",
                  "Bar Corp",
                  0.7m)
        {
            this.SetDataLocators(DefaultQuoteDatumLocations.Instance, null);
        }
    }
}
