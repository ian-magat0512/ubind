// <copyright file="CreateRenewalQuoteCommandHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Quote
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using RedLockNet;
    using UBind.Application.Commands.Quote;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class CreateRenewalQuoteCommandHandlerTests
    {
        private readonly Mock<ICachingResolver> cachingResolverMock = new Mock<ICachingResolver>();
        private readonly Mock<IProductFeatureSettingService> productFeatureSettingServiceMock
            = new Mock<IProductFeatureSettingService>();

        private readonly Mock<IQuoteExpirySettingsProvider> quoteExpirySettingsProviderMock
            = new Mock<IQuoteExpirySettingsProvider>();

        private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolverMock
            = new Mock<IHttpContextPropertiesResolver>();

        private readonly Mock<ICqrsMediator> mediatorMock = new Mock<ICqrsMediator>();
        private readonly Mock<IQuoteAggregateRepository> quoteAggregateRepositoryMock
            = new Mock<IQuoteAggregateRepository>();

        private readonly Mock<UBind.Domain.Services.IPolicyService> policyServiceMock
            = new Mock<UBind.Domain.Services.IPolicyService>();

        private readonly Mock<IQuoteWorkflowProvider> quoteWorkflowProviderMock
            = new Mock<IQuoteWorkflowProvider>();

        private readonly Mock<IProductConfigurationProvider> productConfigurationProviderMock
            = new Mock<IProductConfigurationProvider>();

        private readonly Mock<IPolicyReadModelRepository> policyReadModelRepositoryMock
            = new Mock<IPolicyReadModelRepository>();

        private readonly Mock<IPolicyTransactionTimeOfDayScheme> policyLimitTimesPolicyMock
            = new Mock<IPolicyTransactionTimeOfDayScheme>();

        private readonly Mock<IProductOrganisationSettingRepository> productOrganisationSettingRepositoryMock
            = new Mock<IProductOrganisationSettingRepository>();

        private readonly Mock<IOrganisationService> organisationServiceMock
            = new Mock<IOrganisationService>();

        private readonly Mock<IProductPortalSettingRepository> productPortalSettingRepositoryMock
            = new Mock<IProductPortalSettingRepository>();

        private readonly Mock<IPortalReadModelRepository> portalRepositoryMock
            = new Mock<IPortalReadModelRepository>();

        private readonly Mock<IAdditionalPropertyTransformHelper> additionalPropertyTransformHelper
            = new Mock<IAdditionalPropertyTransformHelper>();

        private readonly IProductService productService;

        private readonly Mock<IAggregateLockingService> aggregateLockingService = new Mock<IAggregateLockingService>();

        private readonly IQuoteWorkflow quoteWorkflow = new DefaultQuoteWorkflow();
        private IClock clock;

        public CreateRenewalQuoteCommandHandlerTests()
        {
            this.clock = new TestClock();
            this.cachingResolverMock.Setup(s => s.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Domain.Product.Product(
                    TenantFactory.DefaultId,
                    ProductFactory.DefaultId,
                    TenantFactory.DefaultName,
                    "test-product",
                    this.clock.Now())));
            this.cachingResolverMock.Setup(s => s.GetTenantOrThrow(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Tenant(
                    TenantFactory.DefaultId,
                    "Test tenant",
                    "test-tenant",
                    null,
                    default,
                    default,
                    this.clock.Now())));
            this.quoteExpirySettingsProviderMock.Setup(s => s.Retrieve(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(QuoteExpirySettings.Default);
            this.quoteExpirySettingsProviderMock.Setup(s => s.Retrieve(It.IsAny<Product>()))
                .Returns(QuoteExpirySettings.Default);
            this.quoteWorkflowProviderMock.Setup(s => s.GetConfigurableQuoteWorkflow(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(this.quoteWorkflow));
            this.productConfigurationProviderMock
                .Setup(s => s.GetFormDataSchema(It.IsAny<ProductContext>(), It.IsAny<WebFormAppType>()))
                .Returns(new FormDataSchema(new JObject()));

            this.productService = new ProductService(
                this.productFeatureSettingServiceMock.Object,
                this.productOrganisationSettingRepositoryMock.Object,
                this.organisationServiceMock.Object,
                this.productPortalSettingRepositoryMock.Object,
                this.portalRepositoryMock.Object,
                this.productConfigurationProviderMock.Object);

            this.aggregateLockingService.Setup(s => s.CreateLockOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>(), AggregateType.Quote))
            .ReturnsAsync(It.IsAny<IRedLock>());
        }

        [Fact]
        public async Task CreateRenewalQuote_WithEnabledProductFeatureSetting_Succeeds()
        {
            // Arrange
            var quoteAggregate = QuoteFactory.CreateNewPolicy();
            this.quoteAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(quoteAggregate);
            var productFeatureSettings = new ProductFeatureSetting(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                this.clock.Now());
            productFeatureSettings.Enable(ProductFeatureSettingItem.RenewalQuotes);
            this.productFeatureSettingServiceMock
                .Setup(s => s.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeatureSettings);
            this.productConfigurationProviderMock
                .Setup(s => s.GetProductConfiguration(It.IsAny<ReleaseContext>(), It.IsAny<WebFormAppType>()))
                .Returns(Task.FromResult((IProductConfiguration)QuoteFactory.ProductConfiguation));
            var command = new CreateRenewalQuoteCommand(
                TenantFactory.DefaultId,
                quoteAggregate.Policy.PolicyId,
                false);
            var handler = new CreateRenewalQuoteCommandHandler(
                this.quoteAggregateRepositoryMock.Object,
                this.cachingResolverMock.Object,
                this.productFeatureSettingServiceMock.Object,
                this.quoteExpirySettingsProviderMock.Object,
                this.httpContextPropertiesResolverMock.Object,
                this.clock,
                this.mediatorMock.Object,
                this.policyServiceMock.Object,
                this.productConfigurationProviderMock.Object,
                this.quoteWorkflowProviderMock.Object,
                this.policyReadModelRepositoryMock.Object,
                this.policyLimitTimesPolicyMock.Object,
                this.productService,
                this.additionalPropertyTransformHelper.Object,
                this.aggregateLockingService.Object);

            // Act
            Func<Task<NewQuoteReadModel>> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        /// <summary>
        /// Test create new business quote with disabled product features settings should throw ErrorException.
        /// </summary>
        [Fact]
        public async Task CreateRenewalQuote_WithDisabledProductFeatureSettings_ThrowsErrorExceptionAsync()
        {
            // Arrange
            var quoteAggregate = QuoteFactory.CreateNewPolicy();
            this.quoteAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(quoteAggregate);
            var productFeatureSettings = new ProductFeatureSetting(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                this.clock.Now());
            productFeatureSettings.Disable(ProductFeatureSettingItem.RenewalQuotes);
            this.productFeatureSettingServiceMock
                .Setup(s => s.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeatureSettings);
            var command = new CreateRenewalQuoteCommand(
                TenantFactory.DefaultId,
                quoteAggregate.Policy.PolicyId,
                false);
            var handler = new CreateRenewalQuoteCommandHandler(
                this.quoteAggregateRepositoryMock.Object,
                this.cachingResolverMock.Object,
                this.productFeatureSettingServiceMock.Object,
                this.quoteExpirySettingsProviderMock.Object,
                this.httpContextPropertiesResolverMock.Object,
                this.clock,
                this.mediatorMock.Object,
                this.policyServiceMock.Object,
                this.productConfigurationProviderMock.Object,
                this.quoteWorkflowProviderMock.Object,
                this.policyReadModelRepositoryMock.Object,
                this.policyLimitTimesPolicyMock.Object,
                this.productService,
                this.additionalPropertyTransformHelper.Object,
                this.aggregateLockingService.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ErrorException>();
        }
    }
}
