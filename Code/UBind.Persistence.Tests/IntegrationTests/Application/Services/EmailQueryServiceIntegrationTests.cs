// <copyright file="EmailQueryServiceIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests.Application.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.Tenant;
    using UBind.Application.Queries.Tenant;
    using UBind.Application.Services;
    using UBind.Application.Services.SystemEmail;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Extensions;
    using UBind.Domain.Extensions.Implementations;
    using UBind.Domain.Factory;
    using UBind.Domain.Quote;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel.Email;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;

    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class EmailQueryServiceIntegrationTests
    {
        private ApplicationStack stack;
        private Guid? performingUserId = Guid.NewGuid();
        private Guid tenantId;
        private Guid productId;

        public EmailQueryServiceIntegrationTests()
        {
            this.stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            this.productId = Guid.NewGuid();
            this.tenantId = Guid.NewGuid();
        }

        [Fact]
        public void GetAll_CallsPaginate()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.Now());

            var emailQueryService =
                new EmailQueryService(this.stack.EmailRepository);

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Admin", "Customer", "User" },
                Sources = new string[] { "Policy", "Quote", "RenewalInvitation" },
                OrganisationIds = new List<Guid> { organisation.Id },
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            var mockIQueryableExtensions = new Mock<IQueryableExtensionsImplementation>();

            IQueryableExtensions.Implementation = mockIQueryableExtensions.Object;

            // Act
            var results = emailQueryService.GetAll(
                tenant.Id, filters);

            // Assert
            mockIQueryableExtensions.Verify(m => m.Paginate(It.IsAny<IQueryable<object>>(), filters));
            IQueryableExtensions.Implementation = new QueryableExtensionsImplementation();
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task GetEmailsForCustomer_ShouldIncludeEmailsFromSubOrganisations_WhenUserIsFromDefaultOrganisation()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(
                organisation.Id,
                this.stack.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            var anotherOrganisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(anotherOrganisation);

            var environment = DeploymentEnvironment.Staging;
            Guid customerId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            // Create emails for default organisation
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new EmailModel(
                    tenant.Id,
                    organisation.Id,
                    product.Id,
                    environment,
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text",
                    "HTML body");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                    emailModel, customerId, personId, organisation.Id, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            // Create emails for sub-organisation
            var emailModel1 = new EmailModel(
                tenant.Id,
                organisation.Id,
                product.Id,
                environment,
                "from@example.com",
                "to@example.com",
                "my subject",
                "Plain text",
                "HTML body");

            var metadata1 = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                emailModel1, customerId, personId, organisation.Id, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
            this.stack.EmailService.InsertEmailAndMetadata(metadata1);

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                CustomerId = customerId,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService
                .GetEmailsForCustomer(tenant.Id, customerId, filters);

            // Assert
            emails.Should().HaveCount(6)
                .And.Contain(email => email.OrganisationId == organisation.Id)
                .And.Contain(email => email.OrganisationId == anotherOrganisation.Id);
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task GetEmailsForCustomer_ShouldIncludeEmailsForSpecificOrganisation_WhenUserIsNotFromDefaultOrganisation()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var anotherOrganisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(anotherOrganisation);

            var environment = DeploymentEnvironment.Staging;
            Guid customerId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            // Create emails for default organisation
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new EmailModel(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    product.Id,
                    environment,
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text",
                    "HTML body");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                    emailModel, customerId, personId, tenant.Details.DefaultOrganisationId, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            // Create emails for sub-organisation
            var emailModel1 = new EmailModel(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                product.Id,
                environment,
                "from@example.com",
                "to@example.com",
                "my subject",
                "Plain text",
                "HTML body");

            var metadata1 = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                emailModel1, customerId, personId, tenant.Details.DefaultOrganisationId, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
            this.stack.EmailService.InsertEmailAndMetadata(metadata1);

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                CustomerId = customerId,
                OrganisationIds = new List<Guid> { anotherOrganisation.Id },
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService
                .GetEmailsForCustomer(tenant.Id, customerId, filters);

            // Assert
            emails.Should().HaveCount(1).And.Contain(email => email.OrganisationId == anotherOrganisation.Id);
        }

        [Fact]
        public async Task GetAll_HasRecords_AsClientAdminFilteredByAdmin()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.CreateTenant(tenant);
            this.stack.CreateProduct(product);
            QuoteAggregate quoteAggregate = QuoteFactory.CreateNewBusinessQuote(tenant.Id).Aggregate;
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    quoteAggregate.TenantId,
                    quoteAggregate.OrganisationId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");
                var organisationId = Guid.NewGuid();
                var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                    emailModel,
                    quoteAggregate.Id,
                    quoteAggregate.Id,
                    default,
                    organisationId,
                    quoteAggregate.CustomerId,
                    Guid.NewGuid(),
                    EmailType.Admin,
                    this.stack.Clock.Now(),
                    this.stack.FileContentRepository);
                metadata.CreateRelationshipFromEntityToEmail(EntityType.Organisation, organisationId, RelationshipType.OrganisationMessage);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Admin" },
                Sources = new string[] { "Policy", "Quote" },
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetAll(
                tenant.Id, filters);

            // Assert
            emails.Count().Should().Be(5);
        }

        [Fact]
        public async Task GetAll_ShouldContainRecords_AsClientAdminFilteredByCustomer()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.CreateTenant(tenant);
            this.stack.CreateProduct(product);
            QuoteAggregate quoteAggregate = QuoteFactory.CreateNewBusinessQuote(tenant.Id).Aggregate;
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            var customerId = Guid.NewGuid();
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    quoteAggregate.TenantId,
                    quoteAggregate.OrganisationId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment,
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text",
                    "Admin HTML Body");

                var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                    emailModel,
                    quoteAggregate.Id,
                    quoteAggregate.Id,
                    default,
                    Guid.NewGuid(),
                    customerId,
                    Guid.NewGuid(),
                    EmailType.Admin,
                    this.stack.Clock.Now(),
                    this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = new string[] { "Policy", "Quote" },
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetAll(
                tenant.Id, filters);

            // Assert
            emails.Should().HaveCount(5);
        }

        [Fact]
        public async Task GetAll_HasRecords_AsClientAdminFilteredByTags()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.CreateTenant(tenant);
            this.stack.CreateProduct(product);
            QuoteAggregate quoteAggregate = QuoteFactory.CreateNewBusinessQuote(tenant.Id).Aggregate;
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    quoteAggregate.TenantId,
                    quoteAggregate.OrganisationId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");
                var organisationId = Guid.NewGuid();
                var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                    emailModel,
                    quoteAggregate.Id,
                    quoteAggregate.Id,
                    default,
                    organisationId,
                    quoteAggregate.CustomerId,
                    Guid.NewGuid(),
                    EmailType.Admin,
                    this.stack.Clock.Now(),
                    this.stack.FileContentRepository);
                metadata.CreateRelationshipFromEntityToEmail(fromEntityType: EntityType.Organisation, organisationId, RelationshipType.OrganisationMessage);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Admin" },
                Sources = new string[] { "Policy", "Quote" },
                Tags = new string[] { "Quote" },
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetAll(
                tenant.Id, filters);

            // Assert
            emails.Count().Should().Be(5);
        }

        [Fact]
        public async Task GetAll_HasNoRecords_AsCustomerFilteredByAdmin()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            var emailType = EmailType.Customer;
            var customerId = Guid.NewGuid();
            this.stack.CreateTenant(tenant);
            this.stack.CreateProduct(product);

            QuoteAggregate quoteAggregate = QuoteFactory.CreateNewBusinessQuote(tenant.Id).Aggregate;
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    quoteAggregate.TenantId,
                    quoteAggregate.OrganisationId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment,
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text",
                    "html body");

                var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                    emailModel,
                    quoteAggregate.Id,
                    quoteAggregate.Id,
                    default,
                    Guid.NewGuid(),
                    customerId,
                    Guid.NewGuid(),
                    emailType,
                    this.stack.Clock.Now(),
                    this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Admin" },
                Sources = new string[] { "Policy", "Quote" },
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                CustomerId = customerId,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetAll(
                tenant.Id, filters);

            // Assert
            emails.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAll_HasRecords_AsCustomerFilteredByCustomer()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            var emailType = EmailType.Customer;
            var customerId = Guid.NewGuid();
            this.stack.CreateTenant(tenant);
            this.stack.CreateProduct(product);
            QuoteAggregate quoteAggregate = QuoteFactory.CreateNewBusinessQuote(tenant.Id).Aggregate;
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    quoteAggregate.TenantId,
                    quoteAggregate.OrganisationId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment,
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text",
                    "html body");

                var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                    emailModel,
                    quoteAggregate.Id,
                    quoteAggregate.Id,
                    default,
                    Guid.NewGuid(),
                    customerId,
                    Guid.NewGuid(),
                    emailType,
                    this.stack.Clock.Now(),
                    this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = new string[] { "Policy", "Quote" },
                CustomerId = customerId,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetAll(
                tenant.Id, filters);

            // Assert
            emails.Count().Should().Be(5);
        }

        [Fact]
        public async Task GetAll_HasRecords_AsCustomerFilteredByTags()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            var emailType = EmailType.Customer;
            var customerId = Guid.NewGuid();
            this.stack.CreateTenant(tenant);
            this.stack.CreateProduct(product);
            QuoteAggregate quoteAggregate = QuoteFactory.CreateNewBusinessQuote(tenant.Id).Aggregate;
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    quoteAggregate.TenantId,
                    quoteAggregate.OrganisationId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment,
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text",
                    "html body");

                var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                    emailModel,
                    quoteAggregate.Id,
                    quoteAggregate.Id,
                    default,
                    Guid.NewGuid(),
                    customerId,
                    Guid.NewGuid(),
                    emailType,
                    this.stack.Clock.Now(),
                    this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = new string[] { "Policy", "Quote" },
                CustomerId = customerId,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetAll(
                tenant.Id, filters);

            // Assert
            emails.Count().Should().Be(5);
        }

        [Fact]
        public async Task GetAll_HasRecords_AsAgentGetCustomersRecords()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var emailType = EmailType.Customer;
            var userType = UserType.Client;
            var emailAddress = "tester" + Guid.NewGuid() + "@ubind.io";
            var ownerUser = await this.CreateUserModelForTenant(userType, emailAddress, tenant);

            // create user.
            var emailAddress2 = "tester" + Guid.NewGuid() + "@ubind.io";
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var customerPerson = PersonAggregate.CreatePerson(
                tenant.Id, defaultOrganisationId, this.performingUserId, SystemClock.Instance.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = emailAddress2,
            };

            customerPerson.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);

            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                ownerUser.Id,
                null);

            var userCustomer = await this.stack.UserService.CreateUserForPerson(customerPerson, customerAggregate);

            QuoteAggregate quoteAggregate = QuoteFactory.CreateNewBusinessQuote(tenant.Id).Aggregate;
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    quoteAggregate.TenantId,
                    quoteAggregate.OrganisationId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment,
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text",
                    "html body");

                var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                    emailModel,
                    quoteAggregate.Id,
                    quoteAggregate.Id,
                    default,
                    userCustomer.Id,
                    customerAggregate.Id,
                    Guid.NewGuid(),
                    emailType,
                    this.stack.Clock.Now(),
                    this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = new string[] { "Policy", "Quote" },
                CustomerId = customerAggregate.Id,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetAll(
                tenant.Id, filters);

            // Assert
            emails.Count().Should().Be(5);
        }

        [Fact]
        public async Task GetAll_HasNoRecords_AsAgentGetCustomersRecordsOfCustomersNotOwnedByAgent()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var emailType = EmailType.Customer;
            var userType = UserType.Client;
            var emailAddress = "tester" + Guid.NewGuid() + "@ubind.io";
            var ownerUser = await this.CreateUserModelForTenant(userType, emailAddress, tenant);

            // create user.
            var emailAddress2 = "tester" + Guid.NewGuid() + "@ubind.io";
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var customerPerson = PersonAggregate.CreatePerson(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                this.performingUserId,
                SystemClock.Instance.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = emailAddress2,
            };

            customerPerson.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);
            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                ownerUser.Id,
                null);

            var userCustomer = await this.stack.UserService.CreateUserForPerson(customerPerson, customerAggregate);

            QuoteAggregate quoteAggregate = QuoteFactory.CreateNewBusinessQuote(tenant.Id).Aggregate;
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    quoteAggregate.TenantId,
                    quoteAggregate.OrganisationId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment,
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text",
                    "html body");

                var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                    emailModel,
                    quoteAggregate.Id,
                    quoteAggregate.Id,
                    default,
                    userCustomer.Id,
                    Guid.NewGuid(),
                    customerAggregate.Id,
                    emailType,
                    this.stack.Clock.Now(),
                    this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = new string[] { "Policy", "Quote" },
                CustomerId = Guid.NewGuid(),
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetAll(
                tenant.Id, filters);

            // Assert
            emails.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAll_HasRecords_AsAgentGetAgentsRecords()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var emailType = EmailType.Customer;
            var userType = UserType.Client;
            var emailAddress = "tester" + Guid.NewGuid() + "@ubind.io";
            var ownerUser = await this.CreateUserModelForTenant(userType, emailAddress, tenant);

            // create user.
            var emailAddress2 = "tester" + Guid.NewGuid() + "@ubind.io";
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var customerPerson = PersonAggregate.CreatePerson(
                tenant.Id, defaultOrganisationId, this.performingUserId, SystemClock.Instance.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = emailAddress2,
            };

            customerPerson.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);
            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                ownerUser.Id,
                null);

            var userCustomer = await this.stack.UserService.CreateUserForPerson(customerPerson, customerAggregate);

            QuoteAggregate quoteAggregate = QuoteFactory.CreateNewBusinessQuote(tenant.Id).Aggregate;
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    quoteAggregate.TenantId,
                    quoteAggregate.OrganisationId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment,
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text",
                    "html body");

                var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                    emailModel,
                    quoteAggregate.Id,
                    quoteAggregate.Id,
                    default,
                    ownerUser.Id,
                    customerAggregate.Id,
                    Guid.NewGuid(),
                    emailType,
                    this.stack.Clock.Now(),
                    this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = new string[] { "Policy", "Quote" },
                CustomerId = customerAggregate.Id,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetAll(
                tenant.Id, filters);

            // Assert
            emails.Count().Should().Be(5);
        }

        [Fact]
        public async Task GetAll_HasNoRecords_AsAgentGetRecordsNotOwned()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var emailType = EmailType.Customer;
            var userType = UserType.Client;
            var emailAddress = "tester" + Guid.NewGuid() + "@ubind.io";
            var ownerUser = await this.CreateUserModelForTenant(userType, emailAddress, tenant);

            // create user.
            var emailAddress2 = "tester" + Guid.NewGuid() + "@ubind.io";
            var customerPerson = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, SystemClock.Instance.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = emailAddress2,
            };
            customerPerson.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);
            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                ownerUser.Id,
                null);

            var userCustomer = await this.stack.UserService.CreateUserForPerson(customerPerson, customerAggregate);

            QuoteAggregate quoteAggregate = QuoteFactory.CreateNewBusinessQuote(tenant.Id).Aggregate;
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    quoteAggregate.TenantId,
                    quoteAggregate.OrganisationId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment,
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text",
                    "html body");

                var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                    emailModel,
                    quoteAggregate.Id,
                    quoteAggregate.Id,
                    default,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    emailType,
                    this.stack.Clock.Now(),
                    this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = new string[] { "Policy", "Quote" },
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                OwnerUserId = ownerUser.Id,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetAll(tenant.Id, filters);

            // Assert
            emails.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAll_HasRecords_AsCustomerGetsOwnRecords()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            var emailType = EmailType.Customer;
            var customerId = Guid.NewGuid();
            this.stack.CreateTenant(tenant);
            this.stack.CreateProduct(product);
            QuoteAggregate quoteAggregate = QuoteFactory.CreateNewBusinessQuote(tenant.Id).Aggregate;
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    quoteAggregate.TenantId,
                    quoteAggregate.OrganisationId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment,
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text",
                    "html body");

                var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                    emailModel,
                    quoteAggregate.Id,
                    quoteAggregate.Id,
                    default,
                    Guid.NewGuid(),
                    customerId,
                    Guid.NewGuid(),
                    emailType,
                    this.stack.Clock.Now(),
                    this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = new string[] { "Policy", "Quote" },
                CustomerId = customerId,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetAll(
                tenant.Id, filters);

            // Assert
            emails.Count().Should().Be(5);
        }

        [Fact]
        public async Task GetByCustomerId_NoRecords_AsAdminFilterByAdmin()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();
            var environment = DeploymentEnvironment.Staging;
            Guid customerId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(
                organisation.Id,
                this.stack.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            await this.stack.OrganisationAggregateRepository.Save(organisation);
            this.stack.TenantRepository.SaveChanges();

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    tenant.Id,
                    organisation.Id,
                    product.Id,
                    environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                    emailModel, customerId, personId, organisation.Id, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Admin" },
                Sources = new string[] { "Policy", "Quote" },
                Tags = new string[] { "Quote" },
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetEmailsForCustomer(
                tenant.Id, customerId, filters);

            // Assert
            emails.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByCustomerId_HasRecords_AsAdminFilterByCustomer()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(
                organisation.Id,
                this.stack.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            var environment = DeploymentEnvironment.Staging;
            Guid customerId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new EmailModel(
                    tenant.Id,
                    organisation.Id,
                    product.Id,
                    environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                    emailModel, customerId, personId, organisation.Id, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetAll(
                tenant.Id, filters);

            // Assert
            emails.Count().Should().Be(5);
        }

        [Fact]
        public async Task GetByCustomerId_NoRecords_AsAdminFilterByUser()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();
            var environment = DeploymentEnvironment.Staging;
            Guid customerId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(
                organisation.Id,
                this.stack.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    tenant.Id,
                    organisation.Id,
                    product.Id,
                    environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                    emailModel, customerId, personId, organisation.Id, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "User" },
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetEmailsForCustomer(
                tenant.Id, customerId, filters);

            // Assert
            emails.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByCustomerId_NoRecords_AsCustomerFilterByAdmin()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();
            var environment = DeploymentEnvironment.Staging;
            Guid customerId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(
                organisation.Id,
                this.stack.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    tenant.Id,
                    organisation.Id,
                    product.Id,
                    environment,
                    "Customer",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                    emailModel, customerId, personId, organisation.Id, EmailType.Customer, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Admin" },
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                CustomerId = customerId,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetEmailsForCustomer(
                tenant.Id, customerId, filters);

            // Assert
            emails.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByCustomerId_HasRecords_AsCustomerFilterByCustomer()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();
            var environment = DeploymentEnvironment.Staging;
            Guid customerId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(
                organisation.Id,
                this.stack.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    tenant.Id,
                    organisation.Id,
                    product.Id,
                    environment,
                    "Customer",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                    emailModel, customerId, personId, organisation.Id, EmailType.Customer, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                CustomerId = customerId,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetEmailsForCustomer(
                tenant.Id, customerId, filters);

            // Assert
            emails.Count().Should().Be(5);
        }

        [Fact]
        public async Task GetByCustomerId_NoRecords_AsCustomerRetrieveAdminEmails()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();
            var environment = DeploymentEnvironment.Staging;
            Guid customerId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(
                organisation.Id,
                this.stack.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    tenant.Id,
                    organisation.Id,
                    product.Id,
                    environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                    emailModel, customerId, personId, organisation.Id, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                CustomerId = customerId,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetEmailsForCustomer(
                tenant.Id, customerId, filters);

            // Assert
            emails.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByCustomerId_NoRecords_AsCustomerFilterByUser()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();
            var environment = DeploymentEnvironment.Staging;
            Guid customerId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(
                organisation.Id,
                this.stack.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    tenant.Id,
                    organisation.Id,
                    product.Id,
                    environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                    emailModel, customerId, personId, organisation.Id, EmailType.Customer, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "User" },
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                CustomerId = customerId,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetEmailsForCustomer(
                tenant.Id, customerId, filters);

            // Assert
            emails.Should().BeEmpty();
        }

        [Fact]
        public void GetByUserId_NoRecords_AsAdminFilterByAdmin()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();
            var environment = DeploymentEnvironment.Staging;
            Guid userId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    product.Id,
                    environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                    emailModel, userId, personId, tenant.Details.DefaultOrganisationId, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Admin" },
                Sources = new string[] { "Policy", "Quote" },
                Tags = new string[] { "Quote" },
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetEmailsForUser(
                tenant.Id, userId, filters);

            // Assert
            emails.Should().BeEmpty();
        }

        [Fact]
        public void GetByUserId_ShouldContainRecords_AsAdminFilterByCustomer()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();
            var environment = DeploymentEnvironment.Staging;
            Guid userId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    product.Id,
                    environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                    emailModel, userId, personId, tenant.Details.DefaultOrganisationId, EmailType.Customer, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = Array.Empty<string>(),
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetEmailsForUser(
                tenant.Id, userId, filters);

            // Assert
            emails.Should().HaveCount(5);
        }

        [Fact]
        public void GetByUserId_HasRecords_AsAdminFilterByUser()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();
            var environment = DeploymentEnvironment.Staging;
            Guid userId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    product.Id,
                    environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                    emailModel, userId, personId, tenant.Details.DefaultOrganisationId, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "User" },
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetEmailsForUser(
                tenant.Id, userId, filters);

            // Assert
            emails.Count().Should().Be(5);
        }

        [Fact]
        public void GetByUserId_NoRecords_AsCustomerFilterByAdmin()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();
            var environment = DeploymentEnvironment.Staging;
            Guid userId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    product.Id,
                    environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                    emailModel, userId, personId, tenant.Details.DefaultOrganisationId, EmailType.Customer, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Admin" },
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                CustomerId = userId,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetEmailsForUser(
                tenant.Id, userId, filters);

            // Assert
            emails.Should().BeEmpty();
        }

        [Fact]
        public void GetByUserId_NoRecords_AsCustomerFilterByCustomer()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();
            var environment = DeploymentEnvironment.Staging;
            Guid userId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new Domain.Quote.EmailModel(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    product.Id,
                    environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                    emailModel, userId, personId, tenant.Details.DefaultOrganisationId, EmailType.Customer, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "Customer" },
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                CustomerId = userId,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetEmailsForUser(
                tenant.Id, userId, filters);

            // Assert
            emails.Should().BeEmpty();
        }

        [Fact]
        public void GetByUserId_NoRecords_AsCustomerFilterByUser()
        {
            // Arrange
            var tenant = TenantFactory.Create(this.tenantId);
            var product = ProductFactory.Create(this.tenantId, this.productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.ProductRepository.Insert(product);
            this.stack.TenantRepository.SaveChanges();
            var environment = DeploymentEnvironment.Staging;
            Guid userId = Guid.NewGuid();
            Guid personId = Guid.NewGuid();

            for (var i = 0; i < 5; ++i)
            {
                var emailModel = new EmailModel(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    product.Id,
                    environment,
                    "Admin",
                    "from@example.com",
                    "to@example.com",
                    "my subject",
                    "Plain text");

                var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                    emailModel, userId, personId, tenant.Details.DefaultOrganisationId, EmailType.Customer, this.stack.Clock.Now(), this.stack.FileContentRepository);
                this.stack.EmailService.InsertEmailAndMetadata(metadata);
            }

            var filters = new EntityListFilters
            {
                SearchTerms = new string[] { "example", "com" },
                Statuses = new string[] { "User" },
                Sources = Array.Empty<string>(),
                Tags = Array.Empty<string>(),
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                CustomerId = userId,
                SortBy = $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}",
                SortOrder = Domain.Enums.SortDirection.Descending,
            }
            .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
            .WithDateIsBeforeFilter("2040-01-01".ToLocalDateFromIso8601());

            // Act
            var emails = this.stack.EmailQueryService.GetEmailsForUser(
                tenant.Id, userId, filters);

            // Assert
            emails.Should().BeEmpty();
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task GetDetails_IsSuccessfulRetrieve_IfUserIsUbindAdmin()
        {
            // Arrange
            var userType = UserType.Master;
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var emailModel = new EmailModel(
                tenant.Id,
                Guid.NewGuid(),
                product.Id,
                DeploymentEnvironment.Staging,
                "test",
                "test",
                "test",
                "test",
                "test");

            var emailAddress = "tester" + Guid.NewGuid() + "@ubind.io";
            var ownerUser = await this.CreateUserModelForTenant(userType, emailAddress, tenant);

            // create user.
            var emailAddress2 = "tester" + Guid.NewGuid() + "@ubind.io";
            var customerPerson = PersonAggregate.CreatePerson(
                 tenant.Id,
                 tenant.Details.DefaultOrganisationId,
                 this.performingUserId,
                 SystemClock.Instance.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = emailAddress2,
            };
            customerPerson.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);
            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                ownerUser.Id,
                null);

            var userCustomer = await this.stack.UserService.CreateUserForPerson(customerPerson, customerAggregate);
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, userType, userCustomer.Id, userCustomer.CustomerId);

            var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                emailModel, userCustomer.Id, customerPerson.Id, tenant.Details.DefaultOrganisationId, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
            this.stack.EmailService.InsertEmailAndMetadata(metadata);

            // Act
            var emailRecord = this.stack.EmailQueryService.GetDetails(tenant.Id, emailModel.Id);

            // Assert
            emailRecord.Should().NotBeNull();
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task GetDetails_IsSuccessRetrieve_IfUserIsClientAdminAndSameTenant()
        {
            // Arrange
            var userType = UserType.Client;
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var emailModel = new EmailModel(
                 tenant.Id,
                 Guid.NewGuid(),
                 product.Id,
                 DeploymentEnvironment.Staging,
                 "test",
                 "test",
                 "test",
                 "test",
                 "test");

            var emailAddress = "tester" + Guid.NewGuid() + "@ubind.io";
            var ownerUser = await this.CreateUserModelForTenant(userType, emailAddress, tenant);

            // create user.
            var emailAddress2 = "tester" + Guid.NewGuid() + "@ubind.io";
            var customerPerson = PersonAggregate.CreatePerson(
                 tenant.Id,
                 tenant.Details.DefaultOrganisationId,
                 this.performingUserId,
                 SystemClock.Instance.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = emailAddress2,
            };

            customerPerson.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);
            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                ownerUser.Id,
                null);

            var userCustomer = await this.stack.UserService.CreateUserForPerson(customerPerson, customerAggregate);
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, userType, userCustomer.Id, userCustomer.CustomerId);

            var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                emailModel, userCustomer.Id, customerPerson.Id, tenant.Details.DefaultOrganisationId, EmailType.User, this.stack.Clock.Now(), this.stack.FileContentRepository);
            this.stack.EmailService.InsertEmailAndMetadata(metadata);

            // Act
            var emailRecord = this.stack.EmailQueryService.GetDetails(tenant.Id, emailModel.Id);

            // Assert
            emailRecord.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDetails_IsNullRetrieve_IfUserIsClientAdminAndNotSameTenant()
        {
            // Arrange
            var userType = UserType.Client;
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var emailModel = new EmailModel(
                 tenant.Id,
                 Guid.NewGuid(),
                 product.Id,
                 null,
                 "test",
                 "test",
                 "test",
                 "test",
                 "test");

            var emailAddress = "tester" + Guid.NewGuid() + "@ubind.io";
            var ownerUser = await this.CreateUserModelForTenant(userType, emailAddress, tenant);

            // create user.
            var emailAddress2 = "tester" + Guid.NewGuid() + "@ubind.io";
            var customerPerson = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, SystemClock.Instance.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = emailAddress2,
            };
            customerPerson.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);
            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                ownerUser.Id,
                null);

            var userCustomer = await this.stack.UserService.CreateUserForPerson(customerPerson, customerAggregate);
            var emailAndMetadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                emailModel, userCustomer.Id, customerPerson.Id, tenant.Details.DefaultOrganisationId, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
            this.stack.EmailService.InsertEmailAndMetadata(emailAndMetadata);

            // Act
            var record = this.stack.EmailQueryService.GetDetails(Guid.NewGuid(), emailModel.Id);

            // Act + Assert
            record.Should().BeNull();
        }

        [Fact]
        public async Task GetDetails_IsSuccessRetrieve_IfUserIsAgentAndGetCustomersSystemEmails()
        {
            // Arrange
            var userType = UserType.Client;
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var emailModel = new EmailModel(
                 tenant.Id,
                 tenant.Details.DefaultOrganisationId,
                 product.Id,
                 DeploymentEnvironment.Staging,
                 "test",
                 "test",
                 "test",
                 "test",
                 "test");

            var emailAddress = "tester" + Guid.NewGuid() + "@ubind.io";
            var ownerUser = await this.CreateUserModelForTenant(userType, emailAddress, tenant);

            // create user.
            var emailAddress2 = "tester" + Guid.NewGuid() + "@ubind.io";
            var customerPerson = PersonAggregate.CreatePerson(
                 tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, SystemClock.Instance.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = emailAddress2,
            };

            customerPerson.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);
            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                ownerUser.Id,
                null);

            var userCustomer = await this.stack.UserService.CreateUserForPerson(customerPerson, customerAggregate);

            var emailAndMetadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                emailModel, userCustomer.Id, customerPerson.Id, tenant.Details.DefaultOrganisationId, EmailType.Admin, this.stack.Clock.Now(), this.stack.FileContentRepository);
            this.stack.EmailService.InsertEmailAndMetadata(emailAndMetadata);

            // Act
            var record = this.stack.EmailQueryService.GetDetails(tenant.Id, emailModel.Id);

            // Act + Assert
            record.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDetails_IsSuccessRetrieve_IfUserIsAgentAndGetCustomersIntegrationEmails()
        {
            // Arrange
            var userType = UserType.Client;
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var emailModel = new EmailModel(
                 tenant.Id,
                 tenant.Details.DefaultOrganisationId,
                 product.Id,
                 DeploymentEnvironment.Staging,
                 "test",
                 "test",
                 "test",
                 "test",
                 "test");

            var emailAddress = "tester" + Guid.NewGuid() + "@ubind.io";
            var ownerUser = await this.CreateUserModelForTenant(userType, emailAddress, tenant);

            // create user.
            var emailAddress2 = "tester" + Guid.NewGuid() + "@ubind.io";
            var customerPerson = PersonAggregate.CreatePerson(
                 tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, SystemClock.Instance.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = emailAddress2,
            };

            customerPerson.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);
            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                ownerUser.Id,
                null);

            var emailAndMetadata = IntegrationEmailMetadataFactory.CreateForPolicy(
                emailModel,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                default,
                Guid.NewGuid(),
                customerAggregate.Id,
                Guid.NewGuid(),
                default,
                EmailType.Admin,
                this.stack.Clock.Now(),
                this.stack.FileContentRepository);
            this.stack.EmailService.InsertEmailAndMetadata(emailAndMetadata);

            // Act
            var record = this.stack.EmailQueryService.GetDetails(tenant.Id, emailModel.Id);

            // Act + Assert
            record.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDetails_IsSuccessRetrieve_IfUserIsOwnerOfEmail()
        {
            // Arrange
            var userType = UserType.Customer;
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var emailModel = new EmailModel(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                product.Id,
                DeploymentEnvironment.Staging,
                "test",
                "test",
                "test",
                "test",
                "test");

            var emailAddress = "tester" + Guid.NewGuid() + "@ubind.io";
            var ownerUser = await this.CreateUserModelForTenant(UserType.Client, emailAddress, tenant);

            // create user.
            var emailAddress2 = "tester" + Guid.NewGuid() + "@ubind.io";
            var customerPerson = PersonAggregate.CreatePerson(
                 tenant.Id,
                 tenant.Details.DefaultOrganisationId,
                 this.performingUserId,
                 SystemClock.Instance.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);
            var personCommonProperties = new PersonCommonProperties
            {
                Email = emailAddress2,
            };

            customerPerson.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                ownerUser.Id,
                null);

            var userCustomer = await this.stack.UserService.CreateUserForPerson(customerPerson, customerAggregate);
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, userType, userCustomer.Id, userCustomer.CustomerId);

            var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                emailModel, userCustomer.Id, customerPerson.Id, tenant.Details.DefaultOrganisationId, EmailType.User, this.stack.Clock.Now(), this.stack.FileContentRepository);
            this.stack.EmailService.InsertEmailAndMetadata(metadata);

            // Act
            var emailRecord = this.stack.EmailQueryService.GetDetails(tenant.Id, emailModel.Id);

            // Assert
            emailRecord.Should().NotBeNull();
        }

        /// <summary>
        /// Note: this should be removed, such scenario is impossible. because the query would not retreive the email.
        /// </summary>
        /// <returns>Async task.</returns>
        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task GetDetails_ReturnsNull_IfUserIsNotOwnerOfEmail()
        {
            // Arrange
            var userType = UserType.Customer;
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var emailModel = new EmailModel(
                    tenant.Id,
                    Guid.NewGuid(),
                    product.Id,
                    null,
                    "test",
                    "test",
                    "test",
                    "test",
                    "test");

            var emailAddress = "tester" + Guid.NewGuid() + "@ubind.io";
            var ownerUser = await this.CreateUserModelForTenant(userType, emailAddress, tenant);

            // create user.
            var emailAddress2 = "tester" + Guid.NewGuid() + "@ubind.io";
            var customerPerson = PersonAggregate.CreatePerson(
                 tenant.Id,
                 tenant.Details.DefaultOrganisationId,
                 this.performingUserId,
                 SystemClock.Instance.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);
            var personCommonProperties = new PersonCommonProperties
            {
                Email = emailAddress2,
            };

            await this.stack.PersonAggregateRepository.Save(customerPerson);

            customerPerson.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                ownerUser.Id,
                null);

            var userCustomer = await this.stack.UserService.CreateUserForPerson(customerPerson, customerAggregate);
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var userAuthData = new UserAuthenticationData(
                tenant.Id, defaultOrganisationId, userType, userCustomer.Id, userCustomer.CustomerId);

            var otherUserAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, userType, userCustomer.Id, Guid.NewGuid());

            var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                emailModel, userCustomer.Id, customerPerson.Id, tenant.Details.DefaultOrganisationId, EmailType.User, this.stack.Clock.Now(), this.stack.FileContentRepository);
            this.stack.EmailService.InsertEmailAndMetadata(metadata);

            // Act
            var result = this.stack.EmailQueryService.GetDetails(tenant.Id, emailModel.Id);

            // Act + Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByQuoteId_IsSuccessRetrieve_IfUserIsOwnerOfEmail()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;

            var emailModel = new EmailModel(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                product.Id,
                DeploymentEnvironment.Staging,
                "test",
                "test",
                "test",
                "test",
                "test");

            var emailAddress = "tester" + Guid.NewGuid() + "@ubind.io";
            var ownerUser = await this.CreateUserModelForTenant(UserType.Client, emailAddress, tenant);

            // create user.
            var emailAddress2 = "tester" + Guid.NewGuid() + "@ubind.io";
            var customerPerson = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, SystemClock.Instance.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = emailAddress2,
            };

            customerPerson.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);
            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson,
                DeploymentEnvironment.Staging,
                ownerUser.Id,
                null);

            var userCustomer = await this.stack.UserService.CreateUserForPerson(customerPerson, customerAggregate);
            var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                emailModel,
                userCustomer.Id,
                customerPerson.Id,
                tenant.Details.DefaultOrganisationId,
                EmailType.User,
                this.stack.Clock.Now(),
                this.stack.FileContentRepository);
            this.stack.EmailService.InsertEmailAndMetadata(metadata);

            // Act
            var emailRecord = this.stack.EmailQueryService.GetDetails(tenant.Id, emailModel.Id);

            // Assert
            emailRecord.Should().NotBeNull();
        }

        private async Task CreateOrganisationForTenant(Tenant tenant)
        {
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            tenant.SetDefaultOrganisation(
                organisation.Id, this.stack.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();
        }

        private async Task<UserAggregate> CreateUserModelForTenant(UserType userType, string emailAddress, Tenant tenant)
        {
            var userSignupModel = new UserSignupModel()
            {
                AlternativeEmail = emailAddress,
                WorkPhoneNumber = "0412345678",
                Email = emailAddress,
                Environment = DeploymentEnvironment.Staging,
                FullName = "test",
                HomePhoneNumber = "0412345678",
                MobilePhoneNumber = "0412345678",
                PreferredName = "test",
                UserType = userType,
                TenantId = tenant.Id,
                OrganisationId = tenant.Details.DefaultOrganisationId,
            };

            return await this.stack.UserService.CreateUser(userSignupModel);
        }

        private async Task<TenantProductModel> CreateProductAndTenant(Guid productId)
        {
            var guid = Guid.NewGuid();
            var tenantAlias = "test-tenant-" + guid;
            var tenantName = "Test Tenant " + guid;
            var tenantId = await this.stack.Mediator.Send(new CreateTenantCommand(tenantName, tenantAlias, null));
            var tenant = await this.stack.Mediator.Send(new GetTenantByIdQuery(tenantId));
            var product = ProductFactory.Create(tenant.Id, productId);
            this.stack.CreateProduct(product);

            return new TenantProductModel
            {
                Tenant = tenant,
                Product = product,
            };
        }

        /// <summary>
        /// Tenant product model to return just for testing.
        /// </summary>
        private class TenantProductModel
        {
#pragma warning disable SA1401 // Fields should be private
            public Tenant Tenant;
            public Domain.Product.Product Product;
#pragma warning restore SA1401 // Fields should be private
        }
    }
}
