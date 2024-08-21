// <copyright file="QuoteCreationIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Commands.QuoteCalculation;
    using UBind.Application.Commands.Tenant;
    using UBind.Application.Queries.Tenant;
    using UBind.Application.Releases;
    using UBind.Application.Tests;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Authentication;
    using UBind.Domain.Commands.Policy;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;
    using FormData = UBind.Domain.Aggregates.Quote.FormData;
    using Product = UBind.Domain.Product;
    using Quote = UBind.Domain.Aggregates.Quote.Quote;
    using Tenant = UBind.Domain.Tenant;

    [Collection(DatabaseCollection.Name)]
    public class QuoteCreationIntegrationTests : IAsyncLifetime
    {
        private readonly Guid? performingUserId = Guid.NewGuid();

        // Factory for stack configured to use real quote expiry setting provider.
        private readonly Func<ApplicationStack> stackFactory = () => new ApplicationStack(
            DatabaseFixture.TestConnectionStringName,
            ApplicationStackConfiguration.WithRealQuoteExpirySettingProvider());

        private Tenant tenantAdmin;
        private Product.Product product;

        public async Task InitializeAsync()
        {
            using (var stack = this.stackFactory())
            {
                this.tenantAdmin = await stack.Mediator.Send(new GetTenantByAliasQuery("test-tenant"));
                if (this.tenantAdmin == null)
                {
                    var tenantId = await stack.Mediator.Send(new CreateTenantCommand("Test Tenant", "test-tenant", null));
                    this.tenantAdmin = await stack.Mediator.Send(new GetTenantByIdQuery(tenantId));
                    this.product = ProductFactory.Create(tenantId);
                    stack.ProductRepository.Insert(this.product);
                    stack.ProductRepository.SaveChanges();
                }

                this.product = ProductFactory.Create(this.tenantAdmin.Id);
                stack.ProductRepository.Insert(this.product);
                stack.ProductRepository.SaveChanges();
            }
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// If you want to run this test, you need to remove the Skip attribute.
        /// This was skipped because it was heavy since we need to trigger 100 calculation and save the snapshot.
        /// This test needed for testing the aggregate snapshot creation when have 100 events triggered.
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = "This test was heavy since we need to trigger 200 calculation and save the snapshot")]
        public async Task CreatQuoteAggregate_ShouldCreateAggregateSnapshot_WhenHave100Events()
        {
            QuoteAggregate quoteAggregate;
            Quote quote;
            FormData formDataJson;

            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                Tenant tenant = TenantFactory.Create();
                Product.Product product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
                stack1.CreateTenant(tenant);
                stack1.CreateProduct(product);

                stack1.DbContext.SaveChanges();

                Assert.True(stack1.TenantRepository.GetTenantById(tenant.Id) != null);
                Assert.True(stack1.ProductRepository.GetProductById(tenant.Id, product.Id) != null);

                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null,
                    this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack1.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                quote = QuoteFactory.CreateNewBusinessQuote(tenant.Id, product.Id, organisationId: organisation.Id);
                quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);
                var numberInterval = stack1.QuoteAggregateRepository.GetSnapshotSaveInterval() + 5;
                for (int i = 0; i < numberInterval; i++)
                {
                    var formData = new FormData(FormDataJsonFactory.GetCustomSample("data1"));
                    quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                    quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                    await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                    quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);

                    if (quoteAggregate.PersistedEventCount == 100)
                    {
                        formDataJson = new FormData(FormDataJsonFactory.GetCustomSample("myFormData"));
                        quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                        quote.UpdateFormData(formDataJson, this.performingUserId, stack1.Clock.Now());
                        IQuoteWorkflow quoteWorkflow = new DefaultQuoteWorkflow();
                        quote.Submit(this.performingUserId, stack1.Clock.Now(), quoteWorkflow);
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                    }
                }

                // wait until the snapshot to be inserted since it being processed on the background.
                await Task.Delay(200);

                // After the snapshot is inserted, we will make sure once we get the aggregate, it should not throw an error already submitted.
                Action action = () => stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                action.Should().NotThrow<ErrorException>();

                var aggregateSnapshot = await stack1.QuoteAggregateSnapshotService.GetAggregateSnapshotAsync(
                               quote.Aggregate.TenantId, quote.Aggregate.Id, AggregateType.Quote);
                aggregateSnapshot.Should().NotBeNull();
                aggregateSnapshot.Aggregate.Should().NotBeNull();

                var unstrippedEvents = quoteAggregate.GetUnstrippedEvents();
                var eventsWithSequenceNumber = unstrippedEvents.Select((@event, i) => new KeyValuePair<int, IEvent<QuoteAggregate, Guid>>(i, @event));
                var events = eventsWithSequenceNumber.Where(x => x.Key > aggregateSnapshot.Version).Select(x => x.Value);
                var snapshotAggregate = aggregateSnapshot.Aggregate;
                snapshotAggregate = snapshotAggregate.ApplyEventsAfterSnapshot(events, aggregateSnapshot.Version);

                // Assert
                snapshotAggregate.Id.Should().Be(quote.Aggregate.Id);
                snapshotAggregate.TenantId.Should().Be(quote.Aggregate.TenantId);
                snapshotAggregate.ProductId.Should().Be(quote.Aggregate.ProductId);
                snapshotAggregate.OrganisationId.Should().Be(quote.Aggregate.OrganisationId);
                snapshotAggregate.Quotes.Count.Should().Be(quoteAggregate.Quotes.Count);
                snapshotAggregate.Quotes[0].Id.Should().Be(quoteAggregate.Quotes[0].Id);
                snapshotAggregate.Quotes[0].IsSubmitted.Should().Be(quoteAggregate.Quotes[0].IsSubmitted);
                snapshotAggregate.Policy.Should().Be(quoteAggregate.Policy);
            }
        }

        [Fact]
        public async Task CreateAdjustmentQuote_CreatesQuoteReadModel()
        {
            // Arrange
            var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);

            stack1.MockMediator.GetTenantByIdOrAliasQuery(this.tenantAdmin);
            stack1.MockMediator.GetProductByIdOrAliasQuery(this.product);
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                this.tenantAdmin.Id,
                this.tenantAdmin.Details.Alias,
                this.tenantAdmin.Details.Name,
                this.tenantAdmin.Details.DefaultOrganisationId,
                this.performingUserId,
                stack1.Clock.GetCurrentInstant());
            this.tenantAdmin.SetDefaultOrganisation(
                organisation.Id,
                stack1.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            stack1.TenantRepository.SaveChanges();
            await stack1.OrganisationAggregateRepository.Save(organisation);

            stack1.AutoServePolicyNumbersForTenant(this.tenantAdmin.Id);
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            var calculationDataModel = JsonConvert.DeserializeObject<SpreadsheetCalculationDataModel>(formDataJson);
            await stack1.ProductOrganisationSettingRepository.UpdateProductSetting(this.tenantAdmin.Id, this.tenantAdmin.Details.DefaultOrganisationId, this.product.Id, true);
            stack1.CreateRelease(new ReleaseContext(this.tenantAdmin.Id, this.product.Id, DeploymentEnvironment.Staging, Guid.NewGuid()));
            var quoteReadModel = await stack1.Mediator.Send(new CreateNewBusinessQuoteCommand(
                this.tenantAdmin.Id,
                this.tenantAdmin.Details.DefaultOrganisationId,
                null,
                this.product.Id,
                DeploymentEnvironment.Staging,
                false,
                null,
                null,
                null));
            var quoteAggregate = stack1.QuoteAggregateRepository.GetById(this.tenantAdmin.Id, quoteReadModel.AggregateId);
            var quote = quoteAggregate.GetQuoteOrThrow(quoteReadModel.Id);
            var quoteCalculationCommand = new QuoteCalculationCommand(
                    quote.ProductContext,
                    quote.Id,
                    null,
                    quote.Type,
                    null,
                    calculationDataModel,
                    null,
                    true,
                    true,
                    quote.Aggregate.OrganisationId);
            await stack1.Mediator.Send(quoteCalculationCommand);
            var originalQuoteAggregate = stack1.QuoteAggregateRepository.GetById(this.tenantAdmin.Id, quote.Aggregate.Id);
            var twelveMonthsAgo = SystemClock.Instance.Today().PlusMonths(-12);
            originalQuoteAggregate.WithCalculationResult(quote.Id, calculationResultJson: CalculationResultJsonFactory.Create(startDate: twelveMonthsAgo));
            await stack1.QuoteAggregateRepository.Save(originalQuoteAggregate);
            originalQuoteAggregate = stack1.QuoteAggregateRepository.GetById(quote.Aggregate.TenantId, quote.Aggregate.Id);
            var quoteOrginal = originalQuoteAggregate.GetQuoteOrThrow(quote.Id);
            var releaseContext = new ReleaseContext(quote.ProductContext, quote.ProductReleaseId.Value);
            await stack1.ApplicationQuoteService.Actualise(
                releaseContext,
                quoteOrginal,
                new Domain.Aggregates.Quote.FormData(formDataJson));
            await stack1.Mediator.Send(new CompletePolicyTransactionCommand(
                this.tenantAdmin.Id,
                quoteOrginal.Id,
                quoteOrginal.LatestCalculationResult.Id,
                new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates())));
            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            stack2.MockMediator.GetTenantByIdOrAliasQuery(this.tenantAdmin);
            stack2.MockMediator.GetProductByIdOrAliasQuery(this.product);
            var updatedQuoteAggregate = stack1.QuoteAggregateRepository.GetById(this.tenantAdmin.Id, originalQuoteAggregate.Id);
            stack2.CreateRelease(new ReleaseContext(this.tenantAdmin.Id, this.product.Id, DeploymentEnvironment.Staging, Guid.NewGuid()));

            // Act
            var adjustmentQuote = await stack2.Mediator.Send(new CreateAdjustmentQuoteCommand(
                this.tenantAdmin.Id, updatedQuoteAggregate.Policy.PolicyId, false));

            // Assert
            var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var quoteReadModel2 = stack3.QuoteReadModelRepository.GetQuoteDetails(adjustmentQuote.TenantId, adjustmentQuote.Id);
            Assert.NotNull(quoteReadModel2);
        }

        [Fact]
        public async Task CreateNewBusinessQuote_ShouldMatchTheSpecifiedOrganisationId()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id);

            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack1.TenantRepository.Insert(tenant);
                stack1.ProductRepository.Insert(product);
                stack1.TenantRepository.SaveChanges();

                stack1.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                stack1.MockMediator.GetProductByIdOrAliasQuery(product);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    tenant.Details.DefaultOrganisationId,
                    this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(
                    organisation.Id,
                    stack1.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
                stack1.TenantRepository.SaveChanges();
                await stack1.OrganisationAggregateRepository.Save(organisation);

                stack1.AutoServePolicyNumbersForTenant(tenant.Id);
                var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
                var calculationDataModel = JsonConvert.DeserializeObject<SpreadsheetCalculationDataModel>(formDataJson);
                await stack1.ProductOrganisationSettingRepository.UpdateProductSetting(tenant.Id, tenant.Details.DefaultOrganisationId, product.Id, true);
                stack1.CreateRelease(new ReleaseContext(tenant.Id, product.Id, DeploymentEnvironment.Staging, Guid.NewGuid()));
                var quoteReadModel = await stack1.Mediator.Send(new CreateNewBusinessQuoteCommand(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    null,
                    product.Id,
                    DeploymentEnvironment.Staging,
                    false,
                    null,
                    null,
                    null));
                var quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteReadModel.AggregateId);
                var quote = quoteAggregate.GetQuoteOrThrow(quoteReadModel.Id);
                var quoteCalculationCommand = new QuoteCalculationCommand(
                    quote.ProductContext,
                    quote.Id,
                    null,
                    quote.Type,
                    quote.ProductReleaseId,
                    calculationDataModel,
                    null,
                    true,
                    true,
                    quote.Aggregate.OrganisationId);
                await stack1.Mediator.Send(quoteCalculationCommand);
                quote.Aggregate.OrganisationId.Should().Be(organisation.Id);
            }
        }

        [Fact]
        public async Task CreateNewBusinessQuote_ShouldNotMatchTheSpecifiedOrganisationId()
        {
            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack1.MockMediator.GetTenantByIdOrAliasQuery(this.tenantAdmin);
                stack1.MockMediator.GetProductByIdOrAliasQuery(this.product);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    this.tenantAdmin.Id,
                    this.tenantAdmin.Details.Alias,
                    this.tenantAdmin.Details.Name,
                    this.tenantAdmin.Details.DefaultOrganisationId,
                    this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                this.tenantAdmin.SetDefaultOrganisation(
                    organisation.Id,
                    stack1.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
                stack1.TenantRepository.SaveChanges();
                await stack1.OrganisationAggregateRepository.Save(organisation);

                stack1.AutoServePolicyNumbersForTenant(this.tenantAdmin.Id);
                var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
                var calculationDataModel = JsonConvert.DeserializeObject<SpreadsheetCalculationDataModel>(formDataJson);
                var differentOrgId = Guid.NewGuid();
                await stack1.ProductOrganisationSettingRepository.UpdateProductSetting(
                    this.tenantAdmin.Id, differentOrgId, this.product.Id, true);
                stack1.CreateRelease(new ReleaseContext(this.tenantAdmin.Id, this.product.Id, DeploymentEnvironment.Staging, Guid.NewGuid()));
                var quoteReadModel = await stack1.Mediator.Send(new CreateNewBusinessQuoteCommand(
                    this.tenantAdmin.Id,
                    differentOrgId,
                    null,
                    this.product.Id,
                    DeploymentEnvironment.Staging,
                    false,
                    null,
                    null,
                    null));
                var quoteAggregate = stack1.QuoteAggregateRepository.GetById(this.tenantAdmin.Id, quoteReadModel.AggregateId);
                var quote = quoteAggregate.GetQuoteOrThrow(quoteReadModel.Id);
                var quoteCalculationCommand = new QuoteCalculationCommand(
                    quote.ProductContext,
                    quote.Id,
                    null,
                    quote.Type,
                    quote.ProductReleaseId,
                    calculationDataModel,
                    null,
                    true,
                    true,
                    quote.Aggregate.OrganisationId);
                await stack1.Mediator.Send(quoteCalculationCommand);
                quote.Aggregate.OrganisationId.Should().NotBe(organisation.Id);
            }
        }

        [Fact]
        public async Task CreateCancellationQuote_ShouldCreateQuoteModel()
        {
            // Arrange
            var userAuthData = new UserAuthenticationData(
                this.tenantAdmin.Id,
                this.tenantAdmin.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                Guid.NewGuid());

            var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            stack1.MockMediator.GetTenantByIdOrAliasQuery(this.tenantAdmin);
            stack1.MockMediator.GetProductByIdOrAliasQuery(this.product);
            stack1.MockReleaseQueryService.Setup(
                x => x.GetRelease(It.IsAny<ReleaseContext>()))
                .Returns((ActiveDeployedRelease)null);
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                this.tenantAdmin.Id,
                this.tenantAdmin.Details.Alias,
                this.tenantAdmin.Details.Name,
                this.tenantAdmin.Details.DefaultOrganisationId,
                this.performingUserId,
                stack1.Clock.GetCurrentInstant());
            this.tenantAdmin.SetDefaultOrganisation(
                organisation.Id,
                stack1.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            stack1.TenantRepository.SaveChanges();
            await stack1.OrganisationAggregateRepository.Save(organisation);

            stack1.AutoServePolicyNumbersForTenant(this.tenantAdmin.Id);
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            SpreadsheetCalculationDataModel calculationDataModel
                = JsonConvert.DeserializeObject<SpreadsheetCalculationDataModel>(formDataJson);
            await stack1.ProductOrganisationSettingRepository.UpdateProductSetting(
                this.tenantAdmin.Id, this.tenantAdmin.Details.DefaultOrganisationId, this.product.Id, true);
            stack1.CreateRelease(new ReleaseContext(this.tenantAdmin.Id, this.product.Id, DeploymentEnvironment.Staging, Guid.NewGuid()));
            var quoteReadModel = await stack1.Mediator.Send(new CreateNewBusinessQuoteCommand(
                this.tenantAdmin.Id,
                this.tenantAdmin.Details.DefaultOrganisationId,
                null,
                this.product.Id,
                DeploymentEnvironment.Staging,
                false,
                null,
                null,
                null));
            var quoteAggregate = stack1.QuoteAggregateRepository.GetById(this.tenantAdmin.Id, quoteReadModel.AggregateId);
            var quote = quoteAggregate.GetQuoteOrThrow(quoteReadModel.Id);
            var quoteCalculationCommand = new QuoteCalculationCommand(
                quote.ProductContext,
                quote.Id,
                null,
                quote.Type,
                quote.ProductReleaseId,
                calculationDataModel,
                null,
                true,
                true,
                quote.Aggregate.OrganisationId);
            await stack1.Mediator.Send(quoteCalculationCommand);
            var originalQuoteAggregate = stack1.QuoteAggregateRepository.GetById(this.tenantAdmin.Id, quote.Aggregate.Id);
            var twelveMonthsAgo = SystemClock.Instance.Today().PlusMonths(-12);
            originalQuoteAggregate.WithCalculationResult(quote.Id, calculationResultJson: CalculationResultJsonFactory.Create(startDate: twelveMonthsAgo));
            await stack1.QuoteAggregateRepository.Save(originalQuoteAggregate);
            originalQuoteAggregate = stack1.QuoteAggregateRepository.GetById(quote.Aggregate.TenantId, quote.Aggregate.Id);
            var quoteOriginal = originalQuoteAggregate.GetQuoteOrThrow(quote.Id);
            var releaseContext = new ReleaseContext(quote.ProductContext, quote.ProductReleaseId.Value);
            await stack1.ApplicationQuoteService.Actualise(
                releaseContext,
                quote,
                new FormData(formDataJson));
            await stack1.Mediator.Send(new CompletePolicyTransactionCommand(
                this.tenantAdmin.Id,
                quoteOriginal.Id,
                quoteOriginal.LatestCalculationResult.Id,
                new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates())));
            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            stack2.MockMediator.GetTenantByIdOrAliasQuery(this.tenantAdmin);
            stack2.MockMediator.GetProductByIdOrAliasQuery(this.product);
            var updatedQuoteAggregate = stack1.QuoteAggregateRepository.GetById(this.tenantAdmin.Id, originalQuoteAggregate.Id);
            stack2.CreateRelease(new ReleaseContext(this.tenantAdmin.Id, this.product.Id, DeploymentEnvironment.Staging, Guid.NewGuid()));

            // Act
            var cancelQuote = await stack2.Mediator.Send(new CreateCancellationQuoteCommand(
                userAuthData.TenantId, updatedQuoteAggregate.Policy.PolicyId, false));

            // Assert
            var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var quoteReadModel2 = stack3.QuoteReadModelRepository.GetQuoteDetails(quoteReadModel.TenantId, cancelQuote.Id);
            quoteReadModel2.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateReplacementQuoteForExpiredQuote_DiscardCurrentAndAddNewQuoteToAggregate_IfQuoteIsRenewal()
        {
            // Arrange
            var environment = DeploymentEnvironment.Development;
            var quoteNumber = "FXOWQ";
            var quoteExpirySettings = new QuoteExpirySettings(30, true);

            using (var stack = this.stackFactory())
            {
                stack.Clock.Increment(Duration.FromSeconds(2)); // needed to have higher created time.
                this.product = stack.ProductService.UpdateQuoteExpirySettings(
                    this.tenantAdmin.Id,
                    this.product.Id,
                    quoteExpirySettings,
                    ProductQuoteExpirySettingUpdateType.UpdateNone,
                    CancellationToken.None);
            }

            QuoteAggregate oldQuoteAggregate;
            Quote quote, firstRenewalQuote, policyIssuedQuote;
            using (var stack = this.stackFactory())
            {
                // Step 1: create quote
                quote = QuoteAggregate.CreateNewBusinessQuote(
                   this.tenantAdmin.Id,
                   this.tenantAdmin.Details.DefaultOrganisationId,
                   this.product.Id,
                   environment,
                   QuoteExpirySettings.Default,
                   this.performingUserId,
                   stack.Clock.Now(),
                   Guid.NewGuid(),
                   Timezones.AET);
                oldQuoteAggregate = quote.Aggregate;

                // Step 2: create customer.
                var defaultOrganisation = this.tenantAdmin.Details.DefaultOrganisationId;
                var customerPerson = PersonAggregate.CreatePerson(
                    this.tenantAdmin.Id, defaultOrganisation, this.performingUserId, stack.Clock.Now());
                var personCommonProperties = new PersonCommonProperties
                {
                    Email = "jeo2@gmail.com",
                    FullName = "Test",
                };

                customerPerson.Update(
                    new PersonalDetails(this.tenantAdmin.Id, personCommonProperties), this.performingUserId, stack.Clock.Now());
                await stack.PersonAggregateRepository.Save(customerPerson);

                // Step 3: associate
                CustomerAggregate customerAggregate = await this.CreateCustomer(
                    stack, customerPerson, environment);
                oldQuoteAggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());
                oldQuoteAggregate.UpdateCustomerDetails(
                    new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now(), quote.Id);
                quote.AssignQuoteNumber(quoteNumber, this.performingUserId, stack.Clock.Now());
                oldQuoteAggregate.WithCalculationResult(quote.Id);

                // Step 4: create policy and renewal quote
                oldQuoteAggregate.WithPolicy(quote.Id, "POLICY-2");
                policyIssuedQuote = oldQuoteAggregate.GetQuoteOrThrow(quote.Id);

                var renewalQuote = oldQuoteAggregate.WithRenewalQuote();
                firstRenewalQuote = oldQuoteAggregate.GetQuoteOrThrow(renewalQuote.Id);

                stack.MockMediator.GetTenantByIdOrAliasQuery(this.tenantAdmin);

                // Step 5: expire the quote
                var now = stack.Clock.Now();
                oldQuoteAggregate.SetQuoteExpiryTime(firstRenewalQuote.Id, now, this.performingUserId, now);
                await stack.QuoteAggregateRepository.Save(oldQuoteAggregate);
            }

            using (var stack = this.stackFactory())
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(this.tenantAdmin);
                stack.MockMediator.GetProductByIdOrAliasQuery(this.product);

                // Act
                var duplicateQuote = await stack.Mediator.Send(new CloneQuoteFromExpiredQuoteCommand(
                    this.tenantAdmin.Id, firstRenewalQuote.Id, DeploymentEnvironment.Development));

                // Assert
                duplicateQuote.Id.Should().NotBe(policyIssuedQuote.Id);
                duplicateQuote.PolicyId.Should().Be(oldQuoteAggregate.Id);
                duplicateQuote.Id.Should().NotBe(policyIssuedQuote.Id);
                duplicateQuote.QuoteNumber.Should().NotBe(policyIssuedQuote.QuoteNumber);
                duplicateQuote.LatestFormData.Should().Be(
                    firstRenewalQuote.LatestFormData.Data.Json);
                policyIssuedQuote.IsDiscarded.Should().BeFalse();
                duplicateQuote.Type.Should().Be(firstRenewalQuote.Type);
                duplicateQuote.CustomerId.Should().Be(oldQuoteAggregate.CustomerId);
                duplicateQuote.PolicyId.Should().NotBeNull();
                policyIssuedQuote.TransactionCompleted.Should().BeTrue();
            }
        }

        [Fact]
        public async Task CreateReplacementQuoteForExpiredQuote_DiscardCurrentAndAddNewQuoteToAggregate_IfQuoteIsAdjustment()
        {
            // Arrange
            var environment = DeploymentEnvironment.Development;
            var quoteNumber = "FXOWQ";
            var quoteExpirySettings = new QuoteExpirySettings(30, true);

            using (var stack = this.stackFactory())
            {
                stack.Clock.Increment(Duration.FromSeconds(2)); // needed to have higher created time.
                this.product = stack.ProductService.UpdateQuoteExpirySettings(
                    this.tenantAdmin.Id,
                    this.product.Id,
                    quoteExpirySettings,
                    ProductQuoteExpirySettingUpdateType.UpdateNone,
                    CancellationToken.None);
            }

            QuoteAggregate oldQuoteAggregate = null;
            Quote firstAdjustmentQuote = null;
            Quote policyIssuedQuote = null;
            Quote quote = null;
            using (var stack = this.stackFactory())
            {
                // Step 1: create quote
                quote = QuoteAggregate.CreateNewBusinessQuote(
                  this.tenantAdmin.Id,
                  this.tenantAdmin.Details.DefaultOrganisationId,
                  this.product.Id,
                  environment,
                  QuoteExpirySettings.Default,
                  this.performingUserId,
                  stack.Clock.Now(),
                  Guid.NewGuid(),
                  Timezones.AET);
                oldQuoteAggregate = quote.Aggregate;

                // Step 2: create customer.
                var defaultOrganisation = this.tenantAdmin.Details.DefaultOrganisationId;
                var customerPerson = PersonAggregate.CreatePerson(
                    this.tenantAdmin.Id, defaultOrganisation, this.performingUserId, stack.Clock.Now());
                var personCommonProperties = new PersonCommonProperties
                {
                    Email = "jeo3@gmail.com",
                    FullName = "Test",
                };

                customerPerson.Update(
                    new PersonalDetails(this.tenantAdmin.Id, personCommonProperties), this.performingUserId, stack.Clock.Now());

                await stack.PersonAggregateRepository.Save(customerPerson);

                // Step 3: associate
                CustomerAggregate customerAggregate = await this.CreateCustomer(
                    stack, customerPerson, environment);
                oldQuoteAggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());
                oldQuoteAggregate.UpdateCustomerDetails(
                    new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now(), quote.Id);
                quote.AssignQuoteNumber(quoteNumber, this.performingUserId, stack.Clock.Now());
                oldQuoteAggregate.WithCalculationResult(quote.Id);

                // Step 4: create policy and adjustment quote
                oldQuoteAggregate.WithPolicy(quote.Id, "POLICY-2");
                policyIssuedQuote = oldQuoteAggregate.GetQuoteOrThrow(quote.Id);

                var adjustmentOldQuote = oldQuoteAggregate.WithAdjustmentQuote();
                firstAdjustmentQuote = oldQuoteAggregate.GetQuoteOrThrow(adjustmentOldQuote.Id);

                stack.MockMediator.GetTenantByIdOrAliasQuery(this.tenantAdmin);

                // Step 5: expire the quote
                var now = stack.Clock.Now();
                oldQuoteAggregate.SetQuoteExpiryTime(firstAdjustmentQuote.Id, now, this.performingUserId, now);
                await stack.QuoteAggregateRepository.Save(oldQuoteAggregate);
            }

            // Act
            using (var stack = this.stackFactory())
            {
                var clonedQuoteReadModel = await stack.Mediator.Send(new CloneQuoteFromExpiredQuoteCommand(
                    this.tenantAdmin.Id, firstAdjustmentQuote.Id, DeploymentEnvironment.Development));
                var newQuoteAggregate = stack.QuoteAggregateRepository.GetById(this.tenantAdmin.Id, oldQuoteAggregate.Id);
                var newQuote = newQuoteAggregate.GetQuoteOrThrow(clonedQuoteReadModel.Id);

                // Assert
                clonedQuoteReadModel.Id.Should().NotBe(policyIssuedQuote.Id);
                newQuote.Id.Should().NotBe(firstAdjustmentQuote.Id);
                newQuote.QuoteNumber.Should().NotBe(firstAdjustmentQuote.QuoteNumber);
                clonedQuoteReadModel.PolicyId.Should().Be(newQuoteAggregate.Id);
                clonedQuoteReadModel.Id.Should().Be(newQuote.Id);
                clonedQuoteReadModel.QuoteNumber.Should().Be(newQuote.QuoteNumber);
                clonedQuoteReadModel.LatestFormData.Should().Be(
                    newQuote.LatestFormData.Data.Json);
                policyIssuedQuote.IsDiscarded.Should().BeFalse();
                clonedQuoteReadModel.QuoteState.Should().Be(newQuote.QuoteStatus);
                clonedQuoteReadModel.Type.Should().Be(newQuote.Type);
                clonedQuoteReadModel.CustomerId.Should().Be(newQuoteAggregate.CustomerId);
                clonedQuoteReadModel.PolicyId.Should().NotBeNull();
                policyIssuedQuote.TransactionCompleted.Should().BeTrue();
                newQuoteAggregate.Id.Should().NotBe(policyIssuedQuote.Id);
            }
        }

        [Fact]
        public async Task CreateReplacementQuoteForExpiredQuote_DiscardCurrentAndAddNewQuoteToAggregate_IfQuoteIsCancellation()
        {
            // Arrange
            var environment = DeploymentEnvironment.Development;
            var quoteNumber = "FXOWQ";
            var quoteExpirySettings = new QuoteExpirySettings(30, true);

            using (var stack = this.stackFactory())
            {
                // needed to have higher created time.
                stack.Clock.Increment(Duration.FromSeconds(2));
                this.product = stack.ProductService.UpdateQuoteExpirySettings(
                    this.tenantAdmin.Id,
                    this.product.Id,
                    quoteExpirySettings,
                    ProductQuoteExpirySettingUpdateType.UpdateNone,
                    CancellationToken.None);
            }

            QuoteAggregate oldQuoteAggregate = null;
            Quote firstCancelQuote = null;
            Quote policyIssuedQuote = null;
            Quote quote = null;
            using (var stack = this.stackFactory())
            {
                // create quote
                quote = QuoteAggregate.CreateNewBusinessQuote(
                    this.tenantAdmin.Id,
                    this.tenantAdmin.Details.DefaultOrganisationId,
                    this.product.Id,
                    environment,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    stack.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);

                oldQuoteAggregate = quote.Aggregate;

                // assign expiry
                oldQuoteAggregate.SetQuoteExpiryFromSettings(
                    quote.Id, this.performingUserId, stack.Clock.Now(), QuoteExpirySettings.Default);

                // create customer.
                var defaultOrganisation = this.tenantAdmin.Details.DefaultOrganisationId;
                var customerPerson = PersonAggregate.CreatePerson(
                    this.tenantAdmin.Id, defaultOrganisation, this.performingUserId, stack.Clock.Now());
                var personCommonProperties = new PersonCommonProperties
                {
                    Email = "jeo1@gmail.com",
                    FullName = "Test",
                };

                customerPerson.Update(
                    new PersonalDetails(this.tenantAdmin.Id, personCommonProperties), this.performingUserId, stack.Clock.Now());
                await stack.PersonAggregateRepository.Save(customerPerson);

                // associate
                CustomerAggregate customerAggregate = await this.CreateCustomer(
                    stack, customerPerson, environment);
                oldQuoteAggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());
                oldQuoteAggregate.UpdateCustomerDetails(
                    new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now(), quote.Id);
                quote.AssignQuoteNumber(quoteNumber, this.performingUserId, stack.Clock.Now());
                oldQuoteAggregate.WithCalculationResult(quote.Id);

                // create policy and renewal quote
                oldQuoteAggregate.WithPolicy(quote.Id, "POLICY-2");
                policyIssuedQuote = oldQuoteAggregate.GetQuoteOrThrow(quote.Id);

                var cancelQuote = oldQuoteAggregate.WithCancellationQuote();
                firstCancelQuote = oldQuoteAggregate.GetQuoteOrThrow(cancelQuote.Id);

                stack.MockMediator.GetTenantByIdOrAliasQuery(this.tenantAdmin);

                // expire the quote
                oldQuoteAggregate.SetQuoteExpiryTime(
                    firstCancelQuote.Id, stack.Clock.Now(), this.performingUserId, stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(oldQuoteAggregate);
            }

            // Act
            using (var stack = this.stackFactory())
            {
                var clonedQuoteReadModel = await stack.Mediator.Send(new CloneQuoteFromExpiredQuoteCommand(
                    this.tenantAdmin.Id, firstCancelQuote.Id, DeploymentEnvironment.Development));
                var newQuoteAggregate = stack.QuoteAggregateRepository.GetById(this.tenantAdmin.Id, oldQuoteAggregate.Id);
                var newQuote = newQuoteAggregate.GetQuoteOrThrow(clonedQuoteReadModel.Id);

                // Assert
                clonedQuoteReadModel.Id.Should().NotBe(policyIssuedQuote.Id);
                clonedQuoteReadModel.Id.Should().NotBe(firstCancelQuote.Id);
                clonedQuoteReadModel.QuoteNumber.Should().NotBe(firstCancelQuote.QuoteNumber);
                clonedQuoteReadModel.PolicyId.Should().Be(newQuoteAggregate.Id);
                clonedQuoteReadModel.Id.Should().Be(newQuote.Id);
                firstCancelQuote.QuoteNumber.Should().NotBe(newQuote.QuoteNumber);
                policyIssuedQuote.IsDiscarded.Should().BeFalse();
                firstCancelQuote.Type.Should().Be(newQuote.Type);
                clonedQuoteReadModel.CustomerId.Should().Be(newQuoteAggregate.CustomerId);
                clonedQuoteReadModel.PolicyId.Should().NotBeNull();
                newQuoteAggregate.Id.Should().NotBe(firstCancelQuote.Id);
            }
        }

        private async Task<CustomerAggregate> CreateCustomer(
            ApplicationStack stack,
            PersonAggregate person,
            DeploymentEnvironment deployment)
        {
            UserSignupModel userSignupModel = this.CreateSignupModel();
            var user = await stack.UserService.CreateUser(userSignupModel);
            var customerAggregate = await stack.CreateCustomerForExistingPerson(
                person, deployment, user.Id, null);
            return customerAggregate;
        }

        private UserSignupModel CreateSignupModel()
        {
            var testEmail = $"test-quoter-deleter{Guid.NewGuid()}@ubind.io";
            var testNumber = "123";

            return new UserSignupModel()
            {
                AlternativeEmail = testEmail,
                WorkPhoneNumber = testNumber,
                Email = testEmail,
                Environment = DeploymentEnvironment.Development,
                FullName = "test",
                HomePhoneNumber = testNumber,
                MobilePhoneNumber = testNumber,
                PreferredName = "test",
                UserType = UserType.Client,
                TenantId = this.tenantAdmin.Id,
                OrganisationId = this.tenantAdmin.Details.DefaultOrganisationId,
            };
        }
    }
}
