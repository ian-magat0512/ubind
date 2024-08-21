// <copyright file="SystemAlertServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;
    using NodaTime;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Services;
    using UBind.Application.Services.Messaging;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Processing;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Repositories.Redis;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Tests the system alerts.
    /// </summary>
    public class SystemAlertServiceTests
    {
        private static readonly Guid TenantId = TenantFactory.DefaultId;
        private static readonly Guid ProductId = ProductFactory.DefaultId;
        private static readonly string TenantAlias = TenantFactory.DefaultAlias;
        private static readonly string ProducAlias = ProductFactory.DefaultProductAlias;
        private readonly SystemAlertService systemAlertService;
        private Mock<IClock> clock = new Mock<IClock>();
        private Mock<ITenantService> tenantService = new Mock<ITenantService>();
        private Mock<ISystemAlertRepository> systemAlertRepository = new Mock<ISystemAlertRepository>();
        private Mock<IEmailComposer> emailComposerService = new Mock<IEmailComposer>();
        private Mock<IMessagingService> messagingService = new Mock<IMessagingService>();
        private Mock<IESystemAlertConfiguration> systemAlertConfiguration = new Mock<IESystemAlertConfiguration>();
        private Mock<IJobClient> jobClient = new Mock<IJobClient>();
        private Mock<ICachingResolver> cachingResolver = new Mock<ICachingResolver>();
        private Mock<ITenantSystemEventEmitter> tenantSystemEventEmitter = new Mock<ITenantSystemEventEmitter>();
        private Mock<IPolicyNumberRepository> policyNumberRepositoryMock = new Mock<IPolicyNumberRepository>();
        private Mock<IClaimNumberRepository> claimNumberRepositoryMock = new Mock<IClaimNumberRepository>();
        private Mock<IInvoiceNumberRepository> invoiceNumberRepositoryMock = new Mock<IInvoiceNumberRepository>();
        private Mock<INumberPoolCountLastCheckedTimestampRepository> numberPoolCountLastCheckedTimestampRepository = new Mock<INumberPoolCountLastCheckedTimestampRepository>();

        public SystemAlertServiceTests()
        {
            this.systemAlertService = new SystemAlertService(
                this.systemAlertRepository.Object,
                this.clock.Object,
                this.tenantService.Object,
                this.emailComposerService.Object,
                this.messagingService.Object,
                this.cachingResolver.Object,
                this.systemAlertConfiguration.Object,
                this.tenantSystemEventEmitter.Object,
                this.jobClient.Object,
                this.policyNumberRepositoryMock.Object,
                this.claimNumberRepositoryMock.Object,
                this.invoiceNumberRepositoryMock.Object,
                this.numberPoolCountLastCheckedTimestampRepository.Object);
        }

        [Fact]
        public async Task UpdateSystemAlert_Succeeds()
        {
            // Arrange
            int warningThreshold = 10;
            int criticalThreshold = 5;

            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            this.systemAlertRepository.Setup(e => e.GetSystemAlertById(systemAlert.TenantId, systemAlert.Id)).Returns(systemAlert);

            // Act
            await this.systemAlertService.UpdateSystemAlert(systemAlert.TenantId, systemAlert.Id, warningThreshold, criticalThreshold);

            // Assert
            this.systemAlertRepository.Verify(m => m.UpdateSystemAlert(systemAlert.TenantId, systemAlert.Id, warningThreshold, criticalThreshold));
        }

        [Fact]
        public async Task QueuePolicyNumberThresholdAlertCheck_UpsertsTimestampIntoRepository_WhenThresholdAlertCheckIsQueued()
        {
            // Arrange
            var tenantId = Guid.Parse("02f6c305-9c6d-4c28-8e9a-4d36544216df");
            var productId = Guid.Parse("46d7ceee-a4d8-4d5a-a68d-33fb885f93c4");
            var environment = DeploymentEnvironment.Development;
            var productAlias = "dev";

            Instant now = SystemClock.Instance.GetCurrentInstant();

            long currentTime = now.ToUnixTimeTicks();
            long lastCheckedTimestampTicks = currentTime + TimeSpan.FromMinutes(-30).Ticks;
            Instant? lastCheckedTimestamp = Instant.FromUnixTimeTicks(lastCheckedTimestampTicks);

            // Mock the behavior of dependencies
            this.cachingResolver.Setup(m => m.GetProductAliasOrThrow(tenantId, productId)).Returns(productAlias);

            this.numberPoolCountLastCheckedTimestampRepository
                 .Setup(s => s.GetLastCheckedTimestamp(tenantId, productAlias, NumberPoolType.PolicyNumbers.ToString()))
                 .Returns(lastCheckedTimestamp);
            this.numberPoolCountLastCheckedTimestampRepository.Setup(m => m.GetLastCheckedTimestamp(tenantId, productAlias, NumberPoolType.PolicyNumbers.ToString())).Returns(lastCheckedTimestamp);

            // Act
            await this.systemAlertService.QueuePolicyNumberThresholdAlertCheck(tenantId, productId, environment);
            await Task.Delay(100);

            // Assert
            this.numberPoolCountLastCheckedTimestampRepository.Verify(
                m => m.UpsertLastCheckedTimestamp(tenantId, productAlias, NumberPoolType.PolicyNumbers.ToString(), It.IsAny<Instant>()),
                Times.Once);
        }

        [Fact]
        public async Task QueuePolicyNumberThresholdAlertCheck_DoesNotUpsertTimestampIntoRepository_WhenThresholdAlertWasRecentlyQueued()
        {
            // Arrange
            var tenantId = Guid.Parse("02f6c305-9c6d-4c28-8e9a-4d36544216df");
            var productId = Guid.Parse("46d7ceee-a4d8-4d5a-a68d-33fb885f93c4");
            var environment = DeploymentEnvironment.Development;
            var productAlias = "dev";

            Instant now = SystemClock.Instance.GetCurrentInstant();

            long currentTime = now.ToUnixTimeTicks();
            long lastCheckedTimestampTicks = currentTime + TimeSpan.FromMinutes(-29).Ticks;
            Instant? lastCheckedTimestamp = Instant.FromUnixTimeTicks(lastCheckedTimestampTicks);

            // Mock the behavior of dependencies
            this.cachingResolver.Setup(m => m.GetProductAliasOrThrow(tenantId, productId)).Returns(productAlias);

            this.numberPoolCountLastCheckedTimestampRepository
                 .Setup(s => s.GetLastCheckedTimestamp(tenantId, productAlias, NumberPoolType.PolicyNumbers.ToString()))
                 .Returns(lastCheckedTimestamp);
            this.numberPoolCountLastCheckedTimestampRepository.Setup(m => m.GetLastCheckedTimestamp(tenantId, productAlias, NumberPoolType.PolicyNumbers.ToString())).Returns(lastCheckedTimestamp);

            // Act
            await this.systemAlertService.QueuePolicyNumberThresholdAlertCheck(tenantId, productId, environment);

            // Assert
            this.numberPoolCountLastCheckedTimestampRepository.Verify(m => m.UpsertLastCheckedTimestamp(tenantId, productAlias, NumberPoolType.PolicyNumbers.ToString(), now), Times.Never);
        }

        // ---- claim

        // Remaining number > warning threshold: No alert
        [Fact]
        public async Task TestTriggerClaimNumberThresholdAlertCheck_WithClaimNumberNotInWarningThreshold_ShouldNotCallWarningAlert()
        {
            // Arrange
            int warningThreshold = 10;
            int criticalThreshold = 5;
            int remainingThreshold = 20;

            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.ClaimNumbers)).Returns(systemAlerts);
            this.claimNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerClaimNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        // Remaining number = warning threshold: Warning alert
        [Fact]
        public async Task TestTriggerClaimNumberThresholdAlertCheck_WithClaimNumberInWarningThreshold_ShouldCallWarningAlert()
        {
            // Arrange
            int warningThreshold = 10;
            int criticalThreshold = 5;
            int remainingThreshold = 10;

            string subject = "Warning: The claim numbers for fake-tenant - fake-product are about to run out in Development environment";
            string claimNumbersInWarningThresholdTemplateFile = "ClaimNumbersInWarningThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.ClaimNumbers)).Returns(systemAlerts);
            this.claimNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerClaimNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, claimNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // Warning threshold > remaining number > critical threshold: No alert
        [Fact]
        public async Task TestTriggerClaimNumberThresholdAlertCheck_NOtInWarningAndCritical_ShouldNOTCallWarningAlert()
        {
            // Arrange
            int warningThreshold = 30;
            int criticalThreshold = 5;
            int remainingThreshold = 20;

            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.ClaimNumbers)).Returns(systemAlerts);
            this.claimNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerClaimNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        // Warning threshold > remaining number = critical threshold: Critical alert
        [Fact]
        public async Task TestTriggerClaimNumberThresholdAlertCheck_InCritical_ShouldCallCriticalAlert()
        {
            // Arrange
            int warningThreshold = 30;
            int criticalThreshold = 5;
            int remainingThreshold = 5;
            string claimNumbersInCriticalThresholdTemplateFile = "ClaimNumbersInCriticalThreshold.cshtml";
            var subject = "Critical: The claim numbers for fake-tenant - fake-product will run out imminently in Development environment";
            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);
            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.ClaimNumbers)).Returns(systemAlerts);
            this.claimNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerClaimNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, claimNumbersInCriticalThresholdTemplateFile, It.IsAny<object>()));
        }

        // Warning threshold > critical threshold ?remaining number: Critical alert
        [Fact]
        public async Task TestTriggerClaimNumberThresholdAlertCheck_InCriticalAndInWarning_ShouldCallCriticalAlert()
        {
            // Arrange
            int warningThreshold = 5;
            int criticalThreshold = 5;
            int remainingThreshold = 5;

            string claimNumbersInCriticalThresholdTemplateFile = "ClaimNumbersInCriticalThreshold.cshtml";
            var subject = "Critical: The claim numbers for fake-tenant - fake-product will run out imminently in Development environment";
            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.ClaimNumbers)).Returns(systemAlerts);
            this.claimNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerClaimNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, claimNumbersInCriticalThresholdTemplateFile, It.IsAny<object>()));
        }

        // Remaining number > warning threshold: No alert
        [Fact]
        public async Task TestTriggerInvoiceNumberThresholdAlertCheck_WithInvoiceNumberNotInWarningThreshold_ShouldNotCallWarningAlert()
        {
            // Arrange
            int warningThreshold = 10;
            int criticalThreshold = 5;
            int remainingThreshold = 20;

            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.InvoiceNumbers)).Returns(systemAlerts);
            this.invoiceNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerInvoiceNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        // -- Cascading Test

        // Threshold from system is used when no tenant and no product settings are persisted.
        [Fact]
        public async Task TestTriggerClaimNumberThresholdAlertCheck_WithClaimNumberInWarningThresholdInTenant_ShouldCallTenantWarningAlert()
        {
            // Arrange
            int masterTenantWarningThreshold = 10;
            int masterTenantCriticalThreshold = 5;

            int remainingThreshold = 10;

            string masterTenantsubject = "Warning: The claim numbers for ubind - fake-product are about to run out in Development environment";
            string claimNumbersInWarningThresholdTemplateFile = "ClaimNumbersInWarningThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var masterTenantSystemAlert = new SystemAlert(Tenant.MasterTenantId, null, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            masterTenantSystemAlert.Update(masterTenantWarningThreshold, masterTenantCriticalThreshold);
            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { masterTenantSystemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.ClaimNumbers)).Returns(systemAlerts);

            var tenant = new Tenant(Tenant.MasterTenantId, "ubind", "ubind", null, default, default, default);
            var product = new Domain.Product.Product(TenantId, ProductId, "fake-product", "fake-product", default);
            this.cachingResolver.Setup(x => x.GetTenantOrNull(TenantId)).Returns(Task.FromResult(tenant));
            this.cachingResolver.Setup(x => x.GetProductOrNull(TenantId, ProductId)).Returns(Task.FromResult(product));
            this.claimNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerClaimNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), masterTenantsubject, claimNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // Threshold from Tenant is used when with tenant and no product settings are persisted.
        [Fact]
        public async Task TestTriggerClaimNumberThresholdAlertCheck_WithClaimNumberInWarningThresholdInMasterTenant_ShouldCallMasterTenantWarningAlert()
        {
            // Arrange
            int tenantWarningThreshold = 10;
            int tenantCriticalThreshold = 5;

            int masterTenantWarningThreshold = 10;
            int masterTenantCriticalThreshold = 5;

            int remainingThreshold = 10;

            string tenantSubject = "Warning: The claim numbers for fake-tenant - fake-product are about to run out in Development environment";
            string claimNumbersInWarningThresholdTemplateFile = "ClaimNumbersInWarningThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var masterTenantSystemAlert = new SystemAlert(Tenant.MasterTenantId, null, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            var tenantSystemAlert = new SystemAlert(TenantId, null, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            tenantSystemAlert.Update(tenantWarningThreshold, tenantCriticalThreshold);
            masterTenantSystemAlert.Update(masterTenantWarningThreshold, masterTenantCriticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { masterTenantSystemAlert, tenantSystemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.ClaimNumbers)).Returns(systemAlerts);
            this.claimNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerClaimNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), tenantSubject, claimNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // Threshold from product should override threshold from tenant.
        [Fact]
        public async Task TestTriggerClaimNumberThresholdAlertCheck_WithClaimNumberInWarningInProduct_ShouldCallProductWarningAlert()
        {
            // Arrange
            int warningThreshold = 8;
            int criticalThreshold = 5;

            int masterTenantWarningThreshold = 8;
            int masterTenantCriticalThreshold = 5;

            int productWarningThreshold = 10;
            int productCriticalThreshold = 5;

            int remainingThreshold = 10;

            string productSubject = "Warning: The claim numbers for fake-tenant - fake-product are about to run out in Development environment";
            string claimNumbersInWarningThresholdTemplateFile = "ClaimNumbersInWarningThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var masterTenantSystemAlert = new SystemAlert(Tenant.MasterTenantId, null, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            var tenantSystemAlert = new SystemAlert(TenantId, null, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            var productSystemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            tenantSystemAlert.Update(warningThreshold, criticalThreshold);
            masterTenantSystemAlert.Update(masterTenantWarningThreshold, masterTenantCriticalThreshold);
            productSystemAlert.Update(productWarningThreshold, productCriticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { masterTenantSystemAlert, tenantSystemAlert, productSystemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.ClaimNumbers)).Returns(systemAlerts);
            this.claimNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerClaimNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), productSubject, claimNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // Threshold from product should override threshold from tenant.
        [Fact]
        public async Task TestTriggerClaimNumberThresholdAlertCheck_WithClaimNumberNotInWarningInProduct_ShouldNotCallAnyAlert()
        {
            // Arrange
            int warningThreshold = 10;
            int criticalThreshold = 5;

            int masterTenantWarningThreshold = 10;
            int masterTenantCriticalThreshold = 5;

            int productWarningThreshold = 8;
            int productCriticalThreshold = 5;

            int remainingThreshold = 10;

            var environment = DeploymentEnvironment.Development;
            var masterTenantSystemAlert = new SystemAlert(Tenant.MasterTenantId, null, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            var tenantSystemAlert = new SystemAlert(TenantId, null, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            var productSystemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            tenantSystemAlert.Update(warningThreshold, criticalThreshold);
            masterTenantSystemAlert.Update(masterTenantWarningThreshold, masterTenantCriticalThreshold);
            productSystemAlert.Update(productWarningThreshold, productCriticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { masterTenantSystemAlert, tenantSystemAlert, productSystemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.ClaimNumbers)).Returns(systemAlerts);
            this.claimNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerClaimNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task TriggerClaimNumberThresholdAlertCheck_WithCriticalThresholdOnlyReached_ShouldCallCriticalAlert()
        {
            // Arrange
            int? warningThreshold = null;
            int criticalThreshold = 5;
            int remainingThreshold = 5;
            var claimNumbersInCriticalThresholdTemplateFile = "ClaimNumbersInCriticalThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var subject = $"Critical: The claim numbers for {TenantId} - {ProductId} will run out imminently in {environment} environment";
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository
                .Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.ClaimNumbers))
                .Returns(systemAlerts);
            this.claimNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerClaimNumberThresholdAlertCheck(
                TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(
                m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, claimNumbersInCriticalThresholdTemplateFile, It.IsAny<object>()));
        }

        [Fact]
        public async Task TriggerClaimNumberThresholdAlertCheck_WithWarningThresholdOnlyReached_ShouldCallWarningAlert()
        {
            // Arrange
            int warningThreshold = 10;
            int? criticalThreshold = null;
            int remainingThreshold = 10;

            var environment = DeploymentEnvironment.Development;
            var subject = $"Warning: The claim numbers for {TenantId} - {ProductId} are about to run out in {environment} environment";
            var claimNumbersInWarningThresholdTemplateFile = "ClaimNumbersInWarningThreshold.cshtml";
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.ClaimNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository
                .Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.ClaimNumbers))
                .Returns(systemAlerts);
            this.claimNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerClaimNumberThresholdAlertCheck(
                TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(
                m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, claimNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // ---- invoice

        // Remaining number = warning threshold: Warning alert
        [Fact]
        public async Task TestTriggerInvoiceNumberThresholdAlertCheck_WithInvoiceNumberInWarningThreshold_ShouldCallWarningAlert()
        {
            // Arrange
            int warningThreshold = 10;
            int criticalThreshold = 5;
            int remainingThreshold = 10;

            string tenantSubject = "Warning: The invoice numbers for fake-tenant - fake-product are about to run out in Development environment";
            string invoiceNumbersInWarningThresholdTemplateFile = "InvoiceNumbersInWarningThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());

            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.InvoiceNumbers)).Returns(systemAlerts);
            this.invoiceNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerInvoiceNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), tenantSubject, invoiceNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // Warning threshold > remaining number > critical threshold: No alert
        [Fact]
        public async Task TestTriggerInvoiceNumberThresholdAlertCheck_NOtInWarningAndCritical_ShouldNOTCallWarningAlert()
        {
            // Arrange
            int warningThreshold = 30;
            int criticalThreshold = 5;
            int remainingThreshold = 20;

            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.InvoiceNumbers)).Returns(systemAlerts);
            this.invoiceNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerInvoiceNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        // Warning threshold > remaining number = critical threshold: Critical alert
        [Fact]
        public async Task TestTriggerInvoiceNumberThresholdAlertCheck_InCritical_ShouldCallCriticalAlert()
        {
            // Arrange
            int warningThreshold = 30;
            int criticalThreshold = 5;
            int remainingThreshold = 5;
            string invoiceNumbersInCriticalThresholdTemplateFile = "InvoiceNumbersInCriticalThreshold.cshtml";
            var subject = "Critical: The invoice numbers for fake-tenant - fake-product will run out imminently in Development environment";
            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.InvoiceNumbers)).Returns(systemAlerts);
            this.invoiceNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerInvoiceNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, invoiceNumbersInCriticalThresholdTemplateFile, It.IsAny<object>()));
        }

        // Warning threshold > critical threshold ?remaining number: Critical alert
        [Fact]
        public async Task TestTriggerInvoiceNumberThresholdAlertCheck_InCriticalAndInWarning_ShouldCallCriticalAlert()
        {
            // Arrange
            int warningThreshold = 5;
            int criticalThreshold = 5;
            int remainingThreshold = 5;

            string invoiceNumbersInCriticalThresholdTemplateFile = "InvoiceNumbersInCriticalThreshold.cshtml";
            var subject = "Critical: The invoice numbers for fake-tenant - fake-product will run out imminently in Development environment";
            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.InvoiceNumbers)).Returns(systemAlerts);
            this.invoiceNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);
            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerInvoiceNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, invoiceNumbersInCriticalThresholdTemplateFile, It.IsAny<object>()));
        }

        // -- Cascading Test

        // Threshold from system is used when no tenant and no product settings are persisted.
        [Fact]
        public async Task TestTriggerInvoiceNumberThresholdAlertCheck_WithInvoiceNumberInWarningThresholdInTenant_ShouldCallTenantWarningAlert()
        {
            // Arrange
            int masterTenantWarningThreshold = 10;
            int masterTenantCriticalThreshold = 5;

            int remainingThreshold = 10;

            string masterTenantsubject = "Warning: The invoice numbers for ubind - fake-product are about to run out in Development environment";
            string invoiceNumbersInWarningThresholdTemplateFile = "InvoiceNumbersInWarningThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var masterTenantSystemAlert = new SystemAlert(Tenant.MasterTenantId, null, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            masterTenantSystemAlert.Update(masterTenantWarningThreshold, masterTenantCriticalThreshold);
            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { masterTenantSystemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.InvoiceNumbers)).Returns(systemAlerts);

            var tenant = new Tenant(Tenant.MasterTenantId, "ubind", "ubind", null, default, default, default);
            var product = new Domain.Product.Product(TenantId, ProductId, "fake-product", "fake-product", default);
            this.cachingResolver.Setup(x => x.GetTenantOrNull(TenantId)).Returns(Task.FromResult(tenant));
            this.cachingResolver.Setup(x => x.GetProductOrNull(TenantId, ProductId)).Returns(Task.FromResult(product));
            this.invoiceNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerInvoiceNumberThresholdAlertCheck(TenantFactory.DefaultId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), masterTenantsubject, invoiceNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // Threshold from Tenant is used when with tenant and no product settings are persisted.
        [Fact]
        public async Task TestTriggerInvoiceNumberThresholdAlertCheck_WithInvoiceNumberInWarningThresholdInMasterTenant_ShouldCallMasterTenantWarningAlert()
        {
            // Arrange
            int tenantWarningThreshold = 10;
            int tenantCriticalThreshold = 5;

            int masterTenantWarningThreshold = 10;
            int masterTenantCriticalThreshold = 5;

            int remainingThreshold = 10;

            string tenantSubject = "Warning: The invoice numbers for fake-tenant - fake-product are about to run out in Development environment";
            string invoiceNumbersInWarningThresholdTemplateFile = "InvoiceNumbersInWarningThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var masterTenantSystemAlert = new SystemAlert(Tenant.MasterTenantId, null, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            var tenantSystemAlert = new SystemAlert(TenantId, null, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            tenantSystemAlert.Update(tenantWarningThreshold, tenantCriticalThreshold);
            masterTenantSystemAlert.Update(masterTenantWarningThreshold, masterTenantCriticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { masterTenantSystemAlert, tenantSystemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.InvoiceNumbers)).Returns(systemAlerts);
            this.invoiceNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerInvoiceNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), tenantSubject, invoiceNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // Threshold from product should override threshold from tenant.
        [Fact]
        public async Task TestTriggerInvoiceNumberThresholdAlertCheck_WithInvoiceNumberInWarningInProduct_ShouldCallProductWarningAlert()
        {
            // Arrange
            int warningThreshold = 8;
            int criticalThreshold = 5;

            int masterTenantWarningThreshold = 8;
            int masterTenantCriticalThreshold = 5;

            int productWarningThreshold = 10;
            int productCriticalThreshold = 5;

            int remainingThreshold = 10;

            string productSubject = "Warning: The invoice numbers for fake-tenant - fake-product are about to run out in Development environment";
            string invoiceNumbersInWarningThresholdTemplateFile = "InvoiceNumbersInWarningThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var masterTenantSystemAlert = new SystemAlert(Tenant.MasterTenantId, null, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            var tenantSystemAlert = new SystemAlert(TenantId, null, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            var productSystemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            tenantSystemAlert.Update(warningThreshold, criticalThreshold);
            masterTenantSystemAlert.Update(masterTenantWarningThreshold, masterTenantCriticalThreshold);
            productSystemAlert.Update(productWarningThreshold, productCriticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { masterTenantSystemAlert, tenantSystemAlert, productSystemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.InvoiceNumbers)).Returns(systemAlerts);
            this.invoiceNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerInvoiceNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), productSubject, invoiceNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // Threshold from product should override threshold from tenant.
        [Fact]
        public async Task TestTriggerInvoiceNumberThresholdAlertCheck_WithInvoiceNumberNotInWarningInProduct_ShouldNotCallAnyAlert()
        {
            // Arrange
            int warningThreshold = 10;
            int criticalThreshold = 5;

            int masterTenantWarningThreshold = 10;
            int masterTenantCriticalThreshold = 5;

            int productWarningThreshold = 8;
            int productCriticalThreshold = 5;

            int remainingThreshold = 10;

            var environment = DeploymentEnvironment.Development;
            var masterTenantSystemAlert = new SystemAlert(Tenant.MasterTenantId, null, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            var tenantSystemAlert = new SystemAlert(TenantId, null, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            var productSystemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            tenantSystemAlert.Update(warningThreshold, criticalThreshold);
            masterTenantSystemAlert.Update(masterTenantWarningThreshold, masterTenantCriticalThreshold);
            productSystemAlert.Update(productWarningThreshold, productCriticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { masterTenantSystemAlert, tenantSystemAlert, productSystemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.InvoiceNumbers)).Returns(systemAlerts);
            this.invoiceNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerInvoiceNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task TriggerInvoiceNumberThresholdAlertCheck_WithCriticalThresholdOnlyReached_ShouldCallCriticalAlert()
        {
            // Arrange
            int? warningThreshold = null;
            int criticalThreshold = 5;
            int remainingThreshold = 5;

            var invoiceNumbersInCriticalThresholdTemplateFile = "InvoiceNumbersInCriticalThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var subject = $"Critical: The invoice numbers for {TenantId} - {ProductId} will run out imminently in {environment} environment";
            var systemAlert = new SystemAlert(
                TenantId, ProductId, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository
                .Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.InvoiceNumbers))
                .Returns(systemAlerts);
            this.invoiceNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerInvoiceNumberThresholdAlertCheck(
                TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(
                m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, invoiceNumbersInCriticalThresholdTemplateFile, It.IsAny<object>()));
        }

        [Fact]
        public async Task TriggerInvoiceNumberThresholdAlertCheck_WithWarningThresholdOnlyReached_ShouldCallWarningAlert()
        {
            // Arrange
            int warningThreshold = 10;
            int? criticalThreshold = null;
            int remainingThreshold = 10;

            var environment = DeploymentEnvironment.Development;
            var tenantSubject = $"Warning: The invoice numbers for {TenantId} - {ProductId} are about to run out in {environment} environment";
            var invoiceNumbersInWarningThresholdTemplateFile = "InvoiceNumbersInWarningThreshold.cshtml";
            var systemAlert = new SystemAlert(
                TenantId, ProductId, SystemAlertType.InvoiceNumbers, this.clock.Object.GetCurrentInstant());

            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository
                .Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.InvoiceNumbers))
                .Returns(systemAlerts);
            this.invoiceNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerInvoiceNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(
                m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), tenantSubject, invoiceNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // -- policy

        // Warning threshold > critical threshold ?remaining number: Critical alert
        [Fact]
        public async Task TestTriggerPolicyNumberThresholdAlertCheck_WithTenantAndNoProduct_ShouldCallAlertFromTenant()
        {
            // Arrange
            int warningThreshold = 5;
            int criticalThreshold = 5;
            int remainingThreshold = 5;

            string policyNumbersInCriticalThresholdTemplateFile = "PolicyNumbersInCriticalThreshold.cshtml";
            var subject = "Critical: The policy numbers for fake-tenant - fake-product will run out imminently in Development environment";
            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.PolicyNumbers)).Returns(systemAlerts);
            this.policyNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerPolicyNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, policyNumbersInCriticalThresholdTemplateFile, It.IsAny<object>()));
        }

        // Remaining number > warning threshold: No alert
        [Fact]
        public async Task TestTriggerPolicyNumberThresholdAlertCheck_WithPolicyNumberNotInWarningThreshold_ShouldNotCallWarningAlert()
        {
            // Arrange
            int warningThreshold = 10;
            int criticalThreshold = 5;
            int remainingThreshold = 20;

            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.PolicyNumbers)).Returns(systemAlerts);
            this.policyNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerPolicyNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        // Remaining number = warning threshold: Warning alert
        [Fact]
        public async Task TestTriggerPolicyNumberThresholdAlertCheck_WithPolicyNumberInWarningThreshold_ShouldCallWarningAlert()
        {
            // Arrange
            int warningThreshold = 10;
            int criticalThreshold = 5;
            int remainingThreshold = 10;

            string subject = "Warning: The policy numbers for fake-tenant - fake-product are about to run out in Development environment";
            string policyNumbersInWarningThresholdTemplateFile = "PolicyNumbersInWarningThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.PolicyNumbers)).Returns(systemAlerts);
            this.policyNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerPolicyNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, policyNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // Warning threshold > remaining number > critical threshold: No alert
        [Fact]
        public async Task TestTriggerPolicyNumberThresholdAlertCheck_NOtInWarningAndCritical_ShouldNOTCallWarningAlert()
        {
            // Arrange
            int warningThreshold = 30;
            int criticalThreshold = 5;
            int remainingThreshold = 20;

            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.PolicyNumbers)).Returns(systemAlerts);
            this.policyNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerPolicyNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        // Warning threshold > remaining number = critical threshold: Critical alert
        [Fact]
        public async Task TestTriggerPolicyNumberThresholdAlertCheck_InCritical_ShouldCallCriticalAlert()
        {
            // Arrange
            int warningThreshold = 30;
            int criticalThreshold = 5;
            int remainingThreshold = 5;
            string policyNumbersInCriticalThresholdTemplateFile = "PolicyNumbersInCriticalThreshold.cshtml";
            var subject = "Critical: The policy numbers for fake-tenant - fake-product will run out imminently in Development environment";
            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.PolicyNumbers)).Returns(systemAlerts);
            this.policyNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerPolicyNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, policyNumbersInCriticalThresholdTemplateFile, It.IsAny<object>()));
        }

        // Warning threshold > critical threshold ?remaining number: Critical alert
        [Fact]
        public async Task TestTriggerPolicyNumberThresholdAlertCheck_InCriticalAndInWarning_ShouldCallCriticalAlert()
        {
            // Arrange
            int warningThreshold = 5;
            int criticalThreshold = 5;
            int remainingThreshold = 5;

            string policyNumbersInCriticalThresholdTemplateFile = "PolicyNumbersInCriticalThreshold.cshtml";
            var subject = "Critical: The policy numbers for fake-tenant - fake-product will run out imminently in Development environment";
            var environment = DeploymentEnvironment.Development;
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.PolicyNumbers)).Returns(systemAlerts);
            this.policyNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            this.SetCachingResolver();

            // Act
            await this.systemAlertService.TriggerPolicyNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, policyNumbersInCriticalThresholdTemplateFile, It.IsAny<object>()));
        }

        // -- Cascading Test

        // Threshold from system is used when no tenant and no product settings are persisted.
        [Fact]
        public async Task TestTriggerPolicyNumberThresholdAlertCheck_WithPolicyNumberInWarningThresholdInTenant_ShouldCallTenantWarningAlert()
        {
            // Arrange
            int masterTenantWarningThreshold = 10;
            int masterTenantCriticalThreshold = 5;
            int remainingThreshold = 10;

            string policyNumbersInWarningThresholdTemplateFile = "PolicyNumbersInWarningThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var masterTenantSystemAlert = new SystemAlert(Tenant.MasterTenantId, null, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            masterTenantSystemAlert.Update(masterTenantWarningThreshold, masterTenantCriticalThreshold);
            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { masterTenantSystemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.PolicyNumbers)).Returns(systemAlerts);
            this.policyNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerPolicyNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), policyNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // Threshold from Tenant is used when with tenant and no product settings are persisted.
        [Fact]
        public async Task TestTriggerPolicyNumberThresholdAlertCheck_WithPolicyNumberInWarningThresholdInMasterTenant_ShouldCallMasterTenantWarningAlert()
        {
            // Arrange
            int tenantWarningThreshold = 10;
            int tenantCriticalThreshold = 5;

            int masterTenantWarningThreshold = 10;
            int masterTenantCriticalThreshold = 5;

            int remainingThreshold = 10;

            string policyNumbersInWarningThresholdTemplateFile = "PolicyNumbersInWarningThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var masterTenantSystemAlert = new SystemAlert(Tenant.MasterTenantId, null, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            var tenantSystemAlert = new SystemAlert(TenantId, null, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            tenantSystemAlert.Update(tenantWarningThreshold, tenantCriticalThreshold);
            masterTenantSystemAlert.Update(masterTenantWarningThreshold, masterTenantCriticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { masterTenantSystemAlert, tenantSystemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.PolicyNumbers)).Returns(systemAlerts);
            this.policyNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerPolicyNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), policyNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // Threshold from product should override threshold from tenant.
        [Fact]
        public async Task TestTriggerPolicyNumberThresholdAlertCheck_WithPolicyNumberInWarningInProduct_ShouldCallProductWarningAlert()
        {
            // Arrange
            int warningThreshold = 8;
            int criticalThreshold = 5;

            int masterTenantWarningThreshold = 8;
            int masterTenantCriticalThreshold = 5;

            int productWarningThreshold = 10;
            int productCriticalThreshold = 5;

            int remainingThreshold = 10;

            string policyNumbersInWarningThresholdTemplateFile = "PolicyNumbersInWarningThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var masterTenantSystemAlert = new SystemAlert(Tenant.MasterTenantId, null, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            var tenantSystemAlert = new SystemAlert(TenantId, null, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            var productSystemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            tenantSystemAlert.Update(warningThreshold, criticalThreshold);
            masterTenantSystemAlert.Update(masterTenantWarningThreshold, masterTenantCriticalThreshold);
            productSystemAlert.Update(productWarningThreshold, productCriticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { masterTenantSystemAlert, tenantSystemAlert, productSystemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.PolicyNumbers)).Returns(systemAlerts);
            this.policyNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerPolicyNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), policyNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        // Threshold from product should override threshold from tenant.
        [Fact]
        public async Task TestTriggerPolicyNumberThresholdAlertCheck_WithPolicyNumberNotInWarningInProduct_ShouldNotCallAnyAlert()
        {
            // Arrange
            int warningThreshold = 10;
            int criticalThreshold = 5;

            int masterTenantWarningThreshold = 10;
            int masterTenantCriticalThreshold = 5;

            int productWarningThreshold = 8;
            int productCriticalThreshold = 5;

            int remainingThreshold = 10;

            var environment = DeploymentEnvironment.Development;
            var masterTenantSystemAlert = new SystemAlert(Tenant.MasterTenantId, null, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            var tenantSystemAlert = new SystemAlert(TenantId, null, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            var productSystemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            tenantSystemAlert.Update(warningThreshold, criticalThreshold);
            masterTenantSystemAlert.Update(masterTenantWarningThreshold, masterTenantCriticalThreshold);
            productSystemAlert.Update(productWarningThreshold, productCriticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { masterTenantSystemAlert, tenantSystemAlert, productSystemAlert };
            this.systemAlertRepository.Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.PolicyNumbers)).Returns(systemAlerts);
            this.policyNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerPolicyNumberThresholdAlertCheck(TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task TriggerPolicyNumberThresholdAlertCheck_WithCriticalThresholdOnlyReached_ShouldCallCriticalAlert()
        {
            // Arrange
            int? warningThreshold = null;
            int criticalThreshold = 5;
            int remainingThreshold = 5;

            var policyNumbersInCriticalThresholdTemplateFile = "PolicyNumbersInCriticalThreshold.cshtml";
            var environment = DeploymentEnvironment.Development;
            var subject = $"Critical: The policy numbers for {TenantId} - {ProductId} will run out imminently in {environment} environment";
            var systemAlert = new SystemAlert(
                TenantId, ProductId, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository
                .Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.PolicyNumbers))
                .Returns(systemAlerts);
            this.policyNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerPolicyNumberThresholdAlertCheck(
                TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(
                m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, policyNumbersInCriticalThresholdTemplateFile, It.IsAny<object>()));
        }

        [Fact]
        public async Task TriggerPolicyNumberThresholdAlertCheck_WithWarningThresholdOnlyReached_ShouldCallWarningAlert()
        {
            // Arrange
            int warningThreshold = 10;
            int? criticalThreshold = null;
            int remainingThreshold = 10;

            var environment = DeploymentEnvironment.Development;
            var subject = $"Warning: The policy numbers for {TenantId} - {ProductId} are about to run out in {environment} environment";
            var policyNumbersInWarningThresholdTemplateFile = "PolicyNumbersInWarningThreshold.cshtml";
            var systemAlert = new SystemAlert(TenantId, ProductId, SystemAlertType.PolicyNumbers, this.clock.Object.GetCurrentInstant());
            systemAlert.Update(warningThreshold, criticalThreshold);

            IEnumerable<SystemAlert> systemAlerts = new SystemAlert[] { systemAlert };
            this.systemAlertRepository
                .Setup(e => e.GetApplicableAlerts(TenantId, ProductId, SystemAlertType.PolicyNumbers))
                .Returns(systemAlerts);
            this.policyNumberRepositoryMock.Setup(e => e.GetAvailableReferenceNumbersCount(TenantId, ProductId, environment)).Returns(remainingThreshold);

            // Act
            await this.systemAlertService.TriggerPolicyNumberThresholdAlertCheck(
                TenantId, ProductId, environment, TenantAlias, ProducAlias);

            // Assert
            this.emailComposerService.Verify(
                m => m.ComposeMailMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), subject, policyNumbersInWarningThresholdTemplateFile, It.IsAny<object>()));
        }

        private void SetCachingResolver(string tenantName = null)
        {
            var tenant = new Tenant(
                TenantId,
                tenantName ?? "fake-tenant",
                tenantName ?? "fake-tenant",
                null,
                default,
                default,
                default);
            var product = new Domain.Product.Product(TenantId, ProductId, "fake-product", "fake-product", default);
            this.cachingResolver.Setup(x => x.GetTenantOrNull(TenantId)).Returns(Task.FromResult(tenant));
            this.cachingResolver.Setup(x => x.GetProductOrNull(TenantId, ProductId)).Returns(Task.FromResult(product));
        }
    }
}
