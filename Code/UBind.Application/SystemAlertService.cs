// <copyright file="SystemAlertService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading.Tasks;
    using Hangfire;
    using MimeKit;
    using NodaTime;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Services;
    using UBind.Application.Services.Messaging;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Processing;
    using UBind.Domain.Product;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Repositories.Redis;
    using UBind.Domain.Services;

    /// <inheritdoc/>
    public class SystemAlertService : ISystemAlertService
    {
        private readonly IPolicyNumberRepository policyNumberRepository;
        private readonly IClaimNumberRepository claimNumberRepository;
        private readonly IInvoiceNumberRepository invoiceNumberRepository;
        private readonly ISystemAlertRepository systemAlertRepository;
        private readonly ITenantService tenantService;
        private readonly IEmailComposer emailComposerService;
        private readonly IMessagingService messagingService;
        private readonly IESystemAlertConfiguration systemAlertConfiguration;
        private readonly ICachingResolver cachingResolver;
        private readonly IJobClient backgroundJobClient;
        private readonly IClock clock;
        private readonly ITenantSystemEventEmitter tenantSystemEventEmitter;
        private readonly INumberPoolCountLastCheckedTimestampRepository numberPoolCountLastCheckedTimestampRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemAlertService"/> class.
        /// </summary>
        /// <param name="systemAlertRepository">The tenant repository.</param>
        /// <param name="clock">Clock for obtaining current time.</param>
        /// <param name="tenantService">The Tenant Service.</param>
        /// <param name="emailComposerService">The Email Composer.</param>
        /// <param name="messagingService">The Messaging service.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="systemAlertConfiguration">The System Alert Configuration.</param>
        /// <param name="backgroundJobClient">Client for queuing background jobs.</param>
        public SystemAlertService(
            ISystemAlertRepository systemAlertRepository,
            IClock clock,
            ITenantService tenantService,
            IEmailComposer emailComposerService,
            IMessagingService messagingService,
            ICachingResolver cachingResolver,
            IESystemAlertConfiguration systemAlertConfiguration,
            ITenantSystemEventEmitter tenantSystemEventEmitter,
            IJobClient backgroundJobClient,
            IPolicyNumberRepository policyNumberRepository,
            IClaimNumberRepository claimNumberRepository,
            IInvoiceNumberRepository invoiceNumberRepository,
            INumberPoolCountLastCheckedTimestampRepository numberPoolCountLastCheckedTimestampRepository)
        {
            Contract.Assert(systemAlertRepository != null);
            this.policyNumberRepository = policyNumberRepository;
            this.claimNumberRepository = claimNumberRepository;
            this.invoiceNumberRepository = invoiceNumberRepository;
            this.systemAlertRepository = systemAlertRepository;
            this.tenantService = tenantService;
            this.clock = clock;
            this.emailComposerService = emailComposerService;
            this.messagingService = messagingService;
            this.cachingResolver = cachingResolver;
            this.systemAlertConfiguration = systemAlertConfiguration;
            this.backgroundJobClient = backgroundJobClient;
            this.tenantSystemEventEmitter = tenantSystemEventEmitter;
            this.numberPoolCountLastCheckedTimestampRepository = numberPoolCountLastCheckedTimestampRepository;
        }

        /// <summary>
        /// Update System ALert.
        /// </summary>
        /// <param name="tenantId">The User Tenant Id.</param>
        /// <param name="systemAlertId">The SystemALertId id.</param>
        /// <param name="warningThreshold">Warning Threshold.</param>
        /// <param name="criticalThreshold">Critical Threshold.</param>
        /// <returns>System Alert.</returns>
        public async Task<SystemAlert> UpdateSystemAlert(
            Guid tenantId, Guid systemAlertId, int? warningThreshold, int? criticalThreshold)
        {
            var systemAlert = this.systemAlertRepository.UpdateSystemAlert(tenantId, systemAlertId, warningThreshold, criticalThreshold);
            await this.EmitEventIfTenantSettingIsModified(tenantId, systemAlert);
            return systemAlert;
        }

        /// <summary>
        /// Disable System ALert.
        /// </summary>
        /// <param name="tenantId">The User Tenant Id.</param>
        /// <param name="systemAlertId">The System ALert Id id to Disable.</param>
        /// <returns>System Alert.</returns>
        public async Task<SystemAlert> DisableSystemAlert(Guid tenantId, Guid systemAlertId)
        {
            var systemAlert = this.systemAlertRepository.DisableSystemAlert(tenantId, systemAlertId);
            await this.EmitEventIfTenantSettingIsModified(tenantId, systemAlert);
            return systemAlert;
        }

        /// <summary>
        /// Enable System ALert.
        /// </summary>
        /// <param name="tenantId">The User Tenant Id.</param>
        /// <param name="systemAlertId">The System ALert Id id to Disable.</param>
        /// <returns>System Alert.</returns>
        public async Task<SystemAlert> EnableSystemAlert(Guid tenantId, Guid systemAlertId)
        {
            var systemAlert = this.systemAlertRepository.EnableSystemAlert(tenantId, systemAlertId);
            await this.EmitEventIfTenantSettingIsModified(tenantId, systemAlert);
            return systemAlert;
        }

        /// <inheritdoc/>
        public IEnumerable<SystemAlert> GetSystemAlerts()
        {
            return this.systemAlertRepository.GetSystemAlerts();
        }

        /// <inheritdoc/>
        public async Task<List<SystemAlert>> GetSystemAlertsByTenantId(Guid tenantId)
        {
            var alerts = this.systemAlertRepository.GetSystemAlertsByTenantId(tenantId).ToList();

            // Lazily create alerts if required
            await this.LazyLoadCreation(tenantId, alerts);

            return alerts;
        }

        /// <inheritdoc/>
        public async Task<List<SystemAlert>> GetSystemAlertsByTenantIdAndProductId(Guid tenantId, Guid productId)
        {
            var alerts = this.systemAlertRepository.GetSystemAlertsByTenantIdAndProductId(tenantId, productId).ToList();

            // Lazily create alerts if required
            await this.LazyLoadCreation(tenantId, alerts, productId);

            return alerts;
        }

        /// <summary>
        /// Get Instance of System Alert.
        /// </summary>
        /// <param name="tenantId">The User Tenant Id.</param>
        /// <param name="systemAlertId">The primary key of System Alert.</param>
        /// <returns>SystemAlert.</returns>
        public SystemAlert GetSystemAlertById(Guid tenantId, Guid systemAlertId)
        {
            var systemAlert = this.GetSystemAlert(tenantId, systemAlertId);
            return systemAlert;
        }

        /// <inheritdoc/>
        public async Task QueuePolicyNumberThresholdAlertCheck(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment)
        {
            Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
            long currentInstantTicks = currentInstant.ToUnixTimeTicks();

            var productAlias = this.cachingResolver.GetProductAliasOrThrow(tenantId, productId);
            var lastCheckedTimestamp = this.numberPoolCountLastCheckedTimestampRepository.GetLastCheckedTimestamp(tenantId, productAlias, NumberPoolType.PolicyNumbers.ToString());

            if (this.ShouldPerformAlertCheck(currentInstantTicks, lastCheckedTimestamp))
            {
                await this.PerformPolicyNumberThresholdAlertCheck(tenantId, productId, productAlias, environment);
            }
        }

        /// <inheritdoc/>
        public async Task QueueInvoiceNumberThresholdAlertCheck(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment)
        {
            Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
            long currentInstantTicks = currentInstant.ToUnixTimeTicks();

            var productAlias = this.cachingResolver.GetProductAliasOrThrow(tenantId, productId);
            var lastCheckedTimestamp = this.numberPoolCountLastCheckedTimestampRepository.GetLastCheckedTimestamp(tenantId, productAlias, NumberPoolType.InvoiceNumbers.ToString());

            if (this.ShouldPerformAlertCheck(currentInstantTicks, lastCheckedTimestamp))
            {
                await this.PerformInvoiceNumberThresholdAlertCheck(tenantId, productId, productAlias, environment);
            }
        }

        /// <inheritdoc/>
        public async Task QueueClaimNumberThresholdAlertCheck(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment)
        {
            Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
            long currentInstantTicks = currentInstant.ToUnixTimeTicks();

            var productAlias = this.cachingResolver.GetProductAliasOrThrow(tenantId, productId);
            var lastCheckedTimestamp = this.numberPoolCountLastCheckedTimestampRepository.GetLastCheckedTimestamp(tenantId, productAlias, NumberPoolType.ClaimNumbers.ToString());

            if (this.ShouldPerformAlertCheck(currentInstantTicks, lastCheckedTimestamp))
            {
                await this.PerformClaimNumberThresholdAlertCheck(tenantId, productId, productAlias, environment);
            }
        }

        /// <inheritdoc/>
        public void QueueCreditNoteNumberThresholdAlertCheck(
            Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            // TODO: Implemented in UB-2231
        }

        /// <inheritdoc/>
        [JobDisplayName("Invoice Number Threshold Alert Check | TENANT: {3}, PRODUCT: {4}, ENVIRONMENT: {2}")]
        public async Task TriggerInvoiceNumberThresholdAlertCheck(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            string tenantAlias,
            string productAlias)
        {
            var applicableAlerts = this.systemAlertRepository.GetApplicableAlerts(
                tenantId, productId, SystemAlertType.InvoiceNumbers);
            var productName = await this.GetProductName(tenantId, productId);
            var numberRemaining = this.invoiceNumberRepository.GetAvailableReferenceNumbersCount(tenantId, productId, environment);
            if (this.IsCriticalAlertTriggered(applicableAlerts, numberRemaining))
            {
                await this.SendEmailInvoiceNumbersInCriticalThreshold(
                    tenantId, productName, environment, numberRemaining);
            }
            else if (this.IsWarningAlertTriggered(applicableAlerts, numberRemaining))
            {
                await this.SendEmailInvoiceNumbersInWarningThreshold(
                    tenantId, productName, environment, numberRemaining);
            }
        }

        /// <inheritdoc/>
        [JobDisplayName("Policy Number Threshold Alert Check | TENANT: {3}, PRODUCT: {4}, ENVIRONMENT: {2}")]
        public async Task TriggerPolicyNumberThresholdAlertCheck(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            string tenantAlias,
            string productAlias)
        {
            var isMutual = TenantHelper.IsMutual(tenantAlias);
            var applicableAlerts = this.systemAlertRepository.GetApplicableAlerts(
                tenantId, productId, SystemAlertType.PolicyNumbers);
            var productName = await this.GetProductName(tenantId, productId);
            var numberRemaining = this.policyNumberRepository.GetAvailableReferenceNumbersCount(tenantId, productId, environment);
            if (this.IsCriticalAlertTriggered(applicableAlerts, numberRemaining))
            {
                await this.SendEmailPolicyNumbersInCriticalThreshold(
                    tenantId,
                    productName,
                    environment,
                    numberRemaining,
                    isMutual);
            }
            else if (this.IsWarningAlertTriggered(applicableAlerts, numberRemaining))
            {
                await this.SendEmailPolicyNumbersInWarningThreshold(
                     tenantId,
                     productName,
                     environment,
                     numberRemaining,
                     isMutual);
            }
        }

        /// <inheritdoc/>
        [JobDisplayName("Claim Number Threshold Alert Check | TENANT: {3}, PRODUCT: {4}, ENVIRONMENT: {2}")]
        public async Task TriggerClaimNumberThresholdAlertCheck(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            string tenantAlias,
            string productAlias)
        {
            var applicableAlerts = this.systemAlertRepository.GetApplicableAlerts(
                tenantId, productId, SystemAlertType.ClaimNumbers);
            var productName = await this.GetProductName(tenantId, productId);
            var claimNumberCount = this.claimNumberRepository.GetAvailableReferenceNumbersCount(tenantId, productId, environment);
            if (this.IsCriticalAlertTriggered(applicableAlerts, claimNumberCount))
            {
                await this.SendEmailClaimNumbersInCriticalThreshold(
                    tenantId,
                    productName,
                    environment,
                    claimNumberCount);
            }
            else if (this.IsWarningAlertTriggered(applicableAlerts, claimNumberCount))
            {
                await this.SendEmailClaimNumbersInWarningThreshold(
                    tenantId,
                    productName,
                    environment,
                    claimNumberCount);
            }
        }

        private SystemAlert GetSystemAlert(Guid tenantId, Guid systemAlertId)
        {
            SystemAlert systemAlert = this.systemAlertRepository.GetSystemAlertById(tenantId, systemAlertId);
            if (systemAlert == null)
            {
                throw new NotFoundException(Errors.General.NotFound("system alert", systemAlertId));
            }

            return systemAlert;
        }

        private bool IsWarningAlertTriggered(IEnumerable<SystemAlert> applicableAlerts, int numberCount)
        {
            return this.IsAlertTriggered(
                applicableAlerts,
                alert => alert?.HasWarningThreshold(),
                alert => alert.IsAtWarningThreshold(numberCount));
        }

        private bool IsCriticalAlertTriggered(IEnumerable<SystemAlert> applicableAlerts, int numberCount)
        {
            return this.IsAlertTriggered(
                applicableAlerts,
                alert => alert?.HasCriticalThreshold(),
                alert => alert.IsAtOrBelowCriticalThreshold(numberCount));
        }

        private bool IsAlertTriggered(
            IEnumerable<SystemAlert> applicableAlerts,
            Func<SystemAlert, bool?> hasThreshold,
            Func<SystemAlert, bool> isAtThreshold)
        {
            var productSpecificAlert = applicableAlerts.SingleOrDefault(alert => alert.ProductId != null);
            if (hasThreshold(productSpecificAlert) == true)
            {
                return isAtThreshold(productSpecificAlert);
            }

            var tenantSpecificAlert = applicableAlerts.SingleOrDefault(
                alert => alert.ProductId == null && alert.TenantId != default
                && alert.TenantId != Domain.Tenant.MasterTenantId);
            if (hasThreshold(tenantSpecificAlert) == true)
            {
                return isAtThreshold(tenantSpecificAlert);
            }

            var ubindDefaultAlert = applicableAlerts.SingleOrDefault(
                alert => alert.TenantId == Domain.Tenant.MasterTenantId);
            if (hasThreshold(ubindDefaultAlert) == true)
            {
                return isAtThreshold(ubindDefaultAlert);
            }

            return false;
        }

        /// <summary>
        /// Send Email Invoice numbers in warning threshold.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productName">The Product name.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="remainingNumber">The number of invoice numbers remaining.</param>
        private async Task SendEmailInvoiceNumbersInWarningThreshold(
            Guid tenantId, string productName, DeploymentEnvironment environment, int remainingNumber)
        {
            var tenantName = await this.GetTenantName(tenantId);
            var organisationId = await this.GetTenanDefaultOrganisationId(tenantId);
            var emailParameters = new { tenantName, remainingNumber, productName };
            var subject = string.Format(
                "Warning: The invoice numbers for {0} - {1} are about to run out in {2} environment",
                tenantName,
                productName,
                environment);
            MimeMessage mailMessage = this.emailComposerService.ComposeMailMessage(
                this.systemAlertConfiguration.From,
                this.systemAlertConfiguration.To,
                this.systemAlertConfiguration.CC,
                subject,
                "InvoiceNumbersInWarningThreshold.cshtml",
                emailParameters);
            this.messagingService.Send(tenantId, mailMessage, organisationId);
        }

        /// <summary>
        /// Get Instance of System Alert.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productName">The Product instance.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="remainingNumber">The number of invoice numbers remaining.</param>
        private async Task SendEmailInvoiceNumbersInCriticalThreshold(
            Guid tenantId, string productName, DeploymentEnvironment environment, int remainingNumber)
        {
            var tenantName = await this.GetTenantName(tenantId);
            var organisationId = await this.GetTenanDefaultOrganisationId(tenantId);
            var emailParameters = new { tenantName, remainingNumber, productName };
            var subject = string.Format(
                "Critical: The invoice numbers for {0} - {1} will run out imminently in {2} environment",
                tenantName,
                productName,
                environment);
            var mailMessage = this.emailComposerService.ComposeMailMessage(
                this.systemAlertConfiguration.From,
                this.systemAlertConfiguration.To,
                this.systemAlertConfiguration.CC,
                subject,
                "InvoiceNumbersInCriticalThreshold.cshtml",
                emailParameters);
            this.messagingService.Send(tenantId, mailMessage, organisationId);
        }

        /// <summary>
        /// Send Email Policy numbers in warning threshold.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productName">The Product name.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="remainingNumber">The number of policy numbers remaining.</param>
        /// <param name="isMutual">If a mutual tenant.</param>
        private async Task SendEmailPolicyNumbersInWarningThreshold(
            Guid tenantId, string productName, DeploymentEnvironment environment, int remainingNumber, bool isMutual)
        {
            var tenantName = await this.GetTenantName(tenantId);
            var organisationId = await this.GetTenanDefaultOrganisationId(tenantId);
            var emailParameters = new { tenantName, remainingNumber, productName };
            var subject = TenantHelper.CheckAndChangeTextToMutual(
                string.Format(
                    "Warning: The policy numbers for {0} - {1} are about to run out in {2} environment",
                    tenantName,
                    productName,
                    environment), isMutual);
            MimeMessage mailMessage = this.emailComposerService.ComposeMailMessage(
                this.systemAlertConfiguration.From,
                this.systemAlertConfiguration.To,
                this.systemAlertConfiguration.CC,
                subject,
                "PolicyNumbersInWarningThreshold.cshtml",
                emailParameters);
            this.messagingService.Send(tenantId, mailMessage, organisationId);
        }

        /// <summary>
        /// Send Email Policy numbers in critical threshold.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productName">The Product name.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="remainingNumber">The number of policy numbers remaining.</param>
        /// <param name="isMutual">If a mutual tenant.</param>
        private async Task SendEmailPolicyNumbersInCriticalThreshold(
            Guid tenantId, string productName, DeploymentEnvironment environment, int remainingNumber, bool isMutual)
        {
            var tenantName = await this.GetTenantName(tenantId);
            var organisationId = await this.GetTenanDefaultOrganisationId(tenantId);
            var emailParameters = new { tenantName, remainingNumber, productName };
            var subject = TenantHelper.CheckAndChangeTextToMutual(
                string.Format(
                    "Critical: The policy numbers for {0} - {1} will run out imminently in {2} environment",
                    tenantName,
                    productName,
                    environment), isMutual);
            var mailMessage = this.emailComposerService.ComposeMailMessage(
                this.systemAlertConfiguration.From,
                this.systemAlertConfiguration.To,
                this.systemAlertConfiguration.CC,
                subject,
                "PolicyNumbersInCriticalThreshold.cshtml",
                emailParameters);
            this.messagingService.Send(tenantId, mailMessage, organisationId);
        }

        /// <summary>
        /// Send Email that Claim numbers are in warning threshold.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productName">The Product instance.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="remainingNumber">The number of claim numbers remaining.</param>
        /// <returns>MailMessage.</returns>
        private async Task<MimeMessage> SendEmailClaimNumbersInWarningThreshold(
            Guid tenantId, string productName, DeploymentEnvironment environment, int remainingNumber)
        {
            var tenantName = await this.GetTenantName(tenantId);
            var organisationId = await this.GetTenanDefaultOrganisationId(tenantId);
            var emailParameters = new { tenantName, remainingNumber, productName };
            var subject = string.Format(
                "Warning: The claim numbers for {0} - {1} are about to run out in {2} environment",
                tenantName,
                productName,
                environment);
            var mailMessage = this.emailComposerService.ComposeMailMessage(
                this.systemAlertConfiguration.From,
                this.systemAlertConfiguration.To,
                this.systemAlertConfiguration.CC,
                subject,
                "ClaimNumbersInWarningThreshold.cshtml",
                emailParameters);
            this.messagingService.Send(tenantId, mailMessage, organisationId);

            return mailMessage;
        }

        /// <summary>
        /// Send Email that Claim numbers are in critical threshold.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productName">The Product instance.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="remainingNumber">The number of claim numbers remaining.</param>
        /// <returns>MailMessage.</returns>
        private async Task<MimeMessage> SendEmailClaimNumbersInCriticalThreshold(
            Guid tenantId, string productName, DeploymentEnvironment environment, int remainingNumber)
        {
            var tenantName = await this.GetTenantName(tenantId);
            var organisationId = await this.GetTenanDefaultOrganisationId(tenantId);

            var emailParameters = new { tenantName, remainingNumber, productName };
            var subject = string.Format(
                "Critical: The claim numbers for {0} - {1} will run out imminently in {2} environment",
                tenantName,
                productName,
                environment);

            var mailMessage = this.emailComposerService.ComposeMailMessage(
                this.systemAlertConfiguration.From,
                this.systemAlertConfiguration.To,
                this.systemAlertConfiguration.CC,
                subject,
                "ClaimNumbersInCriticalThreshold.cshtml",
                emailParameters);

            this.messagingService.Send(tenantId, mailMessage, organisationId);

            return mailMessage;
        }

        private async Task<string> GetTenantName(Guid tenantId)
        {
            var tenant = await this.cachingResolver.GetTenantOrNull(tenantId);
            return tenant?.Details.Name ?? tenantId.ToString();
        }

        private async Task<Guid> GetTenanDefaultOrganisationId(Guid tenantId)
        {
            var tenant = await this.cachingResolver.GetTenantOrNull(tenantId);
            return tenant?.Details.DefaultOrganisationId ?? default;
        }

        private async Task<string> GetProductName(Guid tenantId, Guid productId)
        {
            var product = await this.cachingResolver.GetProductOrNull(tenantId, productId);
            return product?.Details.Name ?? productId.ToString();
        }

        /// <summary>
        /// try to lazy load system alert creation.
        /// </summary>
        /// <param name="tenantId">the tenant id.</param>
        /// <param name="alerts">alerts list.</param>
        /// <param name="productId">product id optional.</param>
        private async Task LazyLoadCreation(Guid tenantId, List<SystemAlert> alerts, Guid? productId = null)
        {
            if (Enum.GetValues(typeof(SystemAlertType)).Length != alerts.Count)
            {
                var savedAlerts = alerts.Select(x => x.Type).ToList();
                var existingTypes = Enum.GetValues(typeof(SystemAlertType)).OfType<SystemAlertType>().ToList();

                var difference = existingTypes.Except(savedAlerts).ToList();
                var systemAlerts = await this.CreateSystemAlerts(tenantId, difference, productId);
                alerts.AddRange(systemAlerts);
            }
        }

        /// <summary>
        /// create system alerts.
        /// </summary>
        /// <param name="tenantId">the tenant id.</param>
        /// <param name="systemAlertTypes">the alert types.</param>
        /// <param name="productId">product id optional.</param>
        /// <returns>system alerts created.</returns>
        private async Task<List<SystemAlert>> CreateSystemAlerts(
            Guid tenantId, List<SystemAlertType> systemAlertTypes, Guid? productId = null)
        {
            List<SystemAlert> alerts = new List<SystemAlert>();

            Domain.Tenant tenant = this.tenantService.GetTenant(tenantId);
            Domain.Product.Product product = null;

            if (productId.HasValue)
            {
                product = await this.cachingResolver.GetProductOrNull(tenantId, productId.Value);
            }

            foreach (SystemAlertType systemAlertType in systemAlertTypes)
            {
                var newAlert = productId.HasValue
                    ? new SystemAlert(
                        tenant.Id, product?.Id, systemAlertType, this.clock.Now())
                    : new SystemAlert(tenant.Id, systemAlertType, this.clock.Now());

                this.systemAlertRepository.AddAlert(newAlert);
                alerts.Add(newAlert);
            }

            this.systemAlertRepository.SaveChanges();

            return alerts;
        }

        private async Task EmitEventIfTenantSettingIsModified(Guid tenantId, SystemAlert systemAlert)
        {
            if (systemAlert == null)
            {
                return;
            }

            if (!systemAlert.ProductId.HasValue)
            {
                // we know that this is a tenant-wide system alert setting if it's not one that is stored against a product
                // so we can make a tenant modified system event.
                await this.tenantSystemEventEmitter
                    .CreateAndEmitSystemEvent(tenantId, SystemEventType.TenantModified);
            }
        }

        private async Task PerformPolicyNumberThresholdAlertCheck(
            Guid tenantId,
            Guid productId,
            string productAlias,
            DeploymentEnvironment environment)
        {
            Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
            await this.numberPoolCountLastCheckedTimestampRepository.UpsertLastCheckedTimestamp(tenantId, productAlias, NumberPoolType.PolicyNumbers.ToString(), currentInstant);

            await Task.Run(() => this.backgroundJobClient.Enqueue<SystemAlertService>(
                s => s.TriggerPolicyNumberThresholdAlertCheck(
                    tenantId, productId, environment, tenantAlias, productAlias),
                new ProductContext(tenantId, productId, environment)));
        }

        private async Task PerformInvoiceNumberThresholdAlertCheck(
           Guid tenantId,
           Guid productId,
           string productAlias,
           DeploymentEnvironment environment)
        {
            Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
            await this.numberPoolCountLastCheckedTimestampRepository.UpsertLastCheckedTimestamp(tenantId, productAlias, NumberPoolType.InvoiceNumbers.ToString(), currentInstant);

            await Task.Run(() => this.backgroundJobClient.Enqueue<SystemAlertService>(
                s => s.TriggerInvoiceNumberThresholdAlertCheck(
                    tenantId, productId, environment, tenantAlias, productAlias),
                new ProductContext(tenantId, productId, environment)));
        }

        private async Task PerformClaimNumberThresholdAlertCheck(
           Guid tenantId,
           Guid productId,
           string productAlias,
           DeploymentEnvironment environment)
        {
            Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
            await this.numberPoolCountLastCheckedTimestampRepository.UpsertLastCheckedTimestamp(tenantId, productAlias, NumberPoolType.ClaimNumbers.ToString(), currentInstant);

            await Task.FromResult(this.backgroundJobClient.Enqueue<SystemAlertService>(
                s => s.TriggerClaimNumberThresholdAlertCheck(
                    tenantId, productId, environment, tenantAlias, productAlias),
                new ProductContext(tenantId, productId, environment)));
        }

        private bool ShouldPerformAlertCheck(long currentTime, Instant? lastCheckedTimestamp)
        {
            // Check if lastCheckedTimestamp is null or if the time difference is greater than 30 minutes
            return !lastCheckedTimestamp.HasValue || (currentTime - lastCheckedTimestamp.Value.ToUnixTimeTicks() > TimeSpan.FromMinutes(30).Ticks);
        }
    }
}
