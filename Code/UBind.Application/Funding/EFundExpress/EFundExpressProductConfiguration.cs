// <copyright file="EFundExpressProductConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.EFundExpress
{
    using System.Configuration;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator;
    using UBind.Domain.Extensions;
    using UBind.Domain.Funding;
    using UBind.Domain.Product;

    /// <summary>
    /// Product configuration for eFundExpress premium funding service.
    /// </summary>
    public class EFundExpressProductConfiguration : IEFundExpressProductConfiguration, IFundingConfiguration
    {
        private IQuoteDatumLocations? quoteDatumLocations;
        private DataLocators? dataLocations;

        /// <summary>
        /// Initializes a new instance of the <see cref="EFundExpressProductConfiguration"/> class.
        /// </summary>
        /// <param name="default">The default configuration.</param>
        /// <param name="overrides">The override configuration.</param>
        public EFundExpressProductConfiguration(EFundExpressProductConfiguration @default, EFundExpressProductConfiguration overrides)
        {
            this.ServiceName = overrides.ServiceName
                ?? @default.ServiceName
                ?? throw this.GetErrorException("serviceName");
            this.BrokerLoginId = overrides.BrokerLoginId
                ?? @default.BrokerLoginId
                ?? throw this.GetErrorException("brokerLoginId");
            this.Password = overrides.Password
                ?? @default.Password
                ?? throw this.GetErrorException("password");
            this.ContractType = overrides.ContractType
                ?? ((IEFundExpressProductConfiguration)@default).ContractType;
            this.UnderwriterName = overrides.UnderwriterName
                ?? @default.UnderwriterName
                ?? throw this.GetErrorException("underwriterName");
            this.FortnightlyInstalments = overrides.FortnightlyInstalments
                ?? ((IEFundExpressProductConfiguration)@default).FortnightlyInstalments;
            this.FixedInterestRate = overrides.FixedInterestRate
                ?? ((IEFundExpressProductConfiguration)@default).FixedInterestRate;
            this.FirstInstalmentFee = overrides.FirstInstalmentFee
                ?? ((IEFundExpressProductConfiguration)@default).FirstInstalmentFee;
            this.NumberOfInstalments = overrides.NumberOfInstalments
                ?? ((IEFundExpressProductConfiguration)@default).NumberOfInstalments;
            this.ClientUrl = overrides.ClientUrl
                ?? @default.ClientUrl
                ?? throw this.GetErrorException("clienrUrl");
            this.PolicyClassCode = overrides.PolicyClassCode
                ?? ((IEFundExpressProductConfiguration)@default).PolicyClassCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EFundExpressProductConfiguration"/> class.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="brokerLoginId">The broker's login ID.</param>
        /// <param name="password">The broker's password.</param>
        /// <param name="underwriterName">The underwriter's name.</param>
        /// <param name="contractType">The type of contract (commercial, business, etc.).</param>
        /// <param name="fortnightlyInstallments">A value indicating if installments are made every two weeks.</param>
        /// <param name="fixedInterestRate">The fixed interest rate to be used.</param>
        /// <param name="fixedInstallmentScheme">The fixed number of installments to be used.</param>
        /// <param name="firstInstalmentFee">The fee to be included in the first instalment.</param>
        /// <param name="clientUrl">The soap client url to be used for the transaction.</param>
        /// <param name="policyClassCode">The policy class code.</param>
        /// <remarks>Exposed for test configuration use.</remarks>
        protected EFundExpressProductConfiguration(
            string serviceName,
            string brokerLoginId,
            string password,
            string underwriterName,
            ContractType contractType,
            bool fortnightlyInstallments,
            decimal fixedInterestRate,
            int fixedInstallmentScheme,
            decimal firstInstalmentFee,
            string clientUrl,
            string policyClassCode)
        {
            this.ServiceName = serviceName;
            this.BrokerLoginId = brokerLoginId;
            this.Password = password;
            this.UnderwriterName = underwriterName;
            this.ContractType = contractType;
            this.FortnightlyInstalments = fortnightlyInstallments;
            this.FixedInterestRate = fixedInterestRate;
            this.NumberOfInstalments = fixedInstallmentScheme;
            this.FirstInstalmentFee = firstInstalmentFee;
            this.ClientUrl = clientUrl;
            this.PolicyClassCode = policyClassCode;
        }

        [JsonConstructor]
        private EFundExpressProductConfiguration(
            string serviceName,
            string brokerLoginId,
            string policyClassCode,
            string password,
            string underwriterName,
            string clientUrl)
        {
            this.ServiceName = serviceName;
            this.BrokerLoginId = brokerLoginId;
            this.PolicyClassCode = policyClassCode;
            this.Password = password;
            this.UnderwriterName = underwriterName;
            this.ClientUrl = clientUrl;
        }

        /// <inheritdoc/>
        [JsonProperty]
        public string ServiceName { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string BrokerLoginId { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string PolicyClassCode { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string Password { get; private set; }

        /// <summary>
        /// Gets the contract type, if specified, or null.
        /// </summary>
        [JsonProperty]
        public ContractType? ContractType { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string UnderwriterName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether fortnightly instalments should be used, if specified, or null.
        /// </summary>
        [JsonProperty]
        public bool? FortnightlyInstalments { get; private set; }

        /// <summary>
        /// Gets the interest rate, if specified, or null.
        /// </summary>
        [JsonProperty]
        public decimal? FixedInterestRate { get; private set; }

        /// <summary>
        /// Gets the total number of instalments (including the first one), if specified, or null.
        /// </summary>
        [JsonProperty]
        public int? NumberOfInstalments { get; private set; }

        /// <summary>
        /// Gets the fee to be included in the first instalment, if specified, or null.
        /// </summary>
        [JsonProperty]
        public decimal? FirstInstalmentFee { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string ClientUrl { get; private set; }

        public ReleaseContext ReleaseContext { get; private set; }

        /// <inheritdoc/>
        FundingServiceName IFundingConfiguration.ServiceName => FundingServiceName.PremiumFunding;

        /// <inheritdoc/>
        ContractType IEFundExpressProductConfiguration.ContractType => this.ContractType.GetValueOrThrow(this.GetErrorException(nameof(this.ContractType)));

        /// <inheritdoc/>
        bool IEFundExpressProductConfiguration.FortnightlyInstalments => this.FortnightlyInstalments.GetValueOrThrow(this.GetErrorException(nameof(this.FortnightlyInstalments)));

        /// <inheritdoc/>
        decimal IEFundExpressProductConfiguration.FixedInterestRate => this.FixedInterestRate.GetValueOrThrow(this.GetErrorException(nameof(this.FixedInterestRate)));

        /// <inheritdoc/>
        int IEFundExpressProductConfiguration.NumberOfInstalments => this.NumberOfInstalments.GetValueOrThrow(this.GetErrorException(nameof(this.NumberOfInstalments)));

        /// <inheritdoc/>
        decimal IEFundExpressProductConfiguration.FirstInstalmentFee => this.FirstInstalmentFee.GetValueOrThrow(this.GetErrorException(nameof(this.FirstInstalmentFee)));

        /// <inheritdoc />
        IQuoteDatumLocations? IDataLocatorConfig.QuoteDataLocations => this.quoteDatumLocations;

        /// <inheritdoc />
        DataLocators? IDataLocatorConfig.DataLocators => this.dataLocations;

        /// <inheritdoc/>
        public IFundingService Create(FundingServiceFactory factory)
        {
            return factory.CreateEfundExpressFundingService(this);
        }

        /// <summary>
        /// Set the data locators configuration.
        /// </summary>
        /// <param name="datumLocations">The quote data locations.</param>
        /// <param name="dataLocations">The data locators.</param>
        public void SetDataLocators(IQuoteDatumLocations? datumLocations, DataLocators? dataLocations)
        {
            this.dataLocations = dataLocations;
            this.quoteDatumLocations = datumLocations;
        }

        public void SetReleaseContext(ReleaseContext releaseContext)
        {
            this.ReleaseContext = releaseContext;
        }

        private ConfigurationErrorsException GetErrorException(string propertyName) =>
            new ConfigurationErrorsException($"{propertyName} must be specified in Principal Finance funding configuration (in the defaults or environment-specific section).");
    }
}
