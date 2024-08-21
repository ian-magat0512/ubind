// <copyright file="QuoteReadModelRepositoryIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.ReadModels
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class QuoteReadModelRepositoryIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private Guid tenantId;
        private Guid productId;

        public QuoteReadModelRepositoryIntegrationTests()
        {
            this.productId = Guid.NewGuid();
            this.tenantId = Guid.NewGuid();
        }

        [Fact]
        public async Task QuoteReadModel_is_updated_when_QuoteAggregate_is_updated()
        {
            QuoteAggregate quoteAggregate;
            Quote quote;

            // Arrange
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                QuoteFactory.QuoteExpirySettingsProvider = stack1.QuoteExpirySettingsProvider;
                Tenant tenant = TenantFactory.Create(this.tenantId);
                Product product = ProductFactory.Create(this.tenantId, this.productId);
                stack1.CreateProduct(product);
                stack1.CreateTenant(tenant);

                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack1.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack1.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                stack1.TenantRepository.GetTenantById(this.tenantId).Should().NotBeNull();
                stack1.ProductRepository.GetProductById(this.tenantId, this.productId).Should().NotBeNull();

                FakePersonalDetails person = new FakePersonalDetails();
                person.TenantId = this.tenantId;

                quote = QuoteFactory.CreateNewBusinessQuote(
                    tenantId: this.tenantId,
                    productId: this.productId,
                    organisationId: organisation.Id);
                quoteAggregate = quote.Aggregate.WithCalculationResult(quote.Id)
                   .WithCustomerDetails(quote.Id)
                   .WithCustomer()
                   .WithQuoteNumber(quote.Id)
                   .WithPolicy(quote.Id);

                var formData = new Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSample("data1"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                int step1Count = quoteAggregate.PersistedEventCount;

                formData = new Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetCustomSample("data2"));
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(formData, this.performingUserId, stack1.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack1.Clock.Now());
                await stack1.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack1.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                int step2Count = quoteAggregate.PersistedEventCount;
            }

            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // get from read model
                var quoteDetails = stack2.QuoteReadModelRepository.GetQuoteDetails(quote.Aggregate.TenantId, quote.Id);
                Assert.Contains("data2", quoteDetails.LatestFormData);

                var formDataJson = FormDataJsonFactory.GetCustomSample("data3");
                quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
                quote.UpdateFormData(new Domain.Aggregates.Quote.FormData(formDataJson), this.performingUserId, stack2.Clock.Now());
                quote.SaveQuote(this.performingUserId, stack2.Clock.Now());
                await stack2.QuoteAggregateRepository.Save(quoteAggregate);
                quoteAggregate = stack2.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, quoteAggregate.Id);
                int step3Count = quoteAggregate.PersistedEventCount;

                // get from read model
                quoteDetails = stack2.QuoteReadModelRepository.GetQuoteDetails(quote.Aggregate.TenantId, quote.Id);

                quoteDetails.LatestFormData.Should().Contain("data3");
            }
        }
    }
}
