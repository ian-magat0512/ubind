// <copyright file="IQuoteReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using NodaTime;

    /// <summary>
    /// Data transfer object for Quote Read Model.
    /// </summary>
    public interface IQuoteReadModelSummary : IBaseReportReadModel
    {
        /// <summary>
        /// Gets Quote Id.
        /// </summary>
        Guid QuoteId { get; }

        /// <summary>
        /// Gets or sets invoice number.
        /// </summary>
        string InvoiceNumber { get; set; }

        /// <summary>
        /// Gets or sets quote tile.
        /// </summary>
        string QuoteTitle { get; set; }

        /// <summary>
        /// Gets or sets quote number.
        /// </summary>
        string QuoteNumber { get; set; }

        /// <summary>
        /// Gets invoice time.
        /// </summary>
        Instant InvoiceTimestamp { get; }

        /// <summary>
        /// Gets quote submission time.
        /// </summary>
        Instant SubmissionTimestamp { get; }

        /// <summary>
        /// Gets policy inception time.
        /// </summary>
        Instant? PolicyInceptionTimestamp { get; }

        /// <summary>
        /// Gets a value indicating whether quote has been invoice.
        /// </summary>
        bool IsInvoiced { get; }

        /// <summary>
        /// Gets a value indicating whether quote is paid for.
        /// </summary>
        bool IsPaidFor { get; }

        /// <summary>
        /// Gets policy number.
        /// </summary>
        string PolicyNumber { get; }

        /// <summary>
        /// Gets or sets a value indicating whether it has been submitted.
        /// </summary>
        bool IsSubmitted { get; set; }

        /// <summary>
        /// Gets a value indicating whether the quote is test data.
        /// </summary>
        bool IsTestData { get; }

        /// <summary>
        /// Gets or sets product name.
        /// </summary>
        string ProductName { get; set; }

        /// <summary>
        /// Gets customer full name.
        /// </summary>
        string CustomerFullName { get; }

        /// <summary>
        /// Gets customer preferredName.
        /// </summary>
        string CustomerPreferredName { get; }

        /// <summary>
        /// Gets the last modified time.
        /// </summary>
        Instant LastModifiedTimestamp { get; }

        /// <summary>
        /// Gets policy expiry date.
        /// </summary>
        LocalDate PolicyExpiryDate { get; }

        /// <summary>
        /// Gets policy inception date.
        /// </summary>
        LocalDate PolicyInceptionDate { get; }

        /// <summary>
        /// Gets the quote's status.
        /// </summary>
        string QuoteState { get; }

        /// <summary>
        /// Gets a value indicating whether a policy has been issued for the quote.
        /// </summary>
        bool PolicyIssued { get; }

        /// <summary>
        /// Gets the quote's created time.
        /// </summary>
        Instant CreatedTimestamp { get; }

        /// <summary>
        /// Gets the quote's cancellation time.
        /// </summary>
        Instant CancellationEffectiveTimestamp { get; }

        /// <summary>
        /// Gets the cancellation date.
        /// </summary>
        LocalDate CancellationEffectiveDate { get; }

        /// <summary>
        /// Gets the date the policy coverage begins.
        /// </summary>
        LocalDate PolicyEffectiveDate { get; }

        /// <summary>
        /// Gets the precise time the policy coverage starts.
        /// </summary>
        Instant PolicyEffectiveTimestamp { get; }

        /// <summary>
        /// Gets the precise time the policy coverage ends.
        /// </summary>
        Instant PolicyEffectiveEndTimestamp { get; }

        /// <summary>
        /// Gets the policy issue date time.
        /// </summary>
        Instant PolicyIssuedTimestamp { get; }

        /// <summary>
        /// Gets the time that the quote will expire.
        /// </summary>
        Instant? ExpiryTimestamp { get; }

        /// <summary>
        /// Gets the precise time the policy ends.
        /// </summary>
        Instant? PolicyExpiryTimestamp { get; }

        DateTimeZone TimeZone { get; }

        string TimeZoneId { get; }

        /// <summary>
        /// Gets quote number.
        /// </summary>
        QuoteType QuoteType { get; }

        /// <summary>
        /// Gets a value indicating whether quote is discarded or not.
        /// </summary>
        bool IsDiscarded { get; }

        /// <summary>
        /// Gets the deployment environment.
        /// </summary>
        DeploymentEnvironment Environment { get; }

        /// <summary>
        /// Gets latest Form data.
        /// </summary>
        string LatestFormData { get; }

        /// <summary>
        /// Gets the latest calculation result for the quote.
        /// </summary>
        CalculationResultReadModel LatestCalculationResult { get; }

        /// <summary>
        /// Gets the ID of the policy.
        /// </summary>
        Guid? PolicyId { get; }

        /// <summary>
        /// Gets the ID of the aggregate.
        /// </summary>
        Guid AggregateId { get; }

        /// <summary>
        /// Gets the ID of the tenant the policy is in.
        /// </summary>
        Guid TenantId { get; }

        /// <summary>
        /// Gets the ID of the organisation the policy is in.
        /// </summary>
        Guid OrganisationId { get; }

        string OrganisationAlias { get; }

        /// <summary>
        /// Gets the ID of the product the policy is for.
        /// </summary>
        Guid ProductId { get; }

        /// <summary>
        /// Gets the Alias of the product the policy is for.
        /// </summary>
        string ProductAlias { get; }

        /// <summary>
        /// Gets the ID of the user who owns the policy.
        /// </summary>
        Guid? OwnerUserId { get; }

        /// <summary>
        /// Gets the ID of the customer for the policy.
        /// </summary>
        Guid? CustomerId { get; }

        /// <summary>
        /// Gets the time policy transaction takes effect.
        /// </summary>
        Instant? PolicyTransactionEffectiveTimestamp { get; }

        Guid? ProductReleaseId { get; }

        int? ProductReleaseMajorNumber { get; }

        int? ProductReleaseMinorNumber { get; }
    }
}
