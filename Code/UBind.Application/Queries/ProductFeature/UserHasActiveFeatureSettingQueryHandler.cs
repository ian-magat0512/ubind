// <copyright file="UserHasActiveFeatureSettingQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.FeatureSettings
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Query Handler for determining if a user has an active Feature setting.
    /// </summary>
    public class UserHasActiveFeatureSettingQueryHandler : IQueryHandler<UserHasActiveFeatureSettingQuery, bool>
    {
        private readonly ICachingResolver cachingResolver;

        public UserHasActiveFeatureSettingQueryHandler(
            IFeatureSettingRepository settingRepository,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public Task<bool> Handle(UserHasActiveFeatureSettingQuery request, CancellationToken cancellationToken)
        {
            if (!request.User.IsAuthenticated())
            {
                return Task.FromResult(false);
            }

            if (request.User.IsMasterUser())
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(this.HasSettings(request.User.GetTenantId(), request.Feature.Humanize()));
            }
        }

        private bool HasSettings(Guid tenantId, string settingsName)
        {
            var activeSettings = this.cachingResolver.GetActiveSettingsOrThrow(tenantId);
            bool hasSettings = activeSettings != null && activeSettings.Where(setting => setting.Name == settingsName).Any();

            return hasSettings;
        }
    }
}
