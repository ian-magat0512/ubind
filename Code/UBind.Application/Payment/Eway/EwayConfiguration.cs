// <copyright file="EwayConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Eway
{
    using Newtonsoft.Json;
    using UBind.Domain.Enums;

    /// <summary>
    /// Account configuration for the Eway payment gateway for an environment.
    /// </summary>
    public class EwayConfiguration : IPaymentConfiguration, IEwayConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EwayConfiguration"/> class.
        /// </summary>
        /// <param name="defaults">Default values.</param>
        /// <param name="overrides">Environment-specific overrides.</param>
        public EwayConfiguration(EwayConfiguration defaults, EwayConfiguration overrides)
        {
            this.Endpoint = overrides.Endpoint != EwayEndpoint.None
                ? overrides.Endpoint
                : defaults.Endpoint;
            this.ApiKey = overrides.ApiKey ?? defaults.ApiKey;
            this.Password = overrides.Password ?? defaults.Password;
            this.ClientSideEncryptionKey = overrides.ClientSideEncryptionKey ?? defaults.ClientSideEncryptionKey;
            this.EncryptionUrl = overrides.EncryptionUrl ?? defaults.EncryptionUrl;
            this.ServerSideEncryptionKey = overrides.ServerSideEncryptionKey ?? defaults.ServerSideEncryptionKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwayConfiguration"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint to use.</param>
        /// <param name="apiKey">The API key to use.</param>
        /// <param name="password">The password to use.</param>
        /// <param name="clientSideEncryptionKey">The client side encryption key to use.</param>
        /// <param name="encryptionUrl">The encryption service url to use.</param>
        /// <param name="serverSideEncryptionKey">The encryption service key pass to use.</param>
        public EwayConfiguration(
            EwayEndpoint endpoint,
            string apiKey,
            string password,
            string clientSideEncryptionKey,
            string encryptionUrl,
            string serverSideEncryptionKey)
        {
            this.Endpoint = endpoint;
            this.ApiKey = apiKey;
            this.Password = password;
            this.ClientSideEncryptionKey = clientSideEncryptionKey;
            this.EncryptionUrl = encryptionUrl;
            this.ServerSideEncryptionKey = serverSideEncryptionKey;
        }

        [JsonConstructor]
        private EwayConfiguration()
        {
        }

        /// <summary>
        /// Gets the endpoint to use.
        /// </summary>
        [JsonProperty]
        public EwayEndpoint Endpoint { get; private set; }

        /// <summary>
        /// Gets the API key to use.
        /// </summary>
        [JsonProperty]
        public string ApiKey { get; private set; }

        /// <summary>
        /// Gets the password to use.
        /// </summary>
        [JsonProperty]
        public string Password { get; private set; }

        /// <summary>
        /// Gets the client side encryption key to use.
        /// </summary>
        [JsonProperty]
        public string ClientSideEncryptionKey { get; private set; }

        /// <summary>
        /// Gets the encryption service url to use.
        /// </summary>
        [JsonProperty]
        public string EncryptionUrl { get; private set; }

        /// <summary>
        /// Gets the server encryption pass key to use.
        /// </summary>
        [JsonProperty]
        public string ServerSideEncryptionKey { get; private set; }

        /// <summary>
        /// Gets the name of the payment gateway.
        /// </summary>
        public PaymentGatewayName GatewayName => PaymentGatewayName.EWay;

        /// <summary>
        /// Create an eway payment gateway.
        /// </summary>
        /// <param name="factory">Payment gateway factory.</param>
        /// <returns>A new instance of an eway payment gateway from the factory.</returns>
        public IPaymentGateway Create(PaymentGatewayFactory factory)
        {
            return factory.CreateEwayPaymentGateway(this);
        }
    }
}
