// <copyright file="PolicyTransaction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote.Commands;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Configuration;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// For representing upsert transactions for a policy on the command side.
    /// </summary>
    public abstract class PolicyTransaction : IPatchableDataHolder
    {
        [System.Text.Json.Serialization.JsonConstructor]
        public PolicyTransaction(
            Guid id,
            Guid? quoteId,
            Guid? productReleaseId,
            int eventSequenceNumber,
            LocalDateTime effectiveDateTime,
            Instant effectiveTimestamp,
            LocalDateTime? expiryDateTime,
            Instant? expiryTimestamp,
            Instant createdTimestamp,
            FormData formData,
            CalculationResult calculationResult)
        {
            this.Id = id;
            this.QuoteId = quoteId;
            this.ProductReleaseId = productReleaseId;
            this.EventSequenceNumber = eventSequenceNumber;
            this.FormData = formData;
            this.CalculationResult = calculationResult;
            this.EffectiveDateTime = effectiveDateTime;
            this.EffectiveTimestamp = effectiveTimestamp;
            this.ExpiryDateTime = expiryDateTime;
            this.ExpiryTimestamp = expiryTimestamp;
            this.CreatedTimestamp = createdTimestamp;
        }

        public PolicyTransaction(
            Domain.ReadModel.Policy.PolicyTransaction policyTransactionReadModel,
            QuoteDataSnapshot quoteDataSnapshot)
        {
            this.Id = policyTransactionReadModel.Id;
            this.QuoteId = policyTransactionReadModel.QuoteId;
            this.EventSequenceNumber = policyTransactionReadModel.EventSequenceNumber;
            this.FormData = quoteDataSnapshot.FormData.Data;
            this.CalculationResult = quoteDataSnapshot.CalculationResult.Data;
            this.EffectiveDateTime = policyTransactionReadModel.EffectiveDateTime;
            this.EffectiveTimestamp = policyTransactionReadModel.EffectiveTimestamp;
            this.ExpiryDateTime = policyTransactionReadModel.ExpiryDateTime;
            this.ExpiryTimestamp = policyTransactionReadModel.ExpiryTimestamp;
            this.CreatedTimestamp = policyTransactionReadModel.CreatedTimestamp;
            this.ProductReleaseId = policyTransactionReadModel.ProductReleaseId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransaction"/> class.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction.</param>
        /// <param name="event">The event that generated the transaction.</param>
        /// <param name="eventSequenceNumber">The event sequence number.</param>
        public PolicyTransaction(Guid transactionId, IPolicyUpsertEvent @event, int eventSequenceNumber)
        {
            this.Id = transactionId;
            this.QuoteId = @event.QuoteId;
            this.EventSequenceNumber = eventSequenceNumber;

            // Note: this null checking was needed for the migration UB-11410 to proceed because some records dont have this.
            // this is triggered when getting an aggregate by id, but the production data have this as null,
            // maybe we can remove this null handling after the migration has happened.
            if (@event.DataSnapshot != null)
            {
                this.FormData = @event.DataSnapshot.FormData.Data;
                this.CalculationResult = @event.DataSnapshot.CalculationResult.Data;
            }
            this.EffectiveDateTime = @event.EffectiveDateTime;
            this.EffectiveTimestamp = @event.EffectiveTimestamp;
            this.ExpiryDateTime = @event.ExpiryDateTime;
            this.ExpiryTimestamp = @event.ExpiryTimestamp;
            this.CreatedTimestamp = @event.Timestamp;
            this.ProductReleaseId = @event.ProductReleaseId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransaction"/> class.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction.</param>
        /// <param name="event">The event that generated the transaction.</param>
        /// <param name="quoteDataSnapshot">The snapshot.</param>
        /// <param name="eventSequenceNumber">The event sequence number.</param>
        public PolicyTransaction(Guid transactionId, IPolicyUpsertEvent @event, QuoteDataSnapshot quoteDataSnapshot, int eventSequenceNumber)
        {
            this.Id = transactionId;
            this.QuoteId = @event.QuoteId;
            this.EventSequenceNumber = eventSequenceNumber;
            this.FormData = quoteDataSnapshot.FormData.Data;
            this.CalculationResult = quoteDataSnapshot.CalculationResult.Data;
            this.EffectiveDateTime = @event.EffectiveDateTime;
            this.EffectiveTimestamp = @event.EffectiveTimestamp;
            this.ExpiryDateTime = @event.ExpiryDateTime;
            this.ExpiryTimestamp = @event.ExpiryTimestamp;
            this.ProductReleaseId = @event.ProductReleaseId;
        }

        [JsonConstructor]
        public PolicyTransaction()
        {
        }

        /// <summary>
        /// Gets the ID of the transaction.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the quote id of the transaction.
        /// </summary>
        [JsonProperty]
        public Guid? QuoteId { get; private set; }

        [JsonProperty]
        public Guid? ProductReleaseId { get; private set; }

        /// <summary>
        /// Gets the sequence number of the event that generated the transaction.
        /// </summary>
        [JsonProperty]
        public int EventSequenceNumber { get; private set; }

        /// <summary>
        /// Gets the effective date time for this policy transaction.
        /// This means different things for different policy transaction types:
        /// New Business: The inception date and policy period start date.
        /// Renewal: The policy period start date.
        /// Adjustment: The adjustment effective date.
        /// Cancellation: The cancellation effective date.
        /// For policies which are not date based, this should be set to the createdDateTime.
        /// </summary>
        public LocalDateTime EffectiveDateTime { get; private set; }

        /// <summary>
        /// Gets the effective timestamp, pre-calculated at the time the transaction was created.
        /// </summary>
        public Instant EffectiveTimestamp { get; private set; }

        /// <summary>
        /// Gets or the expiry date time of the policy.
        /// Only valid for new business, renewal and adjustment transactions.
        /// For policies which are not date based, this would be null.
        /// </summary>
        public LocalDateTime? ExpiryDateTime { get; private set; }

        /// <summary>
        /// Gets the expiry timestamp, pre-calculated at the time the transaction was created.
        /// </summary>
        public Instant? ExpiryTimestamp { get; private set; }

        /// <summary>
        /// Gets or sets the value of the time stamp it was created.
        /// </summary>
        [JsonProperty]
        public Instant CreatedTimestamp { get; protected set; }

        /// <summary>
        /// Gets the form data for the policy upsert.
        /// </summary>
        [JsonProperty]
        public FormData FormData { get; private set; }

        /// <summary>
        /// Gets the calculation result for the policy upsert.
        /// </summary>
        [JsonProperty]
        public ReadWriteModel.CalculationResult CalculationResult { get; private set; }

        public abstract TransactionType Type { get; }

        /// <inheritdoc/>
        public Result<IEnumerable<DataPatchTargetEntity>> SelectAndValidateFormDataPatchTargets(PolicyDataPatchCommand command)
        {
            if (!command.Scope.Applicable(this)
                || command.TargetFormDataPath == null)
            {
                return Result.Success(Enumerable.Empty<DataPatchTargetEntity>());
            }

            var canPatchResult = this.FormData.CanPatchFormModelProperty(command.TargetFormDataPath, command.Rules);
            if (canPatchResult.IsFailure)
            {
                return Result.Failure<IEnumerable<DataPatchTargetEntity>>(
                    $"Cannot patch form data in transaction {this.Id} at {command.TargetFormDataPath}: {canPatchResult.Error}");
            }

            var patchTargets = new List<DataPatchTargetEntity>
            {
                new PolicyTransactionDataPatchTarget(this.Id),
            };

            return Result.Success<IEnumerable<DataPatchTargetEntity>>(patchTargets);
        }

        /// <inheritdoc/>
        public Result<IEnumerable<DataPatchTargetEntity>> SelectAndValidateCalculationResultPatchTargets(PolicyDataPatchCommand command)
        {
            if (!command.Scope.Applicable(this)
                || command.TargetCalculationResultPath == null)
            {
                return Result.Success(Enumerable.Empty<DataPatchTargetEntity>());
            }

            var canPatchResult = this.CalculationResult.CanPatchProperty(
                command.TargetCalculationResultPath, command.Rules);
            if (canPatchResult.IsFailure)
            {
                return Result.Failure<IEnumerable<DataPatchTargetEntity>>(
                    $"Cannot patch calculation result in transaction {this.Id} at {command.TargetCalculationResultPath.Value}: {canPatchResult.Error}");
            }

            var patchTargets = new List<DataPatchTargetEntity>
            {
                new PolicyTransactionDataPatchTarget(this.Id),
            };

            return Result.Success<IEnumerable<DataPatchTargetEntity>>(patchTargets);
        }

        /// <inheritdoc/>
        public void ApplyPatch(PolicyDataPatch patch)
        {
            if (patch.IsApplicable(this))
            {
                if (patch.Type == DataPatchType.FormData)
                {
                    this.FormData.PatchFormModelProperty(patch.Path, patch.Value);
                }
                else if (patch.Type == DataPatchType.CalculationResult)
                {
                    this.CalculationResult.PatchProperty(patch.Path, patch.Value);
                }
            }
        }

        public void Apply(QuoteAggregate.MigrateQuotesAndPolicyTransactionsToNewProductReleaseEvent @event)
        {
            this.ProductReleaseId = @event.NewProductReleaseId;
        }

        public void Apply(QuoteAggregate.MigrateUnassociatedEntitiesToProductReleaseEvent @event)
        {
            this.ProductReleaseId = @event.NewProductReleaseId;
        }

        /// <summary>
        /// Get the quote data retriever for this transaction.
        /// </summary>
        /// <param name="productConfiguration">The product configuration.</param>
        /// <returns>A new instance of <see cref="StandardQuoteDataRetriever" /> with the transaction's quote data.</returns>
        public StandardQuoteDataRetriever GetQuoteData(IProductConfiguration productConfiguration)
        {
            return new StandardQuoteDataRetriever(productConfiguration, this.FormData, this.CalculationResult);
        }
    }
}
