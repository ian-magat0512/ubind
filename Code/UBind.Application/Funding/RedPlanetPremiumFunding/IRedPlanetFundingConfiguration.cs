// <copyright file="IRedPlanetFundingConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding;

using UBind.Domain.Aggregates.Quote.DataLocator;
using UBind.Domain.Funding;

public interface IRedPlanetFundingConfiguration : IDataLocatorConfig, IFundingConfiguration
{
    string Username { get; }

    string Password { get; }

    /// <summary>
    /// Red Planet's Odyssey API base URL
    /// </summary>
    string BaseUrl { get; }

    /// <summary>
    /// Value for contract details product that will be provided by RedPlanet.
    /// Either IPF or PBM. IPF for commercial. PBM for domestic in general.
    /// </summary>
    string Product { get; }

    string CreditorCode { get; }

    /// <summary>
    /// The payment frequency code from Odyssey API
    /// Endpoint: /common/payment-frequencies
    /// </summary>
    string PaymentFrequency { get; }

    decimal CommissionRate { get; }

    int NumberOfInstalments { get; }

    string InsuranceClassCode { get; }

    /// <summary>
    /// The underwriter code from Odyssey API
    /// Endpoint: /common/underwriters
    /// </summary>
    string? UnderwriterCode { get; }

    /// <summary>
    /// An indicator of what specific implementation from Red Planet is going to be used.
    /// Currently, only Arteva (Odyssey API) is available.
    /// </summary>
    RedPlanetFundingType FundingType { get; }
}