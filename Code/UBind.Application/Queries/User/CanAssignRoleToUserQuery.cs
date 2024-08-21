// <copyright file="CanAssignRoleToUserQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Query to check if a role can be assigned to a user.
    /// </summary>
    public class CanAssignRoleToUserQuery : IQuery<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CanAssignRoleToUserQuery"/> class.
        /// </summary>
        /// <param name="performingUserTenantId">The tenant Id of the user who would do the assigning.
        /// We need this to check if they are an orgnisation admin or a tenant admin, because if they are
        /// then we know we can also assign the organisation admin or tenant admin role.</param>
        /// <param name="performingUserId">The user Id of the user who would do the assigning.
        /// We need this to check if they are an orgnisation admin or a tenant admin, because if they are
        /// then we know we can also assign the organisation admin or tenant admin role.</param>
        /// <param name="role">The role.</param>
        /// <param name="user">The user.</param>
        public CanAssignRoleToUserQuery(
            Guid performingUserTenantId,
            Guid performingUserId,
            Role role,
            IUserReadModelSummary user)
        {
            this.PerformingUserTenantId = performingUserTenantId;
            this.PerformingUserId = performingUserId;
            this.User = user;
            this.Role = role;
        }

        /// <summary>
        /// Gets the filtering options.
        /// </summary>
        public Role Role { get; }

        /// <summary>
        /// Gets the filtering options.
        /// </summary>
        public IUserReadModelSummary User { get; }

        /// <summary>
        /// Gets the user Id.
        /// </summary>
        public Guid PerformingUserId { get; }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        public Guid PerformingUserTenantId { get; }
    }
}
