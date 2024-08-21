// <copyright file="IFeatureSettingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Application service for handling Feature setting-related functionality.
    /// </summary>
    public interface IFeatureSettingService
    {
        /// <summary>
        /// Create a new setting and queue its initialization.
        /// </summary>
        /// <param name="settingId">the settings id.</param>
        /// <param name="settingDetails">setting details.</param>
        /// <returns>The new product.</returns>
        Task<Setting> UpdateSetting(string settingId, SettingDetails settingDetails);

        /// <summary>
        /// Retrieves a collection of setting records in the system that satisfy the given parameters.
        /// </summary>
        /// <param name="filters">Additional filters to be applied to the result set.</param>
        /// <returns>A collection of setting records.</returns>
        IEnumerable<Setting> GetSettings(EntityListFilters filters);

        /// <summary>
        /// Retrieves a collection of active setting records in the system for the given tenant.
        /// </summary>
        /// <param name="tenantId">The tenant Id to retrieve.</param>
        /// <returns>A collection of setting records.</returns>
        IEnumerable<Setting> GetActiveSettings(Guid tenantId);

        /// <summary>
        /// Validate if tenant has an active Feature/settings.
        /// </summary>
        /// <param name="tenantId">The Tenant Id to validate.</param>
        /// <param name="feature">The the feature.</param>
        /// <returns>
        /// Returns true if the tenant has an active settings and if user is ubind admin, returns false.
        /// </returns>
        bool TenantHasActiveFeature(Guid tenantId, Feature feature);
    }
}
