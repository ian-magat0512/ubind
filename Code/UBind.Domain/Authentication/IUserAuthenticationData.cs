// <copyright file="IUserAuthenticationData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System;
    using UBind.Domain.Permissions;

    /// <summary>
    /// Interface for exposing user claims (taken from access token).
    /// </summary>
    public interface IUserAuthenticationData
    {
        /// <summary>
        /// Gets the ID of the user.
        /// </summary>
        Guid UserId { get; }

        /// <summary>
        /// Gets the ID of the tenant the user belongs to.
        /// </summary>
        Guid TenantId { get; }

        /// <summary>
        /// Gets the Id of the organisation the user belongs to.
        /// </summary>
        Guid OrganisationId { get; }

        /// <summary>
        /// Gets the user's user type (e.g. ClientAdmin, Customer, Master etc).
        /// </summary>
        UserType UserType { get; }

        /// <summary>
        /// Gets the customer ID for the user, if the user is a customer user, otherwise null.
        /// </summary>
        Guid? CustomerId { get; }

        /// <summary>
        /// Checks if the user has the permission.
        /// </summary>
        /// <param name="permission">The permission to check.</param>
        /// <returns>a boolean value.</returns>
        bool HasPermission(Permission permission);
    }
}
