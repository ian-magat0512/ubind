// <copyright file="RedPlanetPremiumFundingConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UBind.Application.Funding;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.Quote.DataLocator;
using UBind.Domain.Exceptions;
using UBind.Domain.Funding;
using UBind.Domain.Product;

public class RedPlanetPremiumFundingConfiguration : IRedPlanetFundingConfiguration
{
    private IQuoteDatumLocations? quoteDatumLocations;
    private DataLocators? dataLocations;

    public RedPlanetPremiumFundingConfiguration(RedPlanetPremiumFundingConfiguration @default, RedPlanetPremiumFundingConfiguration overrides)
    {
        this.BaseUrl = overrides.BaseUrl
            ?? @default.BaseUrl
            ?? throw new ProductConfigurationException(
                Errors.Product.MisConfiguration("Base URL must be specified in the funding configuration."));

        this.CommissionRate = overrides.CommissionRate != 0 ? overrides.CommissionRate
            : @default.CommissionRate != 0 ? @default.CommissionRate : 0;

        this.NumberOfInstalments = overrides.NumberOfInstalments != 0 ? overrides.NumberOfInstalments
            : ((IRedPlanetFundingConfiguration)@default).NumberOfInstalments;

        this.Password = overrides.Password
            ?? @default.Password
            ?? throw new ProductConfigurationException(
                Errors.Product.MisConfiguration("Password must be specified in the funding configuration."));

        this.CreditorCode = overrides.CreditorCode
            ?? @default.CreditorCode
            ?? throw new ProductConfigurationException(
                Errors.Product.MisConfiguration("Creditor Code must be specified in the funding configuration."));

        this.PaymentFrequency = overrides.PaymentFrequency
            ?? ((IRedPlanetFundingConfiguration)@default).PaymentFrequency;

        this.InsuranceClassCode = overrides.InsuranceClassCode
            ?? ((IRedPlanetFundingConfiguration)@default).InsuranceClassCode;

        this.Username = overrides.Username
            ?? @default.Username
            ?? throw new ProductConfigurationException(
                Errors.Product.MisConfiguration("Username must be specified in the funding configuration."));

        this.Product = overrides.Product
            ?? @default.Product
            ?? throw new ProductConfigurationException(
                Errors.Product.MisConfiguration("Product must be specified in the funding configuration."));

        this.UnderwriterCode = overrides.UnderwriterCode ?? @default.UnderwriterCode;

        this.FundingType = overrides.FundingType != null ? overrides.FundingType
            : @default.FundingType != null ? @default.FundingType
                : throw new ProductConfigurationException(
                Errors.Product.MisConfiguration("Funding Type must be specified in the funding configuration."));
    }

    [JsonConstructor]
    private RedPlanetPremiumFundingConfiguration(
        string username,
        string password,
        string baseUrl,
        string product,
        string paymentFrequency,
        string insuranceClassCode,
        string creditorCode)
    {
        this.Username = username;
        this.Password = password;
        this.BaseUrl = baseUrl;
        this.Product = product;
        this.PaymentFrequency = paymentFrequency;
        this.InsuranceClassCode = insuranceClassCode;
        this.CreditorCode = creditorCode;
    }

    /// <inheritdoc/>
    [JsonProperty]
    public string Username { get; private set; }

    /// <inheritdoc/>
    [JsonProperty]
    public string Password { get; private set; }

    /// <inheritdoc/>
    [JsonProperty]
    public string BaseUrl { get; private set; }

    /// <inheritdoc/>
    [JsonProperty]
    public string Product { get; private set; }

    /// <inheritdoc/>
    [JsonProperty]
    public string CreditorCode { get; private set; }

    /// <inheritdoc/>
    [JsonProperty]
    public string PaymentFrequency { get; private set; }

    [JsonProperty]
    public int NumberOfInstalments { get; private set; }

    [JsonProperty]
    public decimal CommissionRate { get; private set; }

    [JsonProperty]
    public string InsuranceClassCode { get; private set; }

    [JsonProperty]
    public string? UnderwriterCode { get; private set; }

    public FundingServiceName ServiceName => FundingServiceName.RedPlanet;

    [JsonConverter(typeof(StringEnumConverter))]
    public RedPlanetFundingType FundingType { get; private set; }

    public ReleaseContext ReleaseContext { get; private set; }

    IQuoteDatumLocations? IDataLocatorConfig.QuoteDataLocations => this.quoteDatumLocations;

    DataLocators? IDataLocatorConfig.DataLocators => this.dataLocations;

    /// <summary>
    /// Creates a new Premium Funding Service using this configuration (double dispatch).
    /// </summary>
    /// <param name="factory">The factory for creating the service.</param>
    /// <returns>A new instance of a Premium Funding Service.</returns>
    public IFundingService Create(FundingServiceFactory factory)
    {
        return factory.CreateRedPlanetPremiumFundingService(this);
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
}