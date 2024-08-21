// <copyright file="FeatureSettingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Humanizer;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;

    /// <inheritdoc/>
    public class FeatureSettingService : IFeatureSettingService
    {
        private readonly IFeatureSettingRepository settingRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly ITenantSystemEventEmitter tenantSystemEventEmitter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureSettingService"/> class.
        /// </summary>
        /// <param name="settingRepository">The tenant repository.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public FeatureSettingService(
            IFeatureSettingRepository settingRepository,
            ITenantSystemEventEmitter tenantSystemEventEmitter,
            ICachingResolver cachingResolver)
        {
            Contract.Assert(settingRepository != null);
            Contract.Assert(cachingResolver != null);

            this.settingRepository = settingRepository;
            this.cachingResolver = cachingResolver;
            this.tenantSystemEventEmitter = tenantSystemEventEmitter;
        }

        /// <inheritdoc/>
        public async Task<Setting> UpdateSetting(string settingId, SettingDetails settingDetails)
        {
            this.cachingResolver.RemoveCachedFeatureSettings(settingDetails.Tenant.Id);
            var setting = this.settingRepository.GetSettingById(settingId);
            if (setting == null)
            {
                throw new ErrorException(Errors.General.NotFound("setting", settingId));
            }

            setting.Update(settingDetails);
            this.settingRepository.SaveChanges();
            await this.tenantSystemEventEmitter
                .CreateAndEmitSystemEvent(settingDetails.Tenant.Id, SystemEventType.TenantModified);
            return setting;
        }

        /// <inheritdoc/>
        public IEnumerable<Setting> GetSettings(EntityListFilters filters)
        {
            return this.settingRepository.GetSettings(filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Setting> GetActiveSettings(Guid tenantId)
        {
            return this.cachingResolver.GetActiveSettingsOrThrow(tenantId);
        }

        /// <inheritdoc/>
        public bool TenantHasActiveFeature(Guid tenantId, Feature feature)
        {
            return this.HasSettings(tenantId, feature.Humanize());
        }

        private bool HasSettings(Guid tenantId, string settingsName)
        {
            var activeSettings = this.cachingResolver.GetActiveSettingsOrNull(tenantId);
            bool hasSettings = activeSettings != null && activeSettings.Where(setting => setting.Name == settingsName).Any();
            return hasSettings;
        }
    }
}
