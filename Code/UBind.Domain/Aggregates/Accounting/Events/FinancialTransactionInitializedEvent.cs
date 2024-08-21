// <copyright file="FinancialTransactionInitializedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Accounting
{
    using System;
    using Newtonsoft.Json;
    using NodaMoney;
    using NodaTime;
    using UBind.Domain.Accounting;
    using UBind.Domain.Aggregates;
    using UBind.Domain.ValueTypes;

    /// <content>
    /// Nested event and its handling for the <see cref="FinancialTransactionAggregate{TCommercialDocument}" />.
    /// </content>
    public abstract partial class FinancialTransactionAggregate<TCommercialDocument>
    {
        /// <summary>
        /// A financial transaction has been initialized.
        /// </summary>
        public class FinancialTransactionInitializedEvent : Event<FinancialTransactionAggregate<TCommercialDocument>, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FinancialTransactionInitializedEvent"/> class.
            /// </summary>
            /// <param name="id">The transaction id.</param>
            /// <param name="amount">The transaction amount.</param>
            /// <param name="referenceNumber">The reference number.</param>
            /// <param name="transactionTime">The time of financial transaction.</param>
            /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
            /// <param name="transactionPartyModel">The parties involved in the transaction.</param>
            /// <param name="performingUserId">The user that performs this event.</param>
            public FinancialTransactionInitializedEvent(
                Guid tenantId,
                Guid id,
                Money amount,
                string referenceNumber,
                Instant transactionTime,
                Instant createdTimestamp,
                TransactionParties transactionPartyModel,
                Guid? performingUserId)
                : base(tenantId, id, performingUserId, createdTimestamp)
            {
                this.Amount = amount;
                this.ReferenceNumber = referenceNumber;
                this.TransactionTime = transactionTime;
                this.PayerId = transactionPartyModel.PayerId;
                this.PayerType = transactionPartyModel.PayerType;
                this.PayeeId = transactionPartyModel.PayeeId;
                this.PayeeType = transactionPartyModel.PayeeType;
            }

            [JsonConstructor]
            private FinancialTransactionInitializedEvent()
                : base(default, default, default, default)
            {
            }

            /// <summary>
            /// Gets the amount.
            /// </summary>
            [JsonProperty]
            public Money Amount { get; private set; }

            /// <summary>
            /// Gets or sets the reference number.
            /// </summary>
            [JsonProperty]
            public string ReferenceNumber { get; set; }

            /// <summary>
            /// Gets the instant in time the financial transaction initialization occurred.
            /// </summary>
            [JsonProperty]
            public Instant TransactionTime { get; private set; }

            /// <summary>
            /// Gets or sets the payeeId.
            /// </summary>
            [JsonProperty]
            public Guid? PayeeId { get; set; }

            /// <summary>
            /// Gets or sets the payee type.
            /// </summary>
            [JsonProperty]
            public TransactionPartyType? PayeeType { get; set; }

            /// <summary>
            /// Gets or sets the payer id.
            /// </summary>
            [JsonProperty]
            public Guid PayerId { get; set; }

            /// <summary>
            /// Gets or sets the payer type.
            /// </summary>
            [JsonProperty]
            public TransactionPartyType PayerType { get; set; }
        }
    }
}
