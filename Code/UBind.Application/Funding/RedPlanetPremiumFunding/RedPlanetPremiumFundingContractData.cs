// <copyright file="RedPlanetPremiumFundingContractData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding;

using Newtonsoft.Json;
using NodaTime;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
using UBind.Domain.Extensions;
using UBind.Domain.ValueTypes;
using UBind.Application.Funding.RedPlanetPremiumFunding.Models;

/// <summary>
/// For holding contract data used in a proposal.
/// </summary>
public class RedPlanetPremiumFundingContractData
{
    public RedPlanetPremiumFundingContractData(
        StandardQuoteDataRetriever quoteDataRetriever,
        IClock clock,
        Guid quoteId,
        string quoteNumber,
        decimal totalPremium)
    {
        var customerName = quoteDataRetriever.Retrieve<string>(StandardQuoteDataField.CustomerName);
        var customerMobile = quoteDataRetriever.Retrieve<string>(StandardQuoteDataField.CustomerMobile);
        var customerPhone = quoteDataRetriever.Retrieve<string>(StandardQuoteDataField.CustomerPhone);
        var customerEmailAddress = quoteDataRetriever.Retrieve<string>(StandardQuoteDataField.CustomerEmail);
        var insuredName = quoteDataRetriever.Retrieve<string>(StandardQuoteDataField.InsuredName);
        var tradingName = quoteDataRetriever.Retrieve<string>(StandardQuoteDataField.TradingName);
        var address = quoteDataRetriever.Retrieve<Domain.ValueTypes.Address>(StandardQuoteDataField.Address);
        var inceptionDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.InceptionDate);
        var expiryDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate);
        var defaultPhoneNumber = customerMobile ?? customerPhone ?? "Not specified";

        this.InsuredName = insuredName ?? "Insurer name not specified";
        this.TradingName = tradingName ?? "Trading name not specified";
        this.CustomerEmail = customerEmailAddress ?? "no-email-address@provided.com";
        this.CustomerName = customerName ?? "Contact name not specified";
        this.TotalPremiumAmount = totalPremium;
        this.InceptionDate = inceptionDate.HasValue
            ? inceptionDate.Value
            : clock.Now().InZone(Timezones.AET).Date;

        this.ExpiryDate = expiryDate.HasValue
            ? expiryDate.Value
            : clock.Now().InZone(Timezones.AET).Date;

        this.StreetAddress = new Models.Address()
        {
            Address1 = address?.Line1 ?? "Address not specified",
            Suburb = address?.Suburb ?? "Suburb not specified",
            Postcode = address?.Postcode ?? "0",
            State = address?.State != State.Unspecified && address?.State != null
                ? address.State.ToString()
                : "Not specified",
        };

        this.HomePhone = customerPhone ?? defaultPhoneNumber;
        this.MobilePhone = customerMobile ?? defaultPhoneNumber;
        this.PolicyNumber = quoteNumber;
        this.QuoteId = quoteId;
    }

    [JsonConstructor]
    private RedPlanetPremiumFundingContractData()
    {
    }

    [JsonProperty]
    private string InsuredName { get; set; } = string.Empty;

    [JsonProperty]
    private string TradingName { get; set; } = string.Empty;

    [JsonProperty]
    private string CustomerEmail { get; set; } = string.Empty;

    [JsonProperty]
    private decimal TotalPremiumAmount { get; set; }

    [JsonProperty]
    private LocalDate InceptionDate { get; set; }

    [JsonProperty]
    private LocalDate ExpiryDate { get; set; }

    [JsonProperty]
    private Models.Address? StreetAddress { get; set; }

    [JsonProperty]
    private string HomePhone { get; set; } = string.Empty;

    [JsonProperty]
    private string CustomerName { get; set; } = string.Empty;

    [JsonProperty]
    private string MobilePhone { get; set; } = string.Empty;

    [JsonProperty]
    private string PolicyNumber { get; set; } = string.Empty;

    [JsonProperty]
    private Guid QuoteId { get; set; }

    /// <summary>
    /// Generate a new instance of <see cref="CreateUpdateQuoteModel"/> with held data.
    /// </summary>
    /// <returns>An instance of <see cref="CreateUpdateQuoteModel"/>.</returns>
    public CreateUpdateQuoteModel ToQuoteContract(IRedPlanetFundingConfiguration configuration, string? paymentTypeCode = null)
    {
        var inceptionDateTime = this.InceptionDate
                    .AtStartOfDayInZone(Timezones.AET)
                    .ToDateTimeOffset().ToString("s");

        var expiryDateTime = this.ExpiryDate
                    .AtStartOfDayInZone(Timezones.AET)
                    .ToDateTimeOffset().ToString("s");

        var policy = new Policy
        {
            Class = configuration.InsuranceClassCode,
            InceptionDate = inceptionDateTime,
            ExpiryDate = expiryDateTime,
            Amount = this.TotalPremiumAmount,
            PolicyNumber = this.PolicyNumber,
            UnderwriterCode = configuration.UnderwriterCode,
        };

        var contractDetails = new ContractDetails
        {
            Creditor = configuration.CreditorCode,
            Product = configuration.Product,
            Amount = this.TotalPremiumAmount,
            PaymentFrequency = configuration.PaymentFrequency,
            InceptionDate = inceptionDateTime,
            CommissionRate = configuration.CommissionRate,
            NumberOfInstalments = configuration.NumberOfInstalments,
        };

        var clientDetails = new ClientDetails
        {
            ExternalId = this.QuoteId.ToString(),
            LegalName = this.InsuredName,
            Telephone = this.HomePhone,
            Mobile = this.MobilePhone,
            Email = this.CustomerEmail,
            StreetAddress = this.StreetAddress,
            TradingName = this.TradingName,
            Contacts = new List<Contact>
            {
                new Contact
                {
                    FirstName = this.CustomerName,
                    Mobile = this.MobilePhone,
                    Phone = this.HomePhone,
                    Email = this.CustomerEmail,
                },
            },
        };

        var cardPaymentDetail = new PaymentDetails
        {
            PaymentType = paymentTypeCode == "DDR" ? "DDR" : "CC",
            PaymentTypeCode = paymentTypeCode,
            BankBranchCode = paymentTypeCode == "DDR" ? string.Empty : null,
        };

        return new CreateUpdateQuoteModel
        {
            ClientDetails = clientDetails,
            ContractDetails = contractDetails,
            PaymentDetails = cardPaymentDetail,
            Policies = new List<Policy>
            {
                policy,
            },
        };
    }
}
