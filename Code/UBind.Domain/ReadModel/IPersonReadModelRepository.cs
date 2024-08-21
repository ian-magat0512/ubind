// <copyright file="IPersonReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// For retrieving persisted person model.
    /// </summary>
    public interface IPersonReadModelRepository
    {
        /// <summary>
        /// Gets the person record associated with customer by email.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant the request is for.</param>
        /// <param name="organisationId">The Id of the organisation the request is for.</param>
        /// <param name="email">The email of the person to search for.</param>
        /// <returns>The person, if found, otherwise null.</returns>
        PersonReadModel? GetPersonAssociatedWithCustomerByEmail(Guid tenantId, Guid organisationId, string email);

        /// <summary>
        /// Gets the list of person record associated with customer by email.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant the request is for.</param>
        /// <param name="email">The email of the person to search for.</param>
        /// <param name="environment">The environment of the customer related to person.</param>
        /// <returns>The person, if found, otherwise null.</returns>
        IEnumerable<PersonReadModel> GetAllPersonAssociatedWithCustomerByEmail(Guid tenantId, string email, DeploymentEnvironment environment);

        /// <summary>
        /// Gets the list of persons record associated with customer by email and organisation Id.
        /// </summary>
        IEnumerable<PersonReadModel> GetAllPersonsAssociatedWithCustomerByEmailAndOrganisationId(Guid tenantId, Guid organisationId, string email, DeploymentEnvironment environment);

        /// <summary>
        /// Gets the person record associated with user by email and organisation Id.
        /// </summary>
        PersonReadModel? GetPersonAssociatedWithUserByEmailAndOrganisationId(Guid tenantId, Guid organisation, string email, DeploymentEnvironment environment);

        /// <summary>
        /// Retrieves a collection of persons which have a user account matching the
        /// specified customerId and organisation in a given deployment environment.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant.</param>
        /// <param name="organisationId">The unique identifier of the organization to filter by.</param>
        /// <param name="customerId">The Id of the customer to search for.</param>
        /// <param name="environment">The deployment environment in which to perform the search.</param>
        /// <returns>An IEnumerable of PersonReadModel objects matching the specified criteria.</returns>
        IEnumerable<PersonReadModel> GetPersonsWhichHaveAUserAccountInOrganisationByCustomerId(
            Guid tenantId, Guid organisationId, Guid customerId, DeploymentEnvironment environment);

        /// <summary>
        /// Get the person who has user account matching the specified email address and organisation with a given environment.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="organisationId">the Id of the organisation to filter.</param>
        /// <param name="email">The email address of the person to check.</param>
        /// <param name="environment">The environment where the person user belong.</param>
        /// <returns>Return a PersonReadModel object if found.</returns>
        PersonReadModel? GetPersonWhoHasAUserAccountInOrganisationByEmail(
            Guid tenantId, Guid organisationId, string email, DeploymentEnvironment environment);

        /// <summary>
        /// Gets the person record by Id.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant the request is for.</param>
        /// <param name="personId">The Id of the person to search for.</param>
        /// <returns>The person, if found, otherwise null.</returns>
        PersonReadModel? GetPersonById(Guid tenantId, Guid personId);

        /// <summary>
        /// Gets the person record by person Id and organisation Id.
        /// </summary>
        PersonReadModel? GetPersonByIdAndOrganisationId(Guid tenantId, Guid organisationId, Guid personId);

        /// <summary>
        /// Gets a Person by persons's ID without tenantId specifically used for migrations.
        /// Note: this should only be used in a migration,
        /// this doesnt have tenantId parameter because its assumed that the person doesnt have tenantId.. yet.
        /// </summary>
        /// <param name="id">The ID of the person.</param>
        /// <returns>The Person, if found, otherwise null.</returns>
        PersonReadModel? GetByIdWithoutTenantIdForMigrations(Guid id);

        /// <summary>
        /// Gets all persons within a tenant, matching a given filter.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant the request is for.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <returns>All the persons matching a given filter.</returns>
        IEnumerable<IPersonReadModelSummary?> GetPersonsMatchingFilters(Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Gets a Person with includes its additional properties by persons's ID.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the record.</param>
        /// <param name="id">The ID of the person.</param>
        /// <returns>The Person, if found, otherwise null.</returns>
        IPersonReadModelSummary? GetPersonSummaryById(Guid tenantId, Guid id);

        /// <summary>
        /// Gets an enumerable of persons with user Id using the customer Id.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="customerId">The ID of the customer the request is for.</param>
        /// <returns>All the persons associated to the specified customer ID.</returns>
        IEnumerable<IPersonReadModelSummary?> GetPersonsAssociatedWithUsersByCustomerId(Guid tenantId, Guid customerId);

        /// <summary>
        /// Gets an enumerable of persons by customer ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="customerId">The ID of the customer the request is for.</param>
        /// <param name="includeAllProperties">Retrieve list of persons including its properties
        /// like email, phone number and socmed.</param>
        /// <returns>All the persons associated to the specified customer ID.</returns>
        IEnumerable<IPersonReadModelSummary?> GetPersonsByCustomerId(Guid tenantId, Guid customerId, bool includeAllProperties = false);

        /// <summary>
        /// Gets all persons by user ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="userId">The ID of the user the request is for.</param>
        /// <returns>All the persons associated to the specified user ID.</returns>
        IEnumerable<PersonReadModel> GetPersonsByUserId(Guid tenantId, Guid userId);

        /// <summary>
        /// Gets a person with related entities by ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the person is under.</param>
        /// <param name="id">The ID of the person.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The person, if found, otherwise null.</returns>
        IPersonReadModelWithRelatedEntities? GetPersonWithRelatedEntities(
            Guid tenantId, Guid id, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets a person with related entities required for owner assignment by user ID.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="userId"></param>
        /// <returns>The person, if found, otherwise null.</returns>
        PersonReadModel? GetPersonDetailForOwnerAssignmentByUserId(Guid tenantId, Guid userId);

        /// <summary>
        /// Gets the primary person detail by customer Id.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="customerId"></param>
        /// <returns>The person, if found, otherwise null.</returns>
        PersonReadModel? GetPersonAssociatedWithPrimaryPersonByCustmerId(Guid tenantId, Guid customerId);
    }
}
