// <copyright file="PolicyReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Policy;

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Humanizer;
using Newtonsoft.Json;
using NodaTime;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Extensions;
using UBind.Domain.ReadWriteModel;

/// <summary>
/// Read model for policies.
/// </summary>
public class PolicyReadModel : EntityReadModel<Guid>, IReadModel<Guid>, IPolicy
{
    /// <summary>
    /// Initializes the static properties.
    /// </summary>
    static PolicyReadModel()
    {
        SupportsAdditionalProperties = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyReadModel"/> class.
    /// </summary>
    /// <param name="event">The quote initialized event that triggered the policy issued.</param>
    /// <param name="quote">The quote of the policy.</param>
    public PolicyReadModel(
        QuoteAggregate.PolicyIssuedEvent @event,
        NewQuoteReadModel? quote)
        : base(@event.TenantId, @event.AggregateId, @event.Timestamp)
    {
        this.Environment = @event.Environment;
        this.IsTestData = @event.IsTestData;
        this.InceptionDateTime = @event.InceptionDateTime;
        this.InceptionTimestamp = @event.InceptionTimestamp;
        this.ExpiryDateTime = @event.ExpiryDateTime;
        this.ExpiryTimestamp = @event.ExpiryTimestamp;
        this.LastModifiedTimestamp = @event.Timestamp;
        this.TimeZoneId = @event.TimeZoneId;
        this.UpdatePolicyState(@event.Timestamp);
        if (quote != null)
        {
            this.OrganisationId = quote.OrganisationId;
            this.TenantId = quote.TenantId;
            this.ProductId = quote.ProductId;
            this.CustomerId = quote.CustomerId;
            this.CustomerPersonId = quote.CustomerPersonId;
            this.CustomerFullName = quote.CustomerFullName;
            this.CustomerPreferredName = quote.CustomerPreferredName;
            this.CustomerEmail = quote.CustomerEmail;
            this.CustomerAlternativeEmail = quote.CustomerAlternativeEmail;
            this.CustomerMobilePhone = quote.CustomerMobilePhone;
            this.CustomerHomePhone = quote.CustomerHomePhone;
            this.CustomerWorkPhone = quote.CustomerWorkPhone;
            this.OwnerUserId = quote.OwnerUserId;
            this.OwnerPersonId = quote.OwnerPersonId;
            this.OwnerFullName = quote.OwnerFullName;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyReadModel"/> class.
    /// TODO: Remove this constructor and have it only create on PolicyIssuedEvent.
    /// </summary>
    /// <param name="event">The quote initialized event that triggered the policy creation.</param>
    public PolicyReadModel(QuoteAggregate.QuoteInitializedEvent @event)
        : base(@event.TenantId, @event.AggregateId, @event.Timestamp)
    {
        this.OrganisationId = @event.OrganisationId;
        this.TenantId = @event.TenantId;
        this.ProductId = @event.ProductId;
        this.Environment = @event.Environment;
        this.IsTestData = @event.IsTestData;
        this.LastModifiedTimestamp = @event.Timestamp;
        this.CustomerId = @event.CustomerId;
        this.TimeZoneId = @event.TimeZoneId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyReadModel"/> class.
    /// </summary>
    /// <param name="event">The policy imported event that triggered the policy creation.</param>
    public PolicyReadModel(QuoteAggregate.PolicyImportedEvent @event)
        : base(@event.TenantId, @event.AggregateId, @event.Timestamp)
    {
        this.OrganisationId = @event.OrganisationId;
        this.TenantId = @event.TenantId;
        this.ProductId = @event.ProductId;
        this.Environment = @event.Environment;
        this.IsTestData = @event.IsTestData;
        this.PolicyNumber = @event.PolicyNumber;
        this.IssuedTimestamp = @event.Timestamp;
        this.QuoteId = @event.QuoteId;
        this.InceptionDateTime = @event.InceptionDateTime;
        this.InceptionTimestamp = @event.InceptionTimestamp;
        this.ExpiryDateTime = @event.ExpiryDateTime;
        this.ExpiryTimestamp = @event.ExpiryTimestamp;
        this.CalculationResult = @event.PolicyData.QuoteDataSnapshot.CalculationResult.Data;
        this.LastModifiedTimestamp = @event.Timestamp;
        this.TimeZoneId = @event.TimeZoneId;
        if (@event.OwnerUserId != null)
        {
            this.OwnerUserId = @event.OwnerUserId;
            this.OwnerFullName = @event.OwnerFullName;
            this.OwnerPersonId = @event.OwnerPersonId;
        }

        this.UpdatePolicyState(@event.Timestamp);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyReadModel"/> class.
    /// </summary>
    /// <param name="event">The policy issue event that triggered the policy creation.</param>
    public PolicyReadModel(QuoteAggregate.PolicyIssuedWithoutQuoteEvent @event)
        : base(@event.TenantId, @event.AggregateId, @event.Timestamp)
    {
        this.OrganisationId = @event.OrganisationId;
        this.TenantId = @event.TenantId;
        this.ProductId = @event.ProductId;
        this.Environment = @event.Environment;
        this.IsTestData = @event.IsTestData;
        this.PolicyNumber = @event.PolicyNumber;
        this.IssuedTimestamp = @event.Timestamp;
        this.QuoteId = @event.QuoteId;
        this.InceptionDateTime = @event.InceptionDateTime;
        this.InceptionTimestamp = @event.InceptionTimestamp;
        this.ExpiryDateTime = @event.ExpiryDateTime;
        this.ExpiryTimestamp = @event.ExpiryTimestamp;
        this.CalculationResult = @event.PolicyData.QuoteDataSnapshot.CalculationResult.Data;
        this.LastModifiedTimestamp = @event.Timestamp;
        this.TimeZoneId = @event.TimeZoneId;
        this.UpdatePolicyState(@event.Timestamp);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyReadModel"/> class.
    /// </summary>
    /// <param name="policyId">The policy Id.</param>
    /// <param name="createdTimestamp">The created time.</param>
    protected PolicyReadModel(Guid tenantId, Guid policyId, Instant createdTimestamp)
        : base(tenantId, policyId, createdTimestamp)
    {
    }

    // Parameterless constructor for EF.
    private PolicyReadModel()
        : base(default, default, default)
    {
    }

    /// <summary>
    /// Gets or sets an ID identifying which product the quote is for.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the environment the quote belongs to.
    /// </summary>
    public DeploymentEnvironment Environment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to return a test data.
    /// </summary>
    public bool IsTestData { get; set; }

    /// <summary>
    /// Gets the ID of the quote that last created or updated the policy.
    /// </summary>
    public Guid? QuoteId { get; private set; }

    /// <summary>
    /// Gets or sets the user ID of the current owner of the quote.
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
    /// Gets or sets the ID of the customer the quote is assigned to if any, otherwise default.
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
    /// Gets the time the policy was first issued.
    /// </summary>
    public Instant IssuedTimestamp
    {
        get => Instant.FromUnixTimeTicks(this.IssuedTicksSinceEpoch);
        private set => this.IssuedTicksSinceEpoch = value.ToUnixTimeTicks();
    }

    /// <summary>
    /// Gets or sets the time the policy was first issued.
    /// </summary>
    public long IssuedTicksSinceEpoch { get; set; }

    /// <summary>
    /// Gets or sets the policy number.
    /// </summary>
    public string PolicyNumber { get; set; }

    /// <summary>
    /// Gets or sets the policy title.
    /// </summary>
    public string PolicyTitle { get; set; }

    /// <summary>
    /// Gets or sets the policy state.
    /// </summary>
    public string PolicyState { get; set; }

    /// <summary>
    /// Gets or sets the policy inception date.
    /// The inception date is the date the policy was first purchased.
    /// </summary>
    [NotMapped]
    public LocalDateTime InceptionDateTime
    {
        get => LocalDateTime.FromDateTime(this.InceptionDateTimeColumn);
        set => this.InceptionDateTimeColumn = value.ToDateTimeUnspecified();
    }

    [Column("InceptionDateTime", TypeName = "datetime2")]
    public DateTime InceptionDateTimeColumn { get; set; }

    public Instant InceptionTimestamp
    {
        get => Instant.FromUnixTimeTicks(this.InceptionTicksSinceEpoch);
        private set => this.InceptionTicksSinceEpoch = value.ToUnixTimeTicks();
    }

    public long InceptionTicksSinceEpoch { get; set; }

    /// <summary>
    /// Gets or sets the date the current policy period starts.
    /// </summary>
    [NotMapped]
    public LocalDateTime LatestPolicyPeriodStartDateTime
    {
        get => LocalDateTime.FromDateTime(this.LatestPolicyPeriodStartDateTimeColumn);
        set => this.LatestPolicyPeriodStartDateTimeColumn = value.ToDateTimeUnspecified();
    }

    [Column("LatestPolicyPeriodStartDateTime", TypeName = "datetime2")]
    public DateTime LatestPolicyPeriodStartDateTimeColumn { get; set; }

    public Instant LatestPolicyPeriodStartTimestamp
    {
        get => Instant.FromUnixTimeTicks(this.LatestPolicyPeriodStartTicksSinceEpoch);
        set => this.LatestPolicyPeriodStartTicksSinceEpoch = value.ToUnixTimeTicks();
    }

    public long LatestPolicyPeriodStartTicksSinceEpoch { get; set; }

    /// <summary>
    /// Gets or sets the policy cancellation effective date.
    /// </summary>
    [NotMapped]
    public LocalDateTime? CancellationEffectiveDateTime
    {
        get => this.CancellationEffectiveDateTimeColumn.HasValue
            ? LocalDateTime.FromDateTime(this.CancellationEffectiveDateTimeColumn.Value)
            : (LocalDateTime?)null;
        set => this.CancellationEffectiveDateTimeColumn = value?.ToDateTimeUnspecified();
    }

    [Column("CancellationEffectiveDateTime", TypeName = "datetime2")]
    public DateTime? CancellationEffectiveDateTimeColumn { get; set; }

    public Instant? CancellationEffectiveTimestamp
    {
        get => this.CancellationEffectiveTicksSinceEpoch.HasValue
            ? Instant.FromUnixTimeTicks(this.CancellationEffectiveTicksSinceEpoch.Value)
            : (Instant?)null;
        private set => this.CancellationEffectiveTicksSinceEpoch = value?.ToUnixTimeTicks();
    }

    public long? CancellationEffectiveTicksSinceEpoch { get; set; }

    /// <summary>
    /// Gets or sets the policy cancellation effective date.
    /// </summary>
    public LocalDateTime? AdjustmentEffectiveDateTime
    {
        get => this.AdjustmentEffectiveDateTimeColumn.HasValue
            ? LocalDateTime.FromDateTime(this.AdjustmentEffectiveDateTimeColumn.Value)
            : (LocalDateTime?)null;
        set => this.AdjustmentEffectiveDateTimeColumn = value?.ToDateTimeUnspecified();
    }

    [Column("AdjustmentEffectiveDateTime", TypeName = "datetime2")]
    public DateTime? AdjustmentEffectiveDateTimeColumn { get; set; }

    public Instant? AdjustmentEffectiveTimestamp
    {
        get => this.AdjustmentEffectiveTicksSinceEpoch.HasValue
            ? Instant.FromUnixTimeTicks(this.AdjustmentEffectiveTicksSinceEpoch.Value)
            : (Instant?)null;
        private set => this.AdjustmentEffectiveTicksSinceEpoch = value?.ToUnixTimeTicks();
    }

    public long? AdjustmentEffectiveTicksSinceEpoch { get; set; }

    /// <summary>
    /// Gets or sets the policy expiry date.
    /// </summary>
    [NotMapped]
    public LocalDateTime? ExpiryDateTime
    {
        get => this.ExpiryDateTimeColumn.HasValue
            ? LocalDateTime.FromDateTime(this.ExpiryDateTimeColumn.Value)
            : (LocalDateTime?)null;
        set => this.ExpiryDateTimeColumn = value?.ToDateTimeUnspecified();
    }

    [Column("ExpiryDateTime", TypeName = "datetime2")]
    public DateTime? ExpiryDateTimeColumn { get; set; }

    public Instant? ExpiryTimestamp
    {
        get => this.ExpiryTicksSinceEpoch.HasValue
            ? Instant.FromUnixTimeTicks(this.ExpiryTicksSinceEpoch.Value)
            : (Instant?)null;
        set => this.ExpiryTicksSinceEpoch = value?.ToUnixTimeTicks();
    }

    public long? ExpiryTicksSinceEpoch { get; set; }

    [NotMapped]
    public DateTimeZone TimeZone => this.TimeZoneId != null
        ? Timezones.GetTimeZoneByIdOrThrow(this.TimeZoneId)
        : Timezones.AET;

    public string TimeZoneId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the policy has been cancelled or not.
    /// </summary>
    public bool IsCancelled => this.CancellationEffectiveDateTime.HasValue;

    /// <summary>
    /// Gets a value indicating whether the policy family has been discarded.
    /// </summary>
    public bool IsDiscarded { get; private set; }

    public bool IsTermBased => this.ExpiryTimestamp != null;

    /// <summary>
    /// Gets a value indicating whether timestamp values should be used instead
    /// of date time values for official policy dates.
    /// When set to false, the time zone and daylight savings are taken into account
    /// when calculating the real policy expiry dates and other dates.
    /// </summary>
    public bool AreTimestampsAuthoritative { get; private set; }

    /// <summary>
    /// Gets or sets the calculation result used for the latest transaction.
    /// </summary>
    [NotMapped]
    public Domain.ReadWriteModel.CalculationResult CalculationResult
    {
        get => JsonConvert.DeserializeObject<CalculationResult>(
            this.SerializedCalculationResult, CustomSerializerSetting.JsonSerializerSettings);

        set => this.SerializedCalculationResult = JsonConvert.SerializeObject(value);
    }

    /// <summary>
    /// Gets a string containing the calculation result used for the latest upsert transaction.
    /// </summary>
    /// <remarks>
    /// This is only needed until dashboard graphs get policy price data from transactions as they should.
    /// </remarks>
    public string SerializedCalculationResult { get; private set; }

    /// <summary>
    /// Gets representation of <see cref="PolicyReadModel.LastModifiedByUserTicksSinceEpoch"/> in ticks since epoch to allow persisting via EF.
    /// </summary>
    public long? LastModifiedByUserTicksSinceEpoch { get; private set; }

    /// <summary>
    /// Gets or sets the time the policy was last modified by User.
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

    /// <summary>
    /// Gets the time the policy renewal is effective in ticks for persistance.
    /// </summary>
    public long? LatestRenewalEffectiveTicksSinceEpoch { get; private set; }

    /// <summary>
    /// Gets the timestamp the policy latest renewal is effective.
    /// </summary>
    public Instant? LatestRenewalEffectiveTimestamp
    {
        get => this.LatestRenewalEffectiveTicksSinceEpoch.HasValue
            ? Instant.FromUnixTimeTicks(this.LatestRenewalEffectiveTicksSinceEpoch.Value)
            : (Instant?)null;
        private set => this.LatestRenewalEffectiveTicksSinceEpoch = value.HasValue
            ? value.Value.ToUnixTimeTicks()
            : (long?)null;
    }

    /// <summary>
    /// Gets the time the policy retroactive in ticks for persistance.
    /// </summary>
    public long? RetroactiveTicksSinceEpoch { get; private set; }

    /// <summary>
    /// Gets the timestamp the policy retroactive.
    /// </summary>
    public Instant? RetroactiveTimestamp
    {
        get => this.RetroactiveTicksSinceEpoch.HasValue
            ? Instant.FromUnixTimeTicks(this.RetroactiveTicksSinceEpoch.Value)
            : (Instant?)null;
        private set => this.RetroactiveTicksSinceEpoch = value.Value.ToUnixTimeTicks();
    }

    /// <summary>
    /// Gets or sets the Id of organisation the policy belongs to.
    /// </summary>
    public Guid OrganisationId { get; set; }

    [NotMapped]
    public bool IsAdjusted => this.AdjustmentEffectiveTimestamp != null;

    /// <summary>
    /// Record policy issuing in response to event.
    /// </summary>
    /// <param name="event">The policy issued event.</param>
    public void OnPolicyIssued(QuoteAggregate.PolicyIssuedEvent @event)
    {
        this.PolicyNumber = @event.PolicyNumber;
        this.IssuedTimestamp = @event.Timestamp;
        this.InceptionDateTime = @event.InceptionDateTime;
        this.InceptionTimestamp = @event.InceptionTimestamp;
        this.ExpiryDateTime = @event.ExpiryDateTime;
        this.ExpiryTimestamp = @event.ExpiryTimestamp;
        this.PolicyUpsert(@event);
    }

    public void OnPolicyAdjusted(QuoteAggregate.PolicyAdjustedEvent @event)
    {
        this.AdjustmentEffectiveDateTime = @event.EffectiveDateTime;
        this.AdjustmentEffectiveTimestamp = @event.EffectiveTimestamp;
        this.ExpiryDateTime = @event.ExpiryDateTime ?? this.ExpiryDateTime;
        this.ExpiryTimestamp = @event.ExpiryTimestamp ?? this.ExpiryTimestamp;
        this.PolicyUpsert(@event);
    }

    /// <summary>
    /// Upsert policy data in response to an upsert event.
    /// </summary>
    /// <param name="event">The upsert event.</param>
    public void PolicyUpsert(IPolicyUpsertEvent @event)
    {
        this.QuoteId = @event.QuoteId;
        this.CalculationResult = @event.DataSnapshot.CalculationResult.Data;
        this.LastModifiedTimestamp = @event.Timestamp;
        this.ExpiryDateTime = @event.ExpiryDateTime;
        this.ExpiryTimestamp = @event.ExpiryTimestamp;
        this.UpdatePolicyState(@event.Timestamp);
    }

    /// <summary>
    /// Record policy issuing in response to event.
    /// </summary>
    /// <param name="event">The policy issued event.</param>
    public void OnPolicyNumberUpdated(QuoteAggregate.PolicyNumberUpdatedEvent @event)
    {
        this.PolicyNumber = @event.PolicyNumber;
        this.UpdatePolicyState(@event.Timestamp);
    }

    /// <summary>
    /// Patch the calculation result.
    /// </summary>
    /// <param name="patch">The patch to apply.</param>
    public void ApplyPatch(PolicyDataPatch patch)
    {
        if (patch.Type == DataPatchType.CalculationResult)
        {
            // CalculationResult is deserialized/serialized on access,
            // so we need to get it, patch it, and re-set it, to correctly
            // update SerializedCalculationResult, which is what is actually persisted.
            var calculationResult = this.CalculationResult;
            calculationResult.PatchProperty(patch.Path, patch.Value);
            this.CalculationResult = calculationResult;
        }
    }

    /// <summary>
    /// Mark the policy as cancelled.
    /// </summary>
    /// <param name="event">The policy cancelled event.</param>
    public void RecordCancellation(QuoteAggregate.PolicyCancelledEvent @event)
    {
        this.CancellationEffectiveDateTime = @event.EffectiveDateTime;
        this.CancellationEffectiveTimestamp = @event.EffectiveTimestamp;
        this.LastModifiedTimestamp = @event.Timestamp;
        this.UpdatePolicyState(@event.Timestamp);
    }

    /// <summary>
    /// Uncancel a policy due to rollback.
    /// </summary>
    public void Uncancel()
    {
        this.CancellationEffectiveDateTime = null;
        this.CancellationEffectiveTicksSinceEpoch = null;
    }

    /// <summary>
    /// Mark the policy as renewal to set the latest renewal effective date
    /// the inception time is where the policy will be get effective.
    /// So we assign the inception time as the latest renewal effective time of the policy.
    /// </summary>
    /// <param name="event">The policy renewal event.</param>
    public void RecordRenewal(QuoteAggregate.PolicyRenewedEvent @event)
    {
        this.LatestRenewalEffectiveTimestamp = @event.EffectiveTimestamp;
    }

    /// <summary>
    /// Mark the policy family as discarded.
    /// </summary>
    /// <param name="timestamp">The time the policy was cancelled.</param>
    public void RecordDiscarding(Instant timestamp)
    {
        this.IsDiscarded = true;
        this.LastModifiedTimestamp = timestamp;
        this.LastModifiedByUserTimestamp = timestamp;
    }

    /// <summary>
    /// Mark the policy state change.
    /// </summary>
    /// <param name="timestamp">The time the policy was changed.</param>
    public void UpdatePolicyState(Instant timestamp)
    {
        var state = this.GetPolicyState(timestamp).Humanize();
        if (!this.PolicyState.EqualsIgnoreCase(state))
        {
            this.PolicyState = state;
            this.LastModifiedTimestamp = timestamp;
            this.LastModifiedByUserTimestamp = timestamp;
        }
    }

    /// <summary>
    /// Calculate's a policy's state at a given time.
    /// </summary>
    /// <param name="timestamp">The time to calculate the policy's status for.</param>
    /// <returns>The policy state.</returns>
    public PolicyStatus GetPolicyState(Instant timestamp)
    {
        if ((this.CancellationEffectiveTimestamp != default) &&
            ((this.CancellationEffectiveTimestamp < timestamp) ||
                (this.CancellationEffectiveTimestamp == this.InceptionTimestamp)))
        {
            return PolicyStatus.Cancelled;
        }

        if (this.InceptionTimestamp > timestamp)
        {
            return PolicyStatus.Issued;
        }

        if (this.ExpiryTimestamp > timestamp)
        {
            return PolicyStatus.Active;
        }

        return PolicyStatus.Expired;
    }

    /// <summary>
    /// Override function.
    /// </summary>
    /// <returns>The string value of the object.</returns>
    public override string ToString()
    {
        return $"{base.ToString()} Id {this.Id}";
    }
}
