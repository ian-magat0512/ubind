// <copyright file="ConfiguredIQumulateQuoteDatumLocations.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// Specifies the location of quote data for use by iQumulate premium funding.
    /// </summary>
    public class ConfiguredIQumulateQuoteDatumLocations : IIQumulateQuoteDatumLocations
    {
        // Prevent instantiation.
        [JsonConstructor]
        private ConfiguredIQumulateQuoteDatumLocations()
        {
        }

        //// General Parameters //////////////////////////////////////////////////

        [JsonProperty]
        public QuoteDatumLocation Region { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation FirstInstalmentDate { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation PaymentFrequency { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation NumberOfInstalments { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation CommissionRate { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation SettlementDays { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation PaymentMethod { get; private set; }

        //// Client Parameters //////////////////////////////////////////////////

        [JsonProperty]
        public QuoteDatumLocation LegalName { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation TradingName { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation EntityType { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Abn { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Industry { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation StreetAddressStreetLine1 { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation StreetAddressStreetLine2 { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation StreetAddressSuburb { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation StreetAddressState { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation StreetAddressPostcode { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation PostalAddressStreetLine1 { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation PostalAddressStreetLine2 { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation PostalAddressSuburb { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation PostalAddressState { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation PostalAddressPostcode { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation MobileNumber { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation TelephoneNumber { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation FaxNumber { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Email { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Title { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation FirstName { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation LastName { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation IntroducerClientReference { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower1FirstName { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower1LastName { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower1DateOfBirth { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower1DriversLicense { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower2FirstName { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower2LastName { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower2DateOfBirth { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower2DriversLicense { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower3FirstName { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower3LastName { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower3DateOfBirth { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower3DriversLicense { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower4FirstName { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower4LastName { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower4DateOfBirth { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation Borrower4DriversLicense { get; private set; }

        //// Introducer Parameters //////////////////////////////////////////////////

        [JsonProperty]
        public QuoteDatumLocation AffinitySchemeCode { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation IntroducerContactEmail { get; private set; }

        //// Policies Parameters //////////////////////////////////////////////////

        [JsonProperty]
        public QuoteDatumLocation PolicyNumber { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation InvoiceNumber { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation PolicyClassClode { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation PolicyUnderwiterCode { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation PolicyInceptionDate { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation PolicyExpiryDate { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation PolicyAmount { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation DeftReferenceNumber { get; private set; }
    }
}
