// <copyright file="IFeatureSettingRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Repository for storing settings.
    /// </summary>
    public interface IFeatureSettingRepository
    {
        /// <summary>
        /// Insert a new setting into the repository.
        /// </summary>
        /// <param name="setting">The setting to insert.</param>
        void Insert(Setting setting);

        /// <summary>
        /// Update or insert a setting into the repository.
        /// </summary>
        /// <param name="setting">The setting to upsert.</param>
        void Upsert(Setting setting);

        /// <summary>
        /// Retrieve a setting by ID.
        /// </summary>
        /// <param name="id">The ID of the setting to retrieve.</param>
        /// <returns>The retrieved setting.</returns>
        Setting GetSettingById(string id);

        /// <summary>
        /// Retrieve all settings from the repo.
        /// </summary>
        /// <param name="filers">Filters to apply.</param>
        /// <returns>All the settings in the repo.</returns>
        IEnumerable<Setting> GetSettings(EntityListFilters filers);

        /// <summary>
        /// Retrieve all settings from the repo.
        /// </summary>
        /// <param name="tenantId">The tenant Id to get.</param>
        /// <returns>All the settings in the repo.</returns>
        IEnumerable<Setting> GetSettings(Guid tenantId);

        /// <summary>
        /// Retrieve all active settings from the repo for a given tenant.
        /// </summary>
        /// <param name="tenantId">The tenant Id to retrieve.</param>
        /// <returns>All the settings in the repo.</returns>
        IEnumerable<Setting> GetActiveSettings(Guid tenantId);

        /// <summary>
        /// Sets the initial settings for a tenant.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        void SetInitialSettings(Guid tenantId);

        /// <summary>
        /// Populates the feature settings for all tenants to ensure they each contain the records.
        /// </summary>
        void PopulateForAllTenants(IEnumerable<Setting> settings);

        /// <summary>
        /// Save any changes to settings.
        /// </summary>
        void SaveChanges();
    }
}
