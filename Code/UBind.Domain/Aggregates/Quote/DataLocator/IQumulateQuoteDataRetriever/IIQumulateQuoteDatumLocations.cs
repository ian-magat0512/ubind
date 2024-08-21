// <copyright file="IIQumulateQuoteDatumLocations.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using UBind.Domain.Aggregates.Quote;

    public interface IIQumulateQuoteDatumLocations
    {
        //// General Parameters //////////////////////////////////////////////////

        QuoteDatumLocation Region { get; }

        QuoteDatumLocation FirstInstalmentDate { get; }

        QuoteDatumLocation PaymentFrequency { get; }

        QuoteDatumLocation NumberOfInstalments { get; }

        QuoteDatumLocation CommissionRate { get; }

        QuoteDatumLocation SettlementDays { get; }

        QuoteDatumLocation PaymentMethod { get; }

        //// Client Parameters //////////////////////////////////////////////////

        QuoteDatumLocation LegalName { get; }

        QuoteDatumLocation TradingName { get; }

        QuoteDatumLocation EntityType { get; }

        QuoteDatumLocation Abn { get; }

        QuoteDatumLocation Industry { get; }

        /// <summary>
        /// Gets the "street line 1" property of the street address.
        /// string (maxlen=50).
        /// (required) First line of the address. It can’t be PO, GPO or Locked bag address.
        /// </summary>
        QuoteDatumLocation StreetAddressStreetLine1 { get; }

        /// <summary>
        /// Gets the "street line 2" property of the street address.
        /// string (maxlen=50).
        /// (optional) Second line of the address.
        /// </summary>
        QuoteDatumLocation StreetAddressStreetLine2 { get; }

        /// <summary>
        /// Gets the "suburb" property of the street address.
        /// string (maxlen=50).
        /// (required) Suburb.
        /// </summary>
        QuoteDatumLocation StreetAddressSuburb { get; }

        /// <summary>
        /// Gets the "state" property of the street address.
        /// (required) State.
        /// If Region =’AU’, valid values are:
        /// • ACT
        /// • NSW
        /// • NT
        /// • QLD
        /// • SA
        /// • TAS
        /// • VIC
        /// • WA
        /// If Region =’NZ’, state must equal ‘NZ’.
        /// </summary>
        QuoteDatumLocation StreetAddressState { get; }

        /// <summary>
        /// Gets the "postcode" property of the street address.
        /// string (maxlen=4).
        /// (required) Postcode.
        /// </summary>
        QuoteDatumLocation StreetAddressPostcode { get; }

        QuoteDatumLocation PostalAddressStreetLine1 { get; }

        QuoteDatumLocation PostalAddressStreetLine2 { get; }

        QuoteDatumLocation PostalAddressSuburb { get; }

        QuoteDatumLocation PostalAddressState { get; }

        QuoteDatumLocation PostalAddressPostcode { get; }

        QuoteDatumLocation MobileNumber { get; }

        QuoteDatumLocation TelephoneNumber { get; }

        QuoteDatumLocation FaxNumber { get; }

        QuoteDatumLocation Email { get; }

        QuoteDatumLocation Title { get; }

        QuoteDatumLocation FirstName { get; }

        QuoteDatumLocation LastName { get; }

        QuoteDatumLocation IntroducerClientReference { get; }

        QuoteDatumLocation Borrower1FirstName { get; }

        QuoteDatumLocation Borrower1LastName { get; }

        QuoteDatumLocation Borrower1DateOfBirth { get; }

        QuoteDatumLocation Borrower1DriversLicense { get; }

        QuoteDatumLocation Borrower2FirstName { get; }

        QuoteDatumLocation Borrower2LastName { get; }

        QuoteDatumLocation Borrower2DateOfBirth { get; }

        QuoteDatumLocation Borrower2DriversLicense { get; }

        QuoteDatumLocation Borrower3FirstName { get; }

        QuoteDatumLocation Borrower3LastName { get; }

        QuoteDatumLocation Borrower3DateOfBirth { get; }

        QuoteDatumLocation Borrower3DriversLicense { get; }

        QuoteDatumLocation Borrower4FirstName { get; }

        QuoteDatumLocation Borrower4LastName { get; }

        QuoteDatumLocation Borrower4DateOfBirth { get; }

        QuoteDatumLocation Borrower4DriversLicense { get; }

        //// Introducer Parameters //////////////////////////////////////////////////

        QuoteDatumLocation AffinitySchemeCode { get; }

        QuoteDatumLocation IntroducerContactEmail { get; }

        //// Policies Parameters //////////////////////////////////////////////////

        QuoteDatumLocation PolicyNumber { get; }

        QuoteDatumLocation InvoiceNumber { get; }

        QuoteDatumLocation PolicyClassClode { get; }

        QuoteDatumLocation PolicyUnderwiterCode { get; }

        QuoteDatumLocation PolicyInceptionDate { get; }

        QuoteDatumLocation PolicyExpiryDate { get; }

        QuoteDatumLocation PolicyAmount { get; }

        QuoteDatumLocation DeftReferenceNumber { get; }
    }
}
