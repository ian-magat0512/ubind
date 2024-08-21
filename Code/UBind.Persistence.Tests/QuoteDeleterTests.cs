// <copyright file="QuoteDeleterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Application.Commands.Tenant;
    using UBind.Application.Queries.Tenant;
    using UBind.Application.Tests;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Extensions;
    using UBind.Domain.Factory;
    using UBind.Domain.Product;
    using UBind.Domain.Quote;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class QuoteDeleterTests
    {
        private readonly ApplicationStack stack;
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly IQuoteWorkflow quoteWorkflow = new DefaultQuoteWorkflow();

        public QuoteDeleterTests()
        {
            this.stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
        }

        public IClock Clock { get; } = SystemClock.Instance;

        [Fact]
        public async Task DeleteAllTests_DeletesQuotes_WhenQuotesAreInNascentState()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var tenantProductModel = await this.CreateProductAndTenant(productId);
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;

            UBind.Domain.Aggregates.Organisation.Organisation organisation
                = await this.CreateAndGetOrganisationForTenant(tenant);
            UserAggregate user = await this.CreateUserModelForTenant(tenant);

            // Count before data was added
            var beforeQuoteVersionCount = this.stack.DbContext.QuoteVersions.Where(x => x.TenantId == tenant.Id).Count();
            var beforeEventCount = this.stack.DbContext.EventRecordsWithGuidIds.Count();
            var beforeQuoteDocumentCount = this.stack.DbContext.QuoteDocuments.Where(x => x.TenantId == tenant.Id).Count();
            var beforeQuoteCount = this.stack.DbContext.QuoteReadModels.Where(x => x.TenantId == tenant.Id).Count();
            var beforeCustomerCount = this.stack.DbContext.CustomerReadModels.Where(x => x.TenantId == tenant.Id).Count();

            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);

            var nascentQuote = QuoteFactory.CreateNewBusinessQuote(
                tenant.Id,
                product.Id,
                organisationId: organisation.Id);
            var nascentQuoteAggregate = nascentQuote.Aggregate;
            await this.stack.QuoteAggregateRepository.Save(nascentQuoteAggregate);

            EmailModel emailModel = this.GenerateEmailModel();

            var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                emailModel,
                nascentQuoteAggregate.Id,
                nascentQuote.Id,
                default,
                Guid.NewGuid(),
                nascentQuoteAggregate.CustomerId,
                Guid.NewGuid(),
                EmailType.Admin,
                this.stack.Clock.Now(),
                this.stack.FileContentRepository);
            this.stack.EmailService.InsertEmailAndMetadata(metadata);

            this.stack.QuoteVersionReadModelUpdateRepository.Add(new QuoteVersionReadModel
            {
                Id = Guid.NewGuid(),
                QuoteVersionNumber = 1,
                QuoteId = nascentQuoteAggregate.Id,
            });
            this.stack.DbContext.SaveChanges();

            // Act
            this.stack.QuoteReductor.DeleteNascent(this.stack.ProgressLogger, 100, 100, Duration.FromSeconds(0));

            // Assert
            var afterQuoteVersionCount = this.stack.DbContext.QuoteVersions.Where(x => x.AggregateId == nascentQuoteAggregate.Id).Count();
            var afterEventCount = this.stack.DbContext.EventRecordsWithGuidIds.Count(c => c.AggregateId == nascentQuoteAggregate.Id);
            var afterQuoteDocumentCount = this.stack.DbContext.QuoteDocuments.Where(x => x.PolicyId == nascentQuoteAggregate.Id).Count();
            var afterQuoteCount = this.stack.DbContext.QuoteReadModels.Where(x => x.AggregateId == nascentQuoteAggregate.Id).Count();
            var afterCustomerCount = this.stack.DbContext.CustomerReadModels.Where(x => x.TenantId == tenant.Id).Count();

            afterQuoteVersionCount.Should().Be(0);
            afterQuoteDocumentCount.Should().Be(0);
            afterEventCount.Should().Be(0);
            afterQuoteCount.Should().Be(0);
            beforeCustomerCount.Should().Be(afterCustomerCount);
        }

        [Fact]
        public async Task DeleteAllTests_DoesNotDeleteQuotes_WhenQuotesAreNotInNascentState()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var tenantProductModel = await this.CreateProductAndTenant(productId);
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;

            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            UBind.Domain.Aggregates.Organisation.Organisation organisation
                = await this.CreateAndGetOrganisationForTenant(tenant);
            UserAggregate user = await this.CreateUserModelForTenant(tenant);

            // Count before data was added
            var beforeQuoteVersionCount = this.stack.DbContext.QuoteVersions.Where(x => x.TenantId == tenant.Id).Count();
            var beforeEventCount = this.stack.DbContext.EventRecordsWithGuidIds.Count();
            var beforeQuoteDocumentCount = this.stack.DbContext.QuoteDocuments.Where(x => x.TenantId == tenant.Id).Count();
            var beforeClaimCount = this.stack.DbContext.ClaimReadModels.Where(x => x.TenantId == tenant.Id).Count();
            var beforePolicyCount = this.stack.DbContext.Policies.Where(x => x.TenantId == tenant.Id).Count();
            var beforeQuoteCount = this.stack.DbContext.QuoteReadModels.Where(x => x.TenantId == tenant.Id).Count();
            var beforeCustomerCount = this.stack.DbContext.CustomerReadModels.Where(x => x.TenantId == tenant.Id).Count();

            QuoteAggregate quoteAggregate = QuoteFactory.CreateNewPolicy(tenant.Id, product.Id);
            var quote = quoteAggregate.Policy.CreateAdjustmentQuote(
                this.Clock.Now(),
                "Foo",
                Enumerable.Empty<IClaimReadModel>(),
                this.performingUserId,
                this.quoteWorkflow,
                QuoteExpirySettings.Default,
                false,
                Guid.NewGuid());
            quoteAggregate
                .WithFormData(quote.Id)
                .WithCalculationResult(quote.Id);

            var customerPerson = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, this.Clock.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = user.LoginEmail,
            };

            customerPerson.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);

            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                user.Id,
                null);

            quoteAggregate.RecordAssociationWithCustomer(customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, this.Clock.Now());
            quoteAggregate.UpdateCustomerDetails(customerPerson, this.performingUserId, this.Clock.Now(), quoteAggregate.Policy.QuoteId.GetValueOrDefault());
            quoteAggregate.AttachQuoteDocument(
                new QuoteDocument(Guid.NewGuid().ToString(), "Pdf", 3, Guid.NewGuid(), this.Clock.Now()),
                0,
                this.performingUserId,
                this.Clock.Now());
            quoteAggregate = quoteAggregate.WithQuoteVersion(quote.Id);
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);

            EmailModel emailModel = this.GenerateEmailModel();

            var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                emailModel,
                quoteAggregate.Id,
                quoteAggregate.Policy.QuoteId.GetValueOrDefault(),
                default,
                Guid.NewGuid(),
                quoteAggregate.CustomerId,
                Guid.NewGuid(),
                EmailType.Admin,
                this.stack.Clock.Now(),
                this.stack.FileContentRepository);
            this.stack.EmailService.InsertEmailAndMetadata(metadata);

            var incidentDate = new LocalDate(2018, 11, 20);
            var claimAggregate = ClaimAggregate.CreateForPolicy(
                "AAAAAA",
                quoteAggregate,
                Guid.NewGuid(),
                "John Smith",
                "Jonboy",
                this.performingUserId,
                SystemClock.Instance.Now());
            await this.stack.ClaimAggregateRepository.Save(claimAggregate);

            this.stack.QuoteVersionReadModelUpdateRepository.Add(new QuoteVersionReadModel
            {
                Id = Guid.NewGuid(),
                QuoteVersionNumber = 1,
                QuoteId = quoteAggregate.Id,
            });
            this.stack.DbContext.SaveChanges();

            // Act
            this.stack.QuoteReductor.DeleteNascent(this.stack.ProgressLogger, 100, -1, Duration.FromSeconds(0));

            var afterQuoteVersionCount = this.stack.DbContext.QuoteVersions.Count();
            var afterQuoteDocumentCount = this.stack.DbContext.QuoteDocuments.Count();
            var afterEventCount = this.stack.DbContext.EventRecordsWithGuidIds.Count();
            var afterClaimCount = this.stack.DbContext.ClaimReadModels.Count();
            var afterQuoteCount = this.stack.DbContext.QuoteReadModels.Count();
            var afterPolicyCount = this.stack.DbContext.Policies.Count();
            var afterCustomerCount = this.stack.DbContext.CustomerReadModels.Count();

            beforeQuoteVersionCount.Should().NotBe(afterQuoteVersionCount);
            beforeQuoteDocumentCount.Should().NotBe(afterQuoteDocumentCount);
            beforeEventCount.Should().NotBe(afterEventCount);
            beforeClaimCount.Should().NotBe(afterClaimCount);
            beforeQuoteCount.Should().NotBe(afterQuoteCount);
            beforePolicyCount.Should().NotBe(afterPolicyCount);
            beforeCustomerCount.Should().NotBe(afterCustomerCount);
        }

        private async Task<UserAggregate> CreateUserModelForTenant(Tenant tenant)
        {
            var testEmail = "test-quoter-deleter@ubind.io";
            var testNumber = "0412345678";

            var userSignupModel = new UserSignupModel()
            {
                AlternativeEmail = testEmail,
                WorkPhoneNumber = testNumber,
                Email = testEmail,
                Environment = DeploymentEnvironment.Staging,
                FullName = "test",
                HomePhoneNumber = testNumber,
                MobilePhoneNumber = testNumber,
                PreferredName = "TEST",
                UserType = UserType.Client,
                TenantId = tenant.Id,
                OrganisationId = tenant.Details.DefaultOrganisationId,
            };

            return await this.stack.UserService.CreateUser(userSignupModel);
        }

        private async Task<TenantProductModel> CreateProductAndTenant(
            Guid productId)
        {
            var guid = Guid.NewGuid();
            var tenantAlias = "test-tenant-" + guid;
            var tenantName = "Test Tenant " + guid;
            var tenantId = await this.stack.Mediator.Send(new CreateTenantCommand(tenantName, tenantAlias, null));
            var tenant = await this.stack.Mediator.Send(new GetTenantByIdQuery(tenantId));

            Product product = ProductFactory.Create(tenantId, productId);
            this.stack.ProductRepository.Insert(product);
            this.stack.ProductRepository.SaveChanges();

            return new TenantProductModel
            {
                Tenant = tenant,
                Product = product,
            };
        }

        private async Task<UBind.Domain.Aggregates.Organisation.Organisation> CreateAndGetOrganisationForTenant(
            Tenant tenant)
        {
            var organisation = UBind.Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            tenant.SetDefaultOrganisation(
                organisation.Id, this.stack.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            return organisation;
        }

        private EmailModel GenerateEmailModel()
        {
            return new EmailModel(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, "from", "to", "subject", "plainText", "html", "cc", "bcc");
        }

        private class TenantProductModel
        {
#pragma warning disable SA1401 // Fields should be private
            public Tenant Tenant;
            public Product Product;
#pragma warning restore SA1401 // Fields should be private
        }
    }
}
