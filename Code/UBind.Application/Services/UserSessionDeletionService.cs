// <copyright file="UserSessionDeletionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using Hangfire;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Events;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Repositories.Redis;

    public class UserSessionDeletionService : IUserSessionDeletionService
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IUserSessionRepository userSessionRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IUserSystemEventEmitter userSystemEventEmitter;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ILogger<UserSessionDeletionService> logger;
        private readonly IClock clock;

        public UserSessionDeletionService(
            IUserSessionRepository userSessionRepository,
            IUserReadModelRepository userReadModelRepository,
            IUserSystemEventEmitter userSystemEventEmitter,
            IBackgroundJobClient backgroundJobClient,
            ITenantRepository tenantRepository,
            ILogger<UserSessionDeletionService> logger,
            IClock clock,
            IHttpContextPropertiesResolver httpContextPropertiesResolver)
        {
            this.userSessionRepository = userSessionRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.userSystemEventEmitter = userSystemEventEmitter;
            this.backgroundJobClient = backgroundJobClient;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.tenantRepository = tenantRepository;
            this.logger = logger;
        }

        public async Task DeleteForUser(Guid tenantId, Guid userId)
        {
            var hasSessionDeletedForUser = await this.userSessionRepository.DeleteAllSessionsForUser(tenantId, userId);
            if (hasSessionDeletedForUser)
            {
                await this.Emit(
                    tenantId,
                    userId,
                    this.clock.GetCurrentInstant(),
                    this.httpContextPropertiesResolver.PerformingUserId);
            }
        }

        public async Task DeleteByTenant(Guid tenantId, CancellationToken cancellationToken)
        {
            var idsOfUsersWithDeletedSession = await this.userSessionRepository.DeleteAll(tenantId);
            if (idsOfUsersWithDeletedSession.Any())
            {
                this.backgroundJobClient.Enqueue(
                    () => this.EmitSessionInvalidatedForUsers(tenantId, idsOfUsersWithDeletedSession, this.httpContextPropertiesResolver.PerformingUserId, cancellationToken));
            }
        }

        public void DeleteByRoleId(Guid tenantId, Guid roleId, CancellationToken cancellationToken)
        {
            this.backgroundJobClient.Enqueue(
                () => this.DeleteAllUserSessionsByRoleId(tenantId, roleId, cancellationToken));
        }

        public async Task DeleteAllUserSessionsByRoleId(Guid tenantId, Guid roleId, CancellationToken cancellationToken)
        {
            var idsOfUsersWithDeletedSession = new List<Guid>();
            var usersByRoleId = this.userReadModelRepository.GetAllUsersByRoleId(tenantId, roleId);
            foreach (var user in usersByRoleId)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var hasSessionDeletedForUser = await this.userSessionRepository.DeleteAllSessionsForUser(tenantId, user.Id);
                if (hasSessionDeletedForUser)
                {
                    idsOfUsersWithDeletedSession.Add(user.Id);
                }
            }

            await this.EmitSessionInvalidatedForUsers(tenantId, idsOfUsersWithDeletedSession, this.httpContextPropertiesResolver.PerformingUserId, cancellationToken);
        }

        public void ExpireAllUserSessions(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenants = this.tenantRepository.GetTenants(includeMasterTenant: false);
            foreach (var tenant in tenants)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // don't await this, let it fire and forget
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () =>
                {
                    try
                    {
                        var idsOfUsersWithDeletedSession = await this.userSessionRepository.DeleteAll(tenant.Id);
                        if (idsOfUsersWithDeletedSession.Any())
                        {
                            this.backgroundJobClient.Enqueue(
                                () => this.EmitSessionInvalidatedForUsers(tenant.Id, idsOfUsersWithDeletedSession, this.httpContextPropertiesResolver.PerformingUserId, cancellationToken));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"Something went wrong when expiring user sessions for tenant {tenant.Id}: "
                            + $"{ex.Message}");
                    }
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        [JobDisplayName("Expiring user sessions for Tenant with Id {0}")]
        public async Task EmitSessionInvalidatedForUsers(Guid tenantId, List<Guid> idsOfUsersToInvalidate, Guid? performingUserId, CancellationToken cancellationToken)
        {
            var timestamp = this.clock.GetCurrentInstant();
            foreach (var userId in idsOfUsersToInvalidate)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.Emit(tenantId, userId, timestamp, performingUserId);
                await Task.Delay(100);
            }
        }

        public void EnqueueExpireAllUserSessionsByPortalId(Guid tenantId, Guid portalId, CancellationToken cancellationToken)
        {
            this.backgroundJobClient.Enqueue(
                () => this.ExpireAllUserSessionsByPortalId(tenantId, portalId, cancellationToken));
        }

        [JobDisplayName("Expire all user sessions for Portal with Id {1}")]
        public async Task ExpireAllUserSessionsByPortalId(Guid tenantId, Guid portalId, CancellationToken cancellationToken)
        {
            var idsOfUsersWithDeletedSession = new List<Guid>();

            var usersByOrgPortalId = await this.userReadModelRepository.GetAllUserIdsByPortal(tenantId, portalId);
            foreach (var user in usersByOrgPortalId)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var hasSessionDeletedForUser = await this.userSessionRepository.DeleteAllSessionsForUser(tenantId, user);
                if (hasSessionDeletedForUser)
                {
                    idsOfUsersWithDeletedSession.Add(user);
                }
            }

            await this.EmitSessionInvalidatedForUsers(tenantId, idsOfUsersWithDeletedSession, this.httpContextPropertiesResolver.PerformingUserId, cancellationToken);
        }

        private async Task Emit(Guid tenantId, Guid userId, Instant timestamp, Guid? performingUserId)
        {
            var eventTypes = new List<SystemEventType> { SystemEventType.UserSessionInvalidated };
            await this.userSystemEventEmitter.CreateAndEmitSystemEvents(
                tenantId,
                userId,
                eventTypes,
                performingUserId,
                timestamp);
        }
    }
}
