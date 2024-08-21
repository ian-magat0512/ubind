// <copyright file="IPolicyService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;

    public interface IPolicyService
    {
        /// <summary>
        /// Gets the claims lodged against the policy in the last 5 years.
        /// </summary>
        IReadOnlyList<IClaimReadModel> GetPolicyHistoricalClaims(Policy policy);

        /// <summary>
        /// Generates a new quote number and reserves it for the quote.
        /// </summary>
        Task<string> GenerateQuoteNumber(ReleaseContext productContext);

        /// <summary>
        /// Check if renewal is allowed at the current time.
        /// </summary>
        /// <param name="policyDetails">The policy details.</param>
        /// <param name="isMutual">The indicator if policy is mutual.</param>
        /// <returns>returns true if policy can be renew.</returns>
        bool IsRenewalAllowedAtTheCurrentTime(IPolicyReadModelDetails policyDetails, bool isMutual);

        /// <summary>
        /// Gets a policy and subsequent transactions.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="policyId">The ID of the quote hose policy should be returned.</param>
        /// <returns>The policies details.</returns>
        Task<IPolicyReadModelDetails> GetPolicy(Guid tenantId, Guid policyId);

        /// <summary>
        /// Gets a list of application and subsequent policies.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="policyTransactionId">The policy transaction Id.</param>
        /// <returns>The policies details.</returns>
        IPolicyReadModelDetails GetPolicyTransactionForUser(Guid tenantId, Guid policyTransactionId);

        /// <summary>
        /// Retrieves a document associated with a policy transaction.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="documentId">The Id of the document.</param>
        /// <returns>The file content.</returns>
        IFileContentReadModel GetPolicyDocumentContent(Guid tenantId, Guid documentId);

        /// <summary>
        /// Checks if any policy exists for the specified customer.
        /// </summary>
        /// <param name="filters">The filters to apply.</param>
        /// <param name="excludedPolicyIds">The policy Ids to exclude on the search.</param>
        /// <returns>True if policies exist, false otherwise.</returns>
        bool HasPoliciesForCustomer(PolicyReadModelFilters filters, IEnumerable<Guid> excludedPolicyIds);

        /// <summary>
        /// Checks if the specified policy ID exists.
        /// </summary>
        /// <param name="policyId">The ID of the policy to search.</param>
        /// <param name="tenantId">If specified, searches for policies under this tenant.</param>
        /// <returns>True if the policy exist, false otherwise.</returns>
        bool HasPolicy(Guid policyId, Guid? tenantId = null);

        /// <summary>
        /// Loads the past claims and updates the form data with the past claims information so that
        /// the most recent past claims can be considered in calcualtions and shown to the user.
        /// </summary>
        /// <param name="quote">The quote with a policy.</param>
        /// <returns>a Task which can be awaited.</returns>
        Task UpdatePastClaimsIfNotApproved(Quote quote);

        /// <summary>
        /// Gets the policy number from the pool and does not persist.
        /// </summary>
        /// <param name="productContext">The product context.</param>
        /// <returns>A policy number from the pool.</returns>
        public string ConsumePolicyNumber(IProductContext productContext);

        /// <summary>
        /// Gets the policy number from the pool and persist.
        /// </summary>
        /// <param name="productContext">The product context.</param>
        /// <returns>A policy number from the pool.</returns>
        public string ConsumePolicyNumberAndPersist(IProductContext productContext);

        /// <summary>
        /// returns the policy number to the pool and persist.
        /// </summary>
        /// <param name="productContext">The product context.</param>
        /// <returns>A policy number taken from the pool.</returns>
        public void UnConsumePolicyNumberAndPersist(IProductContext productContext, string number);

        /// <summary>
        /// Issues a policy.
        /// </summary>
        /// <param name="progressQuoteState">A value indicating whether the policy operation should progress the quote state.</param>
        /// <param name="externalPolicyNumber">If no externalPolicyNumber is specified, a policy number from the pool will be used..</param>
        /// <returns>A task that can be awaited.</returns>
        Task IssuePolicy(
            Guid calculationResultId,
            Guid? performingUserId,
            QuoteAggregate quoteAggregate,
            NewBusinessQuote newBusinessQuote,
            Domain.Configuration.IProductConfiguration productConfiguration,
            IQuoteWorkflow quoteWorkflow,
            bool progressQuoteState = true,
            string? externalPolicyNumber = "");

        Task IssuePolicy(
           Guid calculationResultId,
           Guid? performingUserId,
           QuoteAggregate quoteAggregate,
           NewBusinessQuote newBusinessQuote,
           Domain.Configuration.IProductConfiguration productConfiguration,
           IQuoteWorkflow quoteWorkflow);

        /// <summary>
        /// Check if refund is allowed.
        /// </summary>
        bool IsRefundAllowed(Policy policy, StandardQuoteDataRetriever quoteDataRetriever, ProductFeatureSetting productFeatureSetting);

        /// <summary>
        /// Throws an error if the policy number given by the user is not unique in the tenant's product environment
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="productId"></param>
        /// <param name="environment"></param>
        /// <param name="policyNumber"></param>
        /// <returns></returns>
        Task ThrowIfPolicyNumberInUse(
           Guid tenantId,
           Guid productId,
           DeploymentEnvironment environment,
           string policyNumber);

        Task<string> CompletePolicyTransaction(
            ReleaseContext releaseContext,
            Quote quote,
            Guid calculationResultId,
            CancellationToken cancellationToken,
            FormData? formData = null,
            string? externalPolicyNumber = null,
            bool progressQuoteState = true);
    }
}
