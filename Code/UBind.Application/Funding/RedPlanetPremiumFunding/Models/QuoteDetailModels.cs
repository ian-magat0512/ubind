// <copyright file="QuoteDetailModels.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class QuoteDetail
{
    [JsonPropertyName("quoteNumber")]
    public string? QuoteNumber { get; set; }

    [JsonPropertyName("quoteVersions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<string>? QuoteVersions { get; set; }

    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Status { get; set; }

    [JsonPropertyName("subStatus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SubStatus { get; set; }

    [JsonPropertyName("quoteType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? QuoteType { get; set; }

    [JsonPropertyName("loanNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LoanNumber { get; set; }

    [JsonPropertyName("contractDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ContractDetails? ContractDetails { get; set; }

    [JsonPropertyName("settlements")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<Settlement>? Settlements { get; set; }

    [JsonPropertyName("clientDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ClientDetails? ClientDetails { get; set; }

    [JsonPropertyName("paymentDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaymentDetails? PaymentDetails { get; set; }

    [JsonPropertyName("policies")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<Policy>? Policies { get; set; }

    [JsonPropertyName("repayments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<Repayment>? Repayments { get; set; }

    [JsonPropertyName("accessDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AccessDetails? AccessDetails { get; set; }

    [JsonPropertyName("notes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<object>? Notes { get; set; }

    [JsonPropertyName("properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Properties { get; set; }

    [JsonPropertyName("customFields")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<object>? CustomFields { get; set; }

    [JsonPropertyName("fundmlSessionId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FundmlSessionId { get; set; }
}

public class AccessDetails
{
    [JsonPropertyName("clientAcceptanceUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClientAcceptanceUrl { get; set; }

    [JsonPropertyName("clientAccessCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClientAccessCode { get; set; }

    [JsonPropertyName("quoteEnquiryUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? QuoteEnquiryUrl { get; set; }

    [JsonPropertyName("olaSuccessUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OlaSuccessUrl { get; set; }

    [JsonPropertyName("olaCancelUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OlaCancelUrl { get; set; }

    [JsonPropertyName("olaSuccessRedirectUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Uri? OlaSuccessRedirectUrl { get; set; }

    [JsonPropertyName("paymentRedirectionUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PaymentRedirectionUrl { get; set; }
}

public class ClientDetails
{
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }

    [JsonPropertyName("externalId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExternalId { get; set; }

    [JsonPropertyName("legalName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LegalName { get; set; }

    [JsonPropertyName("tradingName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TradingName { get; set; }

    [JsonPropertyName("taxIdentifier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TaxIdentifier { get; set; }

    [JsonPropertyName("acn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Acn { get; set; }

    [JsonPropertyName("telephone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Telephone { get; set; }

    [JsonPropertyName("mobile")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Mobile { get; set; }

    [JsonPropertyName("fax")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Fax { get; set; }

    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    [JsonPropertyName("industry")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Industry { get; set; }

    [JsonPropertyName("dateOfBirth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("branchCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BranchCode { get; set; }

    [JsonPropertyName("externalId2")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ExternalId2 { get; set; }

    [JsonPropertyName("autoRenewal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AutoRenewal { get; set; }

    [JsonPropertyName("language")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Language { get; set; }

    [JsonPropertyName("postalAddress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Address? PostalAddress { get; set; }

    [JsonPropertyName("streetAddress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Address? StreetAddress { get; set; }

    [JsonPropertyName("paymentDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? PaymentDetails { get; set; }

    [JsonPropertyName("contacts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<Contact>? Contacts { get; set; }

    [JsonPropertyName("branchCodeList")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? BranchCodeList { get; set; }

    [JsonPropertyName("creditRisk")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? CreditRisk { get; set; }

    [JsonPropertyName("creditRiskLimit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? CreditRiskLimit { get; set; }

    [JsonPropertyName("servicingContact")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ServicingContact { get; set; }

    [JsonPropertyName("customFields")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<CustomField>? CustomFields { get; set; }
}

public partial class Contact
{
    [JsonPropertyName("firstName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastName { get; set; }

    [JsonPropertyName("phone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Phone { get; set; }

    [JsonPropertyName("mobile")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Mobile { get; set; }

    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    [JsonPropertyName("address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Address? Address { get; set; }

    [JsonPropertyName("dateOfBirth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("license")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? License { get; set; }
}

public partial class Address
{
    [JsonPropertyName("address1")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Address1 { get; set; }

    [JsonPropertyName("address2")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Address2 { get; set; }

    [JsonPropertyName("suburb")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Suburb { get; set; }

    [JsonPropertyName("state")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? State { get; set; }

    [JsonPropertyName("postcode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Postcode { get; set; }
}

public class CustomField
{
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Value { get; set; }
}

public class PaymentDetails
{
    [JsonPropertyName("paymentType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PaymentType { get; set; }

    [JsonPropertyName("paymentTypeCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PaymentTypeCode { get; set; }

    [JsonPropertyName("billingName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BillingName { get; set; }

    [JsonPropertyName("bankBranchCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BankBranchCode { get; set; }

    [JsonPropertyName("accountNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("cardNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CardNumber { get; set; }

    [JsonPropertyName("cardExpiry")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CardExpiry { get; set; }

    [JsonPropertyName("cardCVV")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CardCvv { get; set; }

    [JsonPropertyName("cardToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CardToken { get; set; }

    [JsonPropertyName("paymentTokenRequestId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? PaymentTokenRequestId { get; set; }

    [JsonPropertyName("tokenId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TokenId { get; set; }

    [JsonPropertyName("bankAccountType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BankAccountType { get; set; }

    [JsonPropertyName("initialInstalmentAmount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? InitialInstalmentAmount { get; set; }

    [JsonPropertyName("merchantFeeRate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? MerchantFeeRate { get; set; }

    [JsonPropertyName("paymentRedirectUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PaymentRedirectUrl { get; set; }
}

public partial class ContractDetails
{
    [JsonPropertyName("creditor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Creditor { get; set; }

    [JsonPropertyName("product")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Product { get; set; }

    [JsonPropertyName("originator")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Originator { get; set; }

    [JsonPropertyName("scheme")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Scheme { get; set; }

    [JsonPropertyName("productGroup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ProductGroup { get; set; }

    [JsonPropertyName("entityCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EntityCode { get; set; }

    [JsonPropertyName("amount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Amount { get; set; }

    [JsonPropertyName("amountFinanced")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? AmountFinanced { get; set; }

    [JsonPropertyName("annualRate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? AnnualRate { get; set; }

    [JsonPropertyName("applicationFee")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? ApplicationFee { get; set; }

    [JsonPropertyName("applicationFeeOverride")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ApplicationFeeOverride { get; set; }

    [JsonPropertyName("instalmentAmount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? InstalmentAmount { get; set; }

    [JsonPropertyName("numberOfInstalments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NumberOfInstalments { get; set; }

    [JsonPropertyName("invoiceNumbers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InvoiceNumbers { get; set; }

    [JsonPropertyName("paymentFrequency")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PaymentFrequency { get; set; }

    [JsonPropertyName("flatRate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? FlatRate { get; set; }

    [JsonPropertyName("flatRateOverride")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? FlatRateOverride { get; set; }

    [JsonPropertyName("inceptionDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InceptionDate { get; set; }

    [JsonPropertyName("interest")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Interest { get; set; }

    [JsonPropertyName("initialAmountDue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? InitialAmountDue { get; set; }

    [JsonPropertyName("totalAmountRepayable")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? TotalAmountRepayable { get; set; }

    [JsonPropertyName("firstInstalmentPercentage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? FirstInstalmentPercentage { get; set; }

    [JsonPropertyName("minimumRetained")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? MinimumRetained { get; set; }

    [JsonPropertyName("doNotRenew")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? DoNotRenew { get; set; }

    [JsonPropertyName("amountRetained")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? AmountRetained { get; set; }

    [JsonPropertyName("commissionType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CommissionType { get; set; }

    [JsonPropertyName("commissionRate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? CommissionRate { get; set; }

    [JsonPropertyName("commission")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Commission { get; set; }

    [JsonPropertyName("commissionRateOverriden")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? CommissionRateOverriden { get; set; }

    [JsonPropertyName("createdDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedDate { get; set; }

    [JsonPropertyName("isCalculated")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsCalculated { get; set; }

    [JsonPropertyName("settlementDays")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? SettlementDays { get; set; }

    [JsonPropertyName("settlementDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SettlementDate { get; set; }

    [JsonPropertyName("renewedFromLoanNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RenewedFromLoanNumber { get; set; }

    [JsonPropertyName("endorsedLoanNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EndorsedLoanNumber { get; set; }

    [JsonPropertyName("externalContractId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExternalContractId { get; set; }

    [JsonPropertyName("customRepaymentSchedule")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? CustomRepaymentSchedule { get; set; }

    [JsonPropertyName("amountCancellable")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? AmountCancellable { get; set; }

    [JsonPropertyName("amountNonCancellable")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? AmountNonCancellable { get; set; }

    [JsonPropertyName("credContactSeq")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CredContactSeq { get; set; }

    [JsonPropertyName("credContactName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CredContactName { get; set; }

    [JsonPropertyName("credContactEmail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CredContactEmail { get; set; }

    [JsonPropertyName("endorsementId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? EndorsementId { get; set; }

    [JsonPropertyName("signedByFullName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SignedByFullName { get; set; }

    [JsonPropertyName("signedByPosition")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SignedByPosition { get; set; }

    [JsonPropertyName("accountManager")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AccountManager { get; set; }

    [JsonPropertyName("firstPaymentDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FirstPaymentDate { get; set; }

    [JsonPropertyName("firstPaymentDateOverride")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? FirstPaymentDateOverride { get; set; }

    [JsonPropertyName("depositType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DepositType { get; set; }

    [JsonPropertyName("depositRate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? DepositRate { get; set; }

    [JsonPropertyName("depositAmount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? DepositAmount { get; set; }

    [JsonPropertyName("instalmentMerchantFee")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? InstalmentMerchantFee { get; set; }

    [JsonPropertyName("quoteAcceptanceStep")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? QuoteAcceptanceStep { get; set; }

    [JsonPropertyName("currency")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Currency { get; set; }

    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; set; }

    [JsonPropertyName("backgroundFundingStatus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BackgroundFundingStatus { get; set; }

    [JsonPropertyName("lastInstalmentDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastInstalmentDate { get; set; }

    [JsonPropertyName("repaymentFee")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? RepaymentFee { get; set; }

    [JsonPropertyName("skipCalculate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SkipCalculate { get; set; }

    [JsonPropertyName("customAlpha1")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CustomAlpha1 { get; set; }

    [JsonPropertyName("customAlpha2")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CustomAlpha2 { get; set; }

    [JsonPropertyName("customDecimal1")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? CustomDecimal1 { get; set; }

    [JsonPropertyName("customDecimal2")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? CustomDecimal2 { get; set; }

    [JsonPropertyName("creditorName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreditorName { get; set; }

    [JsonPropertyName("displayRateType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DisplayRateType { get; set; }

    [JsonPropertyName("stampDuty")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? StampDuty { get; set; }

    [JsonPropertyName("allowInertia")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AllowInertia { get; set; }

    [JsonPropertyName("instalmentsDue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? InstalmentsDue { get; set; }

    [JsonPropertyName("regulated")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Regulated { get; set; }

    [JsonPropertyName("retailFlag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RetailFlag { get; set; }

    [JsonPropertyName("disclosedCommission")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? DisclosedCommission { get; set; }

    [JsonPropertyName("invoiceToBeSent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? InvoiceToBeSent { get; set; }

    [JsonPropertyName("fundingSource")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? FundingSource { get; set; }

    [JsonPropertyName("accountManagerCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AccountManagerCode { get; set; }

    [JsonPropertyName("isOverLimit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsOverLimit { get; set; }
}

public class Note
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("group")]
    public string? Group { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("documentId")]
    public long DocumentId { get; set; }

    [JsonPropertyName("documentName")]
    public string? DocumentName { get; set; }

    [JsonPropertyName("documentType")]
    public string? DocumentType { get; set; }
}

public partial class Policy
{
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Id { get; set; }

    [JsonPropertyName("class")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Class { get; set; }

    [JsonPropertyName("classDescription")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClassDescription { get; set; }

    [JsonPropertyName("classDescriptionAltLang")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClassDescriptionAltLang { get; set; }

    [JsonPropertyName("inceptionDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InceptionDate { get; set; }

    [JsonPropertyName("expiryDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExpiryDate { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("policyNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PolicyNumber { get; set; }

    [JsonPropertyName("invoiceNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InvoiceNumber { get; set; }

    [JsonPropertyName("externalID")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExternalId { get; set; }

    [JsonPropertyName("brokerPolicyID")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BrokerPolicyId { get; set; }

    [JsonPropertyName("underwriterLegalName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UnderwriterLegalName { get; set; }

    [JsonPropertyName("underwriterCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UnderwriterCode { get; set; }

    [JsonPropertyName("underwriterTaxIdentifier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UnderwriterTaxIdentifier { get; set; }

    [JsonPropertyName("underwriterExternalId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UnderwriterExternalId { get; set; }

    [JsonPropertyName("mga")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Mga { get; set; }

    [JsonPropertyName("insuredName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InsuredName { get; set; }

    [JsonPropertyName("underwriterBrokerId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UnderwriterBrokerId { get; set; }

    [JsonPropertyName("dueDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DueDate { get; set; }

    [JsonPropertyName("sundryAmount1")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? SundryAmount1 { get; set; }

    [JsonPropertyName("sundryAmount2")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? SundryAmount2 { get; set; }

    [JsonPropertyName("sundryAmount3")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? SundryAmount3 { get; set; }

    [JsonPropertyName("sundryAmount4")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? SundryAmount4 { get; set; }

    [JsonPropertyName("subjectToAudit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SubjectToAudit { get; set; }

    [JsonPropertyName("nonCancellablePercentage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? NonCancellablePercentage { get; set; }

    [JsonPropertyName("nonCancellableAmount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? NonCancellableAmount { get; set; }

    [JsonPropertyName("nonCancellableType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NonCancellableType { get; set; }

    [JsonPropertyName("customFields")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<CustomField>? CustomFields { get; set; }

    [JsonPropertyName("retailFlag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RetailFlag { get; set; }

    [JsonPropertyName("bankPayBillerCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BankPayBillerCode { get; set; }

    [JsonPropertyName("bankPayReference")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BankPayReference { get; set; }
}

public class Repayment
{
    [JsonPropertyName("paymentNumber")]
    public int PaymentNumber { get; set; }

    [JsonPropertyName("date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Date { get; set; }

    [JsonPropertyName("principal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Principal { get; set; }

    [JsonPropertyName("interest")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Interest { get; set; }

    [JsonPropertyName("fee")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Fee { get; set; }

    [JsonPropertyName("total")]
    public decimal? Total { get; set; }
}

public class Settlement
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("sequence")]
    public int Sequence { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("invoice")]
    public string? Invoice { get; set; }

    [JsonPropertyName("delay")]
    public int Delay { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}
