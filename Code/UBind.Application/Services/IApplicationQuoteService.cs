// <copyright file="IApplicationQuoteService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using FormData = UBind.Domain.Aggregates.Quote.FormData;

    /// <summary>
    /// Service for issuing policies.
    /// </summary>
    public interface IApplicationQuoteService
    {
        /// <summary>
        /// Create a new customer, assign a given application to it.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="customerDetails">The customer details.</param>
        /// <param name="portalId">The ID of the portal the customer would be expected to login to,
        /// if they end up having a user account.</param>
        /// <returns>The updated quote.</returns>
        Task<QuoteAggregate> CreateCustomerForApplication(
            Guid tenantId,
            Guid quoteId,
            Guid quoteAggregateId,
            IPersonalDetails customerDetails,
            Guid? portalId);

        /// <summary>
        /// Updates the customer details for application.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="quoteId">The ID of the quote to check for.</param>
        /// <param name="customerId">The ID of the customer to update.</param>
        /// <param name="customerDetails">The new details of the customer.</param>
        /// <param name="portalId">The ID of the portal the customer would be expected to login to,
        /// if they end up having a user account.</param>
        /// <returns>Returns status code 200 if success, otherwise, status code 409.</returns>
        Task<QuoteAggregate> UpdateCustomerForApplication(
            Guid tenantId, Guid quoteId, Guid customerId, IPersonalDetails customerDetails, Guid? portalId);

        /// <summary>
        /// Issue a quote for a given application.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant for that quote.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="formData">The latest form data.</param>
        /// <returns>The updated application.</returns>
        Task<QuoteAggregate> CreateVersion(
            Guid tenantId,
            Guid quoteId,
            FormData formData);

        /// <summary>
        /// Handles the actualise request.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="formData">The latest form data.</param>
        /// <returns>A task from which the updated application can be retrieved.</returns>
        Task Actualise(
            ReleaseContext releaseContext,
            Quote quote,
            FormData? formData);

        /// <summary>
        /// Discards an incomplete adjustment quote.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant for that quote.</param>
        /// <param name="parentQuoteId">The ID of the parent quote the adjustment quote is for.</param>
        /// <param name="userTenantId">The tenant ID the current portal user belongs to.</param>
        /// <returns>A task from which the updated application can be retrieved.</returns>
        Task<QuoteAggregate> DiscardQuote(Guid tenantId, Guid parentQuoteId, Guid userTenantId);

        /// <summary>
        /// Set the expiration of quote to the current time.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant for that quote.</param>
        /// <param name="quoteId">The ID of the quote on that quote aggregate.</param>
        /// <returns>A task from which the updated application can be retrieved.</returns>
        Task<QuoteAggregate> ExpireNow(Guid tenantId, Guid quoteId);

        /// <summary>
        /// Set the expiry date of the quote.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant for that quote.</param>
        /// <param name="quoteId">The ID of the quote on that quote aggregate.</param>
        /// <param name="dateTime">The expiry datetime of the quote.</param>
        /// <returns>A task from which the updated application can be retrieved.</returns>
        Task<QuoteAggregate> SetExpiry(Guid tenantId, Guid quoteId, Instant dateTime);

        /// <summary>
        /// Trigger event when opening quotes.
        /// </summary>
        Task TriggerEventWhenQuoteIsOpened(NewQuoteReadModel quote, bool performingUserIsCustomer);
    }
}
