// <copyright file="IUserLoginEmailRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using UBind.Domain.Aggregates.User;

    /// <summary>
    /// Defines the interface for managing user login emails in the uBind application.
    /// </summary>
    public interface IUserLoginEmailRepository
    {
        /// <summary>
        /// Retrieves a user's login email based on their tenant ID, and email address.
        /// If there a multiple, it will return the first that matches the organization ID.
        /// </summary>
        /// <param name="tenantId">The ID of the user's tenant.</param>
        /// <param name="organisationId">The ID of the user's organization within the tenant.</param>
        /// <param name="email">The email address associated with the user's login.</param>
        /// <returns>The user's login email or null if not found.</returns>
        UserLoginEmail? GetUserLoginByEmail(Guid tenantId, Guid organisationId, string email);

        List<UserLoginEmail> GetUserLoginsByEmail(Guid tenantId, string email);

        /// <summary>
        /// Adds a user login email to the repository.
        /// </summary>
        /// <param name="userLoginEmail">The user login email to add.</param>
        void Add(UserLoginEmail userLoginEmail);

        UserLoginEmail GetUserLoginEmailByEmail(Guid tenantId, Guid organisationId, PortalUserType portalUserType, string loginEmail);

        UserLoginEmail GetUserLoginEmailById(Guid tenantId, Guid organisationId, PortalUserType portalUserType, Guid id);
    }
}
