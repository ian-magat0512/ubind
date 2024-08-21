// <copyright file="ISystemAlertRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;

    /// <summary>
    /// Repository for storing systemAlerts.
    /// </summary>
    public interface ISystemAlertRepository
    {
        /// <summary>
        /// Create a new systemAlert into the repository.
        /// </summary>
        /// <param name="systemAlert">The systemAlert to create.</param>
        /// <returns>SystemAlert.</returns>
        SystemAlert CreateSystemAlert(SystemAlert systemAlert);

        /// <summary>
        /// Add a new systemAlert into the repository.
        /// </summary>
        /// <param name="systemAlert">The systemAlert to create.</param>
        void AddAlert(SystemAlert systemAlert);

        /// <summary>
        /// Retrieve a systemAlert by ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="systemAlertId">The ID of the systemAlert to retrieve.</param>
        /// <returns>The retrieved systemAlert.</returns>
        SystemAlert GetSystemAlertById(Guid tenantId, Guid systemAlertId);

        /// <summary>
        /// Retrieve all systemAlerts from the repo.
        /// </summary>
        /// <returns>All the systemAlerts in the repo.</returns>
        IEnumerable<SystemAlert> GetSystemAlerts();

        /// <summary>
        /// Get the active system alert type filter by product id and type.
        /// </summary>
        /// <param name="tenantId">The Tenant Id to filter.</param>
        /// <param name="systemAlertType">The system alert type name to filter.</param>
        /// <returns>Instance of SystemAlert.</returns>
        SystemAlert GetActiveSystemAlertsByTenantIdByType(Guid tenantId, SystemAlertType systemAlertType);

        /// <summary>
        /// Get the system alert type filter by product id and type.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="productId">The Product Id to filter.</param>
        /// <param name="systemAlertType">The system alert type name to filter.</param>
        /// <returns>Instance of SystemAlert.</returns>
        SystemAlert GetSystemAlertsByProductIdAndByType(Guid tenantId, Guid productId, SystemAlertType systemAlertType);

        /// <summary>
        /// Get the system alert type filter by product id and type.
        /// </summary>
        /// <param name="tenantId">The Tenant Id to filter.</param>
        /// <param name="productId">The Product Id to filter.</param>
        /// <param name="systemAlertType">The system alert type name to filter.</param>
        /// <returns>Instance of SystemAlert.</returns>
        IEnumerable<SystemAlert> GetApplicableAlerts(Guid tenantId, Guid productId, SystemAlertType systemAlertType);

        /// <summary>
        /// Update System Alert.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="systemAlertId">The SystemALertId id.</param>
        /// <param name="warningThreshold">Warning Threshold.</param>
        /// <param name="criticalThreshold">Critical Threshold.</param>
        /// <returns>System Alert.</returns>
        SystemAlert UpdateSystemAlert(Guid tenantId, Guid systemAlertId, int? warningThreshold, int? criticalThreshold);

        /// <summary>
        /// Enable System Alert.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The System alert Id to enable.</param>
        /// <returns>System Alert.</returns>
        SystemAlert EnableSystemAlert(Guid tenantId, Guid id);

        /// <summary>
        /// Disable System Alert.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The System alert Id to disable.</param>
        /// <returns>System Alert.</returns>
        SystemAlert DisableSystemAlert(Guid tenantId, Guid id);

        /// <summary>
        /// Get the system alert type filter by product id and type.
        /// </summary>
        /// <param name="tenantId">The Tenant Id to filter.</param>
        /// <param name="systemAlertType">The system alert type name to filter.</param>
        /// <returns>Instance of SystemAlert.</returns>
        SystemAlert GetSystemAlertsByTenantIdByType(Guid tenantId, SystemAlertType systemAlertType);

        /// <summary>
        /// Gets all the tenant-level system alerts for a given tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>All the tenant-level system alerts for a given tenant.</returns>
        IEnumerable<SystemAlert> GetSystemAlertsByTenantId(Guid tenantId);

        /// <summary>
        /// Gets all the tenant-level system alerts for a given tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>All the tenant-level system alerts for a given tenant.</returns>
        IEnumerable<SystemAlert> GetSystemAlertsByTenantIdAndProductId(Guid tenantId, Guid productId);

        /// <summary>
        /// Persist changes to the database.
        /// </summary>
        void SaveChanges();
    }
}
