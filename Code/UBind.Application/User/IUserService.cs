// <copyright file="IUserService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.User
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Entities;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Represents the user service.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the user belongs to.</param>
        /// <param name="userId">The ID of the user to return.</param>
        /// <returns>The user with matching ID.</returns>
        IUserReadModelSummary GetUser(Guid tenantId, Guid userId);

        /// <summary>
        /// Gets a user's detail by ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the user belongs to.</param>
        /// <param name="userId">The ID of the user to return.</param>
        /// <returns>The user with matching ID.</returns>
        UserReadModelDetail GetUserDetail(Guid tenantId, Guid userId);

        /// <summary>
        /// Method that creates a new entry for the specified user.
        /// </summary>
        /// <param name="model">User signup model.</param>
        /// <param name="authenticationMethodId">The authentication method ID (for the external identity provider).</param>
        /// <param name="externalUserId">The user's ID in the external identity provider.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<UserAggregate> CreateUser(
            UserSignupModel model,
            Guid? authenticationMethodId = null,
            string? externalUserId = null);

        Task<List<Permission>> GetEffectivePermissions(UserReadModel user, OrganisationReadModel organisation);

        /// <summary>
        /// Method that creates a new entry for the specified user with association with a quote customer.
        /// </summary>
        /// <param name="model">User signup model.</param>
        /// <param name="quoteId">the associated customer.</param>
        /// <param name="productId">the product id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<UserModel> CreateUserForQuoteCustomer(
            UserSignupModel model, Guid quoteId, Guid? productId = null);

        /// <summary>
        /// Method that creates a new entry for the specified user with association with a person.
        /// </summary>
        /// <param name="personAggregate">The person aggregate.</param>
        /// <param name="customerAggregate">The customer aggregate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<UserAggregate> CreateUserForPerson(
            PersonAggregate personAggregate, CustomerAggregate? customerAggregate = null);

        Task ActivateUser(UserAggregate userAggregate, string? clearTextPassword = null);

        /// <summary>
        /// Creates default ubind and client admin accounts for a new organisation.
        /// </summary>
        /// <remarks>
        /// Soon when user for organisation, we will get rid of tenant entity.
        /// </remarks>
        /// <param name="tenant">The new tenant.</param>
        /// <param name="organisation">The new organisation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateDefaultUsersForOrganisationAsync(Domain.Tenant tenant, Organisation organisation);

        /// <summary>
        /// Method that updates a specified user.
        /// </summary>
        /// <param name="tenantId">The ID of the user's tenant.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="model">The user update model.</param>
        /// <param name="properties">The collection the contains additional property values from an edit form.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<UserAggregate> Update(
            Guid tenantId,
            Guid userId,
            UserUpdateModel model,
            List<AdditionalPropertyValueUpsertModel> properties = null);

        /// <summary>
        /// Deletes a user from the system.
        /// </summary>
        /// <param name="tenantId">The ID of the user's tenant.</param>
        /// <param name="userId">The unique identifier of the user to be deleted.</param>
        Task Delete(Guid tenantId, Guid userId);

        /// <summary>
        /// Method that add Role to specified user.
        /// </summary>
        /// <param name="tenantId">The ID of the user's tenant.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="roleId">The user role id.</param>
        /// <returns>An awaitable task.</returns>
        Task AddRoleToUser(Guid tenantId, Guid userId, Guid roleId);

        /// <summary>
        /// Method that remove Role to specified user.
        /// </summary>
        /// <param name="tenantId">The ID of the user's tenant.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="roleId">The user role id.</param>
        /// <returns>An awaitable task.</returns>
        Task RemoveUserRole(Guid tenantId, Guid userId, Guid roleId);

        /// <summary>
        /// Method that get the roles of a user.
        /// </summary>
        /// <param name="tenantId">The user's tenant id.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>The list of roles.</returns>
        List<Role> GetUserRoles(Guid tenantId, Guid userId);

        /// <summary>
        /// Gets the available roles of a user.
        /// </summary>
        /// <param name="tenantId">The user's tenant id.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>The list of available roles.</returns>
        List<Role> GetAvailableUserRoles(Guid tenantId, Guid userId);

        /// <summary>
        /// Processes the uploading of the user's profile picture by persisting the provided file
        /// and updating the value of the user's profile picture id.
        /// </summary>
        /// <param name="tenantId">The user's tenant id.</param>
        /// <param name="formFile">Profile picture file from the HTTP request.</param>
        /// <param name="user">The user read model.</param>
        /// <returns>The new id of the persisted profile picture.</returns>
        Task<Guid> SaveProfilePictureForUser(Guid tenantId, IFormFile formFile, IUserReadModelSummary user);

        /// <summary>
        /// Ensures that the client user has unique email address, and throws an exception
        /// if found to be otherwise.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="email">The email address of the user.</param>
        /// <param name="customerId">The customer Id.</param>
        void ThrowIfClientUserEmailAddressIsTaken(Guid tenantId, string email, Guid? customerId);

        /// <summary>
        /// Ensures that the customer user has unique email address, and throws an exception
        /// if found to be otherwise.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="email">The email address of the user.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        void ThrowIfCustomerUserEmailAddressIsTaken(Guid tenantId, string email, Guid organisationId);

        /// <summary>
        /// Creates a new user account for a person record and send Activation invitation.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="personId">The Id of the person to be associated with the new user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<UserAggregate> CreateCustomerUserForPersonAndSendActivationInvitation(
            Guid tenantId,
            Guid personId,
            DeploymentEnvironment environment);
    }
}
