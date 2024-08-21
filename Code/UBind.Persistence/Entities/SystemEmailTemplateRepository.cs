// <copyright file="SystemEmailTemplateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MoreLinq;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence.ReadModels;

    /// <summary>
    /// Email Template Repository.
    /// </summary>
    public class SystemEmailTemplateRepository : ISystemEmailTemplateRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEmailTemplateRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public SystemEmailTemplateRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public SystemEmailTemplate GetTemplateSettingById(Guid tenantId, Guid emailTemplateSettingId)
        {
            var entity = this.dbContext.EmailTemplateSettings
                .Where(e => e.TenantId == tenantId)
                .FirstOrDefault(emailSettings => emailSettings.Id == emailTemplateSettingId);
            if (entity == null)
            {
                throw new NotFoundException(
                    Errors.General.NotFound("email template setting", emailTemplateSettingId.ToString()));
            }

            return entity;
        }

        /// <inheritdoc/>
        public IEnumerable<SystemEmailTemplate> GetTemplatesByTenantId(Guid tenantId)
        {
            return this.dbContext.EmailTemplateSettings
                .Where(emailSettings => emailSettings.TenantId == tenantId)
                .Where(emailSettings => emailSettings.ProductId == null)
                .Where(emailSettings => emailSettings.PortalId == null)
                .OrderBy(emailSetting => emailSetting.Type)
                .ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<SystemEmailTemplate> GetTemplatesByProductId(Guid tenantId, Guid productId)
        {
            return this.dbContext.EmailTemplateSettings
                .Where(emailSettings => emailSettings.TenantId == tenantId)
                .Where(emailSettings => emailSettings.ProductId == productId)
                .Where(emailSettings => emailSettings.PortalId == null)
                .OrderBy(emailSetting => emailSetting.Type)
                .ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<SystemEmailTemplate> GetTemplatesByPortalId(Guid tenantId, Guid? portalId)
        {
            return this.dbContext.EmailTemplateSettings
                .Where(emailSettings => emailSettings.TenantId == tenantId)
                .Where(emailSettings => emailSettings.ProductId == null)
                .Where(emailSettings => emailSettings.PortalId == portalId)
                .OrderBy(emailSetting => emailSetting.Type)
                .ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<ISystemEmailTemplateSummary> GetApplicableTemplates(
            Guid tenantId, SystemEmailType type, Guid? productId, Guid? portalId)
        {
            var query = this.dbContext.EmailTemplateSettings
                .Where(template => template.Enabled)
                .Where(template => template.Type == type)
                .Where(template =>
                    (template.TenantId == Tenant.MasterTenantId) // Master templates
                    || (template.TenantId == tenantId && template.ProductId == null && template.PortalId == null) // Tenant-level overrides
                    || (template.TenantId == tenantId && template.ProductId == productId && template.PortalId == null) // Product-specific overrides
                    || (portalId != null && template.TenantId == tenantId && template.PortalId == portalId)) // Portal specific templates

                .OrderByDescending(template => template.TenantId == Tenant.MasterTenantId) // Order master first (true comes before false when sorting in descending order)
                .ThenByDescending(template => template.ProductId == null && template.PortalId == null) // Order tenant level templates next
                .ThenByDescending(template => template.ProductId == productId) // Order product-specific templates second last (false comes before true when sorting in ascending order)
                .ThenByDescending(template => template.PortalId == portalId) // Order portal-specific templates last (false comes before true when sorting in ascending order)
                .Select(template => new SystemEmailTemplateSummary
                {
                    Id = template.Id,
                    CreatedTicksSinceEpoch = template.CreatedTicksSinceEpoch,
                    TenantId = template.TenantId,
                    ProductId = template.ProductId,
                    PortalId = template.PortalId,
                    Type = template.Type,
                    Enabled = template.Enabled,
                    Data = template.Data,
                }).ToList();

            return query;
        }

        /// <inheritdoc/>
        public void Insert(SystemEmailTemplate emailTemplateSetting)
        {
            this.dbContext.EmailTemplateSettings.Add(emailTemplateSetting);
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }
    }
}
