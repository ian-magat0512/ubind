// <copyright file="AssociatePolicyWithCustomerCommandHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands.Policy
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Policy;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class AssociatePolicyWithCustomerCommandHandlerTests
    {
        private readonly Guid organisationId = Guid.NewGuid();
        private readonly Mock<ICachingResolver> cachingResolverMock = new Mock<ICachingResolver>();
        private readonly Mock<IProductFeatureSettingService> productFeatureSettingServiceMock
            = new Mock<IProductFeatureSettingService>();

        private readonly Mock<IQuoteExpirySettingsProvider> quoteExpirySettingsProviderMock
            = new Mock<IQuoteExpirySettingsProvider>();

        private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolverMock
            = new Mock<IHttpContextPropertiesResolver>();

        private readonly Mock<IQuoteAggregateRepository> quoteAggregateRepositoryMock
            = new Mock<IQuoteAggregateRepository>();

        private readonly Mock<IQuoteAggregateResolverService> quoteAggregateResolverService
            = new Mock<IQuoteAggregateResolverService>();

        private readonly Mock<IProductOrganisationSettingRepository> productOrganisationSettingRepositoryMock
            = new Mock<IProductOrganisationSettingRepository>();

        private readonly Mock<ICustomerReadModelRepository> customerReadModelRepository
            = new Mock<ICustomerReadModelRepository>();

        private readonly Mock<ICustomerAggregateRepository> customerAggregateRepository = new Mock<ICustomerAggregateRepository>();

        private readonly Mock<IPersonAggregateRepository> personAggregateRepository = new Mock<IPersonAggregateRepository>();

        private IClock clock;

        public AssociatePolicyWithCustomerCommandHandlerTests()
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
        }

        [Fact]
        public async Task AssociateQuoteAggregateWithCustomer_WhenPolicyIsImported_Succeeds()
        {
            // Arrange
            var productFeatureSettings = new ProductFeatureSetting(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                this.clock.Now());
            this.productFeatureSettingServiceMock
                .Setup(s => s.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeatureSettings);
            var oldCustomerId = Guid.NewGuid();
            var performingUserId = this.httpContextPropertiesResolverMock.Object.PerformingUserId;
            var personAggregate = PersonAggregate.CreatePerson(
                TenantFactory.DefaultId, this.organisationId, performingUserId, this.clock.Now());
            var customerAggregate = this.CreateCustomer(TenantFactory.DefaultId, personAggregate);
            this.customerAggregateRepository.Setup(e => e.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(customerAggregate);
            var quoteAggregate = QuoteFactory.CreateImportedPolicy(
               TenantFactory.DefaultId,
               ProductFactory.DefaultId,
               DeploymentEnvironment.Staging,
               this.organisationId,
               Guid.NewGuid(),
               oldCustomerId,
               new PersonalDetails(personAggregate));
            quoteAggregate.WithCustomer();
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForPolicy(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(quoteAggregate);
            var customerReadModelDetail = this.CreateCustomerReadModelDetail(
                quoteAggregate.TenantId, quoteAggregate.OrganisationId, personAggregate);
            this.customerReadModelRepository.Setup(e => e.GetCustomerById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(customerReadModelDetail);
            this.personAggregateRepository.Setup(e => e.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(personAggregate);

            var quoteReadModelRepositoryMock = new Mock<IQuoteReadModelRepository>();
            quoteReadModelRepositoryMock.Setup(c => c.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(It.IsAny<NewQuoteReadModel>());
            var authorisationServiceMock = new Mock<IAuthorisationService>();
            var sut = new AssociatePolicyWithCustomerCommandHandler(
                new Mock<IEmailRepository>().Object,
                new Mock<ICqrsMediator>().Object,
                this.quoteAggregateResolverService.Object,
                this.quoteAggregateRepositoryMock.Object,
                this.customerAggregateRepository.Object,
                this.personAggregateRepository.Object,
                this.customerReadModelRepository.Object,
                this.httpContextPropertiesResolverMock.Object,
                this.clock);

            // Act
            await sut.AssociateQuoteAggregateWithCustomer(quoteAggregate.TenantId, customerAggregate.Id, quoteAggregate, quoteAggregate.Id);

            // Assert
            quoteAggregate.Should().NotBeNull();
            quoteAggregate.CustomerId.Should().Be(customerAggregate.Id).And.NotBe(oldCustomerId);
        }

        private CustomerAggregate CreateCustomer(Guid tenantId, PersonAggregate personAggregate)
        {
            var performingUserId = this.httpContextPropertiesResolverMock.Object.PerformingUserId;
            var timestamp = SystemClock.Instance.GetCurrentInstant();
            var customerAggregate = CustomerAggregate.CreateNewCustomer(
                tenantId, personAggregate, DeploymentEnvironment.Staging, performingUserId, null, timestamp);
            return customerAggregate;
        }

        private CustomerReadModelDetail CreateCustomerReadModelDetail(Guid tenantId, Guid organisationId, PersonAggregate personAggregate)
        {
            var timestamp = SystemClock.Instance.GetCurrentInstant();
            var customerAggregate = this.CreateCustomer(tenantId, personAggregate);
            var personReadModel = PersonReadModel.CreatePerson(
                tenantId, organisationId, customerAggregate.PrimaryPersonId, timestamp);
            personReadModel.Email = "randomCustomer1234@mail.com";

            var customerReadModel = new CustomerReadModel(
                Guid.NewGuid(), personReadModel, DeploymentEnvironment.Staging, null, timestamp, false);
            customerReadModel.People.Add(personReadModel);

            return new CustomerReadModelDetail
            {
                Id = customerReadModel.Id,
                PrimaryPersonId = customerReadModel.PrimaryPersonId,
                Environment = customerReadModel.Environment,
                FullName = customerReadModel.PrimaryPerson.FullName,
                TenantId = tenantId,
                OrganisationId = organisationId,
                Email = customerReadModel.PrimaryPerson.Email,
            };
        }
    }
}
