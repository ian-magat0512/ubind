// <copyright file="SystemEventObservableUnitTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests.Application.SystemEvents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application;
    using UBind.Application.Report;
    using UBind.Application.SystemEvents;
    using UBind.Application.SystemEvents.Payload;
    using UBind.Domain;
    using UBind.Domain.Authentication;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class SystemEventObservableUnitTests
    {
        private ProductContext productContext = new ProductContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DeploymentEnvironment.Development);

        private IDisposable productServiceSubscription;
        private IDisposable reportServiceSubscription;

        [Fact]
        public void Trigger_SuccessfulCallToMultipleSubscribers_IfHaveTwoSubscribers()
        {
            // Arrange
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var systemEvent = SystemEvent.CreateWithPayload(
                this.productContext.TenantId,
                Guid.NewGuid(),
                this.productContext.ProductId,
                this.productContext.Environment,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload,
                new TestClock().Now());
            var type = systemEvent.GetType();
            ISystemEventObservable systemEventObservable = new SystemEventObservable();
            TestProductService productService = new TestProductService();
            TestReportService reportService = new TestReportService();
            this.productServiceSubscription =
                 systemEventObservable
                 .Where(x => x.EventType == SystemEventType.CustomerExpiredQuoteOpened) // subscribed to specific event.
                 .Subscribe(productService);
            this.reportServiceSubscription =
                 systemEventObservable
                 .Where(x => x.EventType == SystemEventType.CustomerExpiredQuoteOpened) // subscribed to specific event.
                 .Subscribe(reportService);

            // Act
            systemEventObservable.Trigger(systemEvent);

            // Assert
            productService.Value.Should().Be(systemEvent.PayloadJson);
            reportService.Value.Should().Be(systemEvent.PayloadJson);
        }

        [Fact]
        public void Trigger_NoSubscribersCalled_IfObservableIsDisposed()
        {
            // Arrange
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var systemEvent = SystemEvent.CreateWithPayload(
                this.productContext.TenantId,
                Guid.NewGuid(),
                this.productContext.ProductId,
                this.productContext.Environment,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload,
                new TestClock().Now());

            ISystemEventObservable systemEventObservable = new SystemEventObservable();
            TestProductService productService = new TestProductService();
            TestReportService reportService = new TestReportService();
            this.productServiceSubscription =
                 systemEventObservable
                  .Where(x => x.EventType == SystemEventType.CustomerExpiredQuoteOpened) // subscribed to specific event.
                 .Subscribe(productService);
            this.reportServiceSubscription =
                 systemEventObservable
                 .Where(x => x.EventType == SystemEventType.CustomerExpiredQuoteOpened) // subscribed to specific event.
                 .Subscribe(reportService);

            systemEventObservable.Trigger(systemEvent); // trigger once
            systemEventObservable.Dispose(); // then dispose

            // Act
            systemEventObservable.Trigger(systemEvent); // trigger again, check if triggered

            // Assert
            "done".Should().Be(productService.Value);
            "done".Should().Be(reportService.Value);
        }

        [Fact]
        public void Trigger_SuccessfulCallOneSubscriber_IfHaveTwoSubscribersButOneUnsubscribed()
        {
            // Arrange
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var systemEvent = SystemEvent.CreateWithPayload(
                this.productContext.TenantId,
                Guid.NewGuid(),
                this.productContext.ProductId,
                this.productContext.Environment,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload,
                new TestClock().Now());
            ISystemEventObservable systemEventObservable = new SystemEventObservable();
            TestProductService productService = new TestProductService();
            TestReportService reportService = new TestReportService();
            this.productServiceSubscription =
                systemEventObservable
                .Where(x => x.EventType == SystemEventType.CustomerExpiredQuoteOpened) // subscribed to specific event.
                .Subscribe(productService);
            this.reportServiceSubscription =
                 systemEventObservable
                 .Where(x => x.EventType == SystemEventType.CustomerExpiredQuoteOpened) // subscribed to specific event.
                 .Subscribe(reportService);
            this.reportServiceSubscription.Dispose(); // dispose immediately

            // Act
            systemEventObservable.Trigger(systemEvent);

            // Assert
            productService.Value.Should().Be(systemEvent.PayloadJson);
            reportService.Value.Should().NotBe(systemEvent.PayloadJson);
        }

        [Fact]
        public void Trigger_SuccessfulCallOneSubscriber_IfHaveTwoSubscribersButOneIsSubscribedToSomethingElse()
        {
            // Arrange
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var systemEvent = SystemEvent.CreateWithPayload(
                this.productContext.TenantId,
                Guid.NewGuid(),
                this.productContext.ProductId,
                this.productContext.Environment,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload,
                new TestClock().Now());
            SystemEventObservable systemEventObservable = new SystemEventObservable();
            TestProductService productService = new TestProductService();
            TestReportService reportService = new TestReportService();
            this.productServiceSubscription =
                  systemEventObservable
                  .Where(x => x.EventType == SystemEventType.CustomerExpiredQuoteOpened) // subscribed to specific event.
                  .Subscribe(productService);
            this.reportServiceSubscription =
                 systemEventObservable
                  .Where(x => x.EventType == SystemEventType.Custom) // subscribed to specific event.
                 .Subscribe(reportService);

            // Act
            systemEventObservable.Trigger(systemEvent);

            // Assert
            productService.Value.Should().Be(systemEvent.PayloadJson);
            reportService.Value.Should().NotBe(systemEvent.PayloadJson);
        }

        [Fact]
        public void Trigger_NoSubscribersCalled_IfUnsubscribedAfterTriggerThenRetrigger()
        {
            // Arrange
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var systemEvent = SystemEvent.CreateWithPayload(
                this.productContext.TenantId,
                Guid.NewGuid(),
                this.productContext.ProductId,
                this.productContext.Environment,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload,
                new TestClock().Now());
            ISystemEventObservable systemEventObservable = new SystemEventObservable();
            TestProductService productService = new TestProductService();
            TestReportService reportService = new TestReportService();
            this.productServiceSubscription =
                systemEventObservable
                 .Where(x => x.EventType == SystemEventType.CustomerExpiredQuoteOpened) // subscribed to specific event.
                .Subscribe(productService);
            this.reportServiceSubscription =
                 systemEventObservable
                  .Where(x => x.EventType == SystemEventType.CustomerExpiredQuoteOpened) // subscribed to specific event.
                 .Subscribe(reportService);

            systemEventObservable.Trigger(systemEvent); // trigger
            this.productServiceSubscription.Dispose(); // dispose them.
            this.reportServiceSubscription.Dispose(); // dispose them.

            var payload2 = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var systemEvent2 = SystemEvent.CreateWithPayload(
                this.productContext.TenantId,
                Guid.NewGuid(),
                this.productContext.ProductId,
                this.productContext.Environment,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload2,
                new TestClock().Now());

            // Act
            systemEventObservable.Trigger(systemEvent2); // send another trigger to check if it will take effect.

            // Assert
            productService.Value.Should().NotBe(systemEvent2.PayloadJson);
            reportService.Value.Should().NotBe(systemEvent2.PayloadJson);
        }

        [Fact]
        public void Trigger_NoSubscribersCalled_IfHaveTwoSubscribersButNoOneSubscribedToSpecificEventThrown()
        {
            // Arrange
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var systemEvent = SystemEvent.CreateWithPayload(
                this.productContext.TenantId,
                Guid.NewGuid(),
                this.productContext.ProductId,
                DeploymentEnvironment.Development,
                SystemEventType.Custom,
                payload,
                new TestClock().Now());
            ISystemEventObservable systemEventObservable = new SystemEventObservable();
            TestProductService productService = new TestProductService();
            TestReportService reportService = new TestReportService();
            this.productServiceSubscription =
              systemEventObservable
              .Where(x => x.EventType == SystemEventType.CustomerExpiredQuoteOpened) // subscribed to specific event.
              .Subscribe(productService);
            this.reportServiceSubscription =
                 systemEventObservable
                 .Where(x => x.EventType == SystemEventType.CustomerExpiredQuoteOpened) // subscribed to specific event.
                 .Subscribe(reportService);

            // Act
            systemEventObservable.Trigger(systemEvent);

            // Assert
            productService.Value.Should().NotBe(systemEvent.PayloadJson);
            reportService.Value.Should().NotBe(systemEvent.PayloadJson);
        }
    }

    /// <summary>
    /// The sample product service to sample system events.
    /// </summary>
