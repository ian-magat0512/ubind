// <copyright file="IUserSessionDeletionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    /// <summary>
    /// Service for deleting user session and emitting user related system event.
    /// </summary>
    public interface IUserSessionDeletionService
    {
        Task DeleteForUser(Guid tenantId, Guid userId);

        void DeleteByRoleId(Guid tenantId, Guid roleId, CancellationToken cancellationToken);

        Task DeleteByTenant(Guid tenantId, CancellationToken cancellationToken);

        void ExpireAllUserSessions(CancellationToken cancellationToken);

        void EnqueueExpireAllUserSessionsByPortalId(Guid tenantId, Guid portalId, CancellationToken cancellationToken);
    }
}
