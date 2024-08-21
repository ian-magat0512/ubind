// <copyright file="IIqumulateConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.Iqumulate
{
    using UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever;

    /// <summary>
    /// Configuration for Premium Funding service usage.
    /// </summary>
    public interface IIqumulateConfiguration
    {
        /// <summary>
        /// Gets the Base Url.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Gets the ActionUrl.
        /// </summary>
        string ActionUrl { get; }

        /// <summary>
        /// Gets the Iframe bubbled up Message Origin Url.
        /// </summary>
        string MessageOriginUrl { get; }

        /// <summary>
        /// Gets the affinity scheme code for the funding request.
        /// </summary>
        string AffinitySchemeCode { get; }

        /// <summary>
        /// Gets the email address to cc customer communications to.
        /// </summary>
        string IntroducerContactEmail { get; }

        /// <summary>
        /// Gets the policy class code.
        /// </summary>
        string PolicyClassCode { get; }

        /// <summary>
        /// Gets the policy underwriter code.
        /// </summary>
        string PolicyUnderwriterCode { get; }

        /// <summary>
        /// Gets the Payment method.
        /// </summary>
        string PaymentMethod { get; }

        /// <summary>
        /// Gets the field that will be set to true when the IQumulate Iframe form is successful.
        /// </summary>
        string AcceptanceConfirmationField { get; }

        /// <summary>
        /// Gets the data location for extract quote data for use in premium funding.
        /// </summary>
        ConfiguredIQumulateQuoteDatumLocations QuoteDataLocations { get; }
    }
}