#pragma warning disable SA1402 // File may only contain a single type
    public class TestProductService : IProductService, IObserver<SystemEvent>
#pragma warning restore SA1402 // File may only contain a single type
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value { get; private set; }

        public void OnNext(SystemEvent systemEvent)
        {
            this.Value = systemEvent.PayloadJson;
        }

        public void OnError(Exception error)
        {
            this.Value = "error";
        }

        public Task<Product> CreateProduct(Guid tenantId, string productAlias, string name, Guid? productId = null)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            this.Value = "done";
        }

        public Task<Product> CreateProduct(Guid tenantId, Guid productId, string name)
        {
            throw new NotImplementedException();
        }

        public ProductDeploymentSetting GetDeploymentSettings(string tenantId, string productId)
        {
            throw new NotImplementedException();
        }

        public Task InitializeProduct(Guid tenantId, Guid productId, bool createComponentFiles = false)
        {
            throw new NotImplementedException();
        }

        public Product UpdateProduct(Guid tenantId, Guid productId, string name, bool disabled, bool deleted, QuoteExpirySettings productQuoteExpirySetting = null, ProductQuoteExpirySettingUpdateType updateType = ProductQuoteExpirySettingUpdateType.UpdateNone)
        {
            throw new NotImplementedException();
        }

        public Task InitializeProduct(string tenantId, string productId, bool createComponentFiles = false)
        {
            throw new NotImplementedException();
        }

        public Product UpdateDeploymentSettings(Guid tenantId, Guid productId, ProductDeploymentSetting deploymentSettings)
        {
            throw new NotImplementedException();
        }

        public Product UpdateQuoteExpirySettings(
            Guid tenantId,
            Guid productId,
            QuoteExpirySettings expirySettings,
            ProductQuoteExpirySettingUpdateType updateType,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ProductDeploymentSetting GetDeploymentSettings(Guid tenantId, Guid productId)
        {
            throw new NotImplementedException();
        }

        public Task<Product> CreateOrUpdateProduct(Guid tenantId, string productId, string name, bool disabled, bool deleted)
        {
            throw new NotImplementedException();
        }

        public Task SeedFilesAsync(Guid tenantId, Guid productId, string environment, FileModel file, string folder)
        {
            throw new NotImplementedException();
        }

        public Product GetProductById(string tenantId, string productId)
        {
            throw new NotImplementedException();
        }

        public Product GetProductById(Guid tenantId, Guid productId)
        {
            throw new NotImplementedException();
        }

        public Product UpdateProduct(string tenantId, string productId, string name, string alias, bool disabled, bool deleted, QuoteExpirySettings productQuoteExpirySetting = null, ProductQuoteExpirySettingUpdateType updateType = ProductQuoteExpirySettingUpdateType.UpdateNone)
        {
            throw new NotImplementedException();
        }

        public Product UpdateProduct(Guid tenantId, Guid productId, string name, string alias, bool disabled, bool deleted, QuoteExpirySettings productQuoteExpirySetting = null, ProductQuoteExpirySettingUpdateType updateType = ProductQuoteExpirySettingUpdateType.UpdateNone)
        {
            throw new NotImplementedException();
        }

        public Task RenameProductDirectoryIfTenantAliasChange(Guid tenantId)
        {
            throw new NotImplementedException();
        }

        Task<Product> IProductService.UpdateProduct(
            Guid tenantId,
            Guid productId,
            string name,
            string alias,
            bool disabled,
            bool deleted,
            CancellationToken cancellationToken,
            QuoteExpirySettings productQuoteExpirySetting,
            ProductQuoteExpirySettingUpdateType updateType)
        {
            throw new NotImplementedException();
        }

        public Task ThrowErrorIfNewProductDirectoryExists(Guid tenantId, string newTenantAlias)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// The sample product service to sample system events.
    /// </summary>
#pragma warning disable SA1402 // File may only contain a single type
    public class TestReportService : IObserver<SystemEvent>
#pragma warning restore SA1402 // File may only contain a single type
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value { get; private set; }

        public Task<ReportPresentationModel> CreateAsync(string tenantId, string name, string description, IEnumerable<string> products, string sourceData, string mimeType, string filename, string body, DeploymentEnvironment environment, IUserAuthenticationData userAuthData)
        {
            throw new NotImplementedException();
        }

        public string GenerateFilename(IUserAuthenticationData userAuthData, string tenantId, string name, string description, string mimeType, DateTime from, DateTime to, string filename)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GenerateReportFileContent(IUserAuthenticationData userAuthData, DeploymentEnvironment environment, Guid reportId, DateTime from, DateTime to, bool saveGeneratedReport, bool includeTestData)
        {
            throw new NotImplementedException();
        }

        public ReportFilePresentationModel GetReportFileFromReport(Guid reportId, Guid reportFileId, IUserAuthenticationData userAuthData)
        {
            throw new NotImplementedException();
        }

        public ReportPresentationModel GetReportForUser(IUserAuthenticationData userAuthData, Guid reportId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ReportPresentationModel> GetReportsByTenantId(string tenantId, IUserAuthenticationData userAuthData, DeploymentEnvironment environment)
        {
            throw new NotImplementedException();
        }

        public Task<ReportPresentationModel> UpdateAsync(Guid reportId, string name, string description, IEnumerable<string> products, string sourceData, string mimeType, string filename, string body, bool isDeleted, DeploymentEnvironment environment, IUserAuthenticationData userAuthData)
        {
            throw new NotImplementedException();
        }

        public void OnNext(SystemEvent systemEvent)
        {
            this.Value = systemEvent.PayloadJson;
        }

        public void OnError(Exception error)
        {
            this.Value = "error";
        }

        public void OnCompleted()
        {
            this.Value = "done";
        }
    }
}
