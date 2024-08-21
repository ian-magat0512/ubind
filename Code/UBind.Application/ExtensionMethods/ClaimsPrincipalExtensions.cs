// <copyright file="ClaimsPrincipalExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Infrastructure;
    using UBind.Domain;
    using UBind.Domain.Authentication;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;

    /// <summary>
    /// Extension methods for <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        private static Dictionary<string, Permission> permissionMapping = Enum.GetValues(typeof(Permission))
            .Cast<Permission>()
            .ToDictionary(p => p.ToString().ToUpperFirstChar(), p => p);

        /// <summary>
        /// Checks if the user is a valid authenticated user by checking if the identifier is available.
        /// </summary>
        /// <param name="user">An instance of <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>Returns true if user is authenticated, otherwise false.</returns>
        public static bool IsAuthenticated(this ClaimsPrincipal user)
        {
            var claimValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claimValue != null;
        }

        /// <summary>
        /// Gets the user ID from the NameIdentifier claim.
        /// </summary>
        /// <param name="user">An instance of <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The user ID from the NameIdentifier claim.</returns>
        public static Guid? GetId(this ClaimsPrincipal user)
        {
            var claimValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? id = null;
            if (claimValue != null)
            {
                id = new Guid(claimValue);
            }

            return id;
        }

        /// <summary>
        /// Gets the user's UserType from the role claim.
        /// </summary>
        /// <param name="user">An instance of <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The UserType.</returns>
        public static UserType GetUserType(this ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role);
            if (roleClaim == null)
            {
                return UserType.Customer;
            }

            if (!Enum.TryParse<UserType>(roleClaim.Value, out UserType userType))
            {
                throw new ErrorException(Errors.Account.StaleAuthenticationData(
                    "UserType was not one allowed user types."));
            }

            return userType;
        }

        public static bool IsCustomer(this ClaimsPrincipal user)
        {
            return user == null || user?.GetUserType() == UserType.Customer || user?.GetUserType() == null;
        }

        /// <summary>
        /// Gets the user's tenant ID from the Tenant claim.
        /// </summary>
        /// <param name="user">An instance of <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The tenant ID from the Tenant claim.</returns>
        public static Guid GetTenantId(this ClaimsPrincipal user)
        {
            var tenantIdClaim = user.FindFirst(ClaimNames.TenantId);
            if (tenantIdClaim == null)
            {
                throw new ErrorException(Errors.Account.StaleAuthenticationData());
            }

            Guid.TryParse(tenantIdClaim.Value, out Guid id);
            return id;
        }

        /// <summary>
        /// Gets the user's tenant guid ID from the Tenant claim.
        /// </summary>
        /// <param name="user">An instance of <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The tenant ID from the Tenant claim.</returns>
        public static Guid? GetTenantIdOrNull(this ClaimsPrincipal user)
        {
            var tenantIdClaim = user.FindFirst(ClaimNames.TenantId);
            if (tenantIdClaim == null)
            {
                return null;
            }

            Guid.TryParse(tenantIdClaim.Value, out Guid id);
            return id;
        }

        /// <summary>
        /// Gets the user's organisation ID from the Organisation claim.
        /// </summary>
        /// <param name="user">An instance of <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The organisation ID from the Organisation claim.</returns>
        public static Guid GetOrganisationId(this ClaimsPrincipal user)
        {
            var claimValue = user.FindFirst(ClaimNames.OrganisationId)?.Value;
            return claimValue != null ? new Guid(claimValue) : default;
        }

        /// <summary>
        /// Checks if the user is from the master tenancy.
        /// </summary>
        public static bool IsMasterUser(this ClaimsPrincipal user)
        {
            return user.GetTenantId() == Tenant.MasterTenantId;
        }

        /// <summary>
        /// Gets the user's SessionId from the claim.
        /// </summary>
        /// <param name="user">An instance of <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>retrieves sessionId.</returns>
        public static Guid? SessionId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst("SessionId");
            if (claim != null)
            {
                return Guid.Parse(claim.Value);
            }

            return null;
        }

        /// <summary>
        /// Gets the user's customer ID from the CustomerId claim.
        /// </summary>
        /// <param name="user">An instance of <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The customer ID from the customer claim.</returns>
        public static Guid? GetCustomerId(this ClaimsPrincipal user)
        {
            var claimValue = user.FindFirst(ClaimNames.CustomerId)?.Value;
            return claimValue != null ? (Guid?)Guid.Parse(claimValue) : null;
        }

        /// <summary>
        /// Gets the user's permissions.
        /// </summary>
        /// <param name="user">An instance of <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The customer ID from the customer claim.</returns>
        public static List<Permission> GetPermissions(this ClaimsPrincipal user)
        {
            var permissionsClaim = user.Claims.Where(c => c.Type == ClaimNames.Permissions);

            // For backwards compatibility, handle old permissions array stored as string, this code
            // will be removed after all token was renewed or after a month. UB-9826 created for removal.
            if (permissionsClaim.Count() == 1)
            {
                var arrayText = permissionsClaim.First().Value;
                if (arrayText.StartsWith("[") && arrayText.EndsWith("]"))
                {
                    var permissionsJArray = JArray.Parse(arrayText);
                    return permissionsJArray.Select(p => p.ToString().ToUpperFirstChar().ToEnumOrThrow<Permission>()).ToList();
                }
            }

            var permissions = permissionsClaim
                .Select(c =>
                    {
                        string value = c.Value.ToUpperFirstChar();
                        return permissionMapping.ContainsKey(value) ? permissionMapping[value] : (Permission?)null;
                    })
                .Where(p => p.HasValue)
                .Select(p => p.Value)
                .ToList();

            return permissions;
        }

        /// <summary>
        /// Gets the user's authentication data.
        /// </summary>
        /// <param name="user">An instance of <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The combined claims for the user.</returns>
        public static IUserAuthenticationData GetAuthenticationData(this ClaimsPrincipal user)
        {
            if (user == null || user.GetId() == null)
            {
                // if no user id, then theres no authentication data to retrieve.
                return null;
            }

            return new UserAuthenticationData(
                user.GetTenantId(), user.GetOrganisationId(), user.GetUserType(), user.GetId().Value, user.GetCustomerId(), user.GetPermissions());
        }

        /// <summary>
        /// Gets the user's role from the role claim.
        /// </summary>
        /// <param name="user">An instance of <see cref="ClaimsPrincipal"/>.</param>
        /// <param name="userType">The userType to check.</param>
        /// <returns>True if the user has that user Type, otherwise false.</returns>
        public static bool HasUserType(this ClaimsPrincipal user, UserType userType)
        {
            return user.GetUserType() == userType;
        }
    }
}
