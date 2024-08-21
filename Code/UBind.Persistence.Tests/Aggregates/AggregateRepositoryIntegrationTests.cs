// <copyright file="AggregateRepositoryIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Person.Fields;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyDefinition;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Aggregates;
    using UBind.Persistence.ReadModels;
    using UBind.Persistence.ReadModels.Claim;
    using UBind.Persistence.ReadModels.Organisation;
    using UBind.Persistence.ReadModels.Quote;
    using UBind.Persistence.ReadModels.User;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;
    using FormData = UBind.Domain.Aggregates.Quote.FormData;
    using Quotes = UBind.Domain.Aggregates.Quote.Quote;

    [Collection(DatabaseCollection.Name)]
    public class AggregateRepositoryIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly IClock clock = SystemClock.Instance;

        [Fact]
        public async Task Upsert_PersistsUserAggregatesCorrectly()
        {
            // Arrange
            var connectionConfig = new ConnectionStrings();
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            connectionConfig.UBind = config.GetConnectionString(DatabaseFixture.TestConnectionStringName);

            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var timestamp = SystemClock.Instance.GetCurrentInstant();
            var tenant = new Tenant(tenantId, tenantId.ToString(), tenantId.ToString(), null, default, default, timestamp);
            var person = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, timestamp);
            var user = UserAggregate.CreateUser(tenant.Id, userId, UserType.Client, person, this.performingUserId, null, timestamp);
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var userReadModelRepo = new ReadModelUpdateRepository<UserReadModel>(dbContext);
            var loginEmailUpdateRepository = new ReadModelUpdateRepository<UserLoginEmail>(dbContext);
            var userLoginEmailRepository = new UserLoginEmailRepository(dbContext);
            var personReadModelRepo = new ReadModelUpdateRepository<PersonReadModel>(dbContext);
            var roleRepository = new RoleRepository(dbContext);
            var customerReadModelRepo = new ReadModelUpdateRepository<CustomerReadModel>(dbContext);
            var propertyTypeEvaluatorService = this.GeneratePropertyTypeEvaluatorService(dbContext);
            var customerReadModelWriter = new CustomerReadModelWriter(
                customerReadModelRepo,
                personReadModelRepo,
                userReadModelRepo,
                new PolicyReadModelRepository(dbContext, connectionConfig, this.clock),
                new QuoteReadModelRepository(dbContext, connectionConfig, this.clock),
                new EmailRepository(dbContext, this.clock),
                propertyTypeEvaluatorService);
            var emailReadModelrepository = new ReadModelUpdateRepository<EmailAddressReadModel>(dbContext);
            var phoneReadModelrepository = new ReadModelUpdateRepository<PhoneNumberReadModel>(dbContext);
            var addressReadModelrepository = new ReadModelUpdateRepository<StreetAddressReadModel>(dbContext);
            var webAddressReadModelrepository = new ReadModelUpdateRepository<WebsiteAddressReadModel>(dbContext);
            var socialReadModelrepository = new ReadModelUpdateRepository<SocialMediaIdReadModel>(dbContext);
            var messengerReadModelrepository = new ReadModelUpdateRepository<MessengerIdReadModel>(dbContext);
            var userReadModelWriter = new UserReadModelWriter(
                userReadModelRepo, loginEmailUpdateRepository, userLoginEmailRepository, roleRepository, propertyTypeEvaluatorService);
            var authenticationMethodRepository = new AuthenticationMethodReadModelRepository(dbContext);
            var userLinkedIdentityReadModelWriter = new UserLinkedIdentityReadModelWriter(dbContext, authenticationMethodRepository);
            var personReadModelWriter = new PersonReadModelWriter(
                personReadModelRepo,
                customerReadModelRepo,
                emailReadModelrepository,
                phoneReadModelrepository,
                addressReadModelrepository,
                webAddressReadModelrepository,
                messengerReadModelrepository,
                socialReadModelrepository,
                propertyTypeEvaluatorService);
            var userSystemEventEmitter = new Mock<IUserSystemEventEmitter>().Object;
            var userEventAggregator = new UserEventAggregator(
                userReadModelWriter,
                customerReadModelWriter,
                personReadModelWriter,
                userLinkedIdentityReadModelWriter,
                userSystemEventEmitter);
            var eventRepository = new EventRecordRepository(dbContext, connectionConfig);
            var repo = new UserAggregateRepository(
                dbContext,
                eventRepository,
                userEventAggregator,
                new Mock<IAggregateSnapshotService<UserAggregate>>().Object,
                SystemClock.Instance,
                NullLogger<UserAggregateRepository>.Instance,
                new Mock<IServiceProvider>().AddLoggers().Object);

            // Act
            await repo.Save(user);
            dbContext.SaveChanges();
            var dbContext2 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var userReadModelRepo2 = new ReadModelUpdateRepository<UserReadModel>(dbContext2);
            var roleRepository2 = new RoleRepository(dbContext);
            var userReadModelWriter2 = new UserReadModelWriter(
                userReadModelRepo2, loginEmailUpdateRepository, userLoginEmailRepository, roleRepository2, propertyTypeEvaluatorService);
            var authenticationMethodRepository2 = new AuthenticationMethodReadModelRepository(dbContext2);
            var userLinkedIdentityReadModelWriter2 = new UserLinkedIdentityReadModelWriter(dbContext2, authenticationMethodRepository2);
            var customerReadModelRepo2 = new ReadModelUpdateRepository<CustomerReadModel>(dbContext2);
            propertyTypeEvaluatorService = this.GeneratePropertyTypeEvaluatorService(dbContext2);
            var customerReadModelWriter2 = new CustomerReadModelWriter(
                customerReadModelRepo2,
                personReadModelRepo,
                userReadModelRepo2,
                new PolicyReadModelRepository(dbContext, connectionConfig, this.clock),
                new QuoteReadModelRepository(dbContext, connectionConfig, this.clock),
                new EmailRepository(dbContext2, this.clock),
                propertyTypeEvaluatorService);
            var personReadModelRepo2 = new ReadModelUpdateRepository<PersonReadModel>(dbContext2);
            var emailReadModelrepository2 = new ReadModelUpdateRepository<EmailAddressReadModel>(dbContext2);
            var phoneReadModelrepository2 = new ReadModelUpdateRepository<PhoneNumberReadModel>(dbContext2);
            var addressReadModelrepository2 = new ReadModelUpdateRepository<StreetAddressReadModel>(dbContext2);
            var webAddressReadModelrepository2 = new ReadModelUpdateRepository<WebsiteAddressReadModel>(dbContext2);
            var socialReadModelrepository2 = new ReadModelUpdateRepository<SocialMediaIdReadModel>(dbContext2);
            var messengerReadModelrepository2 = new ReadModelUpdateRepository<MessengerIdReadModel>(dbContext2);
            var personReadModelWriter2 = new PersonReadModelWriter(
                personReadModelRepo2,
                customerReadModelRepo2,
                emailReadModelrepository2,
                phoneReadModelrepository2,
                addressReadModelrepository2,
                webAddressReadModelrepository2,
                messengerReadModelrepository2,
                socialReadModelrepository2,
                propertyTypeEvaluatorService);
            var userSystemEventEmitter2 = new Mock<IUserSystemEventEmitter>().Object;
            var userEventAggregator2 = new UserEventAggregator(
                userReadModelWriter2,
                customerReadModelWriter2,
                personReadModelWriter2,
                userLinkedIdentityReadModelWriter2,
                userSystemEventEmitter2);
            eventRepository = new EventRecordRepository(dbContext2, connectionConfig);
            var repo2 = new UserAggregateRepository(
                dbContext2,
                eventRepository,
                userEventAggregator2,
                new Mock<IAggregateSnapshotService<UserAggregate>>().Object,
                SystemClock.Instance,
                NullLogger<UserAggregateRepository>.Instance,
                new Mock<IServiceProvider>().AddLoggers().Object);
            var retrievedUser = repo2.GetById(tenant.Id, userId);

            // Assert
            person.Id.Should().Be(retrievedUser.PersonId);
        }

        [Fact]
        public async Task Upsert_UpdatesReadModelCorrectly()
        {
            // Arrange
            var connectionConfig = new ConnectionStrings();
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            connectionConfig.UBind = config.GetConnectionString(DatabaseFixture.TestConnectionStringName);
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var timestamp = SystemClock.Instance.GetCurrentInstant();
            var tenant = new Tenant(tenantId, tenantId.ToString(), tenantId.ToString(), null, default, default, timestamp);
            var person = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, timestamp);
            var user = UserAggregate.CreateUser(person.TenantId, userId, UserType.Client, person, this.performingUserId, null, timestamp);
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var userReadModelRepository = new ReadModelUpdateRepository<UserReadModel>(dbContext);
            var userLoginEmailUpdateRepository = new ReadModelUpdateRepository<UserLoginEmail>(dbContext);
            var userLoginEmailRepository = new UserLoginEmailRepository(dbContext);
            var roleRepository = new RoleRepository(dbContext);
            var tenantRepository = new TenantRepository(dbContext);
            var userReadModelWriter = new UserReadModelWriter(
                userReadModelRepository, userLoginEmailUpdateRepository, userLoginEmailRepository, roleRepository, null);
            var eventRepository = new EventRecordRepository(dbContext, connectionConfig);
            var repo = new UserAggregateRepository(
                dbContext,
                eventRepository,
                userReadModelWriter,
                new Mock<IAggregateSnapshotService<UserAggregate>>().Object,
                SystemClock.Instance,
                NullLogger<UserAggregateRepository>.Instance,
                new Mock<IServiceProvider>().AddLoggers().Object);

            // Act
            await repo.Save(user);
            dbContext.SaveChanges();

            // Assert
            var dbContext2 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var readModel = dbContext2.Users.Single(u => u.Id == userId);

            readModel.CreatedTimestamp.Should().Be(timestamp);
            readModel.PersonId.Should().Be(person.Id);
        }

        [Fact]
        public async Task GetById_LoadsAllEvents_ForAlreadyLoadedAggregate()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            tenant.SetDefaultOrganisation(organisationId, this.clock.Now());
            stack1.CreateTenant(tenant);

            var originalQuote = QuoteAggregate.CreateNewBusinessQuote(
                tenant.Id,
                organisationId,
                productId,
                DeploymentEnvironment.Staging,
                QuoteExpirySettings.Default,
                this.performingUserId,
                this.clock.Now(),
                Guid.NewGuid(),
                Timezones.AET);
            var originalQuoteAggregate = originalQuote.Aggregate;

            var firstFormUpdate = "{ \"Update\": 1 }";
            originalQuote.UpdateFormData(new FormData(firstFormUpdate), this.performingUserId, this.clock.Now());
            await stack1.QuoteAggregateRepository.Save(originalQuoteAggregate);

            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var quoteAggregate = stack2.QuoteAggregateRepository.GetById(tenant.Id, originalQuoteAggregate.Id);
            var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var concurrentQuoteAggregate = stack3.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
            var secondFormUpdate = "{ \"Update\": 2 }";
            var quote = concurrentQuoteAggregate.GetQuoteOrThrow(originalQuote.Id);
            quote.UpdateFormData(new FormData(secondFormUpdate), this.performingUserId, this.clock.Now());
            await stack3.QuoteAggregateRepository.Save(concurrentQuoteAggregate);

            // Act
            quote = concurrentQuoteAggregate.GetQuoteOrThrow(originalQuote.Id);

            // Assert
            secondFormUpdate.Should().Be(quote.LatestFormData.Data.Json);
        }

        [Fact]
        public async Task GetById_LoadsNewEvents_WhenAggregateUpdatedInOtherDbContext()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            tenant.SetDefaultOrganisation(organisationId, this.clock.Now());
            stack1.CreateTenant(tenant);

            var quote = QuoteAggregate.CreateNewBusinessQuote(
                tenant.Id,
                organisationId,
                productId,
                DeploymentEnvironment.Staging,
                QuoteExpirySettings.Default,
                this.performingUserId,
                stack1.Clock.Now(),
                Guid.NewGuid(),
                Timezones.AET);
            await stack1.QuoteAggregateRepository.Save(quote.Aggregate);
            stack1.DbContext.Dispose();

            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var loadedQuote = stack2.QuoteAggregateRepository.GetById(tenant.Id, quote.Aggregate.Id);
            var thread = new Thread(async () =>
            {
                var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
                var parallelQuote = stack3.QuoteAggregateRepository.GetById(tenant.Id, quote.Aggregate.Id);
                quote = parallelQuote.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(new FormData(FormDataJsonFactory.Sample), this.performingUserId, this.clock.Now());
                await stack3.QuoteAggregateRepository.Save(parallelQuote);
            });
            thread.Start();
            thread.Join();

            // Act
            var reloadedQuote = stack2.QuoteAggregateRepository.GetById(tenant.Id, quote.Aggregate.Id);

            // Assert
            reloadedQuote.PersistedEventCount.Should().Be(2);
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task Save_ResetsDbContextUndoingAdds_WhenConcurrencyExceptionOccurs()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            Quotes quote = null;
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                tenant.SetDefaultOrganisation(organisationId, this.clock.Now());
                stack1.TenantRepository.Insert(tenant);
                stack1.TenantRepository.SaveChanges();
                quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenant.Id,
                    organisationId,
                    productId,
                    DeploymentEnvironment.Staging,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    stack1.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);
                await stack1.QuoteAggregateRepository.Save(quote.Aggregate);
            }

            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var loadedQuote = stack2.QuoteAggregateRepository.GetById(tenant.Id, quote.Id);
            var thread = new Thread(async () =>
            {
                var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
                var parallelQuote = stack3.QuoteAggregateRepository.GetById(tenant.Id, quote.Id);
                quote.UpdateFormData(new FormData(FormDataJsonFactory.Sample), this.performingUserId, this.clock.Now());
                await stack3.QuoteAggregateRepository.Save(parallelQuote);
            });
            thread.Start();
            thread.Join();

            quote.UpdateFormData(new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates()), this.performingUserId, this.clock.Now());

            // Act
            try
            {
                await stack2.QuoteAggregateRepository.Save(loadedQuote);
            }
            catch (ConcurrencyException ex)
            {
                // Ignore expected concurrency exception.
                Console.WriteLine(ex.Message);
            }

            // Assert
            var dbContextHasAddedEntities = stack2.DbContext.ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added)
                .Any();
            dbContextHasAddedEntities.Should().BeFalse();
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task Save_ResetsDbContextUndoingModifications_WhenConcurrencyExceptionOccurs()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            Quotes quote = null;
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                tenant.SetDefaultOrganisation(organisationId, this.clock.Now());
                stack1.TenantRepository.Insert(tenant);
                stack1.TenantRepository.SaveChanges();

                quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenant.Id,
                    organisationId,
                    productId,
                    DeploymentEnvironment.Staging,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    stack1.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);
                await stack1.QuoteAggregateRepository.Save(quote.Aggregate);
            }

            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var loadedQuote = stack2.QuoteAggregateRepository.GetById(tenant.Id, quote.Id);
            var thread = new Thread(async () =>
            {
                var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
                var parallelQuote = stack3.QuoteAggregateRepository.GetById(tenant.Id, quote.Id);
                quote.UpdateFormData(new FormData(FormDataJsonFactory.Sample), this.performingUserId, this.clock.Now());
                await stack3.QuoteAggregateRepository.Save(parallelQuote);
            });
            thread.Start();
            thread.Join();

            quote.UpdateFormData(new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates()), this.performingUserId, this.clock.Now());

            // Act
            try
            {
                await stack2.QuoteAggregateRepository.Save(loadedQuote);
            }
            catch (ConcurrencyException ex)
            {
                // Ignore expected concurrency exception.
                Console.WriteLine(ex.Message);
            }

            // Assert
            var dbContextHasAddedEntities = stack2.DbContext.ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added)
                .Any();
            dbContextHasAddedEntities.Should().BeFalse();
        }

        // To use this test comment out the transaction code in DatabaseIntegrationTests.
        [Fact(Skip = "This test will fail when run inside the transaction setup in DatabaseIntegrationTests.")]
        public async Task Save_DoesNotPersistAnyDataFromPreviouslyFailedSaves_WhenConcurrencyExceptionOccursAndDifferentEventsAreGenerated()
        {
            // Arrange
            //////////

            // Tenant / product are required or quote details cannot be queried.
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var product = ProductFactory.Create(tenantId, productId);
            using (var stack0 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                if (stack0.TenantRepository.GetTenantById(tenant.Id) == null)
                {
                    stack0.TenantRepository.Insert(tenant);
                }

                if (stack0.ProductRepository.GetProductById(tenant.Id, product.Id) == null)
                {
                    stack0.ProductRepository.Insert(product);
                }

                stack0.DbContext.SaveChanges();
            }

            // Create and persist a quote.
            QuoteAggregate quoteAggregate = null;
            Quotes quote = null;
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenant.Id,
                    organisationId,
                    product.Id,
                    DeploymentEnvironment.Staging,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    stack1.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);
                quoteAggregate = quote.Aggregate;
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
            }

            // Load the quote from a new stack in the main thread.
            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var loadedQuote = stack2.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
            var formData1 = FormDataJsonFactory.GetUniqueSample();

            // Update and save the quote in a different thread.
            var thread = new Thread(async () =>
            {
                using (var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
                {
                    var parallelQuote = stack3.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                    quote.UpdateFormData(new FormData(formData1), this.performingUserId, this.clock.Now());
                    await stack3.QuoteAggregateRepository.Save(parallelQuote);
                }
            });
            thread.Start();
            thread.Join();

            // Try and update and save the loaded quote in the main thread.
            var formData2 = FormDataJsonFactory.GetUniqueSample();
            var calculationResultJson = CalculationResultJsonFactory.Create();
            var formDataSchema = new FormDataSchema(new JObject());
            var formData = new FormData(formData2);
            var calculationData = new CachingJObjectWrapper(calculationResultJson);
            quote.UpdateFormData(formData, this.performingUserId, this.clock.Now());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            quote.RecordCalculationResult(
                CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver),
                calculationData,
                this.clock.Now(),
                formDataSchema,
                false,
                this.performingUserId);
            quote.UpdateFormData(new FormData(formData2), this.performingUserId, this.clock.Now());
            var concurrencyExceptionThrown = false;

            // Swallow the expected concurrency exception.
            try
            {
                await stack2.QuoteAggregateRepository.Save(loadedQuote);
            }
            catch (ConcurrencyException)
            {
                concurrencyExceptionThrown = true;
            }

            // Reload the quote aggregate in the same stack (same DbContext)
            var reloadedQuote = stack2.QuoteAggregateRepository.GetById(tenant.Id, loadedQuote.Id);
            var quoteReload = reloadedQuote.GetQuoteOrThrow(quote.Id);

            // Update and save the reloaded quote in the main thread.
            var formData3 = FormDataJsonFactory.GetUniqueSample();
            quote.UpdateFormData(new FormData(formData3), this.performingUserId, this.clock.Now());

            // Act
            //////
            await stack2.QuoteAggregateRepository.Save(reloadedQuote);

            // Assert
            ////////
            var quoteDetails = stack2.QuoteReadModelRepository.GetQuoteDetails(tenant.Id, quoteReload.Id);
            Assert.True(concurrencyExceptionThrown, "If concurrency exception has not been thrown, test isn't testing properly.");
            quoteDetails.LatestFormData.Should().BeEquivalentTo(formData3, "The read model is not being updated from the saved event.");
            quoteDetails.SerializedLatestCalculationResult.Should().BeNull("The read model is being updated with data from the failed event saving.");
        }

        private PropertyTypeEvaluatorService GeneratePropertyTypeEvaluatorService(IUBindDbContext uBindDbContext)
        {
            var mockConnectionConfiguration = new Mock<IConnectionConfiguration>().Object;
            var textAdditionalPropertyValueReadModelWriter = new TextAdditionalPropertyValueReadModelWriter(
                new ReadModelUpdateRepository<TextAdditionalPropertyValueReadModel>(uBindDbContext));
            var additionalPropertyDefinitionRepository = new AdditionalPropertyDefinitionRepository(uBindDbContext);
            var quoteReadModelRepository = new QuoteReadModelRepository(
                uBindDbContext, mockConnectionConfiguration, this.clock);
            var claimReadModelRepository = new ClaimReadModelRepository(
                uBindDbContext, new Mock<ICachingResolver>().Object, this.clock, mockConnectionConfiguration);
            var policyReadModelRepository = new PolicyReadModelRepository(
                uBindDbContext, mockConnectionConfiguration, this.clock);
            var claimVersionReadModelRepository = new ClaimVersionReadModelRepository(uBindDbContext);
            var quoteVersionReadModelRepository = new QuoteVersionReadModelRepository(uBindDbContext);
            var policyTransactionReadModelRepository = new PolicyTransactionReadModelRepository(uBindDbContext, mockConnectionConfiguration);
            var additionalPropertyFilterResolver = new AdditionalPropertyDefinitionFilterResolver(
                quoteReadModelRepository, claimReadModelRepository, policyReadModelRepository, policyTransactionReadModelRepository, claimVersionReadModelRepository, quoteVersionReadModelRepository);
            var textAdditionalPropertyWritableReadModelMock = new Mock<IWritableReadModelRepository<TextAdditionalPropertyValueReadModel>>(
                MockBehavior.Strict);
            var dictionary = new Dictionary<AdditionalPropertyDefinitionType, IAdditionalPropertyValueProcessor>
            {
                {
                    AdditionalPropertyDefinitionType.Text,
                    new TextAdditionalPropertyValueProcessor(
                        new TextAdditionalPropertyValueReadModelRepository(
                            uBindDbContext,
                            additionalPropertyDefinitionRepository,
                            additionalPropertyFilterResolver),
                        this.clock,
                        new TextAdditionalPropertyValueAggregateRepository(
                            uBindDbContext,
                            new Mock<IEventRecordRepository>().Object,
                            textAdditionalPropertyValueReadModelWriter,
                            new Mock<IAggregateSnapshotService<TextAdditionalPropertyValue>>().Object,
                            this.clock,
                            NullLogger<TextAdditionalPropertyValueAggregateRepository>.Instance,
                            new Mock<IServiceProvider>().AddLoggers().Object),
                        new ReadModelUpdateRepository<TextAdditionalPropertyValueReadModel>(uBindDbContext))
                },
            };
            var propertyTypeEvaluatorService = new PropertyTypeEvaluatorService(dictionary);
            return propertyTypeEvaluatorService;
        }
    }
}
