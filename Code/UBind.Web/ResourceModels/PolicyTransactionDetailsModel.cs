// <copyright file="PolicyTransactionDetailsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// For serving policy transaction details.
    /// </summary>
    public class PolicyTransactionDetailsModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransactionDetailsModel"/> class.
        /// </summary>
        /// <param name="policyDetails">The details of the policy the transaction belongs to.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="transactionsQuote">The transactions quote.</param>
        /// <param name="time">The current time.</param>
        /// <param name="timeZone">The timezone for the policy.</param>
        public PolicyTransactionDetailsModel(
            IPolicyReadModelDetails policyDetails,
            PolicyTransaction transaction,
            NewQuoteReadModel transactionsQuote,
            Instant time,
            DateTimeZone timeZone,
            string productAlias,
            ReleaseBase? release)
        {
            this.ProductAlias = productAlias;
            this.ProductName = policyDetails.ProductName;
            this.PolicyNumber = policyDetails.PolicyNumber;
            this.PolicyOwnerUserId = policyDetails.OwnerUserId;
            this.CreatedDateTime = transaction.CreatedTimestamp.ToExtendedIso8601String();
            this.LastModifiedDateTime = transaction.LastModifiedTimestamp.ToExtendedIso8601String();
            this.QuoteReference = transaction.QuoteNumber;
            this.QuoteId = transaction.QuoteId;
            this.QuoteOwnerUserId = transactionsQuote?.OwnerUserId;
            this.PolicyId = transaction.PolicyId;
            this.EffectiveDateTime = transaction.EffectiveTimestamp.ToExtendedIso8601String();
            this.ExpiryDateTime = transaction.ExpiryTimestamp?.ToExtendedIso8601String();
            this.TransactionStatus = transaction.GetTransactionStatus(policyDetails.AreTimestampsAuthoritative, timeZone, time);
            this.Documents = policyDetails.Documents
                .Where(document => document.QuoteOrPolicyTransactionId == transaction.Id)
                .Where(document => document.OwnerType == DocumentOwnerType.Policy)
                .Select(document => new DocumentSetModel(document));
            this.Owner = policyDetails.OwnerUserId.HasValue ? new UserSummaryModel(policyDetails.OwnerUserId.Value, policyDetails.OwnerFullName) : null;

            if (policyDetails.CustomerId.HasValue)
            {
                this.Customer = new CustomerSimpleModel(policyDetails.CustomerId.Value, policyDetails.CustomerFullName, policyDetails.CustomerOwnerUserId);
            }

            this.EventTypeSummary = transaction.GetEventTypeSummary();
            this.Status = policyDetails.GetDetailStatus(policyDetails.AreTimestampsAuthoritative, timeZone, time);
            if (release != null && release is Release deployedRelease)
            {
                this.ProductReleaseId = deployedRelease.Id;
                this.ProductReleaseNumber = deployedRelease.Number + "." + deployedRelease.MinorNumber;
            }
        }

        /// <summary>
        /// Gets the policy number.
        /// </summary>
        [JsonProperty]
        public string PolicyNumber { get; private set; }

        /// <summary>
        /// Gets the policy owner user id.
        /// </summary>
        [JsonProperty]
        public Guid? PolicyOwnerUserId { get; private set; }

        /// <summary>
        /// Gets or set the quote id.
        /// </summary>
        [JsonProperty]
        public Guid? QuoteId { get; private set; }

        /// <summary>
        /// Gets the quotes owner user id of the transaction.
        /// </summary>
        [JsonProperty]
        public Guid? QuoteOwnerUserId { get; private set; }

        /// <summary>
        /// Gets or set the status.
        /// </summary>
        [JsonProperty]
        public string Status { get; private set; }

        /// <summary>
        /// Gets or sets the Policy id.
        /// </summary>
        [JsonProperty]
        public Guid PolicyId { get; set; }

        /// <summary>
        /// Gets the quote reference.
        /// </summary>
        [JsonProperty]
        public string QuoteReference { get; private set; }

        /// <summary>
        /// Gets or sets the alias of the product the policy is issued for.
        /// </summary>
        [JsonProperty]
        public string ProductAlias { get; set; }

        /// <summary>
        /// Gets or sets the name of the product the policy is issued for.
        /// </summary>
        [JsonProperty]
        public string ProductName { get; set; }

        /// <summary>
        /// Gets the created timestamp.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the last modified timestamp.
        /// </summary>
        [JsonProperty]
        public string LastModifiedDateTime { get; private set; }

        [JsonProperty]
        public string EffectiveDateTime { get; }

        [JsonProperty]
        public string ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets the status of the transaction.
        /// </summary>
        [JsonProperty]
        public string TransactionStatus { get; private set; }

        /// <summary>
        /// Gets the type of transaction, e.g. Renewed, Cancelled, Adjusted etc.
        /// </summary>
        [JsonProperty]
        public string EventTypeSummary { get; private set; }

        /// <summary>
        /// Gets the documents associated with the transaction.
        /// </summary>
        [JsonProperty]
        public IEnumerable<DocumentSetModel> Documents { get; private set; }

        /// <summary>
        /// Gets or sets the customer for whom the policy is for.
        /// </summary>
        [JsonProperty]
        public CustomerSimpleModel Customer { get; set; }

        /// <summary>
        /// Gets or sets the summary details of the referrer/owner of the quote/policy.
        /// </summary>
        [JsonProperty]
        public UserSummaryModel Owner { get; set; }

        /// <summary>
        /// Gets or sets the ID of the product release used to create this policy transaction.
        /// </summary>
        [JsonProperty]
        public Guid? ProductReleaseId { get; set; }

        /// <summary>
        /// Gets or sets the number of the product release used to create this policy transaction.
        /// </summary>
        [JsonProperty]
        public string? ProductReleaseNumber { get; set; }
    }
}
