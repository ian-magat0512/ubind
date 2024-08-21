// <copyright file="ISystemAlertService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Service for triggering system alerts (e.g. low policy numbers).
    /// </summary>
    public interface ISystemAlertService
    {
        /// <summary>
        /// Retrieve tenant-level alerts for a given tenant, lazily creating them if necessary.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to get alerts for.</param>
        /// <returns>The tenant's alerts.</returns>
        Task<List<SystemAlert>> GetSystemAlertsByTenantId(Guid tenantId);

        /// <summary>
        /// Retrieve product-level alerts for a given product, lazily creating them if necessary.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the product belongs to.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The product's alerts.</returns>
        Task<List<SystemAlert>> GetSystemAlertsByTenantIdAndProductId(Guid tenantId, Guid productId);

        /// <summary>
        /// Retrieves a collection of alert records in the system that satisfy the given parameters.
        /// </summary>
        /// <returns>A collection of alert records.</returns>
        IEnumerable<SystemAlert> GetSystemAlerts();

        /// <summary>
        /// Get Instance of System Alert.
        /// </summary>
        /// <param name="userTenantId">The User Tenant Id.</param>
        /// <param name="systemAlertId">The primary key of System Alert.</param>
        /// <returns>SystemAlert.</returns>
        SystemAlert GetSystemAlertById(Guid userTenantId, Guid systemAlertId);

        /// <summary>
        /// Update System Alert.
        /// </summary>
        /// <param name="userTenantId">The User guid Tenant Id.</param>
        /// <param name="systemAlertId">The SystemALertId id.</param>
        /// <param name="warningThreshold">Warning Threshold.</param>
        /// <param name="criticalThreshold">Critical Threshold.</param>
        /// <returns>System Alert.</returns>
        Task<SystemAlert> UpdateSystemAlert(Guid userTenantId, Guid systemAlertId, int? warningThreshold, int? criticalThreshold);

        /// <summary>
        /// Disable System Alert.
        /// </summary>
        /// <param name="userTenantId">The User Tenant Id.</param>
        /// <param name="systemAlertId,">The System Alert Id to update.</param>
        /// <returns>System Alert.</returns>
        Task<SystemAlert> DisableSystemAlert(Guid userTenantId, Guid systemAlertId);

        /// <summary>
        /// Enable System Alert.
        /// </summary>
        /// <param name="userTenantId">The User Tenant Id.</param>
        /// <param name="systemAlertId,">The System Alert Id to update.</param>
        /// <returns>System Alert.</returns>
        Task<SystemAlert> EnableSystemAlert(Guid userTenantId, Guid systemAlertId);

        /// <summary>
        /// Process Invoice threshold.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="productId">The Product id.</param>
        /// <param name="environment">The environment.</param>
        Task TriggerInvoiceNumberThresholdAlertCheck(Guid tenantId, Guid productId, DeploymentEnvironment environment, string tenantAlias, string? productAlias);

        /// <summary>
        /// Perform checking if policy numbers are in low threshold.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="productId">The Product id.</param>
        /// <param name="environment">The environment.</param>
        Task TriggerPolicyNumberThresholdAlertCheck(Guid tenantId, Guid productId, DeploymentEnvironment environment, string tenantAlias, string? productAlias);

        /// <summary>
        /// Process Claim threshold is running low.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="productId">The Product id.</param>
        /// <param name="environment">The environment.</param>
        Task TriggerClaimNumberThresholdAlertCheck(Guid tenantId, Guid productId, DeploymentEnvironment environment, string tenantAlias, string? productAlias);

        /// <summary>
        /// Process Invoice threshold.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="productId">The Product id.</param>
        /// <param name="environment">The environment.</param>
        Task QueueInvoiceNumberThresholdAlertCheck(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Perform checking if policy numbers are in low threshold.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="productId">The Product id.</param>
        /// <param name="environment">The environment.</param>
        Task QueuePolicyNumberThresholdAlertCheck(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Process Claim threshold is running low.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="productId">The Product id.</param>
        /// <param name="environment">The environment.</param>
        Task QueueClaimNumberThresholdAlertCheck(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Process credit note reference number threshold alerts.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the reference numbers are for.</param>
        /// <param name="productId">The ID of the product the numbers are for.</param>
        /// <param name="environment">The environment being used.</param>
        void QueueCreditNoteNumberThresholdAlertCheck(Guid tenantId, Guid productId, DeploymentEnvironment environment);
    }
}
