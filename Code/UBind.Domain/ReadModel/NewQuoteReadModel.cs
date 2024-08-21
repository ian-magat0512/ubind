// <copyright file="NewQuoteReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel;

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Extensions;
using UBind.Domain.Product;
using UBind.Domain.ReadModel.Policy;
using UBind.Domain.ValueTypes;

/// <summary>
/// For representing quotes.
/// </summary>
public class NewQuoteReadModel : EntityReadModel<Guid>, IProductContext
{
    /// <summary>
    /// Initializes the static properties.
    /// </summary>
    static NewQuoteReadModel()
    {
        SupportsAdditionalProperties = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NewQuoteReadModel"/> class.
    /// </summary>
    /// <param name="event">A quote initialized event.</param>
    public NewQuoteReadModel(QuoteAggregate.QuoteInitializedEvent @event)
        : base(@event.TenantId, @event.QuoteId, @event.Timestamp)
    {
        this.AggregateId = @event.AggregateId;
        this.TenantId = @event.TenantId;
        this.ProductId = @event.ProductId;
        this.Type = QuoteType.NewBusiness;
        this.QuoteState = QuoteStatus.Nascent.ToString();
        this.LatestFormData = @event.FormDataJson;
        this.QuoteNumber = @event.QuoteNumber;
        this.Environment = @event.Environment;
        this.TimeZoneId = @event.TimeZoneId;
        this.HadCustomerOnCreation = @event.CustomerId != null;
        if (@event.InitialQuoteState != null)
        {
            this.QuoteState = @event.InitialQuoteState;
        }
        this.ProductReleaseId = @event.ProductReleaseId;
    }

    public NewQuoteReadModel(QuoteAggregate.AggregateCreationFromPolicyEvent @event)
        : base(@event.TenantId, @event.QuoteId.Value, @event.Timestamp)
    {
        this.AggregateId = @event.AggregateId;
        this.TenantId = @event.TenantId;
        this.ProductId = @event.ProductId;
        this.Type = QuoteType.NewBusiness;
        this.QuoteState = QuoteStatus.Nascent.ToString();
        this.Environment = @event.Environment;
        this.TimeZoneId = @event.TimeZoneId;
        this.HadCustomerOnCreation = @event.CustomerId != null;
        this.ProductReleaseId = @event.ProductReleaseId;
    }

    public NewQuoteReadModel(QuoteAggregate.QuoteImportedEvent @event)
        : base(@event.TenantId, @event.QuoteId, @event.Timestamp)
    {
        this.AggregateId = @event.AggregateId;
        this.ProductId = @event.ProductId;
        this.OrganisationId = @event.OrganisationId;
        this.Type = @event.Type;
        this.CustomerId = @event.CustomerId;
        this.LatestFormData = @event.FormDataJson;
        this.Environment = @event.Environment;
        this.TimeZoneId = @event.TimeZoneId;
        this.ProductReleaseId = @event.ProductReleaseId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NewQuoteReadModel"/> class.
    /// </summary>
    /// <param name="aggregate">The quote aggregate.</param>
    /// <param name="event">A quote initialized event.</param>
    public NewQuoteReadModel(QuoteAggregate aggregate, QuoteAggregate.AdjustmentQuoteCreatedEvent @event)
        : base(@event.TenantId, @event.QuoteId, @event.Timestamp)
    {
        this.AggregateId = @event.AggregateId;
        this.Type = QuoteType.Adjustment;
        this.QuoteNumber = @event.QuoteNumber;
        this.QuoteState = QuoteStatus.Incomplete.ToString();
        this.LatestFormData = @event.FormDataJson;
        this.TimeZoneId = aggregate.Policy.TimeZone.ToString();
        this.HadCustomerOnCreation = aggregate.CustomerId != null;
        if (@event.InitialQuoteState != null)
        {
            this.QuoteState = @event.InitialQuoteState;
        }
        this.ProductReleaseId = @event.ProductReleaseId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NewQuoteReadModel"/> class.
    /// </summary>
    /// <param name="event">A quote initialized event.</param>
    public NewQuoteReadModel(QuoteAggregate aggregate, QuoteAggregate.CancellationQuoteCreatedEvent @event)
        : base(@event.TenantId, @event.QuoteId, @event.Timestamp)
    {
        this.AggregateId = @event.AggregateId;
        this.Type = QuoteType.Cancellation;
        this.QuoteNumber = @event.QuoteNumber;
        this.QuoteState = QuoteStatus.Incomplete.ToString();
        this.LatestFormData = @event.FormDataJson;
        this.TimeZoneId = aggregate.Policy.TimeZone.ToString();
        this.HadCustomerOnCreation = aggregate.CustomerId != null;
        if (@event.InitialQuoteState != null)
        {
            this.QuoteState = @event.InitialQuoteState;
        }
        this.ProductReleaseId = @event.ProductReleaseId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NewQuoteReadModel"/> class.
    /// </summary>
    /// <param name="event">A quote initialized event.</param>
    public NewQuoteReadModel(QuoteAggregate aggregate, QuoteAggregate.RenewalQuoteCreatedEvent @event)
        : base(@event.TenantId, @event.QuoteId, @event.Timestamp)
    {
        this.AggregateId = @event.AggregateId;
        this.Type = QuoteType.Renewal;
        this.QuoteNumber = @event.QuoteNumber;
        this.QuoteState = QuoteStatus.Incomplete.ToString();
        this.LatestFormData = @event.FormDataJson;
        this.TimeZoneId = aggregate.Policy.TimeZone.ToString();
        this.HadCustomerOnCreation = aggregate.CustomerId != null;
        if (@event.InitialQuoteState != null)
        {
            this.QuoteState = @event.InitialQuoteState;
        }
        this.ProductReleaseId = @event.ProductReleaseId;
    }

    // Parameterless constructor for EF.
    protected NewQuoteReadModel()
    {
    }

    /// <summary>
    /// Gets the ID of the quote aggregate.
    /// </summary>
    public Guid AggregateId { get; private set; }

    /// <summary>
    /// Gets or sets the ID of the policy family the quote belongs to.
    /// </summary>
    public Guid? PolicyId { get; set; }

    /// <summary>
    /// Gets or sets the number of the policy family the quote belongs to.
    /// </summary>
    public string PolicyNumber { get; set; }

    /// <summary>
    /// Gets the type of quote.
    /// </summary>
    public QuoteType Type { get; private set; }

    /// <summary>
    /// Gets or sets the quote number.
    /// </summary>
    public string QuoteNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the quote has been submitted.
    /// </summary>
    public bool IsSubmitted { get; set; }

    /// <summary>
    /// Gets or sets the time the quote was submitted, if applicable, otherwise the default date.
    /// </summary>
    public Instant SubmissionTimestamp
    {
        get
        {
            return Instant.FromUnixTimeTicks(this.SubmissionTicksSinceEpoch);
        }

        set
        {
            this.SubmissionTicksSinceEpoch = value.ToUnixTimeTicks();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the quote has been invoiced.
    /// </summary>
    public bool IsInvoiced { get; private set; }

    /// <summary>
    /// Gets the invoice number.
    /// </summary>
    public string InvoiceNumber { get; private set; }

    /// <summary>
    /// Gets the time the quote was invoiced, if applicable, otherwise the default date.
    /// </summary>
    public Instant InvoiceTimestamp
    {
        get
        {
            return Instant.FromUnixTimeTicks(this.InvoiceTicksSinceEpoch);
        }

        private set
        {
            this.InvoiceTicksSinceEpoch = value.ToUnixTimeTicks();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the quote has a credit note issued.
    /// </summary>
    public bool IsCreditNoteIssued { get; private set; }

    /// <summary>
    /// Gets the credit note number assigned to this quote.
    /// </summary>
    public string CreditNoteNumber { get; private set; }

    /// <summary>
    /// Gets the time the quote was issued a credit note, if applicable, otherwise the default date.
    /// </summary>
    public Instant CreditNoteTimestamp
    {
        get
        {
            return Instant.FromUnixTimeTicks(this.CreditNoteTicksSinceEpoch);
        }

        private set
        {
            this.CreditNoteTicksSinceEpoch = value.ToUnixTimeTicks();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the quote has been paid for.
    /// </summary>
    public bool IsPaidFor { get; set; }

    /// <summary>
    /// Gets or sets the payment type used in paying the quote.
    /// </summary>
    public string PaymentGateway { get; set; }

    /// <summary>
    /// Gets or sets the payment type used in paying the quote.
    /// </summary>
    public string PaymentResponseJson { get; set; }

    /// <summary>
    /// Gets or sets the time the quote was paid for, if applicable, otherwise the default date.
    /// </summary>
    public Instant PaymentTimestamp
    {
        get
        {
            return Instant.FromUnixTimeTicks(this.PaymentTicksSinceEpoch);
        }

        set
        {
            this.PaymentTicksSinceEpoch = value.ToUnixTimeTicks();
        }
    }

    /// <summary>
    /// Gets or sets the payment reference, if the quote was paid for, otherwise null.
    /// </summary>
    public string PaymentReference { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the quote has been funded via premium funding.
    /// </summary>
    public bool IsFunded { get; set; }

    /// <summary>
    /// Gets the external reference for the accepted funding proposal, if any.
    /// </summary>
    public string? FundingId { get; private set; }

    public Guid? FundingInternalId { get; private set; }

    public string? FundingAcceptanceUrl { get; private set; }

    public string? FundingContractUrl { get; private set; }

    public string? FundingProposalResponseJson { get; private set; }

    /// <summary>
    /// Gets the amount the premium funding was for.
    /// </summary>
    public decimal? AmountFunded { get; private set; }

    /// <summary>
    /// Gets the frequency of premium funding repayments.
    /// </summary>
    public Frequency? FundingPaymentFrequency { get; private set; }

    /// <summary>
    /// Gets the number of instalments premium funding is to be repaid in.
    /// </summary>
    public int? FundingNumberOfInstallments { get; private set; }

    /// <summary>
    /// Gets the amount of the initial premium funding repayment.
    /// </summary>
    public decimal? FundingInitialInstalmentAmount { get; private set; }

    /// <summary>
    /// Gets the amount of the regular premium funding repayments.
    /// </summary>
    public decimal? FundingRegularInstalmentAmount { get; private set; }

    /// <summary>
    /// The funding instalment merchant fee multiplier applied by the funding provider
    /// Example: 0.007900 represents a 0.7900% interest rate.
    /// </summary>
    public decimal? FundingInstalmentMerchantFeeMultiplier { get; private set; }

    /// <summary>
    /// The interest rate applied by the funding provider for instalments.
    /// Example: 0.154803 represents a 15.4803% interest rate.
    /// </summary>
    public decimal? FundingInstalmentInterestRate { get; private set; }

    /// <summary>
    /// Gets the time that the premium funding proposal was accepted.
    /// </summary>
    public Instant? FundingTimestamp
    {
        get
        {
            return this.FundingTicksSinceEpoch != null
                ? Instant.FromUnixTimeTicks(this.FundingTicksSinceEpoch.Value)
                : null;
        }

        private set
        {
            this.FundingTicksSinceEpoch = value?.ToUnixTimeTicks();
        }
    }

    /// <summary>
    /// Gets or sets the time that the quote will expire.
    /// </summary>
    public Instant? ExpiryTimestamp
    {
        get => this.ExpiryTicksSinceEpoch.HasValue
           ? Instant.FromUnixTimeTicks(this.ExpiryTicksSinceEpoch.Value)
           : (Instant?)null;

        set => this.ExpiryTicksSinceEpoch = value.HasValue
            ? value.Value.ToUnixTimeTicks()
            : (long?)null;
    }

    /// <summary>
    /// Gets or sets the time the quote was last updated by User.
    /// </summary>
    public Instant? LastModifiedByUserTimestamp
    {
        get => this.LastModifiedByUserTicksSinceEpoch.HasValue
           ? Instant.FromUnixTimeTicks(this.LastModifiedByUserTicksSinceEpoch.Value)
           : (Instant?)null;

        set => this.LastModifiedByUserTicksSinceEpoch = value.HasValue
            ? value.Value.ToUnixTimeTicks()
            : (long?)null;
    }

    [NotMapped]
    public DateTimeZone TimeZone => this.TimeZoneId != null
        ? Timezones.GetTimeZoneByIdOrThrow(this.TimeZoneId)
        : Timezones.AET;

    public string TimeZoneId { get; private set; }

    /// <summary>
    /// Gets or sets the latest form data for the quote.
    /// </summary>
    public string LatestFormData { get; set; }

    /// <summary>
    /// Gets or sets the ID of the latest calculation result.
    /// </summary>
    public Guid LatestCalculationResultId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the form data used in the latest calculation result.
    /// </summary>
    public Guid LatestCalculationResultFormDataId { get; set; }

    /// <summary>
    /// Gets or sets the latest calculation result as raw JSON.
    /// </summary>
    public string LatestCalculationResultJson { get; set; }

    /// <summary>
    /// Gets or sets the latest calculation result of the quote.
    /// </summary>
    [NotMapped]
    public Domain.ReadWriteModel.CalculationResult LatestCalculationResult
    {
        get => this.SerializedLatestCalculationResult != null
            ? JsonConvert.DeserializeObject<ReadWriteModel.CalculationResult>(
                this.SerializedLatestCalculationResult, CustomSerializerSetting.JsonSerializerSettings)
            : null;

        set => this.SerializedLatestCalculationResult = JsonConvert.SerializeObject(value);
    }

    /// <summary>
    /// Gets a string containing the latest calculation result serizlied to Json.
    /// </summary>
    public string SerializedLatestCalculationResult { get; private set; }

    /// <summary>
    /// Gets the quote's status.
    /// </summary>
    public QuoteStatus Status =>
        this.PolicyIssued || this.IsSubmitted ? QuoteStatus.Complete :
        this.QuoteNumber != null ? QuoteStatus.Incomplete :
        QuoteStatus.Nascent;

    /// <summary>
    /// Gets or sets the quote state.
    /// </summary>
    public string QuoteState { get; set; }

    /// <summary>
    /// Gets or sets the total amount payable for the quote or policy.
    /// </summary>
    public decimal? TotalPayable { get; set; }

    /// <summary>
    /// Gets a value indicating whether a policy has been issued for the quote.
    /// </summary>
    public bool PolicyIssued { get; private set; }

    /// <summary>
    /// Gets representation of <see cref="NewQuoteReadModel.LastModifiedByUserTimestamp"/> in ticks since epoch to allow persisting via EF.
    /// </summary>
    public long? LastModifiedByUserTicksSinceEpoch { get; private set; }

    /// <summary>
    /// Gets representation of <see cref="NewQuoteReadModel.ExpiryTimestamp"/> in ticks since epoch to allow persisting via EF.
    /// </summary>
    public long? ExpiryTicksSinceEpoch { get; private set; }

    /// <summary>
    /// Gets the invoive time in ticks since the epoch for persistence in EF.
    /// </summary>
    public long InvoiceTicksSinceEpoch { get; private set; }

    /// <summary>
    /// Gets the submission time in ticks since the epoch for persistence in EF.
    /// </summary>
    public long SubmissionTicksSinceEpoch { get; private set; }

    /// <summary>
    /// Gets the payment time in ticks since the epoch for persistence in EF.
    /// </summary>
    public long PaymentTicksSinceEpoch { get; private set; }

    /// <summary>
    /// Gets the time the premium funding proposal was accepted in ticks since the epoch for persistence in EF.
    /// </summary>
    public long? FundingTicksSinceEpoch { get; private set; }

    /// <summary>
    /// Gets the credit note time in ticks since the epoch for persistence in EF.
    /// </summary>
    public long CreditNoteTicksSinceEpoch { get; private set; }

    /// <summary>
    /// Gets or sets the quote workflow step.
    /// </summary>
    public string WorkflowStep { get; set; }

    /// <summary>
    /// Gets or sets the environment where the quote is created.
    /// </summary>
    public DeploymentEnvironment Environment { get; set; }

    /// <summary>
    /// Gets or sets the Id of the quote customer.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the person who is the customer for the quote if any, otherwise default.
    /// </summary>
    public Guid? CustomerPersonId { get; set; }

    /// <summary>
    /// Gets or sets the Customer's full name.
    /// </summary>
    public string CustomerFullName { get; set; }

    /// <summary>
    /// Gets or sets the Customer's preferred name.
    /// </summary>
    public string CustomerPreferredName { get; set; }

    /// <summary>
    /// Gets or sets the customer's email address.
    /// </summary>
    public string CustomerEmail { get; set; }

    /// <summary>
    /// Gets or sets the customer's alternative email address.
    /// </summary>
    public string CustomerAlternativeEmail { get; set; }

    /// <summary>
    /// Gets or sets the customer's mobile phone number.
    /// </summary>
    public string CustomerMobilePhone { get; set; }

    /// <summary>
    /// Gets or sets the customer's home phone number.
    /// </summary>
    public string CustomerHomePhone { get; set; }

    /// <summary>
    /// Gets or sets the customer's work phone number.
    /// </summary>
    public string CustomerWorkPhone { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the quote was initially created against an existing customer.
    /// </summary>
    public bool HadCustomerOnCreation { get; set; }

    /// <summary>
    /// Gets or sets the Id of quote owner.
    /// </summary>
    public Guid? OwnerUserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the person who owns this quote.
    /// </summary>
    public Guid? OwnerPersonId { get; set; }

    /// <summary>
    /// Gets or sets the full name of the person who owns this quote.
    /// </summary>
    public string OwnerFullName { get; set; }

    /// <summary>
    /// Gets or sets the Id of policy transaction.
    /// </summary>
    public Guid? PolicyTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the Id of product the quote belongs to.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the Id of organisation the quote belongs to.
    /// </summary>
    public Guid OrganisationId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to return a test data.
    /// </summary>
    public bool IsTestData { get; set; }

    /// <summary>
    /// Gets or sets the name to be displayed against a quote if customer is not available.
    /// </summary>
    public string QuoteTitle { get; set; }

    /// <summary>
    /// Gets a value indicating whether the quote has been discarded.
    /// </summary>
    public bool IsDiscarded { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the quote has been actualised.
    /// </summary>
    public bool IsActualised { get; set; }

    public long? PolicyTransactionEffectiveTicksSinceEpoch { get; private set; }

    public long? PolicyExpiryTicksSinceEpoch { get; private set; }

    public Instant? PolicyExpiryTimestamp
    {
        get => this.PolicyExpiryTicksSinceEpoch.HasValue
           ? Instant.FromUnixTimeTicks(this.PolicyExpiryTicksSinceEpoch.Value)
           : null;

        set => this.PolicyExpiryTicksSinceEpoch = value.HasValue
            ? value.Value.ToUnixTimeTicks()
            : null;
    }

    public Instant? PolicyTransactionEffectiveTimestamp
    {
        get => this.PolicyTransactionEffectiveTicksSinceEpoch.HasValue
           ? Instant.FromUnixTimeTicks(this.PolicyTransactionEffectiveTicksSinceEpoch.Value)
           : null;

        set => this.PolicyTransactionEffectiveTicksSinceEpoch = value.HasValue
            ? value.Value.ToUnixTimeTicks()
            : null;
    }

    /// <summary>
    /// Gets or sets a value indicating which product release was associated on the quote to be used.
    /// </summary>
    public Guid? ProductReleaseId { get; set; }

    /// <summary>
    /// Record details of premium funding.
    /// </summary>
    /// <param name="fundingProposal">The accepted premium funding proposal.</param>
    /// <param name="fundingTimestamp">The time the proposal was accepted.</param>
    public void RecordFunding(FundingProposal fundingProposal, Instant fundingTimestamp)
    {
        if (fundingProposal == null)
        {
            return;
        }

        this.FundingId = fundingProposal.ExternalId;
        this.FundingInternalId = fundingProposal.InternalId;
        this.AmountFunded = fundingProposal.PaymentBreakdown.AmountFunded;
        this.FundingPaymentFrequency = fundingProposal.PaymentBreakdown.PaymentFrequency;
        this.FundingNumberOfInstallments = fundingProposal.PaymentBreakdown.NumberOfInstalments;
        this.FundingInitialInstalmentAmount = fundingProposal.PaymentBreakdown.InitialInstalmentAmount;
        this.FundingRegularInstalmentAmount = fundingProposal.PaymentBreakdown.RegularInstalmentAmount;
        this.FundingInstalmentMerchantFeeMultiplier = fundingProposal.PaymentBreakdown.InstalmentMerchantFeeMultiplier;
        this.FundingInstalmentInterestRate = fundingProposal.PaymentBreakdown.InterestRate;
        this.FundingTimestamp = fundingTimestamp;
        this.FundingAcceptanceUrl = fundingProposal.AcceptanceUrl;
        this.FundingContractUrl = fundingProposal.ContractUrl;
        this.FundingProposalResponseJson = fundingProposal.ProposalResponse;
    }

    /// <summary>
    /// Add invoice for the quote.
    /// </summary>
    /// <param name="invoiceNumber">Invoice number for the quote.</param>
    /// <param name="invoiceTimestamp">Invoice time stamp.</param>
    public void RecordInvoiceIssued(string invoiceNumber, Instant invoiceTimestamp)
    {
        if (this.IsInvoiced)
        {
            throw new InvalidOperationException("Cannot invoice a quote that is already invoiced.");
        }

        this.IsInvoiced = true;
        this.InvoiceNumber = invoiceNumber;
        this.InvoiceTimestamp = invoiceTimestamp;
        this.LastModifiedTimestamp = invoiceTimestamp;
    }

    /// <summary>
    /// Record that a policy has been issued for the quote.
    /// </summary>
    /// <param name="policy">The policy issued.</param>
    /// <param name="timestamp">The time the policy was issued.</param>
    public void RecordPolicyIssued(PolicyReadModel policy, Instant timestamp)
    {
        this.PolicyId = policy.Id;
        this.PolicyNumber = policy.PolicyNumber;
        this.PolicyIssued = true;
        this.LastModifiedTimestamp = timestamp;
        this.PolicyTransactionId = this.Id;
    }

    /// <summary>
    /// Add invoice number.
    /// </summary>
    /// <param name="invoiceNumber">The invoice number.</param>
    public void AddInvoiceNumber(string invoiceNumber)
    {
        this.InvoiceNumber = invoiceNumber;
    }

    /// <summary>
    /// Mark the quote as discarded.
    /// </summary>
    /// <param name="timestamp">The time the policy was cancelled.</param>
    public void RecordDiscarding(Instant timestamp)
    {
        this.IsDiscarded = true;
        this.LastModifiedTimestamp = timestamp;
    }

    /// <summary>
    /// Adds a credit note to the quote.
    /// </summary>
    /// <param name="creditNoteNumber">The reference number for the credit note.</param>
    /// <param name="creditNoteTimestamp">The credit note timestamp.</param>
    public void RecordCreditNoteIssued(string creditNoteNumber, Instant creditNoteTimestamp)
    {
        if (this.IsCreditNoteIssued)
        {
            throw new InvalidOperationException("Cannot issue a credit note to a quote that is already has a credit note.");
        }

        this.IsCreditNoteIssued = true;
        this.CreditNoteNumber = creditNoteNumber;
        this.CreditNoteTimestamp = creditNoteTimestamp;
        this.LastModifiedTimestamp = creditNoteTimestamp;
    }

    /// <summary>
    /// Converts the value to string.
    /// </summary>
    /// <returns>The value in string format.</returns>
    public override string ToString()
    {
        string formDataSummary = this.LatestFormData == null
            ? "LatestFormData is NULL"
            : "LatestFromData is: " + this.LatestFormData;
        return $"{base.ToString()} Id {this.Id} ({formDataSummary})";
    }

    /// <summary>
    /// Patch form data.
    /// </summary>
    /// <param name="patch">The patch to apply.</param>
    public void ApplyPatch(PolicyDataPatch patch)
    {
        // The null checker here is temporarily needed here due to the issue on newly created renewals
        // that doesn't record the latest form data. For more info, check this permalink.
        // https://bitbucket.aptiture.com/projects/UBINDPL/repos/application/pull-requests/116/diff?commentId=393#Code/UBind.Persistence/ReadModels/NewQuoteReadModel.cs?t=467.
        if (patch.Type == DataPatchType.FormData && !string.IsNullOrEmpty(this.LatestFormData))
        {
            JObject obj = JObject.Parse(this.LatestFormData);
            var formModel = obj.SelectToken("formModel") as JObject;
            formModel.PatchProperty(patch.Path, patch.Value);
            this.LatestFormData = JsonConvert.SerializeObject(obj);
        }
        else if (patch.Type == DataPatchType.CalculationResult)
        {
            var calculationResult = this.LatestCalculationResult;
            if (calculationResult != null)
            {
                calculationResult.PatchProperty(patch.Path, patch.Value);

                // Reset to update serialized version for persistance.
                this.LatestCalculationResult = calculationResult;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the quote can be edited.
    /// If the quote is complete, submitted, discarded, or expired then it cannot be edited.
    /// </summary>
    public bool IsEditable()
    {
        return
            !this.QuoteState.EqualsIgnoreCase(StandardQuoteStates.Complete)
            && !this.IsSubmitted
            && !this.IsDiscarded
            && !this.QuoteState.EqualsIgnoreCase(StandardQuoteStates.Expired);
    }
}
