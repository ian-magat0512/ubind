// <copyright file="UserAuthenticationData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Humanizer;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// For representing authenticated user claims.
    /// </summary>
    public class UserAuthenticationData : IUserAuthenticationData
    {
        private List<Permission> permissions;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthenticationData"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the user belongs to.</param>
        /// <param name="organisationId">The Id of the organisation the user belongs to.</param>
        /// <param name="userType">The role the user is in.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="customerId">The ID of the customer the user represents, if a customer user, or default.</param>
        /// <param name="permissions">The permissions of the user.</param>
        public UserAuthenticationData(
            Guid tenantId,
            Guid organisationId,
            UserType userType,
            Guid userId,
            Guid? customerId,
            List<Permission> permissions = null)
        {
            this.UserId = userId;
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.UserType = (UserType)Enum.Parse(typeof(UserType), userType.Humanize());
            this.CustomerId = customerId;
            this.permissions = permissions == null ? new List<Permission>() : permissions;
        }

        public UserAuthenticationData(IUserReadModelSummary userReadModelSummary)
        {
            this.UserId = userReadModelSummary.Id;
            this.TenantId = userReadModelSummary.TenantId;
            this.OrganisationId = userReadModelSummary.OrganisationId;
            this.UserType = (UserType)Enum.Parse(typeof(UserType), userReadModelSummary.UserType.Humanize());
            this.CustomerId = userReadModelSummary.CustomerId;
            this.permissions = userReadModelSummary.GetPermissions().ToList();
        }

        /// <inheritdoc/>
        public Guid UserId { get; }

        /// <inheritdoc/>
        public Guid TenantId { get; }

        /// <inheritdoc/>
        public Guid OrganisationId { get; }

        /// <inheritdoc/>
        public UserType UserType { get; }

        /// <inheritdoc/>
        public Guid? CustomerId { get; }

        /// <inheritdoc/>
        public bool HasPermission(Permission permission)
        {
            return this.permissions.Any(x => x == permission);
        }
    }
}
