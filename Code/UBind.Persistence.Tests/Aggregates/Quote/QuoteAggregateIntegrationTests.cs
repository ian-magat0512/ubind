// <copyright file="QuoteAggregateIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Aggregates.Quote
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using RedLockNet;
    using UBind.Application;
    using UBind.Application.Commands.Customer;
    using UBind.Application.Export;
    using UBind.Application.Services;
    using UBind.Application.Services.SystemEmail;
    using UBind.Application.Tests.Fakes;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class QuoteAggregateIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly IClock clock = SystemClock.Instance;
        private readonly IUBindDbContext dbContext;
        private readonly DefaultQuoteWorkflowProvider quoteWorkflowProvider;
        private readonly IQuoteWorkflow quoteWorkflow = new DefaultQuoteWorkflow();
        private IQuoteEventObserver eventObserver;
        private IQuoteAggregateRepository quoteAggregateRepository;
        private Mock<IUserAggregateRepository> userAggregateRepository;
        private Mock<IPersonAggregateRepository> personAggregateRepository;
        private Mock<ICustomerAggregateRepository> customerAggregateRepository;
        private Mock<ICustomerReadModelRepository> customerReadModelRepository;
        private Mock<IClaimReadModelRepository> claimReadModelRepository;
        private Mock<IUserLoginEmailRepository> userLoginEmailRepository;
        private IQuoteExpirySettingsProvider quoteExpirySettingsProvider;
        private Mock<ITenantRepository> tenantRepository;
        private ICustomerService customerService;
        private Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolver;
        private Mock<IQuoteAggregateResolverService> quoteAggregateResolverService;
        private Mock<IOrganisationReadModelRepository> organisationReadModelRepository;
        private Mock<IProductFeatureSettingService> productFeatureSettingService;
        private Mock<ICqrsMediator> mediator;

        private IPolicyService policyService;
        private IApplicationQuoteService applicationQuoteService;

        private IEmailTemplateService emailTemplateService;
        private ISystemEmailService systemEmailService;
        private DefaultProductConfigurationProvider defaultProducConfigProvider;
        private Mock<IPersonReadModelRepository> personReadModelRepository;
        private Mock<IAdditionalPropertyValueService> additionalPropertyValueService;
        private Mock<ICachingResolver> cachingResolver = new Mock<ICachingResolver>();
        private Mock<IQuoteSystemEventEmitter> quoteSystemEventEmitter = new Mock<IQuoteSystemEventEmitter>();
        private Mock<IAggregateSnapshotService<QuoteAggregate>> aggregateSnapshotService;
        private Mock<IServiceProvider> serviceProvider;
        private EventRecordRepository eventRecordRepository;
        private ConnectionStrings connectionConfig;

        public QuoteAggregateIntegrationTests()
        {
            this.dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            this.connectionConfig = new ConnectionStrings();
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            this.connectionConfig.UBind = config.GetConnectionString(DatabaseFixture.TestConnectionStringName);
            this.eventObserver = new Mock<IQuoteEventObserver>().Object;
            this.aggregateSnapshotService = new Mock<IAggregateSnapshotService<QuoteAggregate>>();
            this.serviceProvider = new Mock<IServiceProvider>().AddLoggers();
            this.eventRecordRepository = new EventRecordRepository(this.dbContext, this.connectionConfig);
            this.quoteExpirySettingsProvider = new DefaultExpirySettingsProvider();
            this.quoteAggregateRepository = new QuoteAggregateRepository(
                this.dbContext,
                this.eventRecordRepository,
                this.eventObserver,
                this.aggregateSnapshotService.Object,
                this.clock,
                NullLogger<QuoteAggregateRepository>.Instance,
                this.serviceProvider.Object);
            this.userAggregateRepository = new Mock<IUserAggregateRepository>();
            this.tenantRepository = new Mock<ITenantRepository>();
            this.personAggregateRepository = new Mock<IPersonAggregateRepository>();
            this.customerAggregateRepository = new Mock<ICustomerAggregateRepository>();
            this.customerReadModelRepository = new Mock<ICustomerReadModelRepository>();
            this.claimReadModelRepository = new Mock<IClaimReadModelRepository>();
            this.userLoginEmailRepository = new Mock<IUserLoginEmailRepository>();
            this.defaultProducConfigProvider = new DefaultProductConfigurationProvider();
            this.httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
            this.quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
            this.productFeatureSettingService = new Mock<IProductFeatureSettingService>();
            this.personReadModelRepository = new Mock<IPersonReadModelRepository>();
            var productFeatureSettings = new ProductFeatureSetting(
                TenantFactory.DefaultId, ProductFactory.DefaultId, this.clock.Now());
            this.organisationReadModelRepository = new Mock<IOrganisationReadModelRepository>();
            this.productFeatureSettingService.Setup(x => x.GetProductFeature(
                It.IsAny<Guid>(),
                It.IsAny<Guid>()))
                .Returns(productFeatureSettings);
            this.additionalPropertyValueService = new Mock<IAdditionalPropertyValueService>();
            this.mediator = new Mock<ICqrsMediator>();

            this.quoteWorkflowProvider = new DefaultQuoteWorkflowProvider();
            this.SetupMockObjects();
            this.SetupAggregatorsAndRepositories();
            this.SetupServices();
        }

        [Fact]
        public async Task SubmissionIsPersisted()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                tenantId,
                organisationId,
                productId,
                DeploymentEnvironment.Staging,
                QuoteExpirySettings.Default,
                this.performingUserId,
                this.clock.Now(),
                Guid.NewGuid(),
                Timezones.AET);
            var quoteAggregate = quote.Aggregate;
            var formDataId = quote.UpdateFormData(new FormData("{}"), this.performingUserId, this.clock.Now());
            var formDataSchema = new FormDataSchema(new JObject());
            var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create());
            var quoteDataRetreiver = new StandardQuoteDataRetriever(
                        new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates()),
                        new CachingJObjectWrapper(CalculationResultJsonFactory.Create()));
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataId;
            quote.RecordCalculationResult(
                    calculationResult,
                    calculationData,
                    this.clock.Now(),
                    formDataSchema,
                    false,
                    this.performingUserId);
            quote.Submit(default, this.clock.Now(), this.quoteWorkflow);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            using (var newContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                this.eventRecordRepository = new EventRecordRepository(newContext, this.connectionConfig);
                var repo = new QuoteAggregateRepository(
                    newContext,
                    this.eventRecordRepository,
                    this.eventObserver,
                    this.aggregateSnapshotService.Object,
                    this.clock,
                    NullLogger<QuoteAggregateRepository>.Instance,
                    this.serviceProvider.Object);

                // Act
                var retrievedQuote = repo.GetById(tenantId, quoteAggregate.Id);
                var quoteRetrieved = retrievedQuote.GetQuoteOrThrow(quote.Id);

                // Assert
                quoteRetrieved.IsSubmitted.Should().BeTrue();
            }
        }

        [Fact]
        public async Task PolicyIsPersisted()
        {
            // Arrange
            var quoteAggregate = QuoteFactory.CreateNewPolicy();

            // Act
            await this.quoteAggregateRepository.Save(quoteAggregate);

            // Assert
            using (var newContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                this.eventRecordRepository = new EventRecordRepository(newContext, this.connectionConfig);
                var repo = new QuoteAggregateRepository(
                    newContext,
                    this.eventRecordRepository,
                    this.eventObserver,
                    this.aggregateSnapshotService.Object,
                    this.clock,
                    NullLogger<QuoteAggregateRepository>.Instance,
                    this.serviceProvider.Object);
                var retrievedApplication = repo.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                var quote = retrievedApplication.GetQuoteOrThrow(retrievedApplication.Policy.QuoteId.GetValueOrDefault());

                quote.Type.Should().Be(QuoteType.NewBusiness);
                quote.TransactionCompleted.Should().BeTrue();

                // Quote factory defaults inception date to today.
                var expectedInceptionDate = this.clock.Now().InZone(Timezones.AET).Date;
                retrievedApplication.Policy.LatestPolicyPeriodStartDateTime.Date.Should().Be(expectedInceptionDate);
            }
        }

        [Fact]
        public async Task QuoteNumberIsPersisted()
        {
            // Arrange
            var quoteNumber = "FOOBAR";
            var quote = QuoteFactory.CreateNewBusinessQuote();
            var quoteAggregate = quote.Aggregate
                .WithCustomer()
                .WithQuoteNumber(quote.Id, quoteNumber);

            // Act
            await this.quoteAggregateRepository.Save(quoteAggregate);

            // Assert
            using (var newContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                this.eventRecordRepository = new EventRecordRepository(newContext, this.connectionConfig);
                var repo = new QuoteAggregateRepository(
                    newContext,
                    this.eventRecordRepository,
                    this.eventObserver,
                    this.aggregateSnapshotService.Object,
                    this.clock,
                    NullLogger<QuoteAggregateRepository>.Instance,
                    this.serviceProvider.Object);
                var retrievedQuote = repo.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                var quotes = retrievedQuote.GetQuoteOrThrow(quote.Id);
                quotes.QuoteNumber.Should().Be(quoteNumber);
            }
        }

        [Fact]
        public async Task QuoteVersionIsPersisted()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var productId = Guid.NewGuid();
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                tenantId,
                tenant.Details.DefaultOrganisationId,
                productId,
                DeploymentEnvironment.Staging,
                QuoteExpirySettings.Default,
                this.performingUserId,
                this.clock.Now(),
                Guid.NewGuid(),
                Timezones.AET);
            var customerPerson = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, this.clock.Now());
            var customerAggregate = CustomerAggregate.CreateNewCustomer(
                tenant.Id, customerPerson, DeploymentEnvironment.Staging, this.performingUserId, null, this.clock.Now());
            quote.Aggregate.RecordAssociationWithCustomer(
                customerAggregate, customerPerson, this.performingUserId, this.clock.Now());
            var formDataId = quote.UpdateFormData(new FormData("{}"), this.performingUserId, this.clock.Now());
            var formDataSchema = new FormDataSchema(new JObject());
            var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create());
            var quoteDataRetreiver = new StandardQuoteDataRetriever(
                        new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates()),
                        new CachingJObjectWrapper(CalculationResultJsonFactory.Create()));
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataId;
            quote.RecordCalculationResult(
                    calculationResult,
                    calculationData,
                    this.clock.Now(),
                    formDataSchema,
                    false,
                    this.performingUserId);
            quote.AssignQuoteNumber("12345", this.performingUserId, this.clock.Now());
            quote.Aggregate.CreateVersion(this.performingUserId, this.clock.Now(), quote.Id);
            await this.quoteAggregateRepository.Save(quote.Aggregate);

            using (var newContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                this.eventRecordRepository = new EventRecordRepository(newContext, this.connectionConfig);
                var repo = new QuoteAggregateRepository(
                    newContext,
                    this.eventRecordRepository,
                    this.eventObserver,
                    this.aggregateSnapshotService.Object,
                    this.clock,
                    NullLogger<QuoteAggregateRepository>.Instance,
                    this.serviceProvider.Object);

                // Act
                var retrievedQuote = repo.GetById(tenant.Id, quote.Aggregate.Id);
                var quotes = retrievedQuote.GetQuoteOrThrow(quote.Id);

                // Assert
                quotes.VersionNumber.Should().Be(1);
            }
        }

        // TODO: Convert to persistence test.
        [Fact]
        public void IssuePolicy_ResultsInNewBusinessTransaction_WhenSuccessful()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewBusinessQuote();
            var aggregate = quote.Aggregate
                .WithCalculationResult(quote.Id)
                .WithCustomerDetails(quote.Id)
                .WithCustomer()
                .WithQuoteNumber(quote.Id);

            // Act
            var newBusinessQuote = quote as NewBusinessQuote;
            newBusinessQuote.IssuePolicy(
                quote.LatestCalculationResult.Id,
                () => "POL001",
                QuoteFactory.ProductConfiguation,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                Guid.NewGuid(),
                this.clock.Now(),
                this.quoteWorkflow);

            // Assert
            aggregate.Policy.Transactions.Should().ContainSingle();
        }

        [Fact]
        public async Task CustomerOperation_UpdatesCustomerDetailsOnly_WhenQuoteHasCustomerAssigned()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var productId = Guid.NewGuid();
            var customer = CustomerAggregate.CreateNewCustomer(
                tenantId,
                QuoteFactory.CreatePersonAggregate(tenantId),
                DeploymentEnvironment.Development,
                this.performingUserId,
                null,
                this.clock.Now());
            var quote = QuoteFactory.CreateNewBusinessQuote(tenantId, productId);
            var aggregate = quote.Aggregate
                .WithCalculationResult(quote.Id)
                .WithCustomer(customer)
                .WithCustomerDetails(quote.Id, QuoteFactory.CreatePersonAggregate(tenantId));
            await this.quoteAggregateRepository.Save(aggregate);

            var personCommonProperties = new PersonCommonProperties
            {
                FullName = "Foo",
                MobilePhoneNumber = "04 1234 1234",
                Email = "foo@example.com",
            };
            var newUpdatedDetails = PersonAggregate.CreatePersonFromPersonalDetails(
                tenantId,
                tenant.Details.DefaultOrganisationId,
                new PersonalDetails(tenantId, personCommonProperties),
                this.performingUserId,
                this.clock.Now());

            var customerDetails = new PersonalDetails(newUpdatedDetails);
            var customerSummary = new CustomerReadModelDetail
            {
                Id = customer.Id,
                TenantId = tenantId,
                PrimaryPersonId = customer.PrimaryPersonId,
            };

            this.customerReadModelRepository.Setup(e => e.GetCustomerById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(customerSummary);
            this.customerAggregateRepository.Setup((s) => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(customer);
            this.personAggregateRepository
                .Setup((p) => p.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(QuoteFactory.CreatePersonAggregate(tenantId)));
            this.quoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateIdForQuoteId(quote.Id))
                .Returns(quote.Aggregate.Id);
            this.quoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(quote.Aggregate.TenantId, quote.Id))
                .Returns(quote.Aggregate);

            // Act + Assert
            quote.LatestCustomerDetails.Data.FullName
                .Should().Be(QuoteFactory.CreatePersonAggregate(tenantId).FullName);
            quote.LatestCustomerDetails.Data.Email
                .Should().Be(QuoteFactory.CreatePersonAggregate(tenantId).Email);
            var quoteAggregate = await this.applicationQuoteService.CreateCustomerForApplication(
                tenant.Id, quote.Id, quote.Aggregate.Id, customerDetails, null);
            quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
            var latestCustomerDetails = quote.LatestCustomerDetails;
            quoteAggregate.CustomerId.Should().Be(customer.Id);
            latestCustomerDetails.Data.FullName.Should().Be("Foo");
        }

        [Fact]
        public async Task CustomerOperation_AssignsNewCustomer_WhenQuoteHasNoCustomerAssigned()
        {
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();

            // Arrange
            var tenant = new Tenant(
                tenantId,
                default,
                default,
                default,
                default,
                default,
                this.clock.Now());
            this.tenantRepository.Setup(x => x.GetTenantById(tenantId)).Returns(tenant);
            this.tenantRepository.Setup(x => x.GetTenantById(tenant.Id)).Returns(tenant);

            var quote = QuoteFactory.CreateNewBusinessQuote(tenantId);
            var aggregate = quote.Aggregate
                .WithCalculationResult(quote.Id);

            await this.quoteAggregateRepository.Save(aggregate);

            var personCommonProperties = new PersonCommonProperties
            {
                FullName = "Foo",
                MobilePhoneNumber = "04 1234 1234",
                Email = "foo@example.com",
                OrganisationId = organisationId,
            };
            this.tenantRepository.Setup(s => s.GetTenantById(tenantId)).Returns(tenant);

            var personDetails = new PersonalDetails(tenantId, personCommonProperties);
            var newUpdatedDetails = PersonAggregate.CreatePerson(
                tenantId, organisationId, this.performingUserId, this.clock.Now());
            newUpdatedDetails.Update(
                personDetails, this.performingUserId, this.clock.Now());
            var customerDetails = new PersonalDetails(newUpdatedDetails);
            this.quoteAggregateResolverService.Setup(
                s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(aggregate);

            this.cachingResolver.Setup(c => c.GetTenantOrThrow(tenantId)).Returns(Task.FromResult(tenant));
            this.personAggregateRepository.Setup((p) => p.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(newUpdatedDetails);

            var newCustomer = CustomerAggregate.CreateNewCustomer(
                tenantId, newUpdatedDetails, DeploymentEnvironment.Development, this.performingUserId, null, this.clock.Now());
            this.mediator.Setup(s => s.Send(It.IsAny<CreateCustomerCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(newCustomer.Id));
            this.customerAggregateRepository.Setup(x => x.GetById(tenant.Id, newCustomer.Id)).Returns(newCustomer);

            // Act
            var quoteAggregate = await this.applicationQuoteService.CreateCustomerForApplication(
                tenant.Id, quote.Id, aggregate.Id, customerDetails, null);
            var quotes = quoteAggregate.GetQuoteOrThrow(quote.Id);

            // Assert
            quoteAggregate.HasCustomer.Should().BeTrue();
            quotes.LatestCustomerDetails.Data.FullName.Should().Be(customerDetails.FullName);
        }

        private void SetupMockObjects()
        {
            this.userAggregateRepository = new Mock<IUserAggregateRepository>();
            this.tenantRepository = new Mock<ITenantRepository>();
            this.personAggregateRepository = new Mock<IPersonAggregateRepository>();
            this.customerAggregateRepository = new Mock<ICustomerAggregateRepository>();
            this.customerReadModelRepository = new Mock<ICustomerReadModelRepository>();
            this.claimReadModelRepository = new Mock<IClaimReadModelRepository>();
            this.cachingResolver = new Mock<ICachingResolver>();
        }

        private void SetupServices()
        {
            this.customerService = new CustomerService(
                this.customerAggregateRepository.Object,
                this.customerReadModelRepository.Object,
                this.userAggregateRepository.Object,
                this.personAggregateRepository.Object,
                this.httpContextPropertiesResolver.Object,
                this.personReadModelRepository.Object,
                SystemClock.Instance,
                this.additionalPropertyValueService.Object,
                this.cachingResolver.Object);

            this.policyService = new PolicyService(
                this.quoteAggregateRepository,
                SystemClock.Instance,
                this.cachingResolver.Object,
                this.claimReadModelRepository.Object,
                this.defaultProducConfigProvider,
                new UniqueIdentifierService(this.dbContext, this.cachingResolver.Object, SystemClock.Instance),
                new Mock<IQuoteDocumentReadModelRepository>().Object,
                new Mock<IPolicyNumberRepository>().Object,
                new Mock<IPolicyReadModelRepository>().Object,
                new Mock<ISystemAlertService>().Object,
                new QuoteReferenceNumberGenerator(
                    new UniqueNumberSequenceGenerator(DatabaseFixture.TestConnectionString)),
                this.httpContextPropertiesResolver.Object,
                this.productFeatureSettingService.Object,
                this.quoteWorkflowProvider,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                this.dbContext);
            var mockAggregateLockingService = new Mock<IAggregateLockingService>();
            mockAggregateLockingService
                .Setup(a => a.CreateLockOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AggregateType>()))
                .ReturnsAsync(It.IsAny<IRedLock>());

            this.applicationQuoteService = new ApplicationQuoteService(
                this.quoteAggregateRepository,
                this.tenantRepository.Object,
                this.userAggregateRepository.Object,
                this.personAggregateRepository.Object,
                this.quoteWorkflowProvider,
                this.customerService,
                this.quoteExpirySettingsProvider,
                this.customerAggregateRepository.Object,
                this.userLoginEmailRepository.Object,
                this.policyService,
                this.httpContextPropertiesResolver.Object,
                SystemClock.Instance,
                this.quoteAggregateResolverService.Object,
                this.organisationReadModelRepository.Object,
                this.productFeatureSettingService.Object,
                this.systemEmailService,
                this.mediator.Object,
                this.quoteSystemEventEmitter.Object);

            var mockTenantSystemEventEmitter = new Mock<ITenantSystemEventEmitter>();
            this.emailTemplateService = new EmailTemplateService(
                new Mock<ISystemEmailTemplateRepository>().Object,
                this.cachingResolver.Object,
                mockTenantSystemEventEmitter.Object,
                SystemClock.Instance);
            this.systemEmailService = new SystemEmailService(
            new Mock<IEmailService>().Object,
            new Mock<ISmtpClientConfiguration>().Object,
            this.emailTemplateService,
            new MailClientFactory(),
            new Mock<IJobClient>().Object,
            NullLogger<SystemEmailService>.Instance,
            this.mediator.Object,
            SystemClock.Instance,
            new Mock<IFileContentRepository>().Object);
        }

        private void SetupAggregatorsAndRepositories()
        {
            this.eventObserver = new Mock<IQuoteEventObserver>().Object;
            this.quoteAggregateRepository = new QuoteAggregateRepository(
                this.dbContext,
                this.eventRecordRepository,
                this.eventObserver,
                this.aggregateSnapshotService.Object,
                this.clock,
                NullLogger<QuoteAggregateRepository>.Instance,
                this.serviceProvider.Object);
        }
    }
}
