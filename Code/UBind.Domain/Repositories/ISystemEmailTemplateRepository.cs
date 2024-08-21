// <copyright file="ISystemEmailTemplateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;

    /// <summary>
    ///  Repository for Email Template.
    /// </summary>
    public interface ISystemEmailTemplateRepository
    {
        /// <summary>
        /// Retrieve an email template from the repo based on ID.
        /// </summary>
        /// <returns>The email template setting.</returns>
        /// <exception cref="NotFoundException">Thrown when the email template is not found.</exception>
        /// <param name="emailTemplateSettingId">The ID of the email template.</param>
        SystemEmailTemplate GetTemplateSettingById(Guid tenantId, Guid emailTemplateSettingId);

        /// <summary>
        /// Retrieve all email templates from the repo by tenant ID.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>Email templates filtered by tenant ID.</returns>
        IEnumerable<SystemEmailTemplate> GetTemplatesByTenantId(Guid tenantId);

        /// <summary>
        /// Retrieve all email templates from the repo by tenant ID and product ID.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <returns>EmailTemplateSetting filtered by tenant ID and product ID.</returns>
        IEnumerable<SystemEmailTemplate> GetTemplatesByProductId(Guid tenantId, Guid productId);

        /// <summary>
        /// Retrieve all email templates from the repository by portal Id.
        /// </summary>
        /// <param name="tenantId">The value of the tenant Id.</param>
        /// <param name="portalId">The value of the portal Id.</param>
        /// <returns>Enumerable of system email templates filtered by tenant Id and portal Id.</returns>
        IEnumerable<SystemEmailTemplate> GetTemplatesByPortalId(Guid tenantId, Guid? portalId);

        /// <summary>
        /// Get all applicable templates for an email associated with a given product.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the product belongs to.</param>
        /// <param name="type">The type of the email the templates are for.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="portalId">The ID of the portal.</param>
        /// <returns>The set of applicable templates, in order of increasing specifificity.</returns>
        /// <remarks>
        /// Applicable templates (in order of increasing specificity) include: master templates, tenant-specific overrides, and product-specific overrides.
        /// .</remarks>
        IEnumerable<ISystemEmailTemplateSummary> GetApplicableTemplates(
            Guid tenantId, SystemEmailType type, Guid? productId, Guid? portalId);

        /// <summary>
        /// Store an email template setting in the database.
        /// </summary>
        /// <param name="emailTemplateSetting">The new email template setting to store.</param>
        void Insert(SystemEmailTemplate emailTemplateSetting);

        /// <summary>
        /// Persiste changes in entities to the database.
        /// </summary>
        void SaveChanges();
    }
}
