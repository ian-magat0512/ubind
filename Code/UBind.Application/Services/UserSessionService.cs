// <copyright file="UserSessionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System.Security.Claims;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Redis;
    using UBind.Domain.Repositories.Redis;
    using UBind.Domain.Services;

    public class UserSessionService : IUserSessionService
    {
        private readonly IUserSessionRepository userSessionRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly IClock clock;
        private readonly ILogger<UserSessionService> logger;
        private UserSessionModel? userSessionModel;
        private Tenant? tenant;

        public UserSessionService(
            IUserSessionRepository userSessionRepository,
            ICachingResolver cachingResolver,
            IClock clock,
            ILogger<UserSessionService> logger)
        {
            this.userSessionRepository = userSessionRepository;
            this.cachingResolver = cachingResolver;
            this.clock = clock;
            this.logger = logger;
        }

        public async Task<UserSessionModel> Create(UserReadModel user, IEnumerable<Permission> effectivePermissions)
        {
            var userSessionModel = new UserSessionModel(user, effectivePermissions, this.clock.Now());
            await this.userSessionRepository.Upsert(userSessionModel.TenantId, userSessionModel);
            return userSessionModel;
        }

        public async Task<UserSessionModel?> Get(ClaimsPrincipal performingUser)
        {
            Guid? sessionId = performingUser.SessionId();
            if (sessionId == null)
            {
                return null;
            }

            if (this.userSessionModel != null && sessionId != this.userSessionModel.Id)
            {
                throw new InvalidOperationException("You have attempted to access the permissions fom a different "
                    + "session than the one that was used to retrieve the permissions. This should not be possible "
                    + $"if {this.GetType().Name} is setup as a Scoped service in the DI container, as there should be "
                    + "a separate instance per request. Please check that it was setup as a Scoped service.");
            }

            // we cache the user session model in memory for the duration of the current HTTP request, so that we don't
            // have to keep hitting redis for it.
            if (this.userSessionModel == null)
            {
                this.userSessionModel = await this.userSessionRepository.Get(performingUser.GetTenantId(), sessionId.Value);
            }

            return this.userSessionModel;
        }

        public Task Delete(ClaimsPrincipal performingUser)
        {
            Guid? sessionId = performingUser.SessionId();
            if (sessionId == null)
            {
                return Task.CompletedTask;
            }

            Guid? userId = performingUser.GetId();
            if (userId == null)
            {
                throw new ArgumentException("Unable to delete a users session when it did not contain a user ID.");
            }

            return this.userSessionRepository.Delete(performingUser.GetTenantId(), userId.Value, sessionId.Value);
        }

        public Task DeleteAllSessionsForUser(Guid tenantId, Guid userId)
        {
            return this.userSessionRepository.DeleteAllSessionsForUser(tenantId, userId);
        }

        public Task DeleteAll(Guid tenantId)
        {
            return this.userSessionRepository.DeleteAll(tenantId);
        }

        public async Task<bool> HasPasswordExpired()
        {
            if (this.userSessionModel == null)
            {
                throw new ArgumentException("Please call Get() before calling HasPasswordExpired().");
            }

            if (this.tenant == null)
            {
                this.tenant = await this.cachingResolver.GetTenantOrThrow(this.userSessionModel.TenantId);
            }

            if (this.tenant.Details.PasswordExpiryEnabled)
            {
                if (this.userSessionModel.PasswordLastChangedTimestamp != null)
                {
                    long maxPasswordAgeSeconds = (long)this.tenant.Details.MaxPasswordAgeDays * 24 * 60 * 60;
                    Duration maxPasswordAge = Duration.FromSeconds(maxPasswordAgeSeconds);
                    Duration passwordAge = this.clock.Now() - this.userSessionModel.PasswordLastChangedTimestamp.Value;
                    return passwordAge > maxPasswordAge;
                }
            }

            return false;
        }

        public async Task<bool> IsSessionExpired()
        {
            if (this.userSessionModel == null)
            {
                throw new ArgumentException("Please call Get() before calling IsExpired().");
            }

            if (this.tenant == null)
            {
                this.tenant = await this.cachingResolver.GetTenantOrThrow(this.userSessionModel.TenantId);
            }

            if (this.tenant.Details.SessionExpiryMode == SessionExpiryMode.FixedPeriod)
            {
                Duration maxSessionAge = Duration.FromMilliseconds(this.tenant.Details.FixLengthTimeoutInMilliseconds);
                Duration sessionAge = this.clock.Now() - this.userSessionModel.CreatedTimestamp;
                return sessionAge > maxSessionAge;
            }

            return false;
        }

        public async Task<bool> IsSessionIdle()
        {
            if (this.userSessionModel == null)
            {
                throw new ArgumentException("Please call Get() before calling IsIdle().");
            }

            if (this.tenant == null)
            {
                this.tenant = await this.cachingResolver.GetTenantOrThrow(this.userSessionModel.TenantId);
            }

            if (this.tenant.Details.SessionExpiryMode == SessionExpiryMode.InactivityPeriod)
            {
                Duration maxInactivityDuration = Duration.FromMilliseconds(this.tenant.Details.IdleTimeoutInMilliseconds);
                Duration inactivityDuration = this.clock.Now() - this.userSessionModel.LastUsedTimestamp;
                return inactivityDuration > maxInactivityDuration;
            }

            return false;
        }

        public void ExtendIdleTimeout()
        {
            if (this.userSessionModel == null)
            {
                throw new ArgumentException("Please call Get() before calling ExtendIdleTimeout().");
            }

            this.userSessionModel.LastUsedTimestamp = this.clock.Now();
            this.UpsertUserSessionModelInBackground(this.userSessionModel);
        }

        /// <summary>
        /// This is a fire and forget method, so we don't await it and we just log any exceptions.
        /// </summary>
        private async void UpsertUserSessionModelInBackground(UserSessionModel userSessionModel)
        {
            try
            {
                await this.userSessionRepository.Upsert(userSessionModel.TenantId, userSessionModel);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Failed upserting the user session model to redis in the background: " + ex.Message);
            }
        }
    }
}
