// <copyright file="NewBusinessPolicyTransaction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Entities
{
    using System;
    using NodaTime;
    using UBind.Domain.ReadWriteModel;

    public class NewBusinessPolicyTransaction : PolicyTransaction
    {
        [System.Text.Json.Serialization.JsonConstructor]
        public NewBusinessPolicyTransaction(
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
            : base(
                  id,
                  quoteId,
                  productReleaseId,
                  eventSequenceNumber,
                  effectiveDateTime,
                  effectiveTimestamp,
                  expiryDateTime,
                  expiryTimestamp,
                  createdTimestamp,
                  formData,
                  calculationResult)
        {
        }

        public NewBusinessPolicyTransaction(Guid transactionId, QuoteAggregate.PolicyIssuedEvent @event, int eventSequenceNumber)
            : base(transactionId, @event, eventSequenceNumber)
        {
        }

        public NewBusinessPolicyTransaction(Guid transactionId, QuoteAggregate.PolicyImportedEvent @event, int eventSequenceNumber)
            : base(transactionId, @event, eventSequenceNumber)
        {
        }

        public NewBusinessPolicyTransaction(Guid transactionId, QuoteAggregate.PolicyIssuedWithoutQuoteEvent @event, int eventSequenceNumber)
            : base(transactionId, @event, eventSequenceNumber)
        {
        }

        public NewBusinessPolicyTransaction(
            Domain.ReadModel.Policy.PolicyTransaction policyTransactionReadModel,
            QuoteDataSnapshot quoteDataSnapshot)
            : base(policyTransactionReadModel, quoteDataSnapshot)
        {
        }

        public override TransactionType Type => TransactionType.NewBusiness;
    }
}
