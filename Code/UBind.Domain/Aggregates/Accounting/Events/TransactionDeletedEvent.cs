// <copyright file="TransactionDeletedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Accounting
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <content>
    /// Nested event and its handling for the <see cref="FinancialTransactionAggregate{TCommercialDocument}" />.
    /// </content>
    public abstract partial class FinancialTransactionAggregate<TCommercialDocument>
    {
        /// <summary>
        /// A financial transaction has been deleted.
        /// </summary>
        public class TransactionDeletedEvent : Event<FinancialTransactionAggregate<TCommercialDocument>, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TransactionDeletedEvent"/> class.
            /// </summary>
            /// <param name="id">The transaction id.</param>
            /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
            /// <param name="performingUserId">The user that performs this event.</param>
            public TransactionDeletedEvent(
                Guid tenantId,
                Guid id,
                Instant createdTimestamp,
                Guid? performingUserId)
                : base(tenantId, id, performingUserId, createdTimestamp)
            {
                this.IsDeleted = true;
            }

            [JsonConstructor]
            private TransactionDeletedEvent()
                : base(default, default, default, default)
            {
            }

            /// <summary>
            /// Gets a value indicating whether gets the financial transaction is deleted.
            /// </summary>
            [JsonProperty]
            public bool IsDeleted { get; private set; }
        }
    }
}
