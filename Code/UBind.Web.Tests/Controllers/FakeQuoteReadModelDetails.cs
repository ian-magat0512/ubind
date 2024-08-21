// <copyright file="FakeQuoteReadModelDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. Public property must be named correctly instad of adding a comment.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Defines the <see cref="FakeQuoteReadModelDetails" />.
    /// </summary>
    public class FakeQuoteReadModelDetails : IQuoteReadModelDetails
    {
        /// <summary>
        /// Gets or sets the OwnerFullName.
        /// </summary>
        public string OwnerFullName { get; set; }

        /// <summary>
        /// Gets or sets the name of the organisation this quote was created under.
        /// </summary>
        public string OrganisationName { get; set; }

        /// <summary>
        /// Gets or sets the Documents.
        /// </summary>
        public IEnumerable<QuoteDocumentReadModel> Documents { get; set; }

        /// <summary>
        /// Gets or sets the LatestCalculationResultId.
        /// </summary>
        public Guid LatestCalculationResultId { get; set; }

        /// <summary>
        /// Gets or sets the LatestCalculationResultFormDataId.
        /// </summary>
        public Guid LatestCalculationResultFormDataId { get; set; }

        /// <summary>
        /// Gets or sets the Cancellation Time.
        /// </summary>
        public Instant CancellationEffectiveTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the Cancellation date.
        /// </summary>
        public LocalDate CancellationEffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the LatestFormData.
        /// </summary>
        public string LatestFormData { get; set; }

        /// <summary>
        /// Gets or sets the QuoteId.
        /// </summary>
        public Guid QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the TenantId.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the OrganisationId.
        /// </summary>
        public Guid OrganisationId { get; set; }

        public string OrganisationAlias { get; set; }

        /// <summary>
        /// Gets or sets the Quote Title.
        /// </summary>
        public string QuoteTitle { get; set; }

        /// <summary>
        /// Gets or sets the QuoteNumber.
        /// </summary>
        public string QuoteNumber { get; set; }

        /// <summary>
        /// Gets or sets invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Gets or sets invoice time.
        /// </summary>
        public Instant InvoiceTimestamp { get; set; }

        /// <summary>
        /// Gets or sets quote submission time.
        /// </summary>
        public Instant SubmissionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets policy inception time.
        /// </summary>
        public Instant? PolicyInceptionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets invoice.
        /// </summary>
        public bool IsInvoiced { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quote is paidfor.
        /// </summary>
        public bool IsPaidFor { get; set; }

        /// <summary>
        /// Gets or sets the PolicyNumber.
        /// </summary>
        public string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets the Environment.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsSubmitted.
        /// </summary>
        public bool IsSubmitted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsTestData.
        /// </summary>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets the ProductId.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quote is discarded or not.
        /// </summary>
        public bool IsDiscarded { get; set; }

        /// <summary>
        /// Gets or sets the ProductName.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the NameOfProduct.
        /// </summary>
        public string NameOfProduct { get; set; }

        /// <summary>
        /// Gets or sets the OwnerUserId.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the CustomerId.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the CustomerFullName.
        /// </summary>
        public string CustomerFullName { get; set; }

        /// <summary>
        /// Gets or sets the CustomerPreferredName.
        /// </summary>
        public string CustomerPreferredName { get; set; }

        /// <summary>
        /// Gets or sets the LastModifiedTime.
        /// </summary>
        public Instant LastModifiedTimestamp { get; set; }

        /// <summary>
        /// Gets the last modified time by user.
        /// </summary>
        public Instant? LastModifiedByUserTimestamp { get; }

        /// <summary>
        /// Gets or sets the SerializedLatestCalculationResult.
        /// </summary>
        public string SerializedLatestCalculationResult { get; set; }

        /// <summary>
        /// Gets or sets the PolicyExpiryDate.
        /// </summary>
        public LocalDate PolicyExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the PolicyInceptionDate.
        /// </summary>
        public LocalDate PolicyInceptionDate { get; set; }

        /// <summary>
        /// Gets or sets the QuoteState.
        /// </summary>
        public string QuoteState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether PolicyIssued.
        /// </summary>
        public bool PolicyIssued { get; set; }

        /// <summary>
        /// Gets or sets the PolicyEffectiveCancellationTicksSinceEpoch.
        /// </summary>
        public long CancellationEffectiveTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the CreatedTimestamp.
        /// </summary>
        public Instant CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the PolicyIssuedTime.
        /// </summary>
        public Instant PolicyIssuedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsCancelled.
        /// </summary>
        public bool IsCancelled { get; set; }

        /// <summary>
        /// Gets or sets the InceptionDate.
        /// </summary>
        public LocalDate InceptionDate { get; set; }

        /// <summary>
        /// Gets or sets the ExpiryDate.
        /// </summary>
        public LocalDate ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the InceptionTime.
        /// </summary>
        public Instant InceptionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the ExpiryTime.
        /// </summary>
        public Instant? ExpiryTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the QuoteType.
        /// </summary>
        public QuoteType QuoteType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsAdjusted.
        /// </summary>
        public bool IsAdjusted { get; set; }

        /// <summary>
        /// Gets or sets the EffectiveDate.
        /// </summary>
        public LocalDate EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the EffectiveTime.
        /// </summary>
        public Instant EffectiveTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the EffectiveEndTime.
        /// </summary>
        public Instant EffectiveEndTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the PolicyId.
        /// </summary>
        public Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the Aggregate Id.
        /// </summary>
        public Guid AggregateId { get; set; }

        /// <summary>
        /// Gets or sets the LatestCalculationResult.
        /// </summary>
        public CalculationResultReadModel LatestCalculationResult { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the expiry is enabled.
        /// </summary>
        public bool ExpiryEnabled { get; set; }

        /// <inheritdoc/>
        public string PaymentGateway { get; }

        /// <inheritdoc/>
        public string PaymentResponseJson { get; }

        /// <inheritdoc/>
        public string ProductAlias { get; set; }

        /// <inheritdoc/>
        public Guid? PolicyOwnerUserId { get; set; }

        /// <inheritdoc/>k
        public Guid? CustomerOwnerUserId { get; set; }

        public Guid? ProductReleaseId { get; set; }

        public int? ProductReleaseMajorNumber { get; set; }

        public int? ProductReleaseMinorNumber { get; set; }

        public LocalDate PolicyEffectiveDate => throw new NotImplementedException();

        public Instant PolicyEffectiveTimestamp => throw new NotImplementedException();

        public Instant PolicyEffectiveEndTimestamp => throw new NotImplementedException();

        public Instant? PolicyExpiryTimestamp { get; }

        public DateTimeZone TimeZone => throw new NotImplementedException();

        public string TimeZoneId => throw new NotImplementedException();

        public Instant? PolicyTransactionEffectiveTimestamp { get; }
    }
}
