// <copyright file="PremiumFundingConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.PremiumFunding
{
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Funding;
    using UBind.Domain.Product;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Configuration for the Premium Funding service.
    /// </summary>
    public class PremiumFundingConfiguration : IPremiumFundingConfiguration, IFundingConfiguration, IDataLocatorConfig
    {
        private IQuoteDatumLocations? quoteDatumLocations;
        private DataLocators? dataLocations;

        /// <summary>
        /// Initializes a new instance of the <see cref="PremiumFundingConfiguration"/> class based on a default configuration and an override configuration.
        /// </summary>
        /// <param name="default">The default configuration.</param>
        /// <param name="overrides">The override configuration.</param>
        public PremiumFundingConfiguration(PremiumFundingConfiguration @default, PremiumFundingConfiguration overrides)
        {
            this.Username = overrides.Username
                ?? @default.Username
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration("Username must be specified in the premium funding configuration."));
            this.Password = overrides.Password
                ?? @default.Password
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration("Password must be specified in the premium funding configuration."));
            this.ApiVersion = overrides.ApiVersion
                ?? @default.ApiVersion
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration("ApiVersion must be specified in the premium funding configuration."));
            this.ContractType = overrides.ContractType
                ?? ((IPremiumFundingConfiguration)@default).ContractType;
            this.PaymentFrequency = overrides.PaymentFrequency
                ?? ((IPremiumFundingConfiguration)@default).PaymentFrequency;
            this.SettlementDays = overrides.SettlementDays
                ?? ((IPremiumFundingConfiguration)@default).SettlementDays;
            this.NumberOfMonths = overrides.NumberOfMonths
                ?? ((IPremiumFundingConfiguration)@default).NumberOfMonths;
            this.InsurerName = overrides.InsurerName
               ?? @default.InsurerName
               ?? throw new ProductConfigurationException(
                   Errors.Product.MisConfiguration("InsurerName must be specified in the premium funding configuration."));
            this.SettlementToName = overrides.SettlementToName
                ?? @default.SettlementToName
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration("SettlementToName must be specified in the premium funding configuration."));
            this.Commission = overrides.Commission
                ?? @default.Commission
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration("Commission must be specified in the premium funding configuration."));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PremiumFundingConfiguration"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="apiVersion">The API version.</param>
        /// <param name="contractType">The type of the contract (commercial, domestic, etc.).</param>
        /// <param name="paymentFrequency">The payment frequency.</param>
        /// <param name="settlementDays">The number of days to settle.</param>
        /// <param name="numberOfMonths">The number of months the contract will be settled over.</param>
        /// <param name="insurerName">The insurer name.</param>
        /// <param name="settlementToName">The name to whom the settlement should be made to.</param>
        /// <param name="commission">Gets the commission (percentage) to be used.</param>
        /// <remarks>Exposed for test configuration use.</remarks>
        protected PremiumFundingConfiguration(
            string username,
            string password,
            string apiVersion,
            ContractType contractType,
            Frequency paymentFrequency,
            int settlementDays,
            int numberOfMonths,
            string insurerName,
            string settlementToName,
            decimal commission)
        {
            this.Username = username;
            this.Password = password;
            this.ApiVersion = apiVersion;
            this.ContractType = contractType;
            this.PaymentFrequency = paymentFrequency;
            this.SettlementDays = settlementDays;
            this.NumberOfMonths = numberOfMonths;
            this.InsurerName = insurerName;
            this.SettlementToName = settlementToName;
            this.Commission = commission;
        }

        [JsonConstructor]
        private PremiumFundingConfiguration(
            string username,
            string password,
            string apiVersion,
            string insurerName,
            string settlementToName)
        {
            this.Username = username;
            this.Password = password;
            this.ApiVersion = apiVersion;
            this.InsurerName = insurerName;
            this.SettlementToName = settlementToName;
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        [JsonProperty]
        public string Username { get; private set; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        [JsonProperty]
        public string Password { get; private set; }

        /// <summary>
        /// Gets the API version.
        /// </summary>
        [JsonProperty]
        public string ApiVersion { get; private set; }

        /// <inheritdoc/>
        ContractType IPremiumFundingConfiguration.ContractType => this.ContractType.HasValue
            ? this.ContractType.Value
            : throw new ProductConfigurationException(
                Errors.Product.MisConfiguration("ContractType must be specified in the premium funding configuration."));

        /// <summary>
        /// Gets the type of the contract.
        /// </summary>
        [JsonProperty]
        public ContractType? ContractType { get; private set; }

        /// <inheritdoc/>
        Frequency IPremiumFundingConfiguration.PaymentFrequency => this.PaymentFrequency.HasValue
            ? this.PaymentFrequency.Value
            : throw new ProductConfigurationException(
                Errors.Product.MisConfiguration("PaymentFrequency must be specified in the premium funding configuration."));

        /// <summary>
        /// Gets the payment frequency, if specified, otherwise null.
        /// </summary>
        [JsonProperty]
        public Frequency? PaymentFrequency { get; private set; }

        /// <summary>
        /// Gets the number of days to settlement, if specified, otherwise null.
        /// </summary>
        [JsonProperty]
        public int? SettlementDays { get; private set; }

        /// <inheritdoc/>
        int IPremiumFundingConfiguration.SettlementDays =>
            this.SettlementDays.HasValue
                ? this.SettlementDays.Value
                : throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration("SettlementDays must be specified in the premium funding configuration."));

        /// <summary>
        /// Gets the number of months, if specified, otherwise null.
        /// </summary>
        [JsonProperty]
        public int? NumberOfMonths { get; private set; }

        /// <inheritdoc/>
        int IPremiumFundingConfiguration.NumberOfMonths => this.NumberOfMonths.HasValue
            ? this.NumberOfMonths.Value
            : throw new ProductConfigurationException(
                Errors.Product.MisConfiguration("NumberOfMonths must be specified in the premium funding configuration."));

        /// <summary>
        /// Gets the service name.
        /// </summary>
        public FundingServiceName ServiceName => FundingServiceName.PremiumFunding;

        /// <inheritdoc/>
        [JsonProperty]
        public string InsurerName { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string SettlementToName { get; private set; }

        /// <summary>
        /// Gets the commission to be used, if specified, otherwise null.
        /// </summary>
        [JsonProperty]
        public decimal? Commission { get; private set; }

        /// <inheritdoc/>
        decimal IPremiumFundingConfiguration.Commission => this.Commission.HasValue
            ? this.Commission.Value
            : throw new ProductConfigurationException(
                Errors.Product.MisConfiguration("Commission must be specified in the premium funding configuration."));

        public ReleaseContext ReleaseContext { get; private set; }

        /// <inheritdoc />
        IQuoteDatumLocations? IDataLocatorConfig.QuoteDataLocations => this.quoteDatumLocations;

        /// <inheritdoc />
        DataLocators? IDataLocatorConfig.DataLocators => this.dataLocations;

        /// <summary>
        /// Creates a new Premium Funding Service using this configuration (double dispatch).
        /// </summary>
        /// <param name="factory">The factory for creating the service.</param>
        /// <returns>A new instance of a Premium Funding Service.</returns>
        public IFundingService Create(FundingServiceFactory factory)
        {
            return factory.CreatePremiumFundingService(this);
        }

        /// <summary>
        /// Set the data locators configuration.
        /// </summary>
        /// <param name="datumLocations">The quote data locations.</param>
        /// <param name="dataLocations">The data locators.</param>
        public void SetDataLocators(IQuoteDatumLocations datumLocations, DataLocators dataLocations)
        {
            this.dataLocations = dataLocations;
            this.quoteDatumLocations = datumLocations;
        }

        public void SetReleaseContext(ReleaseContext releaseContext)
        {
            this.ReleaseContext = releaseContext;
        }
    }
}
