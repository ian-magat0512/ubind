// <copyright file="ICustomerReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.ReadModel.Customer;

    /// <summary>
    /// For retrieving persisted quote details.
    /// </summary>
    public interface ICustomerReadModelRepository : IRepository
    {
        /// <summary>
        /// Gets a customer by ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The ID of the customer.</param>
        /// <param name="includeTestData">Value indicating if the query would include results from test data.</param>
        /// <returns>The customer, if found, otherwise null.</returns>
        CustomerReadModelDetail GetCustomerById(Guid tenantId, Guid id, bool includeTestData = true);

        /// <summary>
        /// Gets a customer read model by ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The ID of the customer.</param>
        /// <returns>The customer read model, if found, otherwise null.</returns>
        CustomerReadModel GetCustomerReadModelById(Guid tenantId, Guid id);

        /// <summary>
        /// Gets a customer with related entities by ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant the customer is under.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="id">The ID of the customer.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The customer, if found, otherwise null.</returns>
        ICustomerReadModelWithRelatedEntities GetCustomerWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid id, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets a customer with related entities by email.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the customer account is in.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="organisationId">The ID of the organisation the customer is in.</param>
        /// <param name="email">The customer account email.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The customer, if found, otherwise null.</returns>
        ICustomerReadModelWithRelatedEntities GetCustomerWithRelatedEntities(
            Guid tenantId,
            DeploymentEnvironment? environment,
            Guid organisationId,
            string email,
            IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets all customers within a tenant, matching a given filter.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <returns>All the customers owned by a given user and matching a given filter.</returns>
        IEnumerable<CustomerReadModel> GetCustomersMatchingFilter(Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Get customer's summary.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The Id of the customer.</param>
        /// <returns>The customer, if found, otherwise null.</returns>
        ICustomerReadModelSummary GetCustomerSummary(Guid tenantId, Guid id);

        /// <summary>
        /// Gets all customers summary within a tenant, matching a given filter.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <returns>All the customers owned by a given user and matching a given filter.</returns>
        IEnumerable<ICustomerReadModelSummary> GetCustomersSummaryMatchingFilters(
            Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Gets all active user customers summary within a tenant, matching a given filter.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <returns>All the active user customers owned by a given user and matching a given filter.</returns>
        IEnumerable<ICustomerReadModelSummary> GetActiveCustomerUsersSummaryMatchingFilters(Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Gets all customers within the tenant, matching a given filter.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>All the customers owned by a given user and matching a given filter.</returns>
        IEnumerable<ICustomerReadModelWithRelatedEntities> GetCustomersWithRelatedEntities(
            Guid tenantId, EntityListFilters filters, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets all customers person IDs.
        /// </summary>
        /// <returns>Dictionary of customer ID and person ID.</returns>
        IDictionary<Guid, Guid> GetAllExistingCustomersPersonIds(Guid tenantId);

        /// <summary>
        /// Method for creating IQueryable method that retrieve customers and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for customers.</returns>
        IQueryable<CustomerReadModelWithRelatedEntities> CreateQueryForCustomerDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets all the ids for active customers by filters.
        /// </summary>
        /// <param name="entityFilters">Instance of <see cref="EntityFilters"/>.</param>
        /// <returns>List of ids as string.</returns>
        List<Guid> GetIdsForAllActiveCustomersByEntityFilters(EntityFilters entityFilters);

        List<CustomerReadModel> GetDeletedCustomersWithUser();

        IQueryable<CustomerReadModel> GetCustomersForTenantIdAndOrganisationId(Guid tenantId, Guid destinationOrganisationId);

        /// <summary>
        /// Gets the customer's owner ID.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="customerId"></param>
        /// <returns>Owner ID</returns>
        Guid? GetCustomerOwnerId(Guid tenantId, Guid customerId);
    }
}
