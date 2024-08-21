// <copyright file="CreateNewBusinessQuoteCommandHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using RedLockNet;
    using UBind.Application.Commands.Quote;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class CreateNewBusinessQuoteCommandHandlerTests
    {
        private readonly Guid organisationId = Guid.NewGuid();
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

        private readonly Mock<IProductOrganisationSettingRepository> productOrganisationSettingRepositoryMock
            = new Mock<IProductOrganisationSettingRepository>();

        private readonly Mock<IOrganisationService> organisationServiceMock
            = new Mock<IOrganisationService>();

        private readonly Mock<IProductPortalSettingRepository> productPortalSettingRepositoryMock
            = new Mock<IProductPortalSettingRepository>();

        private readonly Mock<IPortalReadModelRepository> portalRepositoryMock
            = new Mock<IPortalReadModelRepository>();

        private readonly IProductService productService;

        private readonly Mock<IPersonReadModelRepository> personReadModelRepository
            = new Mock<IPersonReadModelRepository>();

        private readonly Mock<IProductConfigurationProvider> productConfigurationProviderMock
            = new Mock<IProductConfigurationProvider>();

        private readonly Mock<IAdditionalPropertyTransformHelper> additionalPropertyTransformHelper
            = new Mock<IAdditionalPropertyTransformHelper>();

        private readonly Mock<IPolicyService> policyService = new Mock<IPolicyService>();

        private readonly Mock<IAggregateLockingService> aggregateLockingServiceMock = new Mock<IAggregateLockingService>();

        private IClock clock;

        public CreateNewBusinessQuoteCommandHandlerTests()
        {
            this.clock = new TestClock();
            this.cachingResolverMock.Setup(s => s.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Domain.Product.Product(
                    TenantFactory.DefaultId,
                    ProductFactory.DefaultId,
                    TenantFactory.DefaultName,
                    "Test product",
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
            this.productOrganisationSettingRepositoryMock.Setup(s => s.GetProductOrganisationSettings(
                It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new List<ProductOrganisationSettingModel>
                {
                    new ProductOrganisationSettingModel(
                        "test",
                        this.organisationId,
                        ProductFactory.DefaultId,
                        true,
                        this.clock.Now().ToUnixTimeTicks()),
                });
            this.productFeatureSettingServiceMock
                .Setup(s => s.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new ProductFeatureSetting(
                    TenantFactory.DefaultId,
                    ProductFactory.DefaultId,
                    this.clock.Now()));

            this.productService = new ProductService(
                this.productFeatureSettingServiceMock.Object,
                this.productOrganisationSettingRepositoryMock.Object,
                this.organisationServiceMock.Object,
                this.productPortalSettingRepositoryMock.Object,
                this.portalRepositoryMock.Object,
                this.productConfigurationProviderMock.Object);

            this.aggregateLockingServiceMock.Setup(s => s.CreateLockOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>(), AggregateType.Quote))
                .ReturnsAsync(It.IsAny<IRedLock>());
        }

        [Fact]
        public async Task CreateNewBusinessQuote_WithEnabledProductFeatureSetting_Succeeds()
        {
            // Arrange
            var productFeatureSettings = new ProductFeatureSetting(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                this.clock.Now());
            this.productFeatureSettingServiceMock
                .Setup(s => s.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeatureSettings);
            var command = new CreateNewBusinessQuoteCommand(
                TenantFactory.DefaultId,
                this.organisationId,
                null,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Staging,
                false,
                null,
                null,
                null);
            var handler = new CreateNewBusinessQuoteCommandHandler(
                this.quoteAggregateRepositoryMock.Object,
                this.cachingResolverMock.Object,
                this.quoteExpirySettingsProviderMock.Object,
                this.httpContextPropertiesResolverMock.Object,
                this.clock,
                this.mediatorMock.Object,
                this.productService,
                this.personReadModelRepository.Object,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                this.additionalPropertyTransformHelper.Object,
                this.aggregateLockingServiceMock.Object,
                this.policyService.Object);

            // Act
            Func<Task<NewQuoteReadModel>> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        /// <summary>
        /// Test create new business quote with disabled product features settings should throw ErrorException.
        /// </summary>
        [Fact]
        public async Task CreateNewBusinessQuote_WithDisabledProductFeatureSettings_ThrowsErrorExceptionAsync()
        {
            // Arrange
            var productFeatureSettings = new ProductFeatureSetting(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                this.clock.Now());
            productFeatureSettings.Disable(Domain.Product.ProductFeatureSettingItem.NewBusinessQuotes);
            this.productFeatureSettingServiceMock
                .Setup(s => s.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeatureSettings);
            var command = new CreateNewBusinessQuoteCommand(
                TenantFactory.DefaultId,
                this.organisationId,
                null,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Staging,
                false,
                null,
                null,
                null);
            var handler = new CreateNewBusinessQuoteCommandHandler(
                this.quoteAggregateRepositoryMock.Object,
                this.cachingResolverMock.Object,
                this.quoteExpirySettingsProviderMock.Object,
                this.httpContextPropertiesResolverMock.Object,
                this.clock,
                this.mediatorMock.Object,
                this.productService,
                this.personReadModelRepository.Object,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                this.additionalPropertyTransformHelper.Object,
                this.aggregateLockingServiceMock.Object,
                this.policyService.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("quote.creation.new.business.quote.type.disabled");
        }

        [Fact]
        public async Task CreateNewBusinessQuote_WithValidCustomer_Succeeds()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            this.CreateCustomerReadModelDetail(TenantFactory.DefaultId, this.organisationId, customerId);
            var command = new CreateNewBusinessQuoteCommand(
                TenantFactory.DefaultId,
                this.organisationId,
                null,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Staging,
                false,
                customerId,
                null,
                null);
            var handler = new CreateNewBusinessQuoteCommandHandler(
                this.quoteAggregateRepositoryMock.Object,
                this.cachingResolverMock.Object,
                this.quoteExpirySettingsProviderMock.Object,
                this.httpContextPropertiesResolverMock.Object,
                this.clock,
                this.mediatorMock.Object,
                this.productService,
                this.personReadModelRepository.Object,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                this.additionalPropertyTransformHelper.Object,
                this.aggregateLockingServiceMock.Object,
                this.policyService.Object);

            // Act
            Func<Task<NewQuoteReadModel>> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task CreateNewBusinessQuote_WithInvalidCustomer_ThrowsErrorExceptionAsync()
        {
            // Arrange
            var fakeCustomerId = Guid.NewGuid();
            var productFeatureSettings = new ProductFeatureSetting(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                this.clock.Now());
            this.productFeatureSettingServiceMock
                .Setup(s => s.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeatureSettings);
            var command = new CreateNewBusinessQuoteCommand(
                TenantFactory.DefaultId,
                this.organisationId,
                null,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Staging,
                false,
                fakeCustomerId,
                null,
                null);
            var handler = new CreateNewBusinessQuoteCommandHandler(
                this.quoteAggregateRepositoryMock.Object,
                this.cachingResolverMock.Object,
                this.quoteExpirySettingsProviderMock.Object,
                this.httpContextPropertiesResolverMock.Object,
                this.clock,
                this.mediatorMock.Object,
                this.productService,
                this.personReadModelRepository.Object,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                this.additionalPropertyTransformHelper.Object,
                this.aggregateLockingServiceMock.Object,
                this.policyService.Object);

            // Act
            Func<Task<NewQuoteReadModel>> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("customer.not.found");
        }

        private CustomerAggregate CreateCustomer(Guid tenantId, PersonAggregate personAggregate)
        {
            var performingUserId = this.httpContextPropertiesResolverMock.Object.PerformingUserId;
            var timestamp = SystemClock.Instance.GetCurrentInstant();
            var customerAggregate = CustomerAggregate.CreateNewCustomer(
                tenantId, personAggregate, DeploymentEnvironment.Staging, performingUserId, null, timestamp);
            return customerAggregate;
        }

        private void CreateCustomerReadModelDetail(Guid tenantId, Guid organisationId, Guid customerId)
        {
            var personAggregate = PersonAggregate.CreatePerson(
                TenantFactory.DefaultId, this.organisationId, Guid.NewGuid(), this.clock.Now());
            var timestamp = SystemClock.Instance.GetCurrentInstant();
            var customerAggregate = this.CreateCustomer(tenantId, personAggregate);
            var personReadModel = PersonReadModel.CreatePerson(
                tenantId, organisationId, customerAggregate.PrimaryPersonId, timestamp);
            personReadModel.Email = "randomCustomer1234@mail.com";

            var customerReadModel = new CustomerReadModel(
                customerId, personReadModel, DeploymentEnvironment.Staging, null, timestamp, false);
            customerReadModel.People.Add(personReadModel);
            this.personReadModelRepository.Setup(e => e.GetPersonAssociatedWithPrimaryPersonByCustmerId(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(personReadModel);
        }
    }
}
