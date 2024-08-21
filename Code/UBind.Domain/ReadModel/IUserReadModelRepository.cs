// <copyright file="IUserReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// For retrieving persisted quote details.
    /// </summary>
    public interface IUserReadModelRepository : IRepository
    {
        /// <summary>
        /// List all users in a given tenant and environment that match a filter.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="filters">The filters to appply.</param>
        /// <returns>The list of matching users.</returns>
        List<UserReadModel> GetUsers(Guid tenantId, UserReadModelFilters filters);

        /// <summary>
        /// Gets users with at least one of the given roles.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationId">The organisation ID.</param>
        /// <param name="roleNames">The role names.</param>
        /// <returns>A list of users.</returns>
        IEnumerable<UserReadModel> GetUsersWithRoles(
            Guid tenantId,
            Guid organisationId,
            string[] roleNames);

        IEnumerable<UserReadModel> GetUserWithAnyOfThePermissions(
            Guid tenantId,
            Guid organisationId,
            Permission[] permissions);

        /// <summary>
        /// Get a user by ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>THe user if found, otherwise null.</returns>
        UserReadModel GetUser(Guid tenantId, Guid userId);

        /// <summary>
        /// Get a user with roles by user ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>THe user if found, otherwise null.</returns>
        UserReadModel GetUserWithRoles(Guid tenantId, Guid userId);

        /// <summary>
        /// Get a user details by ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>THe user if found, otherwise null.</returns>
        UserReadModelDetail GetUserDetail(Guid tenantId, Guid userId);

        /// <summary>
        /// Get list of user by matching email with exact match and email with plus sign.
        /// This no tenantId parameter because we need to get the user by email only if exist.
        /// </summary>
        /// <param name="emailAddress">The user email address.</param>
        /// <returns>The list of user matching user with email address.</returns>
        IEnumerable<UserReadModel> GetUsersMatchingEmailAddressIncludingPlusAddressingForAllTenants(
            string emailAddress);

        /// <summary>
        /// Get a user by tenantId and by matching email with exact match and email with plus sign.
        /// </summary>
        /// <param name="tenantId">The user tenant Id.</param>
        /// <param name="emailAddress">The user email address.</param>
        /// <returns>The list of user matching user with email address.</returns>
        IEnumerable<UserReadModel> GetUsersMatchingEmailAddressIncludingPlusAddressing(
            Guid tenantId,
            string emailAddress);

        /// <summary>
        /// Gets the users with an exactly matching email address.
        /// </summary>
        /// <returns></returns>
        List<UserReadModel> GetByEmailAddress(Guid tenantId, string emailAddress);

        /// <summary>
        /// Get user with related entities by id.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the user is for.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The user if found, otherwise null.</returns>
        IUserReadModelWithRelatedEntities GetUserWithRelatedEntities(
            Guid tenantId, Guid userId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Get user with related entities by email.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the user is from.</param>
        /// <param name="email">The user email address.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The user if found, otherwise null.</returns>
        IUserReadModelWithRelatedEntities GetUserWithRelatedEntities(
            Guid tenantId, string email, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets the activated user by email and tenant Guid.
        /// </summary>
        /// <param name="tenantId">The Guid of the tenant.</param>
        /// <param name="email">The email address of the user.</param>
        /// <returns>The user read model.</returns>
        UserReadModel GetInvitedOrActivatedUserByEmailAndTenantId(Guid tenantId, string email);

        /// <summary>
        /// Gets the activated client user by email and tenant Guid.
        /// </summary>
        /// <param name="tenantId">The Guid of the tenant.</param>
        /// <param name="email">The email address of the user.</param>
        /// <returns>The user read model.</returns>
        UserReadModel GetInvitedClientUserByEmailAndTenantId(Guid tenantId, string email);

        /// <summary>
        /// Gets the activated user by email and organisation Guid.
        /// </summary>
        /// <param name="tenantId">The Guid of the tenant.</param>
        /// <param name="email">The email address of the user.</param>
        /// <param name="organisationId">The Guid of the organisation.</param>
        /// <returns>The user read model.</returns>
        UserReadModel GetInvitedUserByEmailTenantIdAndOrganisationId(Guid tenantId, string email, Guid organisationId);

        /// <summary>
        /// Gets the user model by Person Id.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant represented as <see cref="Guid"/>.</param>
        /// <param name="personId">The Id of the person represented as <see cref="Guid"/>.</param>
        /// <returns>The user if found, otherwise null.</returns>
        UserReadModel GetUserByPersonId(Guid tenantId, Guid personId);

        /// <summary>
        /// Gets all users person IDs.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>Dictionary of user ID and person ID.</returns>
        IDictionary<Guid, Guid> GetAllExistingUsersPersonIds(Guid tenantId);

        /// <summary>
        /// Method for creating IQueryable method that retrieve users and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for users.</returns>
        IQueryable<UserReadModelWithRelatedEntities> CreateQueryForUserDetailsWithRelatedEntities(
            Guid tenantId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Get all users in a given tenant that match a filter.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The list of all users as queryable.</returns>
        IQueryable<UserReadModel> GetAllUsersAsQueryable(Guid tenantId);

        /// <summary>
        /// Get a list of user ids.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="skip">Current page number.</param>
        /// <param name="pageSize">The max count per page.</param>
        /// <param name="organisationId">Organisation ID. The value will be supplied if the expected result is
        /// specifically for one organisation. Since that tenant can have multiple organisations.</param>
        /// <returns>List of IDs.</returns>
        List<Guid> GetAllUserIdsBy(Guid tenantId, int skip, int pageSize, Guid? organisationId);

        /// <summary>
        /// Retrieves all the users that is using the specific role.
        /// </summary>
        IEnumerable<UserReadModel> GetAllUsersByRoleId(Guid tenantId, Guid roleId);

        /// <summary>
        /// Get a list of users by tenant and organisation.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="organisationId">Organisation ID.</param>
        /// <returns>List of users.</returns>
        IQueryable<UserReadModel> GetAllUsersByOrganisation(Guid tenantId, Guid organisationId);

        /// <summary>
        /// Save changes.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Gets all users within the tenant, matching a given filter.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>All the users matching a given filter.</returns>
        IEnumerable<UserReadModelWithRelatedEntities> GetUsersWithRelatedEntities(
            Guid tenantId, UserReadModelFilters filters, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets the user account that's linked with the given authentication method.
        /// </summary>
        UserReadModel? GetLinkedUser(Guid tenantId, Guid authenticationMethodId, string userExternalId);

        /// <summary>
        /// Get a list of users by tenant and portal.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="portalId">Portal ID.</param>
        /// <returns>List of users.</returns>
        IQueryable<UserReadModel> GetAllUsersByPortal(Guid tenantId, Guid portalId);

        /// <summary>
        /// Get a list of user IDs by tenant and portal.
        /// </summary>
        Task<List<Guid>> GetAllUserIdsByPortal(Guid tenantId, Guid portalId);
    }
}
