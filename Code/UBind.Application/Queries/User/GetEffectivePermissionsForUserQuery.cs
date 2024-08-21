// <copyright file="GetEffectivePermissionsForUserQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System.Collections.Generic;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;

    /// <summary>
    /// Gets the effective permissions of a user.
    /// The effective permissions are the aggregate permissions from all of the assigned roles of the user,
    /// minus any permissions which may not be enabled for that user's organisation.
    /// </summary>
    public class GetEffectivePermissionsForUserQuery : IQuery<List<Permission>>
    {
        public GetEffectivePermissionsForUserQuery(Guid tenantId, Guid userId)
        {
            this.TenantId = tenantId;
            this.UserId = userId;
        }

        public Guid TenantId { get; }

        public Guid UserId { get; }
    }
}
