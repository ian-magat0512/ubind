// <copyright file="QuoteAggregateRollbackTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Aggregates.Quote
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using MimeKit;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;
    using FormData = UBind.Domain.Aggregates.Quote.FormData;

    [Collection(DatabaseCollection.Name)]
    public class QuoteAggregateRollbackTests
    {
        private readonly Guid performingUserId = Guid.NewGuid();
        private Guid tenantId;
        private Guid productId;

        public QuoteAggregateRollbackTests()
        {
            this.productId = Guid.NewGuid();
            this.tenantId = Guid.NewGuid();
        }

        [Fact]
        public async Task Rolling_back_a_QuoteAggregate_shows_data_at_that_sequence_number()
        {
            QuoteAggregate quoteAggregate;
            int targetRollbackSequenceNumber;

            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId);
                quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                var formData = new FormData(FormDataJsonFactory.GetCustomSample("data1"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                int step1Count = quoteAggregate.PersistedEventCount;
                targetRollbackSequenceNumber = step1Count - 1;

                formData = new FormData(FormDataJsonFactory.GetCustomSample("data2"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);

                formData = new FormData(FormDataJsonFactory.GetCustomSample("data3"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);

                // Act
                quoteAggregate.RollbackTo(targetRollbackSequenceNumber, this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                int step4Count = quoteAggregate.PersistedEventCount;
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);

                // Assert
                Assert.DoesNotContain("data3", quote.LatestFormData.Data.Json);
                Assert.Contains("data1", quote.LatestFormData.Data.Json);
                Assert.DoesNotContain("data2", quote.LatestFormData.Data.Json);
                Assert.DoesNotContain("data3", quote.LatestFormData.Data.Json);
            }
        }

        [Fact]
        public async Task Rolling_back_a_QuoteAggregate_updates_the_quote_read_model_table_correctly()
        {
            QuoteAggregate quoteAggregate;
            Quote quote;
            FormData formDataJson;
            int targetRollbackSequenceNumber;

            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                Tenant tenant = TenantFactory.Create(this.tenantId);
                Product product = ProductFactory.Create(this.tenantId, this.productId);
                stack1.CreateTenant(tenant);
                stack1.CreateProduct(product);

                stack1.DbContext.SaveChanges();

                Assert.True(stack1.TenantRepository.GetTenantById(this.tenantId) != null);
                Assert.True(stack1.ProductRepository.GetProductById(this.tenantId, this.productId) != null);

                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack1.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId, organisationId: organisation.Id);
                quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                formDataJson = new FormData(FormDataJsonFactory.GetCustomSample("RollbackData1"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formDataJson, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                int step1Count = quoteAggregate.PersistedEventCount;
                targetRollbackSequenceNumber = step1Count - 1;

                formDataJson = new FormData(FormDataJsonFactory.GetCustomSample("RollbackData2"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formDataJson, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
            }

            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // get from read model
                var quoteStack2 = quoteAggregate.GetQuoteOrThrow(quote.Id);
                var quoteDetails = stack2.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, quoteStack2.Id);
                Assert.Contains("RollbackData2", quoteDetails.LatestFormData);

                formDataJson = new FormData(FormDataJsonFactory.GetCustomSample("RollbackData3"));
                quoteStack2.UpdateFormData(formDataJson, this.performingUserId, stack2.Clock.Now());
                quoteStack2.SaveQuote(this.performingUserId, stack2.Clock.Now());
                await stack2.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack2.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
            }

            using (var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Act
                quoteAggregate.RollbackTo(targetRollbackSequenceNumber, this.performingUserId, stack3.Clock.Now());
                await stack3.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack3.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                var quoteStack3 = quoteAggregate.GetQuoteOrThrow(quote.Id);
                int step4Count = quoteAggregate.PersistedEventCount;

                // Assert
                var quoteDetails = stack3.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, quoteStack3.Id);
                Assert.True(quoteDetails.LatestFormData != null);
                Assert.Contains("RollbackData1", quoteDetails.LatestFormData);
            }
        }

        [Fact]
        public async Task RollbackTo_ShouldUpdateCorrectly_WhenTheQuoteHasPolicyPolicyTransactionsAndDocuments()
        {
            QuoteAggregate quoteAggregate;
            Quote quote;
            FormData formDataJson;
            int targetRollbackSequenceNumber;
            Guid? policyId;
            QuoteDocument document;

            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                Tenant tenant = TenantFactory.Create(this.tenantId);
                Product product = ProductFactory.Create(this.tenantId, this.productId);
                stack1.CreateTenant(tenant);
                stack1.CreateProduct(product);

                stack1.DbContext.SaveChanges();

                stack1.TenantRepository.GetTenantById(this.tenantId).Should().NotBeNull();
                stack1.ProductRepository.GetProductById(this.tenantId, this.productId).Should().NotBeNull();

                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack1.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId, organisationId: organisation.Id);
                quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                formDataJson = new FormData(FormDataJsonFactory.GetCustomSample("RollbackData1"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formDataJson, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                int step1Count = quoteAggregate.PersistedEventCount;
                targetRollbackSequenceNumber = step1Count - 1;

                formDataJson = new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formDataJson, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());

                // issue a policy
                quoteAggregate = quoteAggregate.WithPolicy(quote.Id);
                policyId = quoteAggregate.Policy.PolicyId;
                var policyTransaction = quoteAggregate.Policy.Transactions.FirstOrDefault();
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);

                // attach a document
                document = this.PrepareDocument(stack1.Clock.Now());
                quoteAggregate.AttachPolicyDocument(document, policyTransaction.Id, Guid.NewGuid(), stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);

                var testDoc = quoteAggregate.GetPolicyDocument(document.Name);
                testDoc.Should().NotBeNull();
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
            }

            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // get from read model
                var quoteStack2 = quoteAggregate.GetQuoteOrThrow(quote.Id);
                var quoteDetails = stack2.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, quoteStack2.Id);
                quoteDetails.LatestFormData.Should().Contain("inceptionDate");

                formDataJson = new FormData(FormDataJsonFactory.GetCustomSample("RollbackData3"));
                quoteStack2.UpdateFormData(formDataJson, this.performingUserId, stack2.Clock.Now());
                quoteStack2.SaveQuote(this.performingUserId, stack2.Clock.Now());
                await stack2.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack2.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
            }

            using (var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Act
                quoteAggregate.RollbackTo(targetRollbackSequenceNumber, this.performingUserId, stack3.Clock.Now());
                await stack3.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack3.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                var quoteStack3 = quoteAggregate.GetQuoteOrThrow(quote.Id);
                int step4Count = quoteAggregate.PersistedEventCount;

                // Assert
                // quote should have the correct formData after rollback
                var quoteDetails = stack3.QuoteReadModelRepository.GetQuoteDetails(this.tenantId, quoteStack3.Id);
                quoteDetails.LatestFormData.Should().NotBeNull();
                quoteDetails.LatestFormData.Should().Contain("RollbackData1");

                // quote should not have policy since rollback point is before policy is issued
                quoteDetails.PolicyId.Should().BeNull();

                // policy transactions should be deleted since rollback point is before policy is issued
                stack3.PolicyTransactionReadModelRepository.GetByPolicyId(policyId.Value).Any().Should().BeFalse();

                // documents generated after rollback point should be deleted
                var testDoc = quoteAggregate.GetPolicyDocument(document.Name);
                testDoc.Should().BeNull();
            }
        }

        [Fact]
        public async Task Rolling_back_a_QuoteAggregate_updates_the_quote_version_read_model_table_correctly()
        {
            QuoteAggregate quoteAggregate;
            FormData formData;
            int targetRollbackSequenceNumber;
            int beforeRollbackQuoteVersionCount;
            Quote quote;

            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                Tenant tenant = TenantFactory.Create(this.tenantId);
                Product product = ProductFactory.Create(this.tenantId, this.productId);
                stack1.CreateTenant(tenant);
                stack1.CreateProduct(product);

                Assert.True(stack1.TenantRepository.GetTenantById(this.tenantId) != null);
                Assert.True(stack1.ProductRepository.GetProductById(this.tenantId, this.productId) != null);

                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack1.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId, organisationId: organisation.Id);
                quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                formData = new FormData(FormDataJsonFactory.GetCustomSample("RollbackQuoteVersionData1"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                int step1Count = quoteAggregate.PersistedEventCount;
                targetRollbackSequenceNumber = step1Count - 1;

                formData = new FormData(FormDataJsonFactory.GetCustomSample("RollbackQuoteVersionData2"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quoteAggregate.CreateVersion(this.performingUserId, stack1.Clock.Now(), quote.Id);
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                var quoteStack1 = quoteAggregate.GetQuoteOrThrow(quote.Id);
                int step2Count = quoteAggregate.PersistedEventCount;

                beforeRollbackQuoteVersionCount = stack1.QuoteVersionReadModelRepository.GetDetailVersionsOfQuote(tenant.Id, quoteStack1.Id).Count();
            }

            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // get from read model
                var quoteStack2 = quoteAggregate.GetQuoteOrThrow(quote.Id);
                var quoteDetails = stack2.QuoteReadModelRepository.GetQuoteDetails(quote.Aggregate.TenantId, quoteStack2.Id);
                Assert.Contains("RollbackQuoteVersionData2", quoteDetails.LatestFormData);

                // Act
                quoteAggregate.RollbackTo(targetRollbackSequenceNumber, this.performingUserId, stack2.Clock.Now());
                await stack2.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack2.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);

                // Assert
                quoteDetails = stack2.QuoteReadModelRepository.GetQuoteDetails(quote.Aggregate.TenantId, quoteStack2.Id);
                Assert.True(quoteDetails.LatestFormData != null);
                Assert.Contains("RollbackQuoteVersionData1", quoteDetails.LatestFormData);

                int afterRollbackQuoteVersionCount = stack2.QuoteVersionReadModelRepository.GetDetailVersionsOfQuote(quote.Aggregate.TenantId, quoteDetails.QuoteId).Count();
                Assert.NotEqual(beforeRollbackQuoteVersionCount, afterRollbackQuoteVersionCount);
            }
        }

        [Fact]
        public async Task Rolling_back_a_rollback_of_a_QuoteAggregate_undoes_the_rollback()
        {
            QuoteAggregate quoteAggregate;
            int targetRollbackSequenceNumber;

            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                Tenant tenant = TenantFactory.Create(this.tenantId);
                stack1.CreateTenant(tenant);
                Product product = ProductFactory.Create(this.tenantId, this.productId);
                stack1.CreateProduct(product);

                var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId);
                quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                var formData = new FormData(FormDataJsonFactory.GetCustomSample("rrdata1"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                int step1Count = quoteAggregate.PersistedEventCount;
                targetRollbackSequenceNumber = step1Count - 1;

                formData = new FormData(FormDataJsonFactory.GetCustomSample("rrdata2"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);

                formData = new FormData(FormDataJsonFactory.GetCustomSample("rrdata3"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                int step3Count = quoteAggregate.PersistedEventCount;

                // Act
                quoteAggregate.RollbackTo(targetRollbackSequenceNumber, this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                int step4Count = quoteAggregate.PersistedEventCount;
                Assert.Contains("rrdata1", quote.LatestFormData.Data.Json);

                // rollback the rollback
                quoteAggregate.RollbackTo(step3Count - 1, this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                int step5Count = quoteAggregate.PersistedEventCount;

                // Assert
                Assert.DoesNotContain("rrdata1", quote.LatestFormData.Data.Json);
                Assert.Contains("rrdata3", quote.LatestFormData.Data.Json);
            }
        }

        [Fact]
        public async Task Rollback_ShouldRestoreFormData_AfterImportedPolicy()
        {
            QuoteAggregate quoteAggregate;
            int targetRollbackSequenceNumber;

            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                Tenant tenant = TenantFactory.Create(this.tenantId);
                stack1.CreateTenant(tenant);
                Product product = ProductFactory.Create(this.tenantId, this.productId);
                stack1.CreateProduct(product);

                var importedPolicy = QuoteFactory.CreateImportedPolicy(this.tenantId, this.productId);
                var importEvent = importedPolicy.UnsavedEvents.FirstOrDefault() as PolicyImportedEvent;
                await stack1.QuoteAggregateRepository.Save(importedPolicy);

                var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId);
                quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                var formData = new FormData(FormDataJsonFactory.GetCustomSample("rrdata1"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                int step1Count = quoteAggregate.PersistedEventCount;
                targetRollbackSequenceNumber = step1Count - 1;

                formData = new FormData(FormDataJsonFactory.GetCustomSample("rrdata2"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);

                formData = new FormData(FormDataJsonFactory.GetCustomSample("rrdata3"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                int step3Count = quoteAggregate.PersistedEventCount;

                // Act
                quoteAggregate.RollbackTo(targetRollbackSequenceNumber, this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                int step4Count = quoteAggregate.PersistedEventCount;
                Assert.Contains("rrdata1", quote.LatestFormData.Data.Json);

                // rollback the rollback
                quoteAggregate.RollbackTo(step3Count - 1, this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                int step5Count = quoteAggregate.PersistedEventCount;

                // Assert
                Assert.DoesNotContain("rrdata1", quote.LatestFormData.Data.Json);
                Assert.Contains("rrdata3", quote.LatestFormData.Data.Json);
            }
        }

        [Fact]
        public async Task ReplayAllEvents_ReplaysEvents_AfterStrippingOutEventsDueToRollback()
        {
            QuoteAggregate quoteAggregate;
            int rollbackSequenceNumber;

            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                Tenant tenant = TenantFactory.Create(this.tenantId);
                stack1.CreateTenant(tenant);
                Product product = ProductFactory.Create(this.tenantId, this.productId);
                stack1.CreateProduct(product);

                var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId);
                quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                var formData = new FormData(FormDataJsonFactory.GetCustomSample("IncompleteQuote"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);

                formData = new FormData(FormDataJsonFactory.GetCustomSample("ApprovedQuote"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                rollbackSequenceNumber = quoteAggregate.PersistedEventCount - 1;

                formData = new FormData(FormDataJsonFactory.GetCustomSample("CompleteQuote"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                int step3Count = quoteAggregate.PersistedEventCount;

                // Rollback to Approved state
                quoteAggregate.RollbackTo(rollbackSequenceNumber, this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                int step4Count = quoteAggregate.PersistedEventCount;

                // Rolled back to InitialiseQuote
                quote.LatestFormData.Data.Json.Should().Contain("ApprovedQuote");

                // rollback before the first rollback - revert to CompleteQuote
                quoteAggregate.RollbackTo(step3Count - 1, this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                int step5Count = quoteAggregate.PersistedEventCount;

                // Act
                await stack1.QuoteAggregateRepository.ReplayAllEventsByAggregateId(quoteAggregate.TenantId, quoteAggregate.Id);
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);

                // Assert
                quote.LatestFormData.Data.Json.Should().NotContain("ApprovedQuote");
                quote.LatestFormData.Data.Json.Should().Contain("CompleteQuote");
            }
        }

        [Fact]
        public async Task RollingBack_QuoteAggregate_WithTwoRollbackEvent_Scenario1()
        {
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId);
                QuoteAggregate quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                for (int i = 0; i < 50; i++)
                {
                    var formData = new FormData(FormDataJsonFactory.GetCustomSample("data1"));
                    quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                    quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                    await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                    quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);

                    if (quoteAggregate.PersistedEventCount == 25)
                    {
                        // first rollback
                        quoteAggregate.RollbackTo(14, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }

                    if (quoteAggregate.PersistedEventCount == 44)
                    {
                        // Second rollback
                        quoteAggregate.RollbackTo(36, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }
                }

                // Act
                int totalEventStripped = quoteAggregate.GetEventsStrippedByRollbacks().Count();

                // Assert
                // expected 19 events stripped.
                totalEventStripped.Should().Be(19);
            }
        }

        [Fact]
        public async Task RollingBack_QuoteAggregate_WithThreeRollbackEvent_Scenario2()
        {
            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId);
                QuoteAggregate quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                for (int i = 0; i < 60; i++)
                {
                    var formData = new FormData(FormDataJsonFactory.GetCustomSample("data1"));
                    quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                    quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                    await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                    quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);

                    if (quoteAggregate.PersistedEventCount == 25)
                    {
                        // first rollback
                        quoteAggregate.RollbackTo(14, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }

                    if (quoteAggregate.PersistedEventCount == 44)
                    {
                        // Second rollback
                        quoteAggregate.RollbackTo(36, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }

                    if (quoteAggregate.PersistedEventCount == 51)
                    {
                        // Third rollback
                        quoteAggregate.RollbackTo(11, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }
                }

                // Act
                int totalEventStripped = quoteAggregate.GetEventsStrippedByRollbacks().Count();

                // Assert
                // expected 40 events stripped.
                totalEventStripped.Should().Be(40);
            }
        }

        [Fact]
        public async Task RollingBack_QuoteAggregate_WithThreeRollbackEvent_Scenario3()
        {
            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId);
                QuoteAggregate quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                for (int i = 0; i < 60; i++)
                {
                    var formData = new FormData(FormDataJsonFactory.GetCustomSample("data1"));
                    quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                    quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                    await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                    quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);

                    if (quoteAggregate.PersistedEventCount == 25)
                    {
                        // first rollback
                        quoteAggregate.RollbackTo(14, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }

                    if (quoteAggregate.PersistedEventCount == 44)
                    {
                        // Second rollback
                        quoteAggregate.RollbackTo(36, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }

                    if (quoteAggregate.PersistedEventCount == 51)
                    {
                        // Second rollback
                        quoteAggregate.RollbackTo(35, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }
                }

                // Act
                int totalEventStripped = quoteAggregate.GetEventsStrippedByRollbacks().Count();

                // Assert
                // expected 27 events stripped.
                totalEventStripped.Should().Be(27);
            }
        }

        [Fact]
        public async Task RollingBack_QuoteAggregate_WithThreeRollbackEvent_Scenario4()
        {
            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId);
                QuoteAggregate quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                for (int i = 0; i < 60; i++)
                {
                    var formData = new FormData(FormDataJsonFactory.GetCustomSample("data1"));
                    quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                    quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                    await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                    quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);

                    if (quoteAggregate.PersistedEventCount == 25)
                    {
                        // first rollback
                        quoteAggregate.RollbackTo(14, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }

                    if (quoteAggregate.PersistedEventCount == 44)
                    {
                        // Second rollback
                        quoteAggregate.RollbackTo(36, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }

                    if (quoteAggregate.PersistedEventCount == 52)
                    {
                        // Second rollback
                        quoteAggregate.RollbackTo(43, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }
                }

                // Act
                int totalEventStripped = quoteAggregate.GetEventsStrippedByRollbacks().Count();

                // Assert
                // expected 20 events stripped
                totalEventStripped.Should().Be(20);
            }
        }

        [Fact]
        public async Task RollingBack_QuoteAggregate_WithThreeRollbackEvent_Scenario5()
        {
            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId);
                QuoteAggregate quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                for (int i = 0; i < 60; i++)
                {
                    var formData = new FormData(FormDataJsonFactory.GetCustomSample("data1"));
                    quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                    quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                    await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                    quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);

                    if (quoteAggregate.PersistedEventCount == 25)
                    {
                        // first rollback
                        quoteAggregate.RollbackTo(14, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }

                    if (quoteAggregate.PersistedEventCount == 26)
                    {
                        // Second rollback
                        quoteAggregate.RollbackTo(18, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }

                    if (quoteAggregate.PersistedEventCount == 52)
                    {
                        // Second rollback
                        quoteAggregate.RollbackTo(43, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }
                }

                // Act
                int totalEventStripped = quoteAggregate.GetEventsStrippedByRollbacks().Count();

                // Assert
                // expected 17 events stripped
                totalEventStripped.Should().Be(17);
            }
        }

        [Fact]
        public async Task RollingBack_QuoteAggregate_WithThreeRollbackEvent_Scenario6()
        {
            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId);
                QuoteAggregate quoteAggregate = quote.Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer()
                    .WithQuoteNumber(quote.Id);

                for (int i = 0; i < 60; i++)
                {
                    var formData = new FormData(FormDataJsonFactory.GetCustomSample("data1"));
                    quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                    quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                    await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                    quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);

                    if (quoteAggregate.PersistedEventCount == 26)
                    {
                        // first rollback
                        quoteAggregate.RollbackTo(14, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }

                    if (quoteAggregate.PersistedEventCount == 44)
                    {
                        // Second rollback
                        quoteAggregate.RollbackTo(36, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }

                    if (quoteAggregate.PersistedEventCount == 51)
                    {
                        // Second rollback
                        quoteAggregate.RollbackTo(25, this.performingUserId, stack1.Clock.Now());
                        await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                        quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                    }
                }

                // Act
                int totalEventStripped = quoteAggregate.GetEventsStrippedByRollbacks().Count();

                // Assert
                // expected 26 events stripped
                totalEventStripped.Should().Be(26);
            }
        }

        private QuoteDocument PrepareDocument(Instant now)
        {
            ContentType contentType = ContentType.Parse("text/plain");
            var document = MimeEntity.Load(contentType, new MemoryStream(new byte[123]));
            document.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);
            document.ContentDisposition.FileName = "test doc";
            var part = (MimePart)document;
            var quoteDocument = new QuoteDocument(
                document.ContentDisposition.FileName,
                document.ContentType.MimeType,
                part.Content.Stream.Length,
                Guid.NewGuid(),
                now);
            return quoteDocument;
        }
    }
}
