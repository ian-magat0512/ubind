// <copyright file="ZaiConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Payment.Zai
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Application.Payment.Zai.ZaiEntities;
    using UBind.Domain.Enums;

    public class ZaiConfiguration : IPaymentConfiguration, IZaiConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZaiConfiguration"/>.
        /// </summary>
        /// <param name="defaults">The default values.</param>
        /// <param name="overrides">The environment-specific values.</param>
        public ZaiConfiguration(IZaiConfiguration defaults, IZaiConfiguration overrides)
        {
            this.ClientId = overrides.ClientId ?? defaults.ClientId;
            this.ClientSecret = overrides.ClientSecret ?? defaults.ClientSecret;
            this.Scope = overrides.Scope ?? defaults.Scope;
            this.PaymentUrl = overrides.PaymentUrl ?? defaults.PaymentUrl;
            this.AuthorizationUrl = overrides.AuthorizationUrl ?? defaults.AuthorizationUrl;
            this.UserCreationUrl = overrides.UserCreationUrl ?? defaults.UserCreationUrl;
            this.UserRetrievalUrl = overrides.UserRetrievalUrl ?? defaults.UserRetrievalUrl;
            this.UserItemRetrievalUrl = overrides.UserItemRetrievalUrl ?? defaults.UserItemRetrievalUrl;
            this.ItemCreationUrl = overrides.ItemCreationUrl ?? defaults.ItemCreationUrl;
            this.ItemUpdateUrl = overrides.ItemUpdateUrl ?? defaults.ItemUpdateUrl;
            this.CardCaptureUrl = overrides.CardCaptureUrl ?? defaults.CardCaptureUrl;
            this.CardRetrievalUrl = overrides.CardRetrievalUrl ?? defaults.CardRetrievalUrl;
            this.FeeCreationUrl = overrides.FeeCreationUrl ?? defaults.FeeCreationUrl;
            this.OrganisationSellerAccounts = overrides.OrganisationSellerAccounts ?? defaults.OrganisationSellerAccounts;
        }

        [JsonConstructor]
        protected ZaiConfiguration(
            string clientId,
            string clientSecret,
            string scope,
            string paymentUrl,
            string authorizationUrl,
            string userCreationUrl,
            string userRetrievalUrl,
            string userItemRetrievalUrl,
            string itemCreationUrl,
            string itemUpdateUrl,
            string cardCaptureUrl,
            string cardRetrievalUrl,
            string feeCreationUrl,
            List<OrganisationSellerAccount> organisationSellerAccounts)
        {
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.Scope = scope;
            this.AuthorizationUrl = authorizationUrl;
            this.PaymentUrl = paymentUrl;
            this.UserCreationUrl = userCreationUrl;
            this.UserRetrievalUrl = userRetrievalUrl;
            this.ItemCreationUrl = itemCreationUrl;
            this.ItemUpdateUrl = itemUpdateUrl;
            this.UserItemRetrievalUrl = userItemRetrievalUrl;
            this.CardCaptureUrl = cardCaptureUrl;
            this.CardRetrievalUrl = cardRetrievalUrl;
            this.FeeCreationUrl = feeCreationUrl;
            this.OrganisationSellerAccounts = organisationSellerAccounts;
        }

        /// <inheritdoc/>
        [JsonProperty]
        public string ClientId { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string ClientSecret { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string Scope { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public List<OrganisationSellerAccount> OrganisationSellerAccounts { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string PaymentUrl { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string AuthorizationUrl { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string UserCreationUrl { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string UserRetrievalUrl { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string UserItemRetrievalUrl { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string ItemCreationUrl { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string ItemUpdateUrl { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string CardCaptureUrl { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string CardRetrievalUrl { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string FeeCreationUrl { get; private set; }

        public PaymentGatewayName GatewayName => PaymentGatewayName.Zai;

        public IPaymentGateway Create(PaymentGatewayFactory factory)
        {
            return factory.CreateZaiPaymentGateway(this);
        }
    }
}
