// <copyright file="TestEFundExpressProductConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Funding.EFundExpress
{
    using UBind.Application.Funding.EFundExpress;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// Defines the <see cref="TestEFundExpressProductConfiguration" />.
    /// </summary>
    public class TestEFundExpressProductConfiguration : EFundExpressProductConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestEFundExpressProductConfiguration"/> class.
        /// </summary>
        public TestEFundExpressProductConfiguration()
            : base(
                  "EFundExpressTest",
                  "ubindfigi",
                  "unity",
                  "Foo Underwriters",
                  Application.Funding.EFundExpress.ContractType.Commercial,
                  false,
                  1.07m,
                  10,
                  45m,
                  "https://uat.redplanetsoftware.com/integration/principal/service.asmx",
                  "PI")
        {
            this.SetDataLocators(DefaultQuoteDatumLocations.Instance, null);
        }
    }
}
