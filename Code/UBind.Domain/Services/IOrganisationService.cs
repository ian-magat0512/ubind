// <copyright file="IOrganisationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.ReadModel;
    using OrganisationAggregate = UBind.Domain.Aggregates.Organisation.Organisation;

    /// <summary>
    /// The interface service for handling organisation-related functionality.
    /// </summary>
    public interface IOrganisationService
    {
        Task<OrganisationAggregate> CreateOrganisation(
            Guid tenantId,
            string? alias,
            string name,
            Guid? managingOrganisationId,
            List<AdditionalPropertyValueUpsertModel>? properties = null,
            List<LinkedIdentity>? linkedIdentities = null);

        /// <summary>
        /// Gets the organisation summary for the given tenant Id and organisation Id.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the summary is for.</param>
        /// <param name="organisationId">The Id of the organisation the summary is for.</param>
        /// <returns>The organisation read model summary.</returns>
        Task<IOrganisationReadModelSummary> GetOrganisationSummaryForTenantIdAndOrganisationId(
            Guid tenantId, Guid organisationId);

        /// <summary>
        /// Gets the organisation summary for the given tenant alias and organisation alias.
        /// </summary>
        /// <param name="tenantAlias">The alias of the tenant the summary is for.</param>
        /// <param name="organisationAlias">The alias of the organisation the summary is for.</param>
        /// <returns>The organisation read model summary.</returns>
        IOrganisationReadModelSummary GetOrganisationSummaryForTenantAliasAndOrganisationAlias(
            string tenantAlias, string organisationAlias);

        /// <summary>
        /// Checks if the organisation Id is the default organisation for the given tenancy.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant to inquire.</param>
        /// <param name="organisationId">The Id of the organisation that represents in GUID.</param>
        /// <returns>Evaluates either the organisation is the default for tenancy.</returns>
        Task<bool> IsOrganisationDefaultForTenant(Guid tenantId, Guid organisationId);

        /// <summary>
        /// Checks if the organisation alias is for the default organisation of a given tenancy.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant to inquire.</param>
        /// <param name="organisationAlias">The alias of the organisation that represents in GUID.</param>
        /// <returns>Evaluates either the organisation is the default for tenancy.</returns>
        Task<bool> IsOrganisationDefaultForTenant(Guid tenantId, string organisationAlias);

        /// <summary>
        /// Gets the default organisation for a given tenancy.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to inquire.</param>
        /// <returns>The organisation read model summary.</returns>
        IOrganisationReadModelSummary GetDefaultOrganisationForTenant(Guid tenantId);

        /// <summary>
        /// Gets the <see cref="Tenant"/> instance for a given organisation Id.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="organisationId">The Id of the organisation to search for.</param>
        /// <returns>Instance of <see cref="Tenant"/>.</returns>
        Tenant GetTenantFromOrganisationId(Guid tenantId, Guid organisationId);

        /// <summary>
        /// An operation that validates if the organisation belongs to tenant and is active and not deleted.
        /// </summary>
        /// <param name="tenantId">The tenant Id it belongs to.</param>
        /// <param name="organisationId">The organisation alias to check for.</param>
        Task ValidateOrganisationBelongsToTenantAndIsActive(Guid tenantId, Guid organisationId);

        void ValidateOrganisationIsActive(OrganisationReadModel organisation, Guid organisationId);

        /// <summary>
        /// An operation that gets the list of organisation aggregates from the given tenant Id.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="filters">Additional filters to be applied to the result set.</param>
        /// <returns>An enumerable collection of filtered organisation summaries.</returns>
        Task<IEnumerable<IOrganisationReadModelSummary>> ListOrganisationsForUser(
            Guid tenantId, OrganisationReadModelFilters filters);

        /// <summary>
        /// An operation that gets the active organisation record in the repository from the given tenant Id.
        /// Gets the active organisation by Id.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the organisation to check.</param>
        /// <param name="organisationId">The Id of the organisation to check.</param>
        /// <returns>The organisation summary that matches the given Id, otherwise, returns an exception.</returns>
        IOrganisationReadModelSummary GetActiveOrganisationById(Guid tenantId, Guid organisationId);

        /// <summary>
        /// An operation that gets the organisation record in the repository from the given tenant Id.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationAggregateId">The Id of the organisation aggregate.</param>
        /// <returns>The organisation summary that matches the given Id.</returns>
        Task<IOrganisationReadModelSummary> GetOrganisationById(
            Guid tenantId, Guid organisationAggregateId);

        /// <summary>
        /// An asynchronous operation that creates a new active non-default organisation entity to the given tenant.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the organisation.</param>
        /// <param name="alias">The alias of the organisation.</param>
        /// <param name="name">The name of the organisation.</param>
        /// <param name="properties">Set additional property values from web domain.</param>
        /// <returns>An asynchronous operation that contains the created summary of organisation.</returns>
        Task<IOrganisationReadModelSummary> CreateActiveNonDefaultAsync(
            Guid tenantId,
            string alias,
            string name,
            List<AdditionalPropertyValueUpsertModel> properties);

        /// <summary>
        /// An asynchronous operation that creates a new default organisation entity to the given tenant.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the organisation.</param>
        /// <param name="alias">The alias of the organisation.</param>
        /// <param name="name">The name of the organisation.</param>
        /// <returns>An asynchronous operation that contains the created summary of organisation.</returns>
        Task<IOrganisationReadModelSummary> CreateDefaultAsync(
            Guid tenantId, string alias, string name);

        /// <summary>
        /// An operation that updates the organisation aggregate details if exists.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="organisationId"></param>
        /// <param name="organisationName"></param>
        /// <param name="organisationAlias"></param>
        /// <param name="properties"></param>
        /// <param name="linkedIdentities"></param>
        /// <returns>An asynchronous operation that contains the organisation aggregate</returns>
        Task<OrganisationAggregate> UpdateOrganisation(
            Guid tenantId,
            Guid organisationId,
            string organisationName,
            string organisationAlias,
            List<AdditionalPropertyValueUpsertModel>? properties = null,
            List<LinkedIdentity>? linkedIdentities = null);

        /// <summary>
        /// An operation that sets the organisation status as active.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the organisation.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <returns>An asynchronous operation that contains the updated summary of organisation.</returns>
        Task<IOrganisationReadModelSummary> Activate(
            Guid tenantId, Guid organisationId);

        /// <summary>
        /// An operation that sets the organisation status as disabled.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the organisation.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <returns>An asynchronous operation that contains the updated summary of organisation.</returns>
        Task<IOrganisationReadModelSummary> Disable(
            Guid tenantId, Guid organisationId);

        /// <summary>
        /// An operation that marks the organisation as deleted.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the organisation.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <returns>An asynchronous operation that contains the updated summary of organisation.</returns>
        Task<IOrganisationReadModelSummary> MarkAsDeleted(
            Guid tenantId, Guid organisationId);

        void ThrowIfOrganisationAliasIsNull(string alias);

        Task ThrowIfHasDuplicateName(Guid tenantId, string name, Guid? organisationId = null);

        Task ThrowIfHasDuplicateAlias(Guid tenantId, string alias, Guid? organisationId = null);

        Task ThrowIfHasDuplicateLinkedIdentity(Guid tenantId, Guid authenticationMethodId, string organisationExternalId, Guid? excludedOrganisationId = null);
    }
}
