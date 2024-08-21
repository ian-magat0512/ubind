// <copyright file="TransactionParties.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Accounting
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// The parties involved in financial transaction (i.e. payments, invoice).
    /// </summary>
    public class TransactionParties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionParties"/> class.
        /// </summary>
        /// <param name="payerId">The payer Id.</param>
        /// <param name="payerType">The payer type.</param>
        /// <param name="payeeId">The payee(receiver) id.</param>
        /// <param name="payeeType">The payee yype.</param>
        [JsonConstructor]
        public TransactionParties(
            Guid payerId,
            TransactionPartyType payerType,
            Guid payeeId,
            TransactionPartyType payeeType)
        {
            this.PayerId = payerId;
            this.PayeeId = payeeId;
            this.PayerType = payerType;
            this.PayeeType = payeeType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionParties"/> class.
        /// </summary>
        /// <param name="payerId">The payer Id.</param>
        /// <param name="payerType">The payer type.</param>
        public TransactionParties(
            Guid payerId,
            TransactionPartyType payerType)
        {
            this.PayerId = payerId;
            this.PayerType = payerType;
        }

        /// <summary>
        /// Gets the payer id.
        /// </summary>
        [JsonProperty]
        public Guid PayerId { get; }

        /// <summary>
        /// Gets the payee id.
        /// </summary>
        [JsonProperty]
        public Guid? PayeeId { get; }

        /// <summary>
        /// Gets the Payer Type.
        /// </summary>
        [JsonProperty]
        public TransactionPartyType PayerType { get; }

        /// <summary>
        /// Gets the Payee id.
        /// </summary>
        [JsonProperty]
        public TransactionPartyType? PayeeType { get; }
    }
}
