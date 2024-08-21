// <copyright file="SystemAlertRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Temporary in-memory repository for use during UI development.
    /// </summary>
    public class SystemAlertRepository : ISystemAlertRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemAlertRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public SystemAlertRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public IEnumerable<SystemAlert> GetSystemAlerts()
        {
            return this.dbContext.SystemAlerts.ToList();
        }

        /// <inheritdoc/>
        public void AddAlert(SystemAlert alert)
        {
            this.dbContext.SystemAlerts.Add(alert);
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public SystemAlert GetSystemAlertById(Guid tenantId, Guid systemAlertId)
        {
            return this.dbContext.SystemAlerts.FirstOrDefault(s => s.TenantId == tenantId && s.Id == systemAlertId);
        }

        /// <inheritdoc/>
        public SystemAlert GetActiveSystemAlertsByTenantIdByType(Guid tenantId, SystemAlertType systemAlertType)
        {
            return this.dbContext.SystemAlerts.FirstOrDefault(systemAlert => systemAlert.TenantId == tenantId && systemAlert.ProductId == null && systemAlert.Type == systemAlertType && !systemAlert.Disabled);
        }

        /// <inheritdoc/>
        public SystemAlert GetSystemAlertsByTenantIdByType(Guid tenantId, SystemAlertType systemAlertType)
        {
            return this.dbContext.SystemAlerts.FirstOrDefault(systemAlert => systemAlert.TenantId == tenantId && systemAlert.ProductId == null && systemAlert.Type == systemAlertType);
        }

        /// <inheritdoc/>
        public IEnumerable<SystemAlert> GetSystemAlertsByTenantId(Guid tenantId)
        {
            return this.dbContext.SystemAlerts
                .Where(alert => alert.TenantId == tenantId)
                .Where(alert => alert.ProductId == null)
                .OrderBy(alert => alert.Type)
                .ToList();
        }

        /// <inheritdoc />
        public IEnumerable<SystemAlert> GetSystemAlertsByTenantIdAndProductId(Guid tenantId, Guid productId)
        {
            return this.dbContext.SystemAlerts
                .Where(alert => alert.TenantId == tenantId && alert.ProductId == productId)
                .ToList();
        }

        /// <inheritdoc/>
        public SystemAlert GetSystemAlertsByProductIdAndByType(Guid tenantId, Guid productId, SystemAlertType systemAlertType)
        {
            return this.dbContext.SystemAlerts.FirstOrDefault(systemAlert =>
            systemAlert.TenantId == tenantId
            && systemAlert.ProductId == productId
            && systemAlert.Type == systemAlertType);
        }

        /// <inheritdoc/>
        public IEnumerable<SystemAlert> GetApplicableAlerts(Guid tenantId, Guid productId, SystemAlertType systemAlertType)
        {
            return this.dbContext.SystemAlerts
                .Where(alert => alert.Type == systemAlertType)
                .Where(alert => alert.Disabled == false)
                .Where(alert => alert.TenantId == tenantId || alert.TenantId == Tenant.MasterTenantId)
                .Where(alert => alert.ProductId == productId || alert.ProductId == null)
                .ToList();
        }

        /// <inheritdoc/>
        public SystemAlert CreateSystemAlert(SystemAlert systemAlert)
        {
            this.dbContext.SystemAlerts.Add(systemAlert);
            this.dbContext.SaveChanges();
            return systemAlert;
        }

        /// <inheritdoc/>
        public SystemAlert UpdateSystemAlert(Guid tenantId, Guid systemAlertId, int? warningThreshold, int? criticalThreshold)
        {
            var systemAlert = this.GetSystemAlertById(tenantId, systemAlertId);
            if (systemAlert == null)
            {
                throw new InvalidOperationException("System Alert Cannot be found.");
            }

            systemAlert.Update(warningThreshold, criticalThreshold);
            this.dbContext.SaveChanges();
            return systemAlert;
        }

        /// <inheritdoc/>
        public SystemAlert DisableSystemAlert(Guid tenantId, Guid id)
        {
            var systemAlert = this.GetSystemAlertById(tenantId, id);
            if (systemAlert == null)
            {
                throw new InvalidOperationException("System Alert Cannot be found.");
            }

            systemAlert.DisableSystemAlert();
            this.dbContext.SaveChanges();
            return this.GetSystemAlertById(tenantId, id);
        }

        /// <inheritdoc/>
        public SystemAlert EnableSystemAlert(Guid tenantId, Guid id)
        {
            var systemAlert = this.GetSystemAlertById(tenantId, id);
            if (systemAlert == null)
            {
                throw new InvalidOperationException("System Alert Cannot be found.");
            }

            systemAlert.EnableSystemAlert();
            this.dbContext.SaveChanges();
            return this.GetSystemAlertById(tenantId, id);
        }
    }
}
