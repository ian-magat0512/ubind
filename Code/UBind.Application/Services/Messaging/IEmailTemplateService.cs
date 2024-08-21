// <copyright file="IEmailTemplateService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Application.Services.SystemEmail;
    using UBind.Domain;

    /// <summary>
    /// Application service for handling tenant/product-related functionality.
    /// </summary>
    public interface IEmailTemplateService
    {
        /// <summary>
        /// Update the system email template based from the provided email template ID for a given user.
        /// </summary>
        /// <returns>The new EmailTemplateSetting.</returns>
        /// <param name="newEmailTemplateData">The data of the new email template.</param>
        Task<SystemEmailTemplate> UpdateEmailTemplate(
            Guid tenantId, Guid emailTemplateId, SystemEmailTemplateData newEmailTemplateData);

        /// <summary>
        /// Get the system email template based from the provided email template ID for a given user.
        /// </summary>
        /// <returns>The Email Settings.</returns>
        /// <param name="emailTemplateId">The ID of the email template.</param>
        SystemEmailTemplate GetTemplateById(Guid tenantId, Guid emailTemplateId);

        /// <summary>
        /// Get the list of system email template based from the provided tenant ID for a given user.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The Email Settings.</returns>
        Task<IEnumerable<SystemEmailTemplate>> GetTemplatesByTenantId(Guid tenantId);

        /// <summary>
        /// Get the list of system email template based from the provided tenant ID and product ID for a given user.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The Email Settings.</returns>
        Task<IEnumerable<SystemEmailTemplate>> GetTemplatesByProductId(Guid tenantId, Guid productId);

        /// <summary>
        /// Get the list of system email templates based from the provided portal ID and tenant ID for a given user.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="portalId">The ID of the portal.</param>
        /// <returns>Enumerable of system email templates.</returns>
        Task<IEnumerable<SystemEmailTemplate>> GetTemplatesByPortalId(Guid tenantId, Guid? portalId);

        /// <summary>
        /// Disables the system email template.
        /// </summary>
        Task Disable(Guid tenantId, Guid emailTemplateId);

        /// <summary>
        /// Enables the system email template.
        /// </summary>
        Task Enable(Guid tenantId, Guid emailTemplateId);

        /// <summary>
        /// Generate the template data to use for a given email type in association with a given product.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the product belongs to.</param>
        /// <param name="type">The email type.</param>
        /// <param name="emailDrop">The email drop model.</param>
        /// <returns>Derived template to use, based on all applicable overrides.</returns>
        SystemEmailTemplateData GenerateTemplateData(Guid tenantId, SystemEmailType type, EmailDrop emailDrop);

        /// <summary>
        /// Generate the template data to use for a given email type in association with a given product.
        /// </summary>
        /// <param name="type">The email type.</param>
        /// <param name="tenantId">The ID of the tenant the product belongs to.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>Derived template to use, based on all applicable overrides.</returns>
        SystemEmailTemplateData GenerateTemplateData(Guid tenantId, SystemEmailType type, Guid? productId, Guid? portalId);
    }
}
