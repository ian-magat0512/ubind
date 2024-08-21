// <copyright file="IPolicyReadModelDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// Data transfer object for quotereadmodel
    /// for a specific product.
    /// </summary>
    public interface IPolicyReadModelDetails : IPolicyReadModelSummary
    {
        /// <summary>
        /// Gets the organisation name.
        /// </summary>
        string OrganisationName { get; }

        /// <summary>
        /// Gets the full name of the person who owns this quote.
        /// </summary>
        string OwnerFullName { get; }

        /// <summary>
        /// Gets the documents associated with the policy.
        /// </summary>
        IEnumerable<QuoteDocumentReadModel> Documents { get; }

        /// <summary>
        /// Gets the latest form data.
        /// </summary>
        string LatestFormData { get; }

        /// <summary>
        /// Gets the transactions associated with a policy.
        /// </summary>
        IEnumerable<PolicyTransactionAndQuote> Transactions { get; }

        /// <summary>
        /// Gets the customers owner user id.
        /// </summary>
        Guid? CustomerOwnerUserId { get; }

        /// <summary>
        /// Gets the quotes owner user id.
        /// </summary>
        Guid? QuoteOwnerUserId { get; }

        /// <summary>
        /// Gets the customer person id.
        /// </summary>
        Guid? CustomerPersonId { get; }

        Guid? DisplayTransactionReleaseId { get; }

        /// <summary>
        /// Gets the "current" transaction for the policy, as defined by the spec here:
        /// https://confluence.aptiture.com/display/UBIND/Portal+UI+changes+for+policy+update%2C+renewal+and+cancellation+-+Acceptance+Criteria.
        /// </summary>
        /// <param name="asAtTimestamp">The time the current transaction is being determined.</param>
        /// <returns>The current transaction, if any, or null.</returns>
        PolicyTransaction GetCurrentTransaction(bool areTimestampsAuthoritative, DateTimeZone timeZone, Instant asAtTimestamp);

        /// <summary>
        /// Gets the transaction that should be used to display the policy details at a given time.
        /// </summary>
        /// <param name="asAtTimestamp">The time.</param>
        /// <returns>The display transaction.</returns>
        /// <remarks>
        /// The display transaction is defined by the spec here:
        /// https://confluence.aptiture.com/display/UBIND/Portal+UI+changes+for+policy+update%2C+renewal+and+cancellation+-+Acceptance+Criteria.
        /// </remarks>
        PolicyTransaction GetDisplayTransaction(bool areTimestampsAuthoritative, DateTimeZone timeZone, Instant asAtTimestamp);

        /// <summary>
        /// Gets the status of the policy to display in the product details.
        /// </summary>
        /// <param name="time">The time to calculate the status at.</param>
        /// <returns>The policy status.</returns>
        /// <remarks>
        /// Note that this is slightly different to the status used to group policies
        /// into tabs in the policy listing, which groups active and renewed together.
        /// </remarks>
        string GetDetailStatus(bool areTimestampsAuthoritative, DateTimeZone timeZone, Instant time);
    }
}
