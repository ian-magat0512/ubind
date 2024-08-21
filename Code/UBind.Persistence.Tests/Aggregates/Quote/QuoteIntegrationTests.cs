// <copyright file="QuoteIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Aggregates.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using NodaTime.Serialization.JsonNet;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Tests;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Commands;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Entities;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Configuration;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;
    using FormData = UBind.Domain.Aggregates.Quote.FormData;

    /// <summary>
    /// Tests for quote integration.
    /// </summary>
    [Collection(DatabaseCollection.Name)]
    public class QuoteIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly IQuoteWorkflow quoteWorkflow = new DefaultQuoteWorkflow();
        private readonly Mock<IProductConfigurationProvider> productConfigurationProviderMock
           = new Mock<IProductConfigurationProvider>();

        private Guid tenantId;
        private Guid productId;
        private Guid organisationId;

        public QuoteIntegrationTests()
        {
            this.productId = Guid.NewGuid();
            this.tenantId = Guid.NewGuid();
        }

        [Fact]
        public async Task Quote_IsUpdatedCorrectly()
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            var clock = NodaTime.SystemClock.Instance;
            QuoteAggregate quoteAggregate = null;
            Quote quote = null;
            var policyNumber = "ABCDEF";

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
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

                this.organisationId = organisation.Id;
            }

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var env = DeploymentEnvironment.Staging;

                quote = QuoteAggregate.CreateNewBusinessQuote(
                    this.tenantId,
                    this.organisationId,
                    this.productId,
                    env,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);
                quoteAggregate = quote.Aggregate;

                var formData = new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
                var formDataId = quote.UpdateFormData(formData, this.performingUserId, clock.Now());
                var customerDetails = new Mock<IPersonalDetails>().SetupAllProperties();
                quoteAggregate.UpdateCustomerDetails(customerDetails.Object, this.performingUserId, clock.Now(), quote.Id);

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
                    clock.Now(),
                    formDataSchema,
                    false,
                    this.performingUserId);
                var timeOfDayScheme = new DefaultPolicyTransactionTimeOfDayScheme();
                var inceptionDate = new NodaTime.LocalDate(2018, 10, 9);
                var expiryDate = new NodaTime.LocalDate(2019, 10, 9);
                var newBusinessQuote = quote as NewBusinessQuote;
                newBusinessQuote.IssuePolicy(
                    quote.LatestCalculationResult.Id,
                    () => policyNumber,
                    QuoteFactory.ProductConfiguation,
                    timeOfDayScheme,
                    this.performingUserId,
                    clock.Now(),
                    this.quoteWorkflow);

                // Act
                await stack.QuoteAggregateRepository.Save(quoteAggregate);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var policyReadModel = stack.PolicyReadModelRepository.GetPolicyDetails(this.tenantId, quoteAggregate.Id);
                Assert.Equal(policyNumber, policyReadModel.PolicyNumber);
            }
        }

        [Fact]
        public async Task QuoteDocumentsArePersistedInReadModel()
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            var clock = NodaTime.SystemClock.Instance;
            var documentXName = Guid.NewGuid().ToString();
            var documentYName = Guid.NewGuid().ToString();
            Quote quote;
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
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

                quote = QuoteFactory.CreateNewBusinessQuote(
                   this.tenantId,
                   this.productId,
                   organisationId: organisation.Id);
                var documentX1 = new Domain.Aggregates.Quote.QuoteDocument(documentXName, "Pdf", 3, Guid.NewGuid(), clock.Now());
                var documentY = new Domain.Aggregates.Quote.QuoteDocument(documentYName, "Pdf", 4, Guid.NewGuid(), clock.Now());
                var documentX2 = new Domain.Aggregates.Quote.QuoteDocument(documentXName, "Pdf", 2000000000, Guid.NewGuid(), clock.Now());

                // Act
                quote.Aggregate.AttachQuoteDocument(documentX1, 0, this.performingUserId, clock.Now());
                quote.Aggregate.AttachQuoteDocument(documentY, 0, this.performingUserId, clock.Now());
                quote.Aggregate.AttachQuoteDocument(documentX2, 0, this.performingUserId, clock.Now());
                await stack.QuoteAggregateRepository.Save(quote.Aggregate);
            }

            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Assert
                var quoteDetails = stack2.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, quote.Id);

                Assert.Equal(2, quoteDetails.Documents.Count());
                Assert.Contains(quoteDetails.Documents, d => d.Name == documentXName);
                Assert.Contains(quoteDetails.Documents, d => d.Name == documentYName);
            }
        }

        [Fact]
        public async Task QuoteDocumentsArePersistedInReadModel_IncludedTransactionsAndVersions()
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            var today = SystemClock.Instance.Now().InZone(Timezones.AET).Date;
            var tomorrow = today.PlusDays(1);
            var clock = NodaTime.SystemClock.Instance;

            Quote quote;
            Guid newBusinessQuoteId;
            Guid quoteVersionId;

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
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

                // Act
                var quoteAggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    organisationId: organisation.Id);
                quote = quoteAggregate.GetQuoteOrThrow(quoteAggregate.Policy.QuoteId.GetValueOrDefault());
                quoteAggregate.WithFormData(quote.Id);
                newBusinessQuoteId = quote.Id;
                var adjustmentQuote = quoteAggregate.WithAdjustmentQuote(clock.GetCurrentInstant(), "ADJUST-001");
                quoteAggregate = adjustmentQuote.Aggregate;
                quote = quoteAggregate.GetQuoteOrThrow(adjustmentQuote.Id);
                var formDataSchema = new FormDataSchema(new JObject());
                var documentVersionX1 = new QuoteDocument("VersionDoc1", "Pdf", 3, Guid.NewGuid(), clock.Now());
                var documentVersionY = new QuoteDocument("VersionDoc2", "Pdf", 4, Guid.NewGuid(), clock.Now());

                var quotedocumentNewBusiness1 = new QuoteDocument("NewBusiness_QuoteDoc1", "Pdf", 3, Guid.NewGuid(), clock.Now());
                var quotedocumentNewBusiness2 = new QuoteDocument("NewBusiness_QuoteDoc2", "Pdf", 4, Guid.NewGuid(), clock.Now());

                var quotedocumentAdjust1 = new QuoteDocument("Adjust_QuoteDoc1", "Pdf", 3, Guid.NewGuid(), clock.Now());
                var quotedocumentAdjust2 = new QuoteDocument("Adjust_QuoteDoc2", "Pdf", 4, Guid.NewGuid(), clock.Now());

                var documentPolicyX = new QuoteDocument("New_PolicyDoc1", "Pdf", 3, Guid.NewGuid(), clock.Now());
                var documentPolicyY = new QuoteDocument("New_PolicyDoc2", "Pdf", 4, Guid.NewGuid(), clock.Now());

                quoteAggregate.AttachQuoteDocument(quotedocumentNewBusiness1, 0, this.performingUserId, clock.Now());
                quoteAggregate.AttachQuoteDocument(quotedocumentNewBusiness2, 0, this.performingUserId, clock.Now());

                var formDataId = quote.UpdateFormData(
                    new FormData(FormDataJsonFactory.GetCustomSample("sample")), this.performingUserId, clock.GetCurrentInstant());
                var formData = new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
                var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Sample);
                var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(
                        new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates()),
                        new CachingJObjectWrapper(CalculationResultJsonFactory.Sample));
                var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
                calculationResult.FormDataId = formDataId;
                quote.RecordCalculationResult(
                    calculationResult,
                    calculationData,
                    clock.Now(),
                    formDataSchema,
                    false,
                    this.performingUserId);

                quoteAggregate.CreateVersion(this.performingUserId, clock.GetCurrentInstant(), quote.Id);
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                var sequenceNumber = quote.Versions.Last().EventSequenceNumber;
                quoteVersionId = quote.Versions.Last().VersionId;
                quoteAggregate.AttachQuoteVersionDocument(documentVersionX1, sequenceNumber, this.performingUserId, clock.Now());
                quoteAggregate.AttachQuoteVersionDocument(documentVersionY, sequenceNumber, this.performingUserId, clock.Now());
                quoteAggregate.AttachQuoteDocument(quotedocumentAdjust1, sequenceNumber, this.performingUserId, clock.Now());
                quoteAggregate.AttachQuoteDocument(quotedocumentAdjust2, sequenceNumber, this.performingUserId, clock.Now());

                var policyTransactionId = quoteAggregate.Policy.Transactions.LastOrDefault().Id;
                quoteAggregate.AttachPolicyDocument(documentPolicyX, policyTransactionId, this.performingUserId, clock.Now());
                quoteAggregate.AttachPolicyDocument(documentPolicyY, policyTransactionId, this.performingUserId, clock.Now());

                await stack.QuoteAggregateRepository.Save(quoteAggregate);
            }

            // Assert
            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var newQuote = stack2.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, newBusinessQuoteId);

                Assert.Equal(4, newQuote.Documents.Count());
                Assert.Contains(newQuote.Documents, d => d.Name == "NewBusiness_QuoteDoc1");
                Assert.Contains(newQuote.Documents, d => d.Name == "NewBusiness_QuoteDoc2");
                Assert.Contains(newQuote.Documents, d => d.Name == "New_PolicyDoc1");
                Assert.Contains(newQuote.Documents, d => d.Name == "New_PolicyDoc2");

                var adjustQuote = stack2.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, quote.Id);
                var version = stack2.QuoteVersionReadModelRepository.GetVersionDetailsById(this.tenantId, quote.VersionId);

                Assert.Equal(2, adjustQuote.Documents.Count());
                Assert.Equal(2, adjustQuote.Documents.Count());
                Assert.Contains(adjustQuote.Documents, d => d.Name == "Adjust_QuoteDoc1");
                Assert.Contains(adjustQuote.Documents, d => d.Name == "Adjust_QuoteDoc2");
                Assert.Contains(version.Documents, d => d.Name == "VersionDoc1");
                Assert.Contains(version.Documents, d => d.Name == "VersionDoc2");
            }
        }

        [Fact]
        public async Task IssuePolicy_ResultsInNewBusinessTransactionReadModel_WhenSuccessful()
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            QuoteAggregate aggregate = null;
            Instant now = SystemClock.Instance.Now();
            var nowUtc = now.InZone(DateTimeZone.Utc);
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Act (IssuePolicy() call inside CreateNewPolicy.)
                QuoteFactory.Clock = stack.Clock;

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

                var org = stack.OrganisationReadModelRepository.Get(tenant.Id, organisation.Id);
                aggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    organisationId: organisation.Id);

                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);

                await stack.QuoteAggregateRepository.Save(aggregate);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quoteDetails = stack.PolicyReadModelRepository.GetPolicyDetails(this.tenantId, aggregate.Id);
                Assert.True(quoteDetails.Transactions.Select(x => x.PolicyTransaction).OfType<NewBusinessTransaction>().Any());
                var transactionCreatedTimestamp = quoteDetails.Transactions.Select(x => x.PolicyTransaction).OfType<NewBusinessTransaction>().Single().CreatedTimestamp;
                var transactionCreatedTimestampUtc = transactionCreatedTimestamp.InZone(DateTimeZone.Utc);

                // previously having errors discrepancy of 1millisecond.
                Assert.Equal(nowUtc.Minute, transactionCreatedTimestampUtc.Minute);
                Assert.Equal(nowUtc.Hour, transactionCreatedTimestampUtc.Hour);
                Assert.Equal(nowUtc.Day, transactionCreatedTimestampUtc.Day);
                Assert.Equal(nowUtc.Year, transactionCreatedTimestampUtc.Year);
                Assert.Equal(nowUtc.Month, transactionCreatedTimestampUtc.Month);
            }
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        public async Task PatchFormData_CopyFieldPatchAllFormAndCalculationResultData_ForGlobalCommand(
            bool targetFormDataPathSet, bool targetCalculationResultPathSet, bool sourcePath)
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            Guid policyId;
            Guid newBusinessQuoteId;

            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                stack1.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack1.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
                var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
                var originalQuote = QuoteFactory.CreateNewBusinessQuote(
                    this.tenantId,
                    this.productId,
                    organisationId: organisation.Id);
                var originalAggregate = originalQuote.Aggregate
                    .WithCustomerDetails(originalQuote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(originalQuote.Id)
                    .WithCalculationResult(originalQuote.Id, formData, calculationResultJson)
                    .WithQuoteVersion(originalQuote.Id)
                    .WithSubmission(originalQuote.Id)
                    .WithPolicy(originalQuote.Id);
                policyId = originalAggregate.Id;
                newBusinessQuoteId = originalQuote.Id;
                await stack1.QuoteAggregateRepository.Save(originalAggregate);
            }

            var validFormDataPath = targetFormDataPathSet
                ? new JsonPath("newProperty")
                : null;
            var validCalculationResultPath = targetCalculationResultPathSet
                ? new JsonPath("questions.ratingPrimary.newObjectProperty.newProperty")
                : null;
            var validSourcePath = sourcePath
                ? new JsonPath("payment.total.premium")
                : null;

            var patchCommand = new CopyFieldPolicyDataPatchCommand(
                validFormDataPath,
                validCalculationResultPath,
                PatchSourceEntity.FirstPolicyTransactionCalculationResult,
                validSourcePath,
                PolicyDataPatchScope.CreateGlobalPatchScope(),
                PatchRules.None);

            // Act
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var aggregate = stack.QuoteAggregateRepository.GetById(this.tenantId, policyId);
                aggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());
                await stack.QuoteAggregateRepository.Save(aggregate);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quoteReadModel = stack.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, newBusinessQuoteId);

                var newProperty = "newProperty";
                quoteReadModel.LatestCalculationResult.Questions.Contains(newProperty).Should().Be(
                    targetCalculationResultPathSet,
                    $"QuoteReadModel latest calculation result patching expectation violation.");
                quoteReadModel.LatestFormData.Contains(newProperty).Should().Be(
                    targetFormDataPathSet,
                    "QuoteReadModel latest form model patching expectation violation.");

                var quoteVersionReadModelDetails = stack.QuoteVersionReadModelRepository.GetVersionDetailsByVersionNumber(this.tenantId, newBusinessQuoteId, 1);
                quoteVersionReadModelDetails.LatestFormData.Contains(newProperty).Should().Be(
                    targetFormDataPathSet,
                    "QuoteVersionReadModel form data patching expectation violation.");
                quoteVersionReadModelDetails.CalculationResultJson.Contains(newProperty).Should().Be(
                    targetCalculationResultPathSet,
                    "QuoteVersionReadModel calculation result patching expectation violation.");

                var policyReadModel = stack.PolicyReadModelRepository.GetPolicyDetails(this.tenantId, policyId);
                policyReadModel.CalculationResult.Questions.Contains(newProperty).Should().Be(
                    targetCalculationResultPathSet,
                    "PolicyReadModel latest calculation result patching expectation violation.");

                foreach (var transaction in policyReadModel.Transactions.OfType<Domain.ReadModel.Policy.PolicyTransaction>())
                {
                    transaction.PolicyData.FormData.Contains(newProperty).Should().Be(
                        targetFormDataPathSet,
                        "Transaction form data patching expectation violation");
                    transaction.PolicyData.CalculationResult.Questions.Contains(newProperty).Should().Be(
                        targetCalculationResultPathSet,
                        "Transaction calculation result patching expectation violation.");
                }
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task PatchFormData_PatchesAllFormAndCalculationResultData_ForGlobalCommand(
            bool targetFormDataPathSet, bool targetCalculationResultPathSet)
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            Guid policyId;
            Guid newBusinessQuoteId;
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                stack1.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack1.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
                var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
                var originalQuote = QuoteFactory.CreateNewBusinessQuote(
                    this.tenantId,
                    this.productId,
                    organisationId: organisation.Id);
                var originalAggregate = originalQuote.Aggregate
                    .WithCustomerDetails(originalQuote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(originalQuote.Id)
                    .WithCalculationResult(originalQuote.Id, formData, calculationResultJson)
                    .WithSubmission(originalQuote.Id)
                    .WithQuoteVersion(originalQuote.Id)
                    .WithPolicy(originalQuote.Id);
                policyId = originalAggregate.Id;
                newBusinessQuoteId = originalQuote.Id;
                await stack1.QuoteAggregateRepository.Save(originalAggregate);
            }

            var validFormDataPath = targetFormDataPathSet
                ? new JsonPath("objectProperty.nestedProperty")
                : null;
            var validCalculationResultPath = targetCalculationResultPathSet
                ? new JsonPath("questions.ratingPrimary.objectProperty.nestedProperty")
                : null;
            var newValue = Guid.NewGuid().ToString();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                validFormDataPath,
                validCalculationResultPath,
                newValue,
                PolicyDataPatchScope.CreateGlobalPatchScope(),
                PatchRules.None);

            // Act
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var aggregate = stack.QuoteAggregateRepository.GetById(this.tenantId, policyId);
                aggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());
                await stack.QuoteAggregateRepository.Save(aggregate);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quoteReadModel = stack.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, newBusinessQuoteId);
                quoteReadModel.LatestCalculationResult.Questions.Contains(newValue).Should().Be(
                    targetCalculationResultPathSet,
                    $"QuoteReadModel latest calculation result patching expectation violation.");
                quoteReadModel.LatestFormData.Contains(newValue).Should().Be(
                    targetFormDataPathSet,
                    "QuoteReadModel latest form model patching expectation violation.");

                var quoteVersionReadModelDetails = stack.QuoteVersionReadModelRepository.GetVersionDetailsByVersionNumber(this.tenantId, newBusinessQuoteId, 1);
                quoteVersionReadModelDetails.LatestFormData.Contains(newValue).Should().Be(
                    targetFormDataPathSet,
                    "QuoteVersionReadModel form data patching expectation violation.");
                quoteVersionReadModelDetails.CalculationResultJson.Contains(newValue).Should().Be(
                    targetCalculationResultPathSet,
                    "QuoteVersionReadModel calculation result patching expectation violation.");

                var policyReadModel = stack.PolicyReadModelRepository.GetPolicyDetails(this.tenantId, policyId);
                policyReadModel.CalculationResult.Questions.Contains(newValue).Should().Be(
                    targetCalculationResultPathSet,
                    "PolicyReadModel latest calculation result patching expectation violation.");

                foreach (var transaction in policyReadModel.Transactions.OfType<Domain.ReadModel.Policy.PolicyTransaction>())
                {
                    transaction.PolicyData.FormData.Contains(newValue).Should().Be(
                        targetFormDataPathSet,
                        "Transaction form data patching expectation violation");
                    transaction.PolicyData.CalculationResult.Questions.Contains(newValue).Should().Be(
                        targetCalculationResultPathSet,
                        "Transaction calculation result patching expectation violation.");
                }
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task PatchFormData_PatchesOnlyQuoteData_WhenScopeIsQuoteFull(
            bool targetFormDataPathSet, bool targetCalculationResultPathSet)
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            Guid policyId;
            Guid newBusinessQuoteId;
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                stack1.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack1.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
                var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
                var originalQuote = QuoteFactory.CreateNewBusinessQuote(
                    this.tenantId,
                    this.productId,
                    organisationId: organisation.Id);
                var originalAggregate = originalQuote.Aggregate
                    .WithCustomerDetails(originalQuote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(originalQuote.Id)
                    .WithCalculationResult(originalQuote.Id, formData, calculationResultJson)
                    .WithSubmission(originalQuote.Id)
                    .WithQuoteVersion(originalQuote.Id)
                    .WithPolicy(originalQuote.Id);
                policyId = originalAggregate.Id;
                newBusinessQuoteId = originalQuote.Id;
                await stack1.QuoteAggregateRepository.Save(originalAggregate);
            }

            var validFormDataPath = targetFormDataPathSet
                ? new JsonPath("objectProperty.nestedProperty")
                : null;
            var validCalculationResultPath = targetCalculationResultPathSet
                ? new JsonPath("questions.ratingPrimary.objectProperty.nestedProperty")
                : null;
            var newValue = Guid.NewGuid().ToString();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                validFormDataPath,
                validCalculationResultPath,
                newValue,
                PolicyDataPatchScope.CreateFullQuotePatchScope(newBusinessQuoteId),
                PatchRules.None);

            // Act
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var aggregate = stack.QuoteAggregateRepository.GetById(this.tenantId, policyId);
                aggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());
                await stack.QuoteAggregateRepository.Save(aggregate);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quoteReadModel = stack.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, newBusinessQuoteId);
                quoteReadModel.LatestCalculationResult.Questions.Contains(newValue).Should().Be(
                    targetCalculationResultPathSet,
                    $"QuoteReadModel latest calculation result patching expectation violation.");
                quoteReadModel.LatestFormData.Contains(newValue).Should().Be(
                    targetFormDataPathSet,
                    "QuoteReadModel latest form model patching expectation violation.");

                var quoteVersionReadModelDetails = stack.QuoteVersionReadModelRepository.GetVersionDetailsByVersionNumber(this.tenantId, newBusinessQuoteId, 1);
                quoteVersionReadModelDetails.LatestFormData.Contains(newValue).Should().Be(
                    targetFormDataPathSet,
                    "QuoteVersionReadModel form data patching expectation violation.");
                quoteVersionReadModelDetails.CalculationResultJson.Contains(newValue).Should().Be(
                    targetCalculationResultPathSet,
                    "QuoteVersionReadModel calculation result patching expectation violation.");

                var policyReadModel = stack.PolicyReadModelRepository.GetPolicyDetails(this.tenantId, policyId);
                policyReadModel.CalculationResult.Questions.Should().NotContain(
                    newValue,
                    "PolicyReadModel latest calculation result patching expectation violation.");

                foreach (var transaction in policyReadModel.Transactions.OfType<Domain.ReadModel.Policy.PolicyTransaction>())
                {
                    Assert.False(
                        transaction.PolicyData.FormData.Contains(newValue),
                        "Transaction form data patching expectation violation");
                    Assert.False(
                        transaction.PolicyData.CalculationResult.Questions.Contains(newValue),
                        "Transaction calculation result patching expectation violation.");
                }
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task PatchFormData_PatchesOnlyLatestQuoteData_WhenScopeIsQuoteLatest(
            bool targetFormDataPathSet, bool targetCalculationResultPathSet)
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            Guid policyId;
            Guid newBusinessQuoteId;
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                stack1.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack1.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
                var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
                var originalQuote = QuoteFactory.CreateNewBusinessQuote(
                    this.tenantId,
                    this.productId,
                    organisationId: organisation.Id);
                var originalAggregate = originalQuote.Aggregate
                    .WithCustomerDetails(originalQuote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(originalQuote.Id)
                    .WithCalculationResult(originalQuote.Id, formData, calculationResultJson)
                    .WithSubmission(originalQuote.Id)
                    .WithQuoteVersion(originalQuote.Id)
                    .WithPolicy(originalQuote.Id);
                policyId = originalAggregate.Id;
                newBusinessQuoteId = originalQuote.Id;
                await stack1.QuoteAggregateRepository.Save(originalAggregate);
            }

            var validFormDataPath = targetFormDataPathSet
                ? new JsonPath("objectProperty.nestedProperty")
                : null;
            var validCalculationResultPath = targetCalculationResultPathSet
                ? new JsonPath("questions.ratingPrimary.objectProperty.nestedProperty")
                : null;
            var newValue = Guid.NewGuid().ToString();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                validFormDataPath,
                validCalculationResultPath,
                newValue,
                PolicyDataPatchScope.CreateLatestQuotePatchScope(newBusinessQuoteId),
                PatchRules.None);

            // Act
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var aggregate = stack.QuoteAggregateRepository.GetById(this.tenantId, policyId);
                aggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());
                await stack.QuoteAggregateRepository.Save(aggregate);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quoteReadModel = stack.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, newBusinessQuoteId);
                quoteReadModel.LatestCalculationResult.Questions.Contains(newValue).Should().Be(
                    targetCalculationResultPathSet,
                    $"QuoteReadModel latest calculation result patching expectation violation.");
                quoteReadModel.LatestFormData.Contains(newValue).Should().Be(
                    targetFormDataPathSet,
                    "QuoteReadModel latest form model patching expectation violation.");

                var quoteVersionReadModelDetails = stack.QuoteVersionReadModelRepository.GetVersionDetailsByVersionNumber(this.tenantId, newBusinessQuoteId, 1);
                quoteVersionReadModelDetails.LatestFormData.Should().NotContain(
                    newValue,
                    "QuoteVersionReadModel form data patching expectation violation.");
                quoteVersionReadModelDetails.CalculationResultJson.Should().NotContain(
                    newValue,
                    "QuoteVersionReadModel calculation result patching expectation violation.");

                var policyReadModel = stack.PolicyReadModelRepository.GetPolicyDetails(this.tenantId, policyId);
                policyReadModel.CalculationResult.Questions.Should().NotContain(
                    newValue,
                    "PolicyReadModel latest calculation result patching expectation violation.");

                foreach (var transaction in policyReadModel.Transactions.OfType<Domain.ReadModel.Policy.PolicyTransaction>())
                {
                    Assert.False(
                        transaction.PolicyData.FormData.Contains(newValue),
                        "Transaction form data patching expectation violation");
                    Assert.False(
                        transaction.PolicyData.CalculationResult.Questions.Contains(newValue),
                        "Transaction calculation result patching expectation violation.");
                }
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task PatchFormData_PatchesOnlyQuoteVersionData_WhenScopeIsQuoteVersion(
            bool targetFormDataPathSet, bool targetCalculationResultPathSet)
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            Guid policyId;
            Guid newBusinessQuoteId;
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                stack1.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack1.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
                var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
                var originalQuote = QuoteFactory.CreateNewBusinessQuote(
                    this.tenantId,
                    this.productId,
                    organisationId: organisation.Id);
                var originalAggregate = originalQuote.Aggregate
                    .WithCustomerDetails(originalQuote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(originalQuote.Id)
                    .WithCalculationResult(originalQuote.Id, formData, calculationResultJson)
                    .WithSubmission(originalQuote.Id)
                    .WithQuoteVersion(originalQuote.Id)
                    .WithPolicy(originalQuote.Id);
                policyId = originalAggregate.Id;
                newBusinessQuoteId = originalQuote.Id;
                await stack1.QuoteAggregateRepository.Save(originalAggregate);
            }

            var validFormDataPath = targetFormDataPathSet
                ? new JsonPath("objectProperty.nestedProperty")
                : null;
            var validCalculationResultPath = targetCalculationResultPathSet
                ? new JsonPath("questions.ratingPrimary.objectProperty.nestedProperty")
                : null;
            var newValue = Guid.NewGuid().ToString();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                validFormDataPath,
                validCalculationResultPath,
                newValue,
                PolicyDataPatchScope.CreateQuoteVersionPatchScope(newBusinessQuoteId, 1),
                PatchRules.None);

            // Act
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var aggregate = stack.QuoteAggregateRepository.GetById(this.tenantId, policyId);
                aggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());
                await stack.QuoteAggregateRepository.Save(aggregate);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quoteReadModel = stack.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, newBusinessQuoteId);
                quoteReadModel.LatestCalculationResult.Questions.Should().NotContain(
                    newValue,
                    $"QuoteReadModel latest calculation result patching expectation violation.");
                quoteReadModel.LatestFormData.Should().NotContain(
                    newValue,
                    "QuoteReadModel latest form model patching expectation violation.");

                var quoteVersionReadModelDetails = stack.QuoteVersionReadModelRepository.GetVersionDetailsByVersionNumber(this.tenantId, newBusinessQuoteId, 1);
                quoteVersionReadModelDetails.LatestFormData.Contains(newValue).Should().Be(
                    targetFormDataPathSet,
                    "QuoteVersionReadModel form data patching expectation violation.");
                quoteVersionReadModelDetails.CalculationResultJson.Contains(newValue).Should().Be(
                    targetCalculationResultPathSet,
                    "QuoteVersionReadModel calculation result patching expectation violation.");

                var policyReadModel = stack.PolicyReadModelRepository.GetPolicyDetails(this.tenantId, policyId);
                policyReadModel.CalculationResult.Questions.Should().NotContain(
                    newValue,
                    "PolicyReadModel latest calculation result patching expectation violation.");

                foreach (var transaction in policyReadModel.Transactions.OfType<Domain.ReadModel.Policy.PolicyTransaction>())
                {
                    Assert.False(
                        transaction.PolicyData.FormData.Contains(newValue),
                        "Transaction form data patching expectation violation");
                    Assert.False(
                        transaction.PolicyData.CalculationResult.Questions.Contains(newValue),
                        "Transaction calculation result patching expectation violation.");
                }
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task PatchFormData_PatchesOnlyPolicyData_WhenScopeIsPolicyTransaction(
            bool targetFormDataPathSet, bool targetCalculationResultPathSet)
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            Guid policyId;
            Guid newBusinessQuoteId;
            Guid policyTransactionId;
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                stack1.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack1.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
                var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
                var originalQuote = QuoteFactory.CreateNewBusinessQuote(
                    this.tenantId,
                    this.productId,
                    organisationId: organisation.Id);
                var originalAggregate = originalQuote.Aggregate
                    .WithCustomerDetails(originalQuote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(originalQuote.Id)
                    .WithCalculationResult(originalQuote.Id, formData, calculationResultJson)
                    .WithSubmission(originalQuote.Id)
                    .WithQuoteVersion(originalQuote.Id)
                    .WithPolicy(originalQuote.Id);
                policyId = originalAggregate.Id;
                newBusinessQuoteId = originalQuote.Id;
                policyTransactionId = originalAggregate.Policy.Transactions.First().Id;
                await stack1.QuoteAggregateRepository.Save(originalAggregate);
            }

            var validFormDataPath = targetFormDataPathSet
                ? new JsonPath("objectProperty.nestedProperty")
                : null;
            var validCalculationResultPath = targetCalculationResultPathSet
                ? new JsonPath("questions.ratingPrimary.objectProperty.nestedProperty")
                : null;
            var newValue = Guid.NewGuid().ToString();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                validFormDataPath,
                validCalculationResultPath,
                newValue,
                PolicyDataPatchScope.CreatePolicyTransactionPatchScope(policyTransactionId),
                PatchRules.None);

            // Act
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var aggregate = stack.QuoteAggregateRepository.GetById(this.tenantId, policyId);
                aggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());
                await stack.QuoteAggregateRepository.Save(aggregate);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quoteReadModel = stack.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, newBusinessQuoteId);
                quoteReadModel.LatestCalculationResult.Questions.Should().NotContain(
                    newValue,
                    $"QuoteReadModel latest calculation result patching expectation violation.");
                quoteReadModel.LatestFormData.Should().NotContain(
                    newValue,
                    "QuoteReadModel latest form model patching expectation violation.");

                var quoteVersionReadModelDetails = stack.QuoteVersionReadModelRepository.GetVersionDetailsByVersionNumber(this.tenantId, newBusinessQuoteId, 1);
                quoteVersionReadModelDetails.LatestFormData.Should().NotContain(
                    newValue,
                    "QuoteVersionReadModel form data patching expectation violation.");
                quoteVersionReadModelDetails.CalculationResultJson.Should().NotContain(
                    newValue,
                    "QuoteVersionReadModel calculation result patching expectation violation.");

                var policyReadModel = stack.PolicyReadModelRepository.GetPolicyDetails(this.tenantId, policyId);
                policyReadModel.CalculationResult.Questions.Contains(newValue).Should().Be(
                    targetCalculationResultPathSet,
                    "PolicyReadModel latest calculation result patching expectation violation.");

                foreach (var transaction in policyReadModel.Transactions.OfType<Domain.ReadModel.Policy.PolicyTransaction>())
                {
                    transaction.PolicyData.FormData.Contains(newValue).Should().Be(
                        targetFormDataPathSet,
                        "Transaction form data patching expectation violation");
                    transaction.PolicyData.CalculationResult.Questions.Contains(newValue).Should().Be(
                        targetCalculationResultPathSet,
                        "Transaction calculation result patching expectation violation.");
                }
            }
        }

        [Fact]
        public void PatchDataCorrected_RoundtripsCorrectly_AsObject()
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
            var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
            var aggregate = QuoteFactory.CreateNewPolicyWithSubmittedQuote(
                this.tenantId, this.productId, DeploymentEnvironment.Staging, formData, calculationResultJson);
            var quote = aggregate.GetQuoteOrThrow(aggregate.Policy.QuoteId.GetValueOrDefault());
            var formModelTargetPath = new JsonPath("valueStringProperty2");
            var command = new CopyFieldPolicyDataPatchCommand(
                formModelTargetPath,
                null,
                PatchSourceEntity.FirstPolicyTransactionCalculationResult,
                new JsonPath("questions.ratingPrimary.objectProperty.nestedProperty"),
                PolicyDataPatchScope.CreateGlobalPatchScope(),
                PatchRules.None);
            var dataHolders = new List<IPatchableDataHolder>();
            dataHolders.AddRange(new List<IPatchableDataHolder>() { quote });
            dataHolders.AddRange(aggregate.Policy?.Transactions ??
                Enumerable.Empty<Domain.Aggregates.Quote.Entities.PolicyTransaction>());
            var results = dataHolders.Select(dh => dh.SelectAndValidateFormDataPatchTargets(command));
            var patchTargets = results.SelectMany(r => r.Value);

            var patch = new PolicyDataPatch(
                DataPatchType.FormData,
                command.TargetFormDataPath,
                command.GetNewValue(aggregate),
                patchTargets);
            var @event = new QuoteAggregate.PolicyDataPatchedEvent(this.tenantId, aggregate.Id, patch, this.performingUserId, Instant.MinValue);

            // Act
            var eventRecord = EventRecordWithGuidId.Create(this.tenantId, aggregate.Id, 0, @event, AggregateType.Quote, Instant.MinValue);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
            };

            var json = JsonConvert.SerializeObject(eventRecord, settings);
            var outputEventRecord = JsonConvert.DeserializeObject<EventRecordWithGuidId>(json, settings);
            var outputEvent = outputEventRecord.GetEvent<QuoteAggregate.PolicyDataPatchedEvent, QuoteAggregate>();

            // Assert
            Assert.NotNull(outputEvent);
            Assert.NotNull(outputEvent.PolicyDatPatch);
            Assert.Equal(aggregate.Id, outputEvent.AggregateId);
            Assert.Equal(Instant.MinValue, outputEvent.Timestamp);
        }

        [Fact]
        public void PatchDataCorrected_RoundtripsCorrectly_AsIEvent()
        {
            // Arrange
            this.CreateTenantAndProduct(this.tenantId, this.productId);
            var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
            var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
            var aggregate = QuoteFactory.CreateNewPolicyWithSubmittedQuote(
                this.tenantId, this.productId, DeploymentEnvironment.Staging, formData, calculationResultJson);
            var quote = aggregate.GetQuoteOrThrow(aggregate.Policy.QuoteId.GetValueOrDefault());
            var targetFormDataPath = new JsonPath("valueStringProperty2");
            var command = new CopyFieldPolicyDataPatchCommand(
                targetFormDataPath,
                null,
                PatchSourceEntity.FirstPolicyTransactionCalculationResult,
                new JsonPath("questions.ratingPrimary.objectProperty.nestedProperty"),
                PolicyDataPatchScope.CreateGlobalPatchScope(),
                PatchRules.None);
            var dataHolders = new List<IPatchableDataHolder>();
            dataHolders.AddRange(new List<IPatchableDataHolder>() { quote });
            dataHolders.AddRange(aggregate.Policy?.Transactions ??
                Enumerable.Empty<Domain.Aggregates.Quote.Entities.PolicyTransaction>());
            var results = dataHolders.Select(dh => dh.SelectAndValidateFormDataPatchTargets(command));
            var patchTargets = results.SelectMany(r => r.Value);

            var patch = new PolicyDataPatch(
                DataPatchType.FormData,
                command.TargetFormDataPath,
                command.GetNewValue(aggregate),
                patchTargets);
            Instant testInstant = Instant.FromUtc(2022, 08, 21, 13, 22, 56);
            var @event = new QuoteAggregate.PolicyDataPatchedEvent(this.tenantId, aggregate.Id, patch, this.performingUserId, testInstant);

            // Act
            var eventRecord = EventRecordWithGuidId.Create(this.tenantId, aggregate.Id, 0, @event, AggregateType.Quote, testInstant);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
            }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            var json = JsonConvert.SerializeObject(eventRecord, settings);
            var outputEventRecord = JsonConvert.DeserializeObject<EventRecordWithGuidId>(json, settings);
            var outputEvent = outputEventRecord.GetEvent<QuoteAggregate.PolicyDataPatchedEvent, QuoteAggregate>();

            // Assert
            Assert.NotNull(outputEvent);
            Assert.NotNull(outputEvent.PolicyDatPatch);
            Assert.Equal(aggregate.Id, outputEvent.AggregateId);
            Assert.Equal(testInstant, outputEvent.Timestamp);
        }

        [Fact]
        public async Task ResetAdjustmentQuote_ReturnFormDataFieldRemoved_WhenResetAdjustmentQuoteIsTrue()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var clock = NodaTime.SystemClock.Instance;
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var questionKey = "AdjusmentQuoteField";
                var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSetForReset(questionKey, resetForNewAdjustmentQuotes: "true");
                var formData = new UBind.Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSampleWithSpecifiedProperty(questionKey, "true", clock.Now().ToLocalDateInAet()));
                var formDataSchema = new FormDataSchema(JObject.Parse(baseConfiguration));

                var aggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    formDataJson: formData.Json,
                    organisationId: organisation.Id);

                // Act
                var adjustmentQuote = aggregate.Policy.CreateAdjustmentQuote(
                    clock.Now(),
                    "FOO",
                    Enumerable.Empty<IClaimReadModel>(),
                    this.performingUserId,
                    this.quoteWorkflow,
                    QuoteExpirySettings.Default,
                    false,
                    Guid.NewGuid(),
                    formDataSchema);

                await stack.QuoteAggregateRepository.Save(aggregate);

                // Assert
                adjustmentQuote.LatestFormData.Data.GetValue(questionKey).Should().BeNull();
            }
        }

        [Fact]
        public async Task ResetAdjustmentQuote_ReturnFormDataFieldRemoved_WhenResetAllNewQuoteIsTrue()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var clock = NodaTime.SystemClock.Instance;
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var questionKey = "AdjusmentAllNewQuoteField";
                var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSetForReset(questionKey, resetForNewQuotes: "true");
                var formData = new UBind.Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSampleWithSpecifiedProperty(questionKey, "true", clock.Now().ToLocalDateInAet()));
                var formDataSchema = new FormDataSchema(JObject.Parse(baseConfiguration));

                var aggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    formDataJson: formData.Json,
                    organisationId: organisation.Id);

                // Act
                var adjustmentQuote = aggregate.Policy.CreateAdjustmentQuote(
                    clock.Now(),
                    "FOO",
                    Enumerable.Empty<IClaimReadModel>(),
                    this.performingUserId,
                    this.quoteWorkflow,
                    QuoteExpirySettings.Default,
                    false,
                    Guid.NewGuid(),
                    formDataSchema);

                await stack.QuoteAggregateRepository.Save(aggregate);

                // Assert
                adjustmentQuote.LatestFormData.Data.GetValue(questionKey).Should().BeNull();
            }
        }

        [Fact]
        public async Task ResetRenewalQuote_ReturnFormDataFieldRemoved_WhenResetRenewalQuoteIsTrue()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var clock = NodaTime.SystemClock.Instance;
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var questionKey = "RenewalQuoteField";
                var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSetForReset(questionKey, resetForNewRenewalQuotes: "true");
                var formData = new UBind.Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSampleWithSpecifiedProperty(questionKey, "true", clock.Now().ToLocalDateInAet()));
                var formDataSchema = new FormDataSchema(JObject.Parse(baseConfiguration));

                var aggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    formDataJson: formData.Json,
                    organisationId: organisation.Id);

                // Act
                var renewalQuote = aggregate.Policy.CreateRenewalQuote(
                    Enumerable.Empty<IClaimReadModel>(),
                    clock.Now(),
                    "FOO",
                    this.performingUserId,
                    this.quoteWorkflow,
                    QuoteExpirySettings.Default,
                    QuoteFactory.ProductConfiguation,
                    false,
                    Guid.NewGuid(),
                    formDataSchema);

                await stack.QuoteAggregateRepository.Save(aggregate);

                // Assert
                renewalQuote.LatestFormData.Data.GetValue(questionKey).Should().BeNull();
            }
        }

        [Fact]
        public async Task ResetRenewalQuote_ReturnFormDataFieldRemoved_WhenResetAllNewQuoteIsTrue()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var clock = NodaTime.SystemClock.Instance;
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var questionKey = "RenewalAllNewQuoteField";
                var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSetForReset(questionKey, resetForNewQuotes: "true");
                var formData = new UBind.Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSampleWithSpecifiedProperty(questionKey, "true", clock.Now().ToLocalDateInAet()));
                var formDataSchema = new FormDataSchema(JObject.Parse(baseConfiguration));

                var aggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    formDataJson: formData.Json,
                    organisationId: organisation.Id);

                // Act
                var renewalQuote = aggregate.Policy.CreateRenewalQuote(
                    Enumerable.Empty<IClaimReadModel>(),
                    clock.Now(),
                    "FOO",
                    this.performingUserId,
                    this.quoteWorkflow,
                    QuoteExpirySettings.Default,
                    QuoteFactory.ProductConfiguation,
                    false,
                    Guid.NewGuid(),
                    formDataSchema);

                await stack.QuoteAggregateRepository.Save(aggregate);

                // Assert
                renewalQuote.LatestFormData.Data.GetValue(questionKey).Should().BeNull();
            }
        }

        [Fact]
        public async Task ResetCancellationQuote_ReturnFormDataFieldRemoved_WhenResetCancellationQuoteIsTrue()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var clock = NodaTime.SystemClock.Instance;
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var questionKey = "CancellationQuoteField";
                var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSetForReset(questionKey, resetForNewCancellationQuotes: "true");
                var formData = new UBind.Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSampleWithSpecifiedProperty(questionKey, "true", clock.Now().ToLocalDateInAet()));
                var formDataSchema = new FormDataSchema(JObject.Parse(baseConfiguration));

                var aggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    formDataJson: formData.Json,
                    organisationId: organisation.Id);

                // Act
                var cancelQuote = aggregate.Policy.CreateCancellationQuote(
                    clock.Now(),
                    "FOO",
                    this.performingUserId,
                    this.quoteWorkflow,
                    QuoteExpirySettings.Default,
                    false,
                    Guid.NewGuid(),
                    formDataSchema);

                await stack.QuoteAggregateRepository.Save(aggregate);

                // Assert
                cancelQuote.LatestFormData.Data.GetValue(questionKey).Should().BeNull();
            }
        }

        [Fact]
        public async Task ResetCancellationQuote_ReturnFormDataFieldRemoved_WhenResetAllNewQuoteIsTrue()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var clock = NodaTime.SystemClock.Instance;
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var questionKey = "CancellationAllNewQuoteField";
                var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSetForReset(questionKey, resetForNewQuotes: "true");
                var formData = new UBind.Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSampleWithSpecifiedProperty(questionKey, "true", clock.Now().ToLocalDateInAet()));
                var formDataSchema = new FormDataSchema(JObject.Parse(baseConfiguration));

                var aggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    formDataJson: formData.Json,
                    organisationId: organisation.Id);

                // Act
                var cancelQuote = aggregate.Policy.CreateCancellationQuote(
                    clock.Now(),
                    "FOO",
                    this.performingUserId,
                    this.quoteWorkflow,
                    QuoteExpirySettings.Default,
                    false,
                    Guid.NewGuid(),
                    formDataSchema);

                await stack.QuoteAggregateRepository.Save(aggregate);

                // Assert
                cancelQuote.LatestFormData.Data.GetValue(questionKey).Should().BeNull();
            }
        }

        [Fact]
        public async Task ResetNewPurchaseQuote_ReturnFormDataFieldRemoved_WhenResetNewPurchaseQuoteIsTrue()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                var product = stack.ProductRepository.GetProductById(tenant.Id, this.productId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var quoteExpirySettings = new QuoteExpirySettings(30, true);
                product = stack.ProductService.UpdateQuoteExpirySettings(
                    tenant.Id, product.Id, quoteExpirySettings, ProductQuoteExpirySettingUpdateType.UpdateNone, CancellationToken.None);
                var env = DeploymentEnvironment.Development;
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null,
                    this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var questionKey = "CloneNewPurchaseQuoteField";
                var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSetForReset(questionKey, resetForNewPurchaseQuotes: "true");
                var formData = new UBind.Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSampleWithSpecifiedProperty(questionKey, "true", stack.Clock.Now().ToLocalDateInAet()));
                var formDataSchema = new FormDataSchema(JObject.Parse(baseConfiguration));
                this.MockProductConfigurationProvider(baseConfiguration);
                var release = stack.CreateRelease(new ReleaseContext(tenant.Id, product.Id, env, Guid.NewGuid()));

                // create quote
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenant.Id,
                    organisation.Id,
                    product.Id,
                    env,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    stack.Clock.Now(),
                    release.ReleaseId,
                    Timezones.AET,
                    formData: formData.JObject);

                var aggregate = quote.Aggregate;
                quote.AssignQuoteNumber("FOO", this.performingUserId, stack.Clock.Now());

                // expire quote
                // Let's set the quote's expiry date to 1 minute before now
                var expiryTimestamp = stack.Clock.Now().Minus(Duration.FromMinutes(1));
                aggregate.SetQuoteExpiryTime(
                    quote.Id, expiryTimestamp, this.performingUserId, expiryTimestamp);
                await stack.QuoteAggregateRepository.Save(aggregate);
                quote = aggregate.GetQuoteOrThrow(quote.Id);

                // Act
                var command = new CloneQuoteFromExpiredQuoteCommand(
                    tenant.Id, quote.Id, DeploymentEnvironment.Development);

                var handler = new CloneQuoteFromExpiredQuoteCommandHandler(
                    stack.QuoteAggregateRepository,
                    stack.QuoteReadModelRepository,
                    stack.CustomerAggregateRepository,
                    stack.UserReadModelRepository,
                    stack.PersonAggregateRepository,
                    this.productConfigurationProviderMock.Object,
                    stack.DefaultWorkflowProvider,
                    stack.QuoteExpirySettingsProvider,
                    stack.HttpContextPropertiesResolver,
                    stack.Clock,
                    stack.QuoteAggregateResolverService,
                    stack.ProductFeatureSettingService,
                    stack.PolicyService,
                    stack.DefaultTimeOfDayScheme,
                    stack.ReleaseQueryService,
                    stack.AggregateLockingService);

                var duplicateQuote = await handler.Handle(command, CancellationToken.None);

                // Assert
                var resultFormData = new FormData(duplicateQuote.LatestFormData);
                resultFormData.GetValue(questionKey).Should().BeNull();
            }
        }

        [Fact]
        public async Task ResetNewPurchaseQuote_ReturnFormDataFieldRemoved_WhenResetNewAllQuoteIsTrue()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var product = stack.ProductRepository.GetProductById(tenant.Id, this.productId);
                stack.MockMediator.GetProductByIdOrAliasQuery(product);
                var quoteExpirySettings = new QuoteExpirySettings(30, true);
                product = stack.ProductService.UpdateQuoteExpirySettings(
                    tenant.Id, product.Id, quoteExpirySettings, ProductQuoteExpirySettingUpdateType.UpdateNone, CancellationToken.None);
                var env = DeploymentEnvironment.Development;
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null,
                    this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var questionKey = "CloneAllNewQuoteField";
                var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSetForReset(questionKey, resetForNewQuotes: "true");
                var formData = new UBind.Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSampleWithSpecifiedProperty(questionKey, "true", stack.Clock.Now().ToLocalDateInAet()));
                var formDataSchema = new FormDataSchema(JObject.Parse(baseConfiguration));
                this.MockProductConfigurationProvider(baseConfiguration);
                var release = stack.CreateRelease(new ReleaseContext(tenant.Id, product.Id, env, Guid.NewGuid()));

                // create quote
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenant.Id,
                    organisation.Id,
                    product.Id,
                    env,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    stack.Clock.Now(),
                    release.ReleaseId,
                    Timezones.AET,
                    formData: formData.JObject);

                var aggregate = quote.Aggregate;
                quote.AssignQuoteNumber("FOO", this.performingUserId, stack.Clock.Now());

                // expire quote
                // Let's set the quote's expiry date to 1 minute before now
                var expiryTimestamp = stack.Clock.Now().Minus(Duration.FromMinutes(1));
                aggregate.SetQuoteExpiryTime(
                    quote.Id, expiryTimestamp, this.performingUserId, expiryTimestamp);
                await stack.QuoteAggregateRepository.Save(aggregate);
                quote = aggregate.GetQuoteOrThrow(quote.Id);

                // Act
                var command = new CloneQuoteFromExpiredQuoteCommand(
                    tenant.Id, quote.Id, DeploymentEnvironment.Development);

                var handler = new CloneQuoteFromExpiredQuoteCommandHandler(
                    stack.QuoteAggregateRepository,
                    stack.QuoteReadModelRepository,
                    stack.CustomerAggregateRepository,
                    stack.UserReadModelRepository,
                    stack.PersonAggregateRepository,
                    this.productConfigurationProviderMock.Object,
                    stack.DefaultWorkflowProvider,
                    stack.QuoteExpirySettingsProvider,
                    stack.HttpContextPropertiesResolver,
                    stack.Clock,
                    stack.QuoteAggregateResolverService,
                    stack.ProductFeatureSettingService,
                    stack.PolicyService,
                    stack.DefaultTimeOfDayScheme,
                    stack.ReleaseQueryService,
                    stack.AggregateLockingService);

                var duplicateQuote = await handler.Handle(command, CancellationToken.None);

                // Assert
                var resultFormData = new FormData(duplicateQuote.LatestFormData);
                resultFormData.GetValue(questionKey).Should().BeNull();
            }
        }

        [Fact]
        public async Task ResetRenewalQuoteClone_ReturnFormDataFieldRemoved_WhenResetRenewalQuoteIsTrue()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                var product = stack.ProductRepository.GetProductById(tenant.Id, this.productId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var quoteExpirySettings = new QuoteExpirySettings(30, true);
                product = stack.ProductService.UpdateQuoteExpirySettings(
                    tenant.Id, product.Id, quoteExpirySettings, ProductQuoteExpirySettingUpdateType.UpdateNone, CancellationToken.None);
                var env = DeploymentEnvironment.Development;
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null,
                    this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var questionKey = "CloneRenewalQuoteField";
                var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSetForReset(questionKey, resetForNewRenewalQuotes: "true");
                var formData = new UBind.Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSampleWithSpecifiedProperty(questionKey, "true", stack.Clock.Now().ToLocalDateInAet()));
                var formDataSchema = new FormDataSchema(JObject.Parse(baseConfiguration));
                this.MockProductConfigurationProvider(baseConfiguration);

                // create quote
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenant.Id,
                    organisation.Id,
                    product.Id,
                    env,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    stack.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET,
                    formData: formData.JObject);

                var aggregate = quote.Aggregate;
                quote.AssignQuoteNumber("FOO", this.performingUserId, stack.Clock.Now());

                // Create policy and renewal quote
                aggregate.WithCustomerDetails(quote.Id)
                         .WithCustomer()
                         .WithCalculationResult(quote.Id)
                         .WithPolicy(quote.Id);

                var renewalQuote = aggregate.Policy.CreateRenewalQuote(
                    Enumerable.Empty<IClaimReadModel>(),
                    stack.Clock.Now(),
                    "FOO",
                    this.performingUserId,
                    this.quoteWorkflow,
                    QuoteExpirySettings.Default,
                    QuoteFactory.ProductConfiguation,
                    false,
                    Guid.NewGuid(),
                    formDataSchema);

                var firstRenewalQuote = aggregate.GetQuoteOrThrow(renewalQuote.Id);

                // expire quote
                // Let's set the quote's expiry date to 1 minute before now
                var expiryTimestamp = stack.Clock.Now().Minus(Duration.FromMinutes(1));
                aggregate.SetQuoteExpiryTime(
                    firstRenewalQuote.Id, expiryTimestamp, this.performingUserId, expiryTimestamp);
                await stack.QuoteAggregateRepository.Save(aggregate);

                // Act
                var command = new CloneQuoteFromExpiredQuoteCommand(
                     tenant.Id, firstRenewalQuote.Id, DeploymentEnvironment.Development);

                var handler = new CloneQuoteFromExpiredQuoteCommandHandler(
                    stack.QuoteAggregateRepository,
                    stack.QuoteReadModelRepository,
                    stack.CustomerAggregateRepository,
                    stack.UserReadModelRepository,
                    stack.PersonAggregateRepository,
                    this.productConfigurationProviderMock.Object,
                    stack.DefaultWorkflowProvider,
                    stack.QuoteExpirySettingsProvider,
                    stack.HttpContextPropertiesResolver,
                    stack.Clock,
                    stack.QuoteAggregateResolverService,
                    stack.ProductFeatureSettingService,
                    stack.PolicyService,
                    stack.DefaultTimeOfDayScheme,
                    stack.ReleaseQueryService,
                    stack.AggregateLockingService);

                var duplicateQuote = await handler.Handle(command, CancellationToken.None);

                // Assert
                var resultFormData = new FormData(duplicateQuote.LatestFormData);
                resultFormData.GetValue(questionKey).Should().BeNull();
            }
        }

        [Fact]
        public async Task ResetAdjustmentQuoteClone_ReturnFormDataFieldRemoved_WhenResetAdjustmentQuoteIsTrue()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var product = stack.ProductRepository.GetProductById(tenant.Id, this.productId);
                var quoteExpirySettings = new QuoteExpirySettings(30, true);
                product = stack.ProductService.UpdateQuoteExpirySettings(
                    tenant.Id, product.Id, quoteExpirySettings, ProductQuoteExpirySettingUpdateType.UpdateNone, CancellationToken.None);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null,
                    this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                var a = stack.Clock.Now();
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now());
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var questionKey = "CloneAdjustmentQuoteField";
                var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSetForReset(questionKey, resetForNewAdjustmentQuotes: "true");
                var formData = new UBind.Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSampleWithSpecifiedProperty(questionKey, "true", stack.Clock.Now().ToLocalDateInAet()));
                var formDataSchema = new FormDataSchema(JObject.Parse(baseConfiguration));
                this.MockProductConfigurationProvider(baseConfiguration);

                var aggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    formDataJson: formData.Json,
                    organisationId: organisation.Id);

                // Act
                var adjustmentQuote = aggregate.Policy.CreateAdjustmentQuote(
                    stack.Clock.Now(),
                    "FOO",
                    Enumerable.Empty<IClaimReadModel>(),
                    this.performingUserId,
                    this.quoteWorkflow,
                    QuoteExpirySettings.Default,
                    false,
                    Guid.NewGuid(),
                    formDataSchema);
                var firstAdjustmentQuote = aggregate.GetQuoteOrThrow(adjustmentQuote.Id);

                // expire quote
                // Let's set the quote's expiry date to 1 minute before now
                var expiryTimestamp = stack.Clock.Now().Minus(Duration.FromMinutes(1));
                aggregate.SetQuoteExpiryTime(
                    firstAdjustmentQuote.Id, expiryTimestamp, this.performingUserId, expiryTimestamp);
                await stack.QuoteAggregateRepository.Save(aggregate);

                // Act
                var command = new CloneQuoteFromExpiredQuoteCommand(
                    tenant.Id, firstAdjustmentQuote.Id, DeploymentEnvironment.Development);

                var handler = new CloneQuoteFromExpiredQuoteCommandHandler(
                stack.QuoteAggregateRepository,
                stack.QuoteReadModelRepository,
                stack.CustomerAggregateRepository,
                stack.UserReadModelRepository,
                stack.PersonAggregateRepository,
                this.productConfigurationProviderMock.Object,
                stack.DefaultWorkflowProvider,
                stack.QuoteExpirySettingsProvider,
                stack.HttpContextPropertiesResolver,
                stack.Clock,
                stack.QuoteAggregateResolverService,
                stack.ProductFeatureSettingService,
                stack.PolicyService,
                stack.DefaultTimeOfDayScheme,
                stack.ReleaseQueryService,
                stack.AggregateLockingService);

                var duplicateQuote = await handler.Handle(command, CancellationToken.None);

                // Assert
                var resultFormData = new FormData(duplicateQuote.LatestFormData);
                resultFormData.GetValue(questionKey).Should().BeNull();
            }
        }

        [Fact]
        public async Task ResetCancellationQuoteClone_ReturnFormDataFieldRemoved_WhenResetCancellationIsTrue()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                this.CreateTenantAndProduct(this.tenantId, this.productId);
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var product = stack.ProductRepository.GetProductById(tenant.Id, this.productId);
                var quoteExpirySettings = new QuoteExpirySettings(30, true);
                product = stack.ProductService.UpdateQuoteExpirySettings(
                    tenant.Id, product.Id, quoteExpirySettings, ProductQuoteExpirySettingUpdateType.UpdateNone, CancellationToken.None);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null,
                    this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var questionKey = "CloneAdjustmentQuoteField";
                var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSetForReset(questionKey, resetForNewCancellationQuotes: "true");
                var formData = new UBind.Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSampleWithSpecifiedProperty(questionKey, "true", stack.Clock.Now().ToLocalDateInAet()));
                var formDataSchema = new FormDataSchema(JObject.Parse(baseConfiguration));
                this.MockProductConfigurationProvider(baseConfiguration);

                var aggregate = QuoteFactory.CreateNewPolicy(
                    this.tenantId,
                    this.productId,
                    formDataJson: formData.Json,
                    organisationId: organisation.Id);

                // Act
                var cancellationQuote = aggregate.Policy.CreateCancellationQuote(
                    stack.Clock.Now(),
                    "FOO",
                    this.performingUserId,
                    this.quoteWorkflow,
                    QuoteExpirySettings.Default,
                    false,
                    Guid.NewGuid(),
                    formDataSchema);
                var firstCancellationQuote = aggregate.GetQuoteOrThrow(cancellationQuote.Id);

                // expire quote
                // Let's set the quote's expiry date to 1 minute before now
                var expiryTimestamp = stack.Clock.Now().Minus(Duration.FromMinutes(1));
                aggregate.SetQuoteExpiryTime(
                    firstCancellationQuote.Id, expiryTimestamp, this.performingUserId, expiryTimestamp);
                await stack.QuoteAggregateRepository.Save(aggregate);

                // Act
                var command = new CloneQuoteFromExpiredQuoteCommand(
                    tenant.Id, firstCancellationQuote.Id, DeploymentEnvironment.Development);

                var handler = new CloneQuoteFromExpiredQuoteCommandHandler(
                    stack.QuoteAggregateRepository,
                    stack.QuoteReadModelRepository,
                    stack.CustomerAggregateRepository,
                    stack.UserReadModelRepository,
                    stack.PersonAggregateRepository,
                    this.productConfigurationProviderMock.Object,
                    stack.DefaultWorkflowProvider,
                    stack.QuoteExpirySettingsProvider,
                    stack.HttpContextPropertiesResolver,
                    stack.Clock,
                    stack.QuoteAggregateResolverService,
                    stack.ProductFeatureSettingService,
                    stack.PolicyService,
                    new DefaultPolicyTransactionTimeOfDayScheme(),
                    stack.ReleaseQueryService,
                    stack.AggregateLockingService);

                var duplicateQuote = await handler.Handle(command, CancellationToken.None);

                // Assert
                var resultFormData = new FormData(duplicateQuote.LatestFormData);
                resultFormData.GetValue(questionKey).Should().BeNull();
            }
        }

        private void MockProductConfigurationProvider(string baseConfiguration)
        {
            IProductConfiguration productConfiguration = new DefaultProductConfiguration();
            var formDataSchema = new FormDataSchema(JObject.Parse(baseConfiguration));
            this.productConfigurationProviderMock
                .Setup(s => s.GetFormDataSchema(It.IsAny<ProductContext>(), It.IsAny<WebFormAppType>()))
                .Returns(formDataSchema);
            this.productConfigurationProviderMock
                .Setup(s => s.GetFormDataSchema(It.IsAny<ReleaseContext>(), It.IsAny<WebFormAppType>()))
                .Returns(formDataSchema);
            this.productConfigurationProviderMock
                .Setup(s => s.GetProductConfiguration(It.IsAny<ReleaseContext>(), It.IsAny<WebFormAppType>()))
                .Returns(Task.FromResult(productConfiguration));
        }

        private void CreateTenantAndProduct(Guid tenantId, Guid productId, Guid organisationId = default)
        {
            // Create tenant and product (required for join when reading quotes and policies).
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = TenantFactory.Create(tenantId);
                var product = ProductFactory.Create(tenantId, productId);
                tenant.SetDefaultOrganisation(organisationId, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.CreateTenant(tenant);
                stack.CreateProduct(product);
            }
        }
    }
}
