// <copyright file="ApplicationPolicyServiceIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests
{
    using FluentAssertions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Commands.QuoteCalculation;
    using UBind.Application.Tests;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Authentication;
    using UBind.Domain.Commands.Policy;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class ApplicationPolicyServiceIntegrationTests
    {
        private readonly IClock clock = SystemClock.Instance;
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly IQuoteWorkflow quoteWorkflow = new DefaultQuoteWorkflow();

        private Guid tenantId;
        private Guid productId;
        private Guid organisationId;

        public ApplicationPolicyServiceIntegrationTests()
        {
            this.productId = Guid.NewGuid();
            this.tenantId = Guid.NewGuid();
            this.organisationId = Guid.NewGuid();
        }

        [Fact]
        public async Task IssuePolicy_UpdatesParentPolicy_ForAdjustmentQuote()
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            Guid originalQuoteAggregateId = default;

            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                var product = stack1.ProductRepository.GetProductById(this.tenantId, this.productId);
                var userAuthData = new UserAuthenticationData(
                    tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, userId, customerId);

                stack1.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                stack1.MockMediator.GetProductByIdOrAliasQuery(product);
                await stack1.ProductOrganisationSettingRepository.UpdateProductSetting(tenant.Id, this.organisationId, product.Id, true);
                var releaseContext = new ReleaseContext(tenant.Id, product.Id, DeploymentEnvironment.Staging, Guid.NewGuid());
                stack1.CreateRelease(releaseContext);
                var quoteReadModel = await stack1.Mediator.Send(new CreateNewBusinessQuoteCommand(
                    this.tenantId,
                    this.organisationId,
                    null,
                    this.productId,
                    DeploymentEnvironment.Staging,
                    false,
                    null,
                    null,
                    null,
                    productRelease: null));
                var calculationDataModel = JsonConvert.DeserializeObject<SpreadsheetCalculationDataModel>(
                    FormDataJsonFactory.GetSampleWithStartAndEndDates());
                stack1.AutoServePolicyNumbersForTenant(this.tenantId);
                var quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteReadModel.TenantId, quoteReadModel.AggregateId);
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
                var originalQuoteAggregate = stack1.QuoteAggregateRepository.GetById(quote.Aggregate.TenantId, quote.Aggregate.Id);
                var twelveMonthsAgo = SystemClock.Instance.Today().PlusMonths(-12);
                originalQuoteAggregate.WithCalculationResult(quote.Id, calculationResultJson: CalculationResultJsonFactory.Create(startDate: twelveMonthsAgo));
                await stack1.QuoteAggregateRepository.Save(originalQuoteAggregate);
                originalQuoteAggregate = stack1.QuoteAggregateRepository.GetById(quote.Aggregate.TenantId, quote.Aggregate.Id);
                originalQuoteAggregateId = originalQuoteAggregate.Id;
                var quoteOrginal = originalQuoteAggregate.GetQuoteOrThrow(quote.Id);
                var formData = new Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
                formData = new Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
                await stack1.ApplicationQuoteService.Actualise(
                    releaseContext,
                    quoteOrginal,
                    formData);
                await stack1.Mediator.Send(new CompletePolicyTransactionCommand(
                    this.tenantId,
                    quoteOrginal.Id,
                    quoteOrginal.LatestCalculationResult.Id,
                    new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates())));
            }

            Guid adjustmentCalculationId = default;
            Guid adjustmentQuoteId = default;
            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quoteAggregate = stack2.QuoteAggregateRepository.GetById(this.tenantId, originalQuoteAggregateId);
                var releaseContext = new ReleaseContext(quoteAggregate.TenantId, quoteAggregate.ProductId, DeploymentEnvironment.Staging, Guid.NewGuid());
                stack2.CreateRelease(releaseContext);
                var adjustmentQuoteReadModel = await stack2.Mediator.Send(new CreateAdjustmentQuoteCommand(
                    this.tenantId, quoteAggregate.Policy.PolicyId, false));
                quoteAggregate = stack2.QuoteAggregateRepository.GetById(this.tenantId, adjustmentQuoteReadModel.AggregateId);
                var adjustmentQuote = (AdjustmentQuote)quoteAggregate.GetQuoteOrThrow(adjustmentQuoteReadModel.Id);
                var quoteCalculationCommand = new QuoteCalculationCommand(
                    adjustmentQuote.ProductContext,
                    adjustmentQuote.Id,
                    null,
                    adjustmentQuote.Type,
                    null,
                    JsonConvert.DeserializeObject<SpreadsheetCalculationDataModel>(FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDates()),
                    null,
                    true,
                    true,
                    adjustmentQuote.Aggregate.OrganisationId);
                await stack2.Mediator.Send(quoteCalculationCommand);
                var adjustmentQuoteAggregate = stack2.QuoteAggregateRepository.GetById(this.tenantId, quoteAggregate.Id);
                var quoteAdjustmnet = adjustmentQuoteAggregate.GetQuoteOrThrow(adjustmentQuote.Id);
                adjustmentCalculationId = quoteAdjustmnet.LatestCalculationResult.Id;
                adjustmentQuoteId = quoteAdjustmnet.Id;
            }

            // Act
            using (var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quoteAggregate = stack3.QuoteAggregateRepository.GetById(this.tenantId, originalQuoteAggregateId);
                var tenant = stack3.TenantRepository.GetTenantById(this.tenantId);
                var userAuthData = new UserAuthenticationData(
                    tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, userId, customerId);
                var releaseContext = new ReleaseContext(quoteAggregate.TenantId, quoteAggregate.ProductId, DeploymentEnvironment.Staging, Guid.NewGuid());
                stack3.CreateRelease(releaseContext);
                stack3.AutoServePolicyNumbersForTenant(this.tenantId);
                var adjustmentPolicy = await stack3.Mediator.Send(new CompletePolicyTransactionCommand(
                    this.tenantId,
                    adjustmentQuoteId,
                    adjustmentCalculationId,
                    new FormData(
                        FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDates())));
            }

            // Assert
            using (var stack4 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var persistedAdjustmentQuote = stack4.QuoteAggregateRepository.GetById(this.tenantId, originalQuoteAggregateId);
                var quote = persistedAdjustmentQuote.GetQuoteOrThrow(adjustmentQuoteId);
                quote.TransactionCompleted.Should().BeTrue();
            }
        }

        [Fact]
        public async Task IssuePolicy_PersistsNewPolicyReadModel_ForNewBusinessQuotes()
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            QuoteAggregate aggregate = null;
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Act (IssuePolicy() call inside CreateNewPolicy.)
                QuoteFactory.Clock = stack.Clock;

                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                aggregate = QuoteFactory.CreateNewPolicy(this.tenantId, this.productId, organisationId: organisation.Id);
                await stack.QuoteAggregateRepository.Save(aggregate);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var policyReadModel = stack.PolicyReadModelRepository.GetPolicyDetails(this.tenantId, aggregate.Id);
                policyReadModel.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task AdjustPolicy_UpdatesPolicyReadModel_ForAdjustmentQuotes()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();
                var aggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    organisationId: organisation.Id);

                // Act
                var adjustmentQuote = aggregate.Policy.CreateAdjustmentQuote(
                    this.clock.Now(),
                    "FOO",
                    Enumerable.Empty<IClaimReadModel>(),
                    this.performingUserId,
                    this.quoteWorkflow,
                    QuoteExpirySettings.Default,
                    TenantHelper.IsMutual(tenant.Details.Alias),
                    Guid.NewGuid());

                aggregate
                   .WithCalculationResult(
                        adjustmentQuote.Id, FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDates())
                   .WithPolicyAdjustmentTransaction(adjustmentQuote.Id);
                await stack.QuoteAggregateRepository.Save(aggregate);

                // Assert
                var policyReadModel = stack.PolicyReadModelRepository.GetPolicyDetails(this.tenantId, aggregate.Id);
                policyReadModel.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task AdjustPolicy_ChangePolicyEndDate_ForAdjustmentQuotes()
        {
            var timeOfDayScheme = new DefaultPolicyTransactionTimeOfDayScheme();

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();
                var aggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    organisationId: organisation.Id);
                var parentPolicyEndDate = aggregate.Policy.ExpiryDateTime;

                // Act
                var adjustmentQuote = aggregate.Policy.CreateAdjustmentQuote(
                    this.clock.Now(),
                    "FOO",
                    Enumerable.Empty<IClaimReadModel>(),
                    this.performingUserId,
                    this.quoteWorkflow,
                    QuoteExpirySettings.Default,
                    false,
                    Guid.NewGuid());

                var startDate = this.clock.Today();
                var endDate = this.clock.Today().PlusYears(2);
                var durationInDays = Period.Between(startDate, endDate, PeriodUnits.Days).Days;
                aggregate = aggregate
                   .WithCalculationResult(
                        adjustmentQuote.Id,
                        FormDataJsonFactory.GetSampleWithStartAndEndDates(startDate, endDate),
                        CalculationResultJsonFactory.Create(true, startDate, durationInDays))
                   .WithPolicyAdjustmentTransaction(adjustmentQuote.Id);
                await stack.QuoteAggregateRepository.Save(aggregate);

                // Assert
                var policyReadModel = stack.PolicyReadModelRepository.GetPolicyDetails(aggregate.TenantId, aggregate.Id);
                policyReadModel.ExpiryDateTime.Should().Be(this.clock.Today().At(timeOfDayScheme.GetEndTime()).PlusYears(2));
                policyReadModel.ExpiryDateTime.Should().NotBeSameAs(parentPolicyEndDate);
            }
        }

        [Fact]
        public async Task AdjustPolicy_ChangePolicyEndDateIsBeforeInceptionDate_ForAdjustmentQuotes()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();
                var aggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    organisationId: organisation.Id);
                var parentPolicyEndDate = aggregate.Policy.ExpiryDateTime.Value.Date;
                var sampleFormData = FormDataJsonFactory.GetSampleWithStartAndEndDates(
                    this.clock.Today(), this.clock.Today().Minus(Period.FromMonths(5)));

                // Act
                var adjustmentQuote = aggregate.Policy.CreateAdjustmentQuote(
                    this.clock.Now(),
                    "FOO",
                    Enumerable.Empty<IClaimReadModel>(),
                    this.performingUserId,
                    this.quoteWorkflow,
                    QuoteExpirySettings.Default,
                    false,
                    Guid.NewGuid());

                // Act
                Action act = () => aggregate
                    .WithCalculationResult(adjustmentQuote.Id, sampleFormData)
                    .WithPolicyAdjustmentTransaction(adjustmentQuote.Id);

                // Assert
                act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("quote.calculation.policy.period.start.date.must.not.be.after.end.date");
            }
        }

        [Fact]
        public async Task IssuePolicy_PersistsNewBusinessTransaction_ForNewBusinessQuotes()
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            QuoteAggregate aggregate = null;
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Act (IssuePolicy() call inside CreateNewPolicy.)
                QuoteFactory.Clock = stack.Clock;

                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                aggregate = QuoteFactory.CreateNewPolicy(this.tenantId, this.productId, organisationId: organisation.Id);
                await stack.QuoteAggregateRepository.Save(aggregate);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var policyReadModel = stack.PolicyReadModelRepository.GetPolicyDetails(this.tenantId, aggregate.Id);
                policyReadModel.Transactions.Select(x => x.PolicyTransaction).OfType<NewBusinessTransaction>().Any().Should().BeTrue();
            }
        }

        /*
         * TODO: check this method
        [Fact]
        public async Task Cancel_PersistsCancellationTransaction()
        {
            // Arrange
            var tenantId = "apsi-tenant5";
            var productId = "prod5";
            this.CreateTenantAndProduct(tenantId, productId);
            var userAuthData = new UserAuthenticationData(Guid.NewGuid(), tenantId, UserType.Agent, Guid.NewGuid());
            QuoteAggregate quoteAggregate = null;
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.AutoServePolicyNumbersForTenant(tenantId);
                var quoteAggregateResult = await stack.ApplicationCalculationService.CreateCalculationAsync(
                    tenantId,
                    productId,
                    DeploymentEnvironment.Staging,
                    default,
                    FormDataJson.GetSampleWithStartAndEndDates(),
                    new FakePersonalDetails());
                quoteAggregateResult.Should().Succeed();

                quoteAggregate = quoteAggregateResult.Value;
                await stack.PolicyService.IssuePolicy(
                    quoteAggregate.Id, quoteAggregate.CurrentQuote.LatestCalculationResult.Id, FormDataJson.GetSampleWithStartAndEndDates(), userAuthData);
            }

            // Act
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                await stack.PolicyService.CreateCancellationQuote(quoteAggregate.Id, tenantId);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var policyReadModel = stack.PolicyReadModelRepository.GetPolicyDetails(quoteAggregate.Id);
                Assert.True(policyReadModel.Transactions.OfType<CancellationTransaction>().Any());
            }
        }
        */
        private void CreateTenantAndProduct(Guid tenantId, Guid productId)
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = TenantFactory.Create(tenantId);
                var product = ProductFactory.Create(tenantId, productId);
                stack.CreateTenant(tenant);
                stack.CreateProduct(product);
            }
        }
    }
}
