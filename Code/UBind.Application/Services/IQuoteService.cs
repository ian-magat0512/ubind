// <copyright file="IQuoteService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hangfire;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Portal service for handling quote-related functionality.
    /// </summary>
    public interface IQuoteService
    {
        /// <summary>
        /// Retrieves the details of a specific quote for a given user.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="quoteId">The ID of the quote to be queried.</param>
        /// <returns>The quote.</returns>
        IQuoteReadModelDetails GetQuoteDetails(Guid tenantId, Guid quoteId);

        /// <summary>
        /// Retrieves the details of a specific quote version related to a specific quote.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="quoteId">The ID of the quote the quote versions are for.</param>
        /// <param name="version">version number.</param>
        /// <returns>quote version.</returns>
        QuoteVersionReadModelDto? GetQuoteVersionForQuote(Guid tenantId, Guid quoteId, int version);

        /// <summary>
        /// Retrieve a list of applications known as quotes for a given user, product and environment.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="filters">The additional filters to be used in retrieving the items.</param>
        /// <returns>Collectin of quotes.</returns>
        IEnumerable<IQuoteReadModelSummary> GetQuotes(Guid tenantId, QuoteReadModelFilters filters);

        /// <summary>
        /// Gets all quote Ids associated with the given policy.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="policyId">The policy Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>List of quote Ids.</returns>
        IEnumerable<Guid> GetQuoteIdsFromPolicy(Guid tenantId, Guid policyId, DeploymentEnvironment environment);

        /// <summary>
        /// Retrieve a specific quote's summary information.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="quoteId">The ID of the quote to be queried.</param>
        /// <returns>The quote summary.</returns>
        IQuoteReadModelSummary GetQuoteSummary(Guid tenantId, Guid quoteId);

        /// <summary>
        /// Retrieves the details of a specific quote version related to a specific quote.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="quoteVersionId">The ID of the quote version.</param>
        /// <returns>quote version.</returns>
        IQuoteVersionReadModelDetails GetQuoteVersionDetail(Guid tenantId, Guid quoteVersionId);

        /// <summary>
        /// Checks if any quote exists for the specified customer.
        /// </summary>
        /// <param name="filters">The filters to apply.</param>
        /// <param name="excludedQuoteIds">The quote Ids to exclude on the search.</param>
        /// <returns>True if quotes exist, false otherwise.</returns>
        bool HasQuotesForCustomer(QuoteReadModelFilters filters, IEnumerable<Guid> excludedQuoteIds);

        /// <summary>
        /// Verifies the availability of a customer quote association invitation.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="associationInvitationId">The ID of the association invitation to be checked.</param>
        /// <param name="quoteId">The ID of the quote for verification.</param>
        /// <param name="performingUserId">The ID of the customer user that triggers the request.</param>
        void VerifyQuoteCustomerAssociation(Guid tenantId, Guid associationInvitationId, Guid quoteId, Guid performingUserId);

        /// <summary>
        /// Gets the quote number from quote Id.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="quoteId">The ID of the quote for inquiry.</param>
        /// <returns>Returns the status of the invitation.</returns>
        string GetQuoteNumber(Guid tenantId, Guid quoteId);

        /// <summary>
        /// Gets the quote state from quote Id.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="quoteId">The ID of the quote for inquiry.</param>
        /// <returns>Returns the state of the quote.</returns>
        string GetQuoteState(Guid tenantId, Guid quoteId);

        /// <summary>
        /// Updates the expiry dates of existing quotes.
        /// </summary>
        /// <param name="settings">The quote expiry settings.</param>
        /// <param name="updateType">The update type for changes in quote expiry settings.</param>
        [JobDisplayName("Update Product Expiry Date Job | TENANT: {0}, PRODUCT: {1}, UPDATE TYPE: {3}")]
        Task UpdateExpiryDates(
            Guid tenantId,
            Guid productId,
            QuoteExpirySettings settings,
            ProductQuoteExpirySettingUpdateType updateType,
            CancellationToken cancellationToken);

        /// <summary>
        /// Removes the quote expiry dates from quotes for a given product
        /// This is used when disabling quote expiry.
        /// </summary>
        [JobDisplayName("Remove Product Expiry Date Job | TENANT: {0}, PRODUCT: {1}")]
        Task RemoveExpiryDates(
            Guid tenantId,
            Guid productId,
            CancellationToken cancellationToken);
    }
}
