// <copyright file="FakeQuoteReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Fakes
{
    using System;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.ReadModel;
    using Quote = UBind.Domain.Aggregates.Quote.Quote;

    /// <summary>
    /// This is a mock event repository implementation.
    /// which saves to an array.
    /// </summary>
    public class FakeQuoteReadModelSummary : IQuoteReadModelSummary
    {
        public FakeQuoteReadModelSummary(Quote quote)
        {
            this.QuoteId = quote.Id;
            this.QuoteNumber = quote.QuoteNumber;
            this.ExpiryTimestamp = quote.CreatedTimestamp;
            this.QuoteType = quote.Type;
            this.TimeZone = quote.TimeZone;
        }

        public Guid QuoteId { get; }

        public string InvoiceNumber { get; set; }

        public string QuoteTitle { get; set; }

        public string QuoteNumber { get; set; }

        public Instant InvoiceTimestamp { get; }

        public Instant SubmissionTimestamp { get; }

        public Instant PolicyInceptionTimestamp { get; }

        public bool IsInvoiced { get; }

        public bool IsPaidFor { get; }

        public string PolicyNumber { get; }

        public bool IsSubmitted { get; set; }

        public bool IsTestData { get; }

        public string ProductName { get; set; }

        public string CustomerFullName { get; }

        public string CustomerPreferredName { get; }

        public Instant LastModifiedTimestamp { get; }

        public LocalDate PolicyExpiryDate { get; }

        public LocalDate PolicyInceptionDate { get; }

        public string QuoteState { get; }

        public bool PolicyIssued { get; }

        public Instant CreatedTimestamp { get; }

        public Instant CancellationEffectiveTimestamp { get; }

        public LocalDate CancellationEffectiveDate { get; }

        public LocalDate PolicyEffectiveDate { get; }

        public Instant PolicyEffectiveTimestamp { get; }

        public Instant PolicyEffectiveEndTimestamp { get; }

        public Instant PolicyIssuedTimestamp { get; }

        public Instant? ExpiryTimestamp { get; }

        public Instant PolicyExpiryTimestamp { get; }

        public DateTimeZone TimeZone { get; }

        public string TimeZoneId { get; }

        public QuoteType QuoteType { get; }

        public bool IsDiscarded { get; }

        public DeploymentEnvironment Environment { get; }

        public string LatestFormData { get; }

        public CalculationResultReadModel LatestCalculationResult { get; }

        public Guid? PolicyId { get; }

        public Guid AggregateId { get; }

        public Guid TenantId { get; }

        public Guid OrganisationId { get; }

        public string OrganisationAlias { get; }

        public Guid ProductId { get; }

        public string ProductAlias { get; }

        public Guid? OwnerUserId { get; }

        public Guid? CustomerId { get; }

        public string PaymentGateway { get; }

        public string PaymentResponseJson { get; }

        public string SerializedLatestCalculationResult { get; }

        public Instant? PolicyTransactionEffectiveTimestamp { get; }

        public Guid? ProductReleaseId { get; }

        public int? ProductReleaseMajorNumber { get; }

        public int? ProductReleaseMinorNumber { get; }

        Instant? IQuoteReadModelSummary.PolicyInceptionTimestamp { get; }

        Instant? IQuoteReadModelSummary.PolicyExpiryTimestamp { get; }
    }
}
