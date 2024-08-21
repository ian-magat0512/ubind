// <copyright file="IEntityAuthorisationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Authorisation
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using UBind.Domain.ReadModel;

    public interface IEntityAuthorisationService
    {
        Task ApplyRestrictionsToFilters(ClaimsPrincipal performingUser, EntityListFilters filters);

        /// <summary>
        /// Throws an exception if the user cannot view any entities of this type.
        /// </summary>
        Task ThrowIfUserCannotViewAny(ClaimsPrincipal performingUser);

        /// <summary>
        /// Throws an exception if the user cannot view the specified entity.
        /// </summary>
        Task ThrowIfUserCannotView(Guid tenantId, Guid entityId, ClaimsPrincipal performingUser);

        /// <summary>
        /// Throws an exception if the user cannot create an entity in the given organisation.
        /// </summary>
        Task ThrowIfUserCannotCreate(Guid tenantId, Guid organisationId, ClaimsPrincipal performingUser);

        /// <summary>
        /// Throws an exception if the user cannot modify the entity.
        /// </summary>
        Task ThrowIfUserCannotModify(Guid tenantId, Guid entityId, ClaimsPrincipal performingUser);

        /// <summary>
        /// Throws an exception if the user cannot delete the entity.
        /// </summary>
        Task ThrowIfUserCannotDelete(Guid tenantId, Guid entityId, ClaimsPrincipal performingUser);
    }
}
