// <copyright file="DeftConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Deft
{
    using Newtonsoft.Json;
    using UBind.Domain.Enums;

    /// <summary>
    /// Account configuration for the DEFT payment gateway for an environment.
    /// </summary>
    public class DeftConfiguration : IPaymentConfiguration, IDeftConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeftConfiguration"/> class.
        /// </summary>
        /// <param name="defaults">Default values.</param>
        /// <param name="overrides">Environment-specific overrides.</param>
        public DeftConfiguration(DeftConfiguration defaults, DeftConfiguration overrides)
        {
            this.AuthorizationUrl = overrides.AuthorizationUrl ?? defaults.AuthorizationUrl;
            this.ClientId = overrides.ClientId ?? defaults.ClientId;
            this.ClientSecret = overrides.ClientSecret ?? defaults.ClientSecret;
            this.PaymentUrl = overrides.PaymentUrl ?? defaults.PaymentUrl;
            this.SurchargeUrl = overrides.SurchargeUrl ?? defaults.SurchargeUrl;
            this.DrnUrl = overrides.DrnUrl ?? defaults.DrnUrl;
            this.BillerCode = overrides.BillerCode ?? defaults.BillerCode;
            this.CrnGeneration = overrides.CrnGeneration ?? defaults.CrnGeneration;
            this.SecurityKey = overrides.SecurityKey ?? defaults.SecurityKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeftConfiguration"/> class.
        /// </summary>
        /// <param name="authorizationUrl">The URL for authorization requests.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="paymentUrl">The URL for payment requests.</param>
        /// <param name="surchargeUrl">The URL for surcharge requests.</param>
        /// <param name="drnUrl">The URL for DRN generation requests.</param>
        /// <param name="billerCode">The DEFT biller code.</param>
        /// <param name="isCrnUniqueAcrossTenant">A value indicating if the CRN should be unique across all products in a tenant.</param>
        /// <param name="crnGenerationMethod">The type of CRN to generate.</param>
        /// <param name="crnPrefix">The prefix to be used in CRN generation, if any, otherwise null.</param>
        protected DeftConfiguration(
            string authorizationUrl,
            string clientId,
            string clientSecret,
            string paymentUrl,
            string surchargeUrl,
            string drnUrl,
            string billerCode,
            bool isCrnUniqueAcrossTenant,
            CrnGenerationMethod crnGenerationMethod,
            string securityKey,
            string crnPrefix = null)
        {
            this.AuthorizationUrl = authorizationUrl;
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.PaymentUrl = paymentUrl;
            this.SurchargeUrl = surchargeUrl;
            this.DrnUrl = drnUrl;
            this.BillerCode = billerCode;
            this.SecurityKey = securityKey;
            this.CrnGeneration = new CrnGenerationConfiguration(
                isCrnUniqueAcrossTenant,
                crnGenerationMethod,
                crnPrefix);
        }

        [JsonConstructor]
        private DeftConfiguration()
        {
        }

        /// <summary>
        /// Gets the URL for retrieving access tokens.
        /// </summary>
        [JsonProperty]
        public string AuthorizationUrl { get; private set; }

        /// <summary>
        /// Gets the API key to use.
        /// </summary>
        [JsonProperty]
        public string ClientId { get; private set; }

        /// <summary>
        /// Gets the password to use.
        /// </summary>
        [JsonProperty]
        public string ClientSecret { get; private set; }

        /// <summary>
        /// Gets the URL for making payment requests..
        /// </summary>
        [JsonProperty]
        public string PaymentUrl { get; private set; }

        /// <summary>
        /// Gets the URL for making surcharge requests..
        /// </summary>
        [JsonProperty]
        public string SurchargeUrl { get; private set; }

        /// <summary>
        /// Gets the URL for obtaining surcharge amounts.
        /// </summary>
        [JsonProperty]
        public string DrnUrl { get; private set; }

        /// <summary>
        /// Gets the Biller code to use in payment requests.
        /// </summary>
        [JsonProperty]
        public string BillerCode { get; private set; }

        /// <summary>
        /// Gets the configuration for CRN generation.
        /// </summary>
        [JsonProperty]
        public CrnGenerationConfiguration CrnGeneration { get; private set; }

        /// <summary>
        /// Gets the passkey to use for data encryption.
        /// </summary>
        [JsonProperty]
        public string SecurityKey { get; private set; }

        /// <summary>
        /// Gets the name of the payment gateway.
        /// </summary>
        public PaymentGatewayName GatewayName => PaymentGatewayName.Deft;

        /// <summary>
        /// Create an eway payment gateway.
        /// </summary>
        /// <param name="factory">Payment gateway factory.</param>
        /// <returns>A new instance of an eway payment gateway from the factory.</returns>
        public IPaymentGateway Create(PaymentGatewayFactory factory)
        {
            return factory.CreateDeftPaymentGateway(this);
        }
    }
}
