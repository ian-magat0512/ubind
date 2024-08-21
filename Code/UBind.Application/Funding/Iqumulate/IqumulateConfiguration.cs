// <copyright file="IqumulateConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.Iqumulate
{
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Funding;
    using UBind.Domain.Product;

    /// <summary>
    /// Configuration for the Premium Funding service.
    /// </summary>
    public class IqumulateConfiguration : IIqumulateConfiguration, IFundingConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IqumulateConfiguration"/> class based on a default configuration and an override configuration.
        /// </summary>
        /// <param name="default">The default configuration.</param>
        /// <param name="overrides">The override configuration.</param>
        public IqumulateConfiguration(IqumulateConfiguration @default, IqumulateConfiguration overrides)
        {
            this.BaseUrl = overrides.BaseUrl
                ?? @default.BaseUrl
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration(
                    "Script URL must be specified in the premium funding configuration."));

            this.ActionUrl = overrides.ActionUrl
                ?? @default.ActionUrl
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration(
                    "Action URL must be specified in the premium funding configuration."));

            this.MessageOriginUrl = overrides.MessageOriginUrl
                ?? @default.MessageOriginUrl
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration(
                    "Message Origin URL must be specified in the premium funding configuration."));

            this.PaymentMethod = overrides.PaymentMethod
                ?? @default.PaymentMethod
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration(
                    "Payment method must be specified in the premium funding configuration."));
            this.AffinitySchemeCode = overrides.AffinitySchemeCode
                ?? @default.AffinitySchemeCode
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration(
                    "Affinity scheme code must be specified in the premium funding configuration."));
            this.IntroducerContactEmail = overrides.IntroducerContactEmail
                ?? @default.IntroducerContactEmail
                ?? null;
            this.PolicyClassCode = overrides.PolicyClassCode
                ?? @default.PolicyClassCode
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration(
                    "Policy class code must be specified in the premium funding configuration."));
            this.PolicyUnderwriterCode = overrides.PolicyUnderwriterCode
                ?? @default.PolicyUnderwriterCode
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration(
                    "Policy underwriter code must be specified in the premium funding configuration."));
            this.AcceptanceConfirmationField = overrides.AcceptanceConfirmationField
                ?? @default.AcceptanceConfirmationField
                ?? throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration(
                    "AcceptanceConfirmationField must be specified in the premium funding configuration."));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IqumulateConfiguration"/> class.
        /// </summary>
        /// <param name="baseUrl">The base url.</param>
        /// <param name="actionUrl">the action url.</param>
        /// <param name="messageOriginUrl">the message origin Url.</param>
        /// <param name="paymentMethod">The payment methods that are supported.</param>
        /// <param name="affinitySchemeCode">The affinity scheme code.</param>
        /// <param name="introducerContactEmail">The introducer contact email.</param>
        /// <param name="policyClassCode">The policy class code.</param>
        /// <param name="policyUnderwriterCode">The policy underwriter code.</param>
        /// <param name="acceptanceConfirmationField">The acceptance confirmation field.</param>
        /// <remarks>Exposed for test configuration use.</remarks>
        protected IqumulateConfiguration(
            string baseUrl,
            string actionUrl,
            string messageOriginUrl,
            string paymentMethod,
            string affinitySchemeCode,
            string introducerContactEmail,
            string policyClassCode,
            string policyUnderwriterCode,
            string acceptanceConfirmationField)
        {
            this.BaseUrl = baseUrl;
            this.MessageOriginUrl = messageOriginUrl;
            this.ActionUrl = actionUrl;
            this.PaymentMethod = paymentMethod;
            this.AffinitySchemeCode = affinitySchemeCode;
            this.IntroducerContactEmail = introducerContactEmail;
            this.PolicyClassCode = policyClassCode;
            this.PolicyUnderwriterCode = policyUnderwriterCode;
            this.AcceptanceConfirmationField = acceptanceConfirmationField;
        }

        [JsonConstructor]
        private IqumulateConfiguration(
            string baseUrl,
            string actionUrl,
            string messageOriginUrl,
            string paymentMethod,
            string affinitySchemeCode,
            string policyUnderwriterCode,
            string acceptanceConfirmationField)
        {
            this.BaseUrl = baseUrl;
            this.ActionUrl = actionUrl;
            this.MessageOriginUrl = messageOriginUrl;
            this.PaymentMethod = paymentMethod;
            this.AffinitySchemeCode = affinitySchemeCode;
            this.PolicyUnderwriterCode = policyUnderwriterCode;
            this.AcceptanceConfirmationField = acceptanceConfirmationField;
        }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        [JsonProperty]
        public string BaseUrl { get; private set; }

        /// <summary>
        /// Gets the Action Url.
        /// </summary>
        [JsonProperty]
        public string ActionUrl { get; private set; }

        /// <summary>
        /// Gets the script URL.
        /// </summary>
        [JsonProperty]
        public string MessageOriginUrl { get; private set; }

        /// <summary>
        /// Gets the supported payment methods.
        /// </summary>
        [JsonProperty]
        public string PaymentMethod { get; private set; }

        /// <summary>
        /// Gets the affinity scheme code.
        /// </summary>
        [JsonProperty]
        public string AffinitySchemeCode { get; private set; }

        /// <summary>
        /// Gets the introducer contact email.
        /// </summary>
        [JsonProperty]
        public string? IntroducerContactEmail { get; private set; }

        /// <summary>
        /// Gets the service name.
        /// </summary>
        public FundingServiceName ServiceName => FundingServiceName.IQumulate;

        /// <inheritdoc/>
        [JsonProperty]
        public string? PolicyClassCode { get; private set; }

        /// <summary>
        /// Gets the policy underwriter code.
        /// </summary>
        [JsonProperty]
        public string PolicyUnderwriterCode { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string AcceptanceConfirmationField { get; private set; }

        public ReleaseContext ReleaseContext { get; private set; }

        /// <inheritdoc/>
        public ConfiguredIQumulateQuoteDatumLocations? QuoteDataLocations { get; set; }

        /// <summary>
        /// Create Iqumulate Configuration.
        /// </summary>
        /// <param name="factory">The service factory.</param>
        /// <returns>A new instance of the <see cref="IqumulateFundingService"/>.</returns>
        public IFundingService Create(FundingServiceFactory factory)
        {
            return factory.CreateIQumulateService(this);
        }

        public void SetReleaseContext(ReleaseContext releaseContext)
        {
            this.ReleaseContext = releaseContext;
        }
    }
}
