// <copyright file="IUserSessionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services;

using System.Security.Claims;
using UBind.Domain.Permissions;
using UBind.Domain.ReadModel.User;
using UBind.Domain.Redis;

/// <summary>
/// Service for accessing the current userSessionModel and caching it for the duration of the request.
/// This must be setup as a Scoped service in the DI container so that there is a new instance per request,
/// because the permissions are cached in this instance.
/// </summary>
public interface IUserSessionService
{
    Task<UserSessionModel> Create(UserReadModel user, IEnumerable<Permission> effectivePermissions);

    Task<UserSessionModel?> Get(ClaimsPrincipal performingUser);

    Task Delete(ClaimsPrincipal performingUser);

    /// <summary>
    /// Deletes all sessions for a given user.
    /// </summary>
    Task DeleteAllSessionsForUser(Guid tenantId, Guid userId);

    /// <summary>
    /// Delets all sessions for users in the given tenancy.
    /// </summary>
    Task DeleteAll(Guid tenantId);

    Task<bool> IsSessionIdle();

    Task<bool> IsSessionExpired();

    Task<bool> HasPasswordExpired();

    void ExtendIdleTimeout();
}
