// <copyright file="ApplicationStackConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Fakes
{
    using UBind.Domain.Funding;

    /// <summary>
    /// Configuration for test application stack.
    /// </summary>
    public class ApplicationStackConfiguration
    {
        /// <summary>
        /// Gets a configuration that defaults to using a default expiry settings provider and no premium funding.
        /// </summary>
        public static ApplicationStackConfiguration Default => new ApplicationStackConfiguration();

        /// <summary>
        /// Gets or sets a value indicating whether quote expiry setting provider should be stubbed with a default provider.
        /// </summary>
        /// <remarks>
        /// Defaults to true, which will avoid the need to setup a product for simpler integration tests.
        /// </remarks>
        public bool UseDefaultQuoteExpirySettingsProvider { get; set; } = true;

        /// <summary>
        /// Gets or sets the name of the funding service to use. Defaults to None, meaning no funding service will be used.
        /// </summary>
        public FundingServiceName FundingServiceName { get; set; } = FundingServiceName.None;

        /// <summary>
        /// Create a configuration that specifies use of a real quote expiry settings provider.
        /// </summary>
        /// <returns>A configuraiton that specifies use of a real quote expiry settings provider.</returns>
        /// <remarks>Use this configuration when you want an integration test that covers use of expiry settings from product configuration..</remarks>
        public static ApplicationStackConfiguration WithRealQuoteExpirySettingProvider()
        {
            return new ApplicationStackConfiguration { UseDefaultQuoteExpirySettingsProvider = false };
        }

        /// <summary>
        /// Create a configuration that specifies use of a real funding service provider.
        /// </summary>
        /// <param name="name">The name of the premium funding service to use.</param>
        /// <returns>A configuraiton that specifies use of a real funding service provider.</returns>
        /// <remarks>Use this configuration when you want an integration test that uses premium funding.</remarks>
        public static ApplicationStackConfiguration WithFundingService(FundingServiceName name)
        {
            return new ApplicationStackConfiguration { FundingServiceName = name };
        }
    }
}
