// <copyright file="CustomerRecordsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Accounting;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// This class contains the wrapped up parameters with list of quotes, claims, payments and refunds for instantiating CustomerDetailsModel.
    /// </summary>
    public class CustomerRecordsModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerRecordsModel"/> class.
        /// </summary>
        /// <param name="quotes">The quotes belonging to the customer.</param>
        /// <param name="claims">The claims belonging to the customer.</param>
        /// <param name="policies">The policies belonging to the customer.</param>
        /// <param name="payments">The payments belonging to the customer.</param>
        /// <param name="refunds">The refunds belonging to the customer.</param>
        /// <param name="person">The person read model.</param>
        public CustomerRecordsModel(
            IEnumerable<IQuoteReadModelSummary> quotes,
            IEnumerable<IClaimReadModelSummary> claims,
            IEnumerable<IPolicyReadModelSummary> policies,
            IEnumerable<IFinancialTransactionReadModel<PaymentAllocationReadModel>> payments,
            IEnumerable<IFinancialTransactionReadModel<RefundAllocationReadModel>> refunds,
            IPersonReadModelSummary person)
        {
            this.Quotes = quotes;
            this.Policies = policies;
            this.Claims = claims;
            this.Payments = payments;
            this.Refunds = refunds;
            this.Person = person;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerRecordsModel"/> class.
        /// </summary>
        [JsonConstructor]
        public CustomerRecordsModel()
        {
        }

        /// <summary>
        /// Gets the  quotes.
        /// </summary>
        [JsonProperty]
        public IEnumerable<IQuoteReadModelSummary> Quotes { get; private set; }

        /// <summary>
        /// Gets the  quotes.
        /// </summary>
        [JsonProperty]
        public IEnumerable<IPolicyReadModelSummary> Policies { get; private set; }

        /// <summary>
        /// Gets the claims.
        /// </summary>
        [JsonProperty]
        public IEnumerable<IClaimReadModelSummary> Claims { get; private set; }

        /// <summary>
        /// Gets the payments.
        /// </summary>
        [JsonProperty]
        public IEnumerable<IFinancialTransactionReadModel<PaymentAllocationReadModel>> Payments { get; private set; }

        /// <summary>
        /// Gets the Refunds.
        /// </summary>
        [JsonProperty]
        public IEnumerable<IFinancialTransactionReadModel<RefundAllocationReadModel>> Refunds { get; private set; }

        /// <summary>
        /// Gets the Person.
        /// </summary>
        [JsonProperty]
        public IPersonReadModelSummary Person { get; private set; }
    }
}
