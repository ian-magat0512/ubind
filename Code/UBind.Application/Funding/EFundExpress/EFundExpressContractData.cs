// <copyright file="EFundExpressContractData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.EFundExpress;

using System.Globalization;
using Newtonsoft.Json;
using NodaTime;
using UBind.Application.ConnectedServices.Funding.EFundExpress;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
using UBind.Domain.Exceptions;
using UBind.Domain.Extensions;
using UBind.Domain.ReadWriteModel;

/// <summary>
/// For holding contract data used in EFundExpress proposal request.
/// </summary>
public class EFundExpressContractData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EFundExpressContractData"/> class.
    /// </summary>
    /// <param name="quoteDataRetriever">The quote data retriever.</param>
    /// <param name="priceBreakdown">The price breakdown.</param>
    /// <param name="configuration">The principal finance funding configuration setting.</param>
    /// <param name="issueOption">The option to use for issuance of funding proposal.</param>
    /// <param name="issueCopy">The option to use for retrieval of copy of funding proposal.</param>
    /// <param name="successUrl">The URL the funding provider will redirect to after funding has been successfully accepted.</param>
    /// <param name="cancellationUrl">The URL the funding provider will redirect to after funding attempt has been cancelled.</param>
    /// <param name="isMutual">The if is mutual.</param>
    /// <param name="quoteNumber">The quoteNumber if provided.</param>
    /// <param name="clock">A clock for default values.</param>
    public EFundExpressContractData(
        StandardQuoteDataRetriever quoteDataRetriever,
        PriceBreakdown priceBreakdown,
        IEFundExpressProductConfiguration configuration,
        int issueOption,
        int issueCopy,
        string successUrl,
        string cancellationUrl,
        bool isMutual,
        string? quoteNumber,
        IClock clock)
    {
        // TODO: add contract-type-specific properties based on contract type.
        if (priceBreakdown.TotalPayable == default)
        {
            throw new ErrorException(Errors.Payment.Funding.TotalPayableAmountMissing(isMutual));
        }

        var customerMobile = quoteDataRetriever.Retrieve(StandardQuoteDataField.CustomerMobile);
        var customerPhone = quoteDataRetriever.Retrieve(StandardQuoteDataField.CustomerPhone);
        var customerName = quoteDataRetriever.Retrieve(StandardQuoteDataField.CustomerName);
        var customerEmail = quoteDataRetriever.Retrieve(StandardQuoteDataField.CustomerEmail);
        var defaultPhoneNumber = customerMobile ?? customerPhone ?? "0412345678";
        var insuredName = quoteDataRetriever.Retrieve(StandardQuoteDataField.InsuredName);
        var abn = quoteDataRetriever.Retrieve(StandardQuoteDataField.Abn);
        var address = quoteDataRetriever.Retrieve<UBind.Domain.ValueTypes.Address>(StandardQuoteDataField.Address);
        var inceptionDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.InceptionDate);
        var expiryDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate);
        this.UsesPlaceholderData = !inceptionDate.HasValue
            || !expiryDate.HasValue
            || (address.Line1 == null)
            || (address.Postcode == null)
            || (customerName == null)
            || (customerMobile == null)
            || (issueOption == 3 && customerEmail == null);

        this.BrokerLoginId = configuration.BrokerLoginId;
        this.Password = configuration.Password;
        this.ContractType = configuration.ContractType.ToString();
        this.IssueOption = issueOption;
        this.IssueCopy = issueCopy;
        this.PolicyClass = configuration.PolicyClassCode;
        this.UnderWriterName = configuration.UnderwriterName;
        this.FundingPeriod = configuration.NumberOfInstalments;
        this.FortnightlyInstalments = configuration.FortnightlyInstalments.ToString();
        this.BrokerUrl = successUrl;
        this.BrokerExitUrl = cancellationUrl;

        this.ClientName = !string.IsNullOrEmpty(insuredName)
            ? insuredName
            : "No insured name provided.";

        this.ContactPerson = customerName ?? "Anonymous";
        this.StreetAddress = !string.IsNullOrEmpty(address.Line1)
            ? address.Line1
            : "No address provided";

        this.State = address.State != Domain.ValueTypes.State.Unspecified
            ? address.State.ToString()
            : "No state provided";

        this.Suburb = !string.IsNullOrEmpty(address.Suburb)
           ? address.Suburb
           : "No suburb provided";

        this.AbnOrAcn = !string.IsNullOrEmpty(abn)
            ? abn
            : string.Empty;

        this.PolicyNumber = quoteNumber ?? "No Policy Number Provided";

        this.PostCode = !string.IsNullOrEmpty(address.Postcode)
           ? address.Postcode
           : "3000";

        this.ClientMobile = customerMobile ?? defaultPhoneNumber;
        this.ClientPhone = customerPhone ?? defaultPhoneNumber;
        this.ClientEmail = customerEmail ?? "anonymous@mail.com";
        this.InceptionDate = inceptionDate.HasValue ?
                       inceptionDate.Value.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("en-AU"))
                       : clock.Now().InZone(Timezones.AET).Date.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("en-AU"));
        this.ExpiryDate = expiryDate.HasValue ?
                        expiryDate.Value.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("en-AU"))
                        : clock.Now().InZone(Timezones.AET).Date.PlusYears(1).ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("en-AU"));
        this.PolicyPremium = priceBreakdown.TotalPayable.ToString();
    }

    [JsonConstructor]
    private EFundExpressContractData()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the contract data uses placeholder data.
    /// </summary>
    /// <remarks>
    /// Using placeholder data that does not affect the pricing allows us to display the price to the use early,
    /// but contracts using placeholder data must be updated before acceptance.
    /// .</remarks>
    [JsonProperty]
    public bool UsesPlaceholderData { get; private set; }

    [JsonProperty]
    private string BrokerLoginId { get; set; }

    [JsonProperty]
    private string Password { get; set; }

    [JsonProperty]
    private string AbnOrAcn { get; set; }

    [JsonProperty]
    private string ContractType { get; set; }

    [JsonProperty]
    private int IssueOption { get; set; }

    [JsonProperty]
    private int IssueCopy { get; set; }

    [JsonProperty]
    private int FundingPeriod { get; set; }

    [JsonProperty]
    private string BrokerUrl { get; set; }

    [JsonProperty]
    private string BrokerExitUrl { get; set; }

    [JsonProperty]
    private string FortnightlyInstalments { get; set; }

    [JsonProperty]
    private string ClientName { get; set; }

    [JsonProperty]
    private string ContactPerson { get; set; }

    [JsonProperty]
    private string StreetAddress { get; set; }

    [JsonProperty]
    private string Suburb { get; set; }

    [JsonProperty]
    private string State { get; set; }

    [JsonProperty]
    private string PostCode { get; set; }

    [JsonProperty]
    private string ClientMobile { get; set; }

    [JsonProperty]
    private string ClientPhone { get; set; }

    [JsonProperty]
    private string ClientEmail { get; set; }

    [JsonProperty]
    private string UnderWriterName { get; set; }

    [JsonProperty]
    private string InceptionDate { get; set; }

    [JsonProperty]
    private string ExpiryDate { get; set; }

    [JsonProperty]
    private string PolicyPremium { get; set; }

    [JsonProperty]
    private string PolicyClass { get; set; }

    [JsonProperty]
    private string PolicyNumber { get; set; }

    /// <summary>
    /// Modify the data for testing.
    /// </summary>
    public void UpdateContractDataForTesting()
    {
        // It seems having the value lower than 1$ cannot be accepted by Efund.
        // This is the minimun acceptable amount accepted by Efund.
        this.PolicyPremium = 1.00M.ToString();
    }

    /// <summary>
    /// Generate a new instance of <see cref="eFundExpressObject"/> with held data.
    /// </summary>
    /// <returns>An instance of <see cref="eFundExpressObject"/>.</returns>
    public eFundExpressObject ToContract()
    {
        var policies = new ClientPolicy[1];
        policies[0] = new ClientPolicy
        {
            UnderwriterName = this.UnderWriterName,
            PolicyInception = this.InceptionDate,
            PolicyExpiry = this.ExpiryDate,
            PolicyPremium = this.PolicyPremium,
            PolicyClass = this.PolicyClass,
            PolicyNumber = this.PolicyNumber,
        };

        return new eFundExpressObject
        {
            BrokerLoginID = this.BrokerLoginId,
            Password = this.Password,
            TypeOfContract = this.ContractType,
            IssueOption = this.IssueOption,
            IssueCopy = this.IssueCopy,
            ClientName = this.ClientName,
            ContactPerson = this.ContactPerson,
            StreetAddress1 = this.StreetAddress,
            ClientABN = this.AbnOrAcn,
            StreetSuburb = this.Suburb,
            StreetState = this.State,
            StreetPostcode = this.PostCode,
            ClientMobile = this.ClientMobile,
            ClientPhone = this.ClientPhone,
            ClientEmail = this.ClientEmail,
            ClientPolicies = policies,
            FundingPeriod = this.FundingPeriod,
            FortnightlyInstalments = this.FortnightlyInstalments,
            BrokerURL = this.BrokerUrl,
            BrokerExitURL = this.BrokerExitUrl,
        };
    }
}
