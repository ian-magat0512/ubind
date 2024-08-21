// <copyright file="IOrganisationReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The interface for organisation read model repository.
    /// </summary>
    public interface IOrganisationReadModelRepository
    {
        /// <summary>
        /// Gets the <see cref="IEnumerable"/> collection of organisation read models.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the organisation.</param>
        /// <param name="filters">The read model filters to apply.</param>
        /// <returns>An enumerable collection of organisation read models.</returns>
        IEnumerable<OrganisationReadModel> Get(Guid tenantId, OrganisationReadModelFilters filters);

        IReadOnlyList<IOrganisationReadModelSummary> GetSummaries(Guid tenantId, OrganisationReadModelFilters filters);

        List<Guid> GetIds(Guid tenantId, OrganisationReadModelFilters filter);

        /// <summary>
        /// Gets the specific organisation read model.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the organisation.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <returns>The organisation read model that you searched for.</returns>
        OrganisationReadModel? Get(Guid tenantId, Guid organisationId);

        /// <summary>
        /// Gets the specific organisation alias by id.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="organisationId"></param>
        /// <returns></returns>
        Task<string?> GetOrganisationAliasById(Guid tenantId, Guid organisationId);

        /// <summary>
        /// Gets organisation read models by its tenant id.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the organisation.</param>
        /// <returns>The organisation read models that you searched for.</returns>
        IEnumerable<OrganisationReadModel> GetOrganisations(Guid tenantId);

        /// <summary>
        /// Gets the specific read model based on the given alias.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the organisation.</param>
        /// <param name="alias">The alias of the organisation.</param>
        /// <returns>The organisation read model that you searched for.</returns>
        OrganisationReadModel? GetByAlias(Guid tenantId, string alias);

        Guid GetIdForAlias(Guid tenantId, string alias);

        /// <summary>
        /// Checks if there is an existing alias under the given tenant Id.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="alias">The alias of the organisation.</param>
        /// <param name="organisationId">The Id of the existing organisation, if any.</param>
        /// <returns>Boolean value whether it has a matching value in the repository.</returns>
        bool IsAliasInUse(Guid tenantId, string alias, Guid? organisationId = null);

        /// <summary>
        /// Checks if there is an existing name under the given tenant Id.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="name">The name of the organisation.</param>
        /// <param name="organisationId">The Id of the existing organisation, if any.</param>
        /// <returns>Boolean value whether it has a matching value in the repository.</returns>
        bool IsNameInUse(Guid tenantId, string name, Guid? organisationId);

        /// <summary>
        /// Gets the specific organisation read model by id.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The organisation read model that you searched for.</returns>
        IOrganisationReadModelWithRelatedEntities? GetOrganisationWithRelatedEntities(
            Guid tenantId, Guid organisationId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets the specific organisation read model by alias.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationAlias">The Id of the organisation.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The organisation read model that you searched for.</returns>
        IOrganisationReadModelWithRelatedEntities? GetOrganisationWithRelatedEntities(
            Guid tenantId, string organisationAlias, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Method for creating IQueryable method that retrieve organisations and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for organisations.</returns>
        IQueryable<OrganisationReadModelWithRelatedEntities> CreateQueryForOrganisationDetailsWithRelatedEntities(
            Guid tenantId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Retrieves organisation ids converted into string by tenant and paging.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="skip">The current page.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>List of organisation IDs in string format.</returns>
        List<Guid> GetOrganisationIdsByTenant(Guid tenantId, int skip, int pageSize);

        /// <summary>
        /// Gets all organisations within the tenant, matching a given filter.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>All the organisations owned by a given user and matching a given filter.</returns>
        IEnumerable<OrganisationReadModelWithRelatedEntities> GetOrganisationsWithRelatedEntities(
            Guid tenantId, OrganisationReadModelFilters filters, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Get the linked organisation based on the tenant, authentication method and organisation.
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="authenticationMethodId">Authentication Method ID</param>
        /// <param name="organisationExternalId">Organisation External ID</param>
        /// <param name="excludeOrganisationId">Specific organisation ID to be excluded from the query.</param>
        /// <returns></returns>
        OrganisationReadModel? GetLinkedOrganisation(Guid tenantId, Guid authenticationMethodId, string organisationExternalId, Guid? excludeOrganisationId = null);

        Task<IEnumerable<Guid>> GetIdsOfDescendantOrganisationsOfOrganisation(Guid tenantId, Guid organisationId);
    }
}
