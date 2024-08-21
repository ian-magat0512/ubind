// <copyright file="IEntityQueryService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.Automation;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Sms;

    /// <summary>
    /// Service for querying entities from the database.
    /// </summary>
    public interface IEntityQueryService
    {
        /// <summary>
        /// Get a queryable collection of events from the database.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the events should belong to.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of events.</returns>
        IQueryable<Event> QueryEvents(Guid tenantId);

        /// <summary>
        /// Get a queryable collection of quotes from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of quotes.</returns>
        IQueryable<IQuoteReadModelWithRelatedEntities> QueryQuotes(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of quote versions from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of quote versions.</returns>
        IQueryable<IQuoteVersionReadModelWithRelatedEntities> QueryQuoteVersions(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of claims from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of claims.</returns>
        IQueryable<IClaimReadModelWithRelatedEntities> QueryClaims(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of claim versions from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of claim version.</returns>
        IQueryable<IClaimVersionReadModelWithRelatedEntities> QueryClaimVersions(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of policies from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of policies.</returns>
        IQueryable<IPolicyReadModelWithRelatedEntities> QueryPolicies(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of policy transactions from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of policy Transactions.</returns>
        IQueryable<IPolicyTransactionReadModelWithRelatedEntities> QueryPolicyTransactions(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of organisations from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of organisations.</returns>
        IQueryable<IOrganisationReadModelWithRelatedEntities> QueryOrganisations(
            Guid tenantId, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of products from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of products.</returns>
        IQueryable<IProductWithRelatedEntities> QueryProducts(
            Guid tenantId, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of tenants from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of tenants.</returns>
        IQueryable<ITenantWithRelatedEntities> QueryTenants(
            Guid tenantId, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of customers from the database.
        /// </summary>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of customers.</returns>
        IQueryable<ICustomerReadModelWithRelatedEntities> QueryCustomers(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of users from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of users.</returns>
        IQueryable<IUserReadModelWithRelatedEntities> QueryUsers(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of documents from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of documents.</returns>
        IQueryable<IDocumentReadModelWithRelatedEntities> QueryDocuments(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of emails from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of emails.</returns>
        IQueryable<IEmailReadModelWithRelatedEntities> QueryEmails(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of sms from the database.
        /// </summary>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of emails.</returns>
        IQueryable<ISmsReadModelWithRelatedEntities> QuerySms(
            Guid tenantId, IEnumerable<string> includedProperties);

        /// <summary>
        /// Get a queryable collection of portals from the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        /// <returns>A <see cref="IQueryable{T}"></see> collection of portals.</returns>
        IQueryable<IPortalWithRelatedEntities> QueryPortals(
            Guid tenantId, IEnumerable<string> includedProperties);
    }
}
