// <copyright file="EmailTemplateService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Services.SystemEmail;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly ISystemEmailTemplateRepository templateRepository;
        private readonly IClock clock;
        private readonly ICachingResolver cachingResolver;
        private readonly ITenantSystemEventEmitter tenantSystemEventEmitter;

        private readonly IDictionary<SystemEmailType, SystemEmailTemplateData> emailTypeDefaultContainer
            = new Dictionary<SystemEmailType, SystemEmailTemplateData>
        {
            { SystemEmailType.AccountActivationInvitation, SystemEmailTemplateData.DefaultActivationData },
            { SystemEmailType.PasswordResetInvitation, SystemEmailTemplateData.DefaultPasswordResetData },
            { SystemEmailType.PasswordExpiredResetInvitation, SystemEmailTemplateData.DefaultPasswordExpiredEmailTemplateData },
            { SystemEmailType.RenewalInvitation, SystemEmailTemplateData.DefaultRenewalInvitationData },
            { SystemEmailType.QuoteAssociationInvitation, SystemEmailTemplateData.DefaultQuoteAssociationInvitationData },
            { SystemEmailType.AccountAlreadyActivated, SystemEmailTemplateData.DefaultAccountAlreadyActivatedData },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailTemplateService"/> class.
        /// </summary>
        /// <param name="repository">The email template repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="clock">A clock.</param>
        public EmailTemplateService(
            ISystemEmailTemplateRepository repository,
            ICachingResolver cachingResolver,
            ITenantSystemEventEmitter tenantSystemEventEmitter,
            IClock clock)
        {
            this.cachingResolver = cachingResolver;
            this.templateRepository = repository;
            this.clock = clock;
            this.tenantSystemEventEmitter = tenantSystemEventEmitter;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SystemEmailTemplate>> GetTemplatesByTenantId(Guid tenantId)
        {
            IList<SystemEmailTemplate> templates = this.templateRepository.GetTemplatesByTenantId(tenantId).ToList();
            var missingEmailTypes = Enum.GetValues(typeof(SystemEmailType)).OfType<SystemEmailType>().ToList()
                .Where(e => !templates.Any(t => t.Type == e));
            foreach (var emailType in missingEmailTypes)
            {
                var template = await this.CreateTenantDefaultTemplate(tenantId, emailType);
                templates.Add(template);
            }

            return templates.OrderBy(c => c.Type.GetAttributeOfType<OrderAttribute>().Order);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SystemEmailTemplate>> GetTemplatesByProductId(Guid tenantId, Guid productId)
        {
            IList<SystemEmailTemplate> templates = this.templateRepository
                .GetTemplatesByProductId(tenantId, productId).ToList();
            var missingEmailTypes = Enum.GetValues(typeof(SystemEmailType)).OfType<SystemEmailType>().ToList()
                .Where(e => templates.All(t => t.Type != e)
                            && e != SystemEmailType.AccountActivationInvitation
                            && e != SystemEmailType.PasswordResetInvitation
                            && e != SystemEmailType.PasswordExpiredResetInvitation);
            foreach (var emailType in missingEmailTypes)
            {
                var template = await this.CreateProductDefaultTemplate(tenantId, productId, emailType);
                templates.Add(template);
            }

            return templates.Where(t => t.Type != SystemEmailType.AccountActivationInvitation
                                    && t.Type != SystemEmailType.PasswordResetInvitation
                                    && t.Type != SystemEmailType.PasswordExpiredResetInvitation)
                .OrderBy(c => c.Type.GetAttributeOfType<OrderAttribute>().Order);
        }

        /// <inheritdoc/>
        public SystemEmailTemplate GetTemplateById(Guid tenantId, Guid emailTemplateId)
        {
            SystemEmailTemplate template = this.templateRepository.GetTemplateSettingById(tenantId, emailTemplateId);
            return template;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SystemEmailTemplate>> GetTemplatesByPortalId(Guid tenantId, Guid? portalId)
        {
            IList<SystemEmailTemplate> templates = this.templateRepository
                .GetTemplatesByPortalId(tenantId, portalId).ToList();
            var missingEmailTypes = Enum.GetValues(typeof(SystemEmailType)).OfType<SystemEmailType>().ToList()
                .Where(e => !templates.Any(t => t.Type == e));
            foreach (var emailType in missingEmailTypes)
            {
                var template = await this.CreatePortalDefaultTemplate(tenantId, portalId, emailType);
                templates.Add(template);
            }

            return templates.OrderBy(c => c.Type.GetAttributeOfType<OrderAttribute>().Order);
        }

        /// <inheritdoc/>
        public async Task<SystemEmailTemplate> UpdateEmailTemplate(Guid tenantId, Guid emailTemplateId, SystemEmailTemplateData newEmailTemplateData)
        {
            SystemEmailTemplate template = this.templateRepository.GetTemplateSettingById(tenantId, emailTemplateId);
            template.Update(newEmailTemplateData);
            this.templateRepository.SaveChanges();
            await this.EmitEventIfTenantSettingIsModified(tenantId, template);
            return template;
        }

        /// <inheritdoc/>
        public async Task Disable(Guid tenantId, Guid emailTemplateId)
        {
            SystemEmailTemplate template = this.templateRepository.GetTemplateSettingById(tenantId, emailTemplateId);
            template.Disable();
            this.templateRepository.SaveChanges();
            await this.EmitEventIfTenantSettingIsModified(tenantId, template);
        }

        /// <inheritdoc/>
        public async Task Enable(Guid tenantId, Guid emailTemplateId)
        {
            SystemEmailTemplate template = this.templateRepository.GetTemplateSettingById(tenantId, emailTemplateId);
            template.Enable();
            this.templateRepository.SaveChanges();
            await this.EmitEventIfTenantSettingIsModified(tenantId, template);
        }

        /// <inheritdoc/>
        public SystemEmailTemplateData GenerateTemplateData(Guid tenantId, SystemEmailType type, EmailDrop emailDrop)
        {
            return this.GenerateTemplateData(tenantId, type, emailDrop.ProductId, emailDrop.PortalId);
        }

        /// <inheritdoc/>
        public SystemEmailTemplateData GenerateTemplateData(
            Guid tenantId, SystemEmailType type, Guid? productId, Guid? portalId)
        {
            this.emailTypeDefaultContainer.TryGetValue(type, out SystemEmailTemplateData data);
            var overrides = this.templateRepository.GetApplicableTemplates(tenantId, type, productId, portalId);

            foreach (var @override in overrides)
            {
                data.Override(@override.Data);
            }

            return data;
        }

        private async Task<SystemEmailTemplate> CreateTenantDefaultTemplate(Guid tenantId, SystemEmailType emailType)
        {
            this.emailTypeDefaultContainer.TryGetValue(emailType, out SystemEmailTemplateData templateData);

            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);

            var template = SystemEmailTemplate
                .CreateTenantEmailTemplateSetting(tenant.Id, emailType, templateData, this.clock.Now());
            this.templateRepository.Insert(template);
            this.templateRepository.SaveChanges();
            return template;
        }

        private async Task<SystemEmailTemplate> CreateProductDefaultTemplate(
            Guid tenantId, Guid productId, SystemEmailType emailType)
        {
            this.emailTypeDefaultContainer.TryGetValue(emailType, out SystemEmailTemplateData templateData);

            var product = await this.cachingResolver.GetProductOrThrow(tenantId, productId);

            var template = SystemEmailTemplate
                .CreateProductEmailTemplateSetting(
                product.TenantId,
                product.Id,
                emailType,
                templateData,
                this.clock.Now());
            this.templateRepository.Insert(template);
            this.templateRepository.SaveChanges();
            return template;
        }

        private async Task<SystemEmailTemplate> CreatePortalDefaultTemplate(Guid tenantId, Guid? portalId, SystemEmailType emailType)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);

            this.emailTypeDefaultContainer.TryGetValue(emailType, out SystemEmailTemplateData templateData);
            var template =
                SystemEmailTemplate.CreatePortalEmailTemplateSetting(
                    tenant.Id,
                    emailType,
                    portalId,
                    templateData,
                    this.clock.Now());
            this.templateRepository.Insert(template);
            this.templateRepository.SaveChanges();
            return template;
        }

        private async Task EmitEventIfTenantSettingIsModified(Guid tenantId, SystemEmailTemplate emailTemplate)
        {
            if (!emailTemplate.PortalId.HasValue && !emailTemplate.ProductId.HasValue)
            {
                await this.tenantSystemEventEmitter.CreateAndEmitSystemEvent(
                    tenantId,
                    SystemEventType.TenantModified);
            }
        }
    }
}
