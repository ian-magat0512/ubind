// <copyright file="ApplicationQuoteServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using RedLockNet;
    using UBind.Application.Commands.Customer;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Services;
    using UBind.Application.Services.Email;
    using UBind.Application.Services.SystemEmail;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class ApplicationQuoteServiceTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly Guid quoteId = Guid.NewGuid();
        private readonly string formDataJson = "{}";
        private readonly string quoteNumber = "QuoteNumber";
        private readonly IClock clock = SystemClock.Instance;
        private readonly Tenant tenant = TenantFactory.Create();
        private readonly Product product = ProductFactory.Create();
        private readonly Mock<IProductFeatureSettingRepository> productFeatureRepository = new Mock<IProductFeatureSettingRepository>();
        private readonly Mock<IQuoteAggregateRepository> quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
        private readonly Mock<IPersonReadModelRepository> personReadModelRepository = new Mock<IPersonReadModelRepository>();
        private readonly Mock<IWritableReadModelRepository<PersonReadModel>> personWritableRepository = new Mock<IWritableReadModelRepository<PersonReadModel>>();
        private readonly Mock<ICustomerReadModelRepository> customerReadModelRepository = new Mock<ICustomerReadModelRepository>();
        private readonly Mock<IUserAggregateRepository> userAggregateRepository = new Mock<IUserAggregateRepository>();
        private readonly Mock<IProductRepository> productRepository = new Mock<IProductRepository>();
        private readonly Mock<ITenantRepository> tenantRepository = new Mock<ITenantRepository>();
        private readonly Mock<IQuoteAggregateResolverService> quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
        private readonly Mock<IQuoteReferenceNumberGenerator> numberGenerator = new Mock<IQuoteReferenceNumberGenerator>();
        private readonly Mock<ICustomerService> customerService = new Mock<ICustomerService>();
        private readonly Mock<IPersonAggregateRepository> personAggregateRepository = new Mock<IPersonAggregateRepository>();
        private readonly Mock<ICustomerAggregateRepository> customerAggregateRepository = new Mock<ICustomerAggregateRepository>();
        private readonly Mock<IUserLoginEmailRepository> userLoginEmailRepository = new Mock<IUserLoginEmailRepository>();
        private readonly Mock<IPolicyService> policyServiceMock = new Mock<IPolicyService>();
        private readonly Mock<IHttpContextPropertiesResolver> currentUserIdentification = new Mock<IHttpContextPropertiesResolver>();
        private readonly Mock<ISystemEmailService> systemEmailService = new Mock<ISystemEmailService>();
        private readonly Mock<IOrganisationReadModelRepository> organisationReadModelRepository = new Mock<IOrganisationReadModelRepository>();
        private readonly Mock<ICqrsMediator> mediator = new Mock<ICqrsMediator>();
        private readonly Mock<IProductService> productService = new Mock<IProductService>();
        private readonly Mock<IUBindDbContext> mockUBindDbContext;
        private readonly Mock<ICachingResolver> mockCachingResolver = new Mock<ICachingResolver>();
        private readonly DefaultQuoteWorkflowProvider quoteWorkflowProvider = new DefaultQuoteWorkflowProvider();
        private readonly DefaultExpirySettingsProvider quoteExpirySettingsProvider = new DefaultExpirySettingsProvider();
        private readonly Mock<IQuoteSystemEventEmitter> quoteSystemEventEmitter = new Mock<IQuoteSystemEventEmitter>();
        private readonly Mock<IErrorNotificationService> errorNotificationService = new Mock<IErrorNotificationService>();
        private readonly Mock<IAggregateLockingService> mockAggregateLockingService = new Mock<IAggregateLockingService>();
        private Quote quote;
        private PersonAggregate personAggregate;
        private PersonalDetails customerDetails;

        public ApplicationQuoteServiceTests()
        {
            this.personAggregate = PersonAggregate.CreatePerson(
                this.tenant.Id, this.tenant.Details.DefaultOrganisationId, this.performingUserId, this.clock.Now());
            this.personAggregate.UpdateFullName("Foo", this.performingUserId, this.clock.Now());
            this.personAggregate.UpdateMobilePhone("04 1234 1234", this.performingUserId, this.clock.Now());
            this.personAggregate.UpdateEmail("foo@example.com", this.performingUserId, this.clock.Now());
            this.customerDetails = new PersonalDetails(this.personAggregate);
            this.mockUBindDbContext = new Mock<IUBindDbContext>();
        }

        [Fact]
        public async Task AssignQuoteNumber_Should_Succeed_WhenQuote_Has_Customer()
        {
            // Arrange
            var productFeatureSettingService = this.GetProductFeatureService();
            var applicationQuoteService = this.GetApplicationQuoteService(productFeatureSettingService);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                this.quote.Aggregate.Environment,
                this.quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            await applicationQuoteService.Actualise(
                releaseContext,
                this.quote,
                new FormData(this.formDataJson));

            // Assert
            Assert.Equal(this.quote.QuoteNumber, this.quoteNumber);
        }

        [Fact]
        public async Task CreateVersion_WithEnabledProductFeature_succeed()
        {
            // Arrange
            var productFeatureSettingService = this.GetProductFeatureService();
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            productFeature.Enable(ProductFeatureSettingItem.NewBusinessQuotes);
            this.mockCachingResolver
                .Setup(c => c.GetProductSettingOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(productFeature);
            var applicationQuoteService = this.GetApplicationQuoteService(productFeatureSettingService);
            var services = new ServiceCollection();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BaseUpdaterJob).GetTypeInfo().Assembly));
            services.AddTransient<IClock>(c => new TestClock());
            services.AddTransient<IProductFeatureSettingService>(c => productFeatureSettingService);
            services.AddTransient<IHttpContextPropertiesResolver>(c => new Mock<IHttpContextPropertiesResolver>().Object);
            services.AddTransient<IQuoteAggregateRepository>(c => this.quoteAggregateRepository.Object);
            services.AddTransient<IQuoteAggregateResolverService>(c => this.quoteAggregateResolverService.Object);
            services.AddTransient<UBind.Domain.Services.IProductService>(c => this.productService.Object);
            services.AddTransient<IUBindDbContext>(_ => this.mockUBindDbContext.Object);
            services.AddTransient<ICqrsMediator, CqrsMediator>();
            services.AddTransient<ICqrsRequestContext>(_ => new CqrsRequestContext());
            services.AddScoped<IAggregateLockingService>(c => this.mockAggregateLockingService.Object);
            services.AddLogging(loggingBuilder => loggingBuilder.AddDebug());

            var serviceProvider = services.BuildServiceProvider();
            var mediator = new CqrsMediator(serviceProvider, NullLogger<CqrsMediator>.Instance, this.errorNotificationService.Object);

            // Act
            var command = new CreateQuoteVersionCommand(
                this.tenant.Id,
                this.product.Id,
                DeploymentEnvironment.Development,
                this.quote.Id,
                new FormData(this.formDataJson));

            await mediator.Send(command, CancellationToken.None);

            var quoteVersion = await applicationQuoteService.CreateVersion(
                this.tenant.Id,
                this.quote.Id,
                new FormData(this.formDataJson));

            // Assert
            quoteVersion.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateVersion_WithDisabledProductFeatureSettings_ShouldNotThrowErrorExceptionAsync()
        {
            // Arrange
            var productFeatureSettingService = this.GetProductFeatureService();
            var productFeature = new ProductFeatureSetting(
                this.tenant.Id,
                this.product.Id,
                this.clock.Now());
            productFeature.Disable(ProductFeatureSettingItem.NewBusinessQuotes);

            this.productFeatureRepository.Setup(e => e.GetProductFeatureSetting(
               It.IsAny<Guid>(),
               It.IsAny<Guid>()))
               .Returns(productFeature);
            var applicationQuoteService = this.GetApplicationQuoteService(productFeatureSettingService);

            // Act
            Func<Task> act = async () => await applicationQuoteService.CreateVersion(
                this.tenant.Id,
                this.quote.Id,
                new FormData(this.formDataJson));

            // Assert
            await act.Should().NotThrowAsync<ErrorException>();
        }

        private ApplicationQuoteService GetApplicationQuoteService(ProductFeatureSettingService productFeatureSettingService)
        {
            var clock = SystemClock.Instance;
            var product = new Product(this.tenant.Id, this.product.Id, "test", "test", clock.Now());
            this.productRepository
                .Setup(x => x.GetProductById(this.tenant.Id, this.product.Id, false))
                .Returns(product);

            var person = new PersonCommonProperties()
            {
                FullName = "Foo",
                MobilePhoneNumber = "04 1234 1234",
                Email = "foo@example.com",
            };
            var personDetails = new PersonalDetails(this.tenant.Id, person);
            var personAggregate = PersonAggregate.CreatePersonFromPersonalDetails(
                this.tenant.Id, this.tenant.Details.DefaultOrganisationId, personDetails, this.performingUserId, clock.Now());
            var customerDetails = new PersonalDetails(personAggregate);
            var customerAggregate = CustomerAggregate.CreateNewCustomer(
                 this.tenant.Id, this.personAggregate, DeploymentEnvironment.Development, this.performingUserId, null, clock.Now());

            var tenant = TenantFactory.Create();
            this.customerAggregateRepository.Setup(e => e.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(customerAggregate);

            var personReadModel = PersonReadModel.CreatePerson(
                this.tenant.Id, this.tenant.Details.DefaultOrganisationId, Guid.NewGuid(), this.clock.Now());
            var customerReadModelDetail = new CustomerReadModelDetail
            {
                Id = customerAggregate.Id,
                PrimaryPersonId = personReadModel.Id,
                Environment = DeploymentEnvironment.Staging,
                UserId = Guid.NewGuid(),
                IsTestData = false,
                OwnerUserId = this.performingUserId.Value,
                OwnerPersonId = this.performingUserId.Value,
                OwnerFullName = "Bob Smith",
                UserIsBlocked = false,
                FullName = "Randy Walsh",
                NamePrefix = "Mr",
                FirstName = "Randy",
                LastName = "Walsh",
                PreferredName = "Rando",
                Email = "r.walsh@testemail.com",
                TenantId = this.tenant.Id,
                DisplayName = "Randy Walsh",
                OrganisationId = this.tenant.Details.DefaultOrganisationId,
                OrganisationName = "Default Organisation",
            };
            this.customerReadModelRepository
                .Setup(e => e.GetCustomerById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .Returns(customerReadModelDetail);
            this.personReadModelRepository
                .Setup(e => e.GetPersonById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(personReadModel);

            this.tenantRepository.Setup(tr => tr.GetTenantById(this.tenant.Id)).Returns(tenant);
            this.quote = QuoteAggregate.CreateNewBusinessQuote(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                product.Id,
                DeploymentEnvironment.Development,
                QuoteExpirySettings.Default,
                this.performingUserId,
                clock.Now(),
                Guid.NewGuid(),
                Timezones.AET);
            this.quote.Aggregate.RecordAssociationWithCustomer(customerAggregate, this.personAggregate, this.performingUserId, clock.Now());
            var formDataId = this.quote.UpdateFormData(new FormData(this.formDataJson), this.performingUserId, clock.Now());
            var formDataSchema = new FormDataSchema(new JObject());
            var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create());
            var quoteDataRetreiver = new StandardQuoteDataRetriever(
                        new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates()),
                        new CachingJObjectWrapper(CalculationResultJsonFactory.Create()));
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataId;
            this.quote.RecordCalculationResult(
                    calculationResult,
                    calculationData,
                    this.clock.Now(),
                    formDataSchema,
                    false,
                    this.performingUserId);
            this.quoteAggregateRepository.Setup(q => q.GetById(tenant.Id, this.quoteId)).Returns(this.quote.Aggregate);
            this.quoteAggregateRepository.Setup(q => q.GetById(tenant.Id, this.quote.Id)).Returns(this.quote.Aggregate);
            this.quoteAggregateRepository.Setup(q => q.GetById(tenant.Id, this.quote.Aggregate.Id)).Returns(this.quote.Aggregate);
            this.personAggregateRepository
                .Setup(e => e.GetById(tenant.Id, customerAggregate.PrimaryPersonId)).Returns(this.personAggregate);
            this.quoteAggregateResolverService
                .Setup(q => q.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(this.quote.Aggregate);
            this.quoteAggregateResolverService
                .Setup(q => q.GetQuoteAggregateIdForQuoteId(It.IsAny<Guid>())).Returns(this.quote.Aggregate.Id);
            this.quote = this.quote.Aggregate.GetQuoteOrThrow(this.quote.Id);
            this.numberGenerator.Setup(ng => ng.Generate())
                .Returns(this.quoteNumber);
            var latestCustomerDetails = this.quote.LatestCustomerDetails?.Data;
            this.customerService
                .Setup(cs => cs.CreateCustomerForNewPerson(
                    tenant.Id,
                    DeploymentEnvironment.Development,
                    latestCustomerDetails,
                    default(Guid),
                    null,
                    false,
                    null))
                .Returns(Task.FromResult(customerAggregate));
            this.personWritableRepository
                .Setup(p => p.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(personReadModel);
            this.mediator
                .Setup(m => m.Send(It.IsAny<CreateCustomerCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(customerAggregate.Id));
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                this.quote.Aggregate.Environment,
                this.quote.ProductReleaseId ?? Guid.NewGuid());
            this.policyServiceMock.Setup(p => p.GenerateQuoteNumber(releaseContext))
                .Returns(Task.FromResult(this.quoteNumber));
            this.mockAggregateLockingService
                .Setup(a => a.CreateLockOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AggregateType>()))
                .ReturnsAsync(It.IsAny<IRedLock>());

            var applicationQuoteService = new ApplicationQuoteService(
                this.quoteAggregateRepository.Object,
                this.tenantRepository.Object,
                this.userAggregateRepository.Object,
                this.personAggregateRepository.Object,
                this.quoteWorkflowProvider,
                this.customerService.Object,
                this.quoteExpirySettingsProvider,
                this.customerAggregateRepository.Object,
                this.userLoginEmailRepository.Object,
                this.policyServiceMock.Object,
                this.currentUserIdentification.Object,
                clock,
                this.quoteAggregateResolverService.Object,
                this.organisationReadModelRepository.Object,
                productFeatureSettingService,
                this.systemEmailService.Object,
                this.mediator.Object,
                this.quoteSystemEventEmitter.Object);

            return applicationQuoteService;
        }

        private ProductFeatureSettingService GetProductFeatureService()
        {
            var tenant = TenantFactory.Create(this.tenant.Id);
            var product = ProductFactory.Create(this.product.Id);
            Mock<DevRelease> devRelease = new Mock<DevRelease>();
            this.productRepository
                .Setup(e => e.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            var productSummary = new Mock<IProductSummary>();
            var deploymentSetting = new ProductDeploymentSetting();
            deploymentSetting.Development = new List<string> { DeploymentEnvironment.Development.ToString() };
            var productDetails = new ProductDetails(this.tenant.Id.ToString(), product.Id.ToString(), false, false, this.clock.Now(), deploymentSetting);
            productSummary.Setup(p => p.Details).Returns(productDetails);
            productSummary.Setup(p => p.Id).Returns(this.product.Id);

            IEnumerable<IProductSummary> productSummaries = new List<IProductSummary>() { productSummary.Object };

            this.productRepository
                .Setup(p => p.GetAllActiveProductSummariesForTenant(this.tenant.Id)).Returns(productSummaries);

            var productFeatureService = new ProductFeatureSettingService(
                this.productFeatureRepository.Object,
                this.clock,
                this.mockCachingResolver.Object);
            return productFeatureService;
        }
    }
}
