// <copyright file="QuoteMigratedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Services;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when a quote has been created.
        /// </summary>
        public class QuoteMigratedEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteMigratedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
            /// <param name="aggregateId">The aggregate Id.</param>
            /// <param name="timestamp">The time the quote was imported.</param>
            /// <param name="policyLimitTimesPolicy">Helper for setting inception and expiry times.</param>
            /// <param name="jsonSanitizer">Service for sanitizing json from calculation results.</param>
            /// <param name="performingUserId">The userId who migrated the quote.</param>
            public QuoteMigratedEvent(
                Guid tenantId,
                Guid aggregateId,
                Instant timestamp,
                IPolicyTransactionTimeOfDayScheme policyLimitTimesPolicy,
                ICalculationJsonSanitizer jsonSanitizer,
                Guid? performingUserId)
                : base(tenantId, aggregateId, performingUserId, timestamp)
            {
                throw new NotImplementedException(
                    "TODO: Fix import problem, where calculation results do not have periods associeated with them.");
                ////this.CalculationResults = application.CalculationResults
                ////    .Select(cr =>
                ////        new QuoteDataUpdate<ReadWriteModel.CalculationResult>(
                ////                cr.Id,
                ////                new ReadWriteModel.CalculationResult(cr.FormDataId, jsonSanitizer.Sanitize(cr.Json)),
                ////                cr.CreatedTimestamp))
                ////    .ToList();
                ////this.CustomerDetails = application.CustomerDetails
                ////    .Select(cd => new QuoteDataUpdate<IPersonalDetails>(cd.Id, new MigratedCustomerDetails(cd), cd.CreatedTimestamp))
                ////    .ToList();
                ////if (application.IsSubmitted)
                ////{
                ////    var submissionDataIds = new QuoteDataSnapshotIds(
                ////        application.Submission.SubmittedFormUpdateId,
                ////        application.Submission.SubmittedCalculationResultId,
                ////        default);
                ////    this.Submission = new QuoteSubmission(submissionDataIds, application.Submission.CreatedTimestamp);
                ////}

                ////if (application.InvoiceHasBeenIssued)
                ////{
                ////    var invoiceDataIds = new QuoteDataSnapshotIds(
                ////        application.Invoice.SubmittedFormUpdateId,
                ////        application.Invoice.SubmittedCalculationResultId,
                ////        default);
                ////    this.Invoice = new Invoice(application.Invoice.InvoiceNumber, invoiceDataIds, application.Invoice.CreatedTimestamp);
                ////}

                ////if (application.PolicyHasBeenIssued)
                ////{
                ////    var policyDataIds = new QuoteDataSnapshotIds(
                ////        application.Policy.SubmittedFormUpdateId,
                ////        application.Policy.SubmittedCalculationResultId,
                ////        default);
                ////    var datePattern = LocalDatePattern.CreateWithInvariantCulture("dd/MM/yyyy");
                ////    var inceptionDate = datePattern.Parse(application.Policy.InceptionDate).GetValueOrThrow();
                ////    var expiryDate = datePattern.Parse(application.Policy.ExpiryDate).GetValueOrThrow();
                ////    var inceptionTime = policyLimitTimesPolicy.GetInceptionTime(inceptionDate);
                ////    var expiryTime = policyLimitTimesPolicy.GetExpiryTime(expiryDate);

                ////    // Effective date and time are equal to start date and time, as all imported quotes
                ////    // will be new-business quotes.
                ////    this.Policy = new Policy(
                ////        application.Policy.PolicyNumber,
                ////        inceptionDate,
                ////        expiryDate,
                ////        inceptionDate,
                ////        inceptionTime,
                ////        expiryTime,
                ////        inceptionTime,
                ////        policyDataIds,
                ////        application.Policy.CreatedTimestamp);
                ////}

                ////this.PaymentAttemptResults = application.PaymentAttempts
                ////    .Select(par =>
                ////        {
                ////            var paymentDataIds = new QuoteDataSnapshotIds(
                ////                par.SubmittedFormUpdateId,
                ////                par.SubmittedCalculationResultId,
                ////                default);
                ////            return par.Outcome == PaymentAttemptOutcome.Success
                ////                ? PaymentAttemptResult.CreateSuccessResponse(
                ////                    new PaymentDetails(par.Reference), paymentDataIds, par.CreatedTimestamp)
                ////                : par.Outcome == PaymentAttemptOutcome.Failed
                ////                    ? PaymentAttemptResult.CreateFailureResponse(par.Errors, paymentDataIds, par.CreatedTimestamp)
                ////                    : PaymentAttemptResult.CreateErrorResponse(par.Errors, paymentDataIds, par.CreatedTimestamp);
                ////        })
                ////    .ToList();
            }

            [JsonConstructor]
            private QuoteMigratedEvent()
            {
            }

            /// <summary>
            /// Gets the created time of the original application entity.
            /// </summary>
            public Instant OriginalCreateTime { get; private set; }

            /// <summary>
            /// Gets the ID of the product the quote is for.
            /// Note: For Backward compatibility with events, It is to be converted to
            /// JsonProperty("ProductNewId") is important.
            /// </summary>
            [JsonProperty("ProductNewId")]
            public Guid ProductId { get; private set; }

            /// <summary>
            /// Gets the environment the quote belongs to.
            /// </summary>
            [JsonProperty]
            public DeploymentEnvironment Environment { get; private set; }

            /// <summary>
            /// Gets the set of form updates.
            /// </summary>
            [JsonProperty]
            public ICollection<QuoteDataUpdate<FormData>> FormUpdates { get; private set; } =
                new Collection<QuoteDataUpdate<FormData>>();

            /// <summary>
            /// Gets the set of file attachments (files uploaded by user).
            /// </summary>
            public ICollection<ApplicationFileAttachment> FileAttachments { get; private set; } =
                new Collection<ApplicationFileAttachment>();

            /// <summary>
            /// Gets the set of calculation results.
            /// </summary>
            [JsonProperty]
            public ICollection<QuoteDataUpdate<ReadWriteModel.CalculationResult>> CalculationResults { get; private set; } =
                new Collection<QuoteDataUpdate<ReadWriteModel.CalculationResult>>();

            /// <summary>
            /// Gets the set of customer details updates.
            /// </summary>
            [JsonProperty]
            public ICollection<QuoteDataUpdate<IPersonalDetails>> CustomerDetails { get; private set; } =
                new Collection<QuoteDataUpdate<IPersonalDetails>>();

            /// <summary>
            /// Gets the submission details, if any, otherwise null.
            /// </summary>
            [JsonProperty]
            public QuoteSubmission Submission { get; private set; }

            /// <summary>
            /// Gets the invoice details, if any, otherwise null.
            /// </summary>
            [JsonProperty]
            public Invoice Invoice { get; private set; }

            /// <summary>
            /// Gets the policy details, if any, otherwise null.
            /// </summary>
            [JsonProperty]
            public Policy Policy { get; private set; }

            /// <summary>
            /// Gets the set of payment attempt results for the quote.
            /// </summary>
            [JsonProperty]
            public ICollection<PaymentAttemptResult> PaymentAttemptResults { get; private set; } =
                new Collection<PaymentAttemptResult>();
        }
    }
}
