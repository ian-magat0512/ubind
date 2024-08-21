// <copyright file="IDeftConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Deft
{
    /// <summary>
    /// Provides configuration details for a DEFT account.
    /// </summary>
    public interface IDeftConfiguration
    {
        /// <summary>
        /// Gets the client ID for obtaining access token.
        /// </summary>
        string ClientId { get; }

        /// <summary>
        /// Gets the client secret to be used for obtaining access token.
        /// </summary>
        string ClientSecret { get; }

        /// <summary>
        /// Gets the biller code that will be used for payment requests.
        /// </summary>
        string BillerCode { get; }

        /// <summary>
        /// Gets the URL for making payment requests.
        /// </summary>
        string PaymentUrl { get; }

        /// <summary>
        /// Gets the URL for obtaining access tokens.
        /// </summary>
        string AuthorizationUrl { get; }

        /// <summary>
        /// Gets the URL for obtaining surcharge amounts.
        /// </summary>
        string SurchargeUrl { get; }

        /// <summary>
        /// Gets the URL for obtaining surcharge amounts.
        /// </summary>
        string DrnUrl { get; }

        /// <summary>
        /// Gets the configuration for CRN generation.
        /// </summary>
        CrnGenerationConfiguration CrnGeneration { get; }

        /// <summary>
        /// Gets the configuration for credit card detail encryption.
        /// </summary>
        string SecurityKey { get; }
    }
}
