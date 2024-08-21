// <copyright file="FundingIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Application;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Commands.QuoteCalculation;
    using UBind.Application.Export.ViewModels;
    using UBind.Application.Person;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Releases;
    using UBind.Application.Tests;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Configuration;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBind.Domain.Funding;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class FundingIntegrationTests
    {
        private readonly IEmailInvitationConfiguration emailConfiguration = new Mock<IEmailInvitationConfiguration>().Object;
        private readonly Mock<IConfigurationService> configurationService = new Mock<IConfigurationService>();
        private readonly IProductConfiguration productConfiguration = new Mock<IProductConfiguration>().Object;
        private readonly Mock<ICqrsMediator> mediator = new Mock<ICqrsMediator>();
        private readonly Guid? performingUserId = Guid.NewGuid();
        private Mock<IPersonService> mockPersonService = new Mock<IPersonService>();
        private DisplayableFieldDto displayableFieldDto = new DisplayableFieldDto(new List<string>(), new List<string>(), true, true);
        private IFormDataPrettifier formDataPrettifier = new Mock<IFormDataPrettifier>().Object;

        private Guid tenantId;
        private Guid productId;
        private Guid organisationId;

        public FundingIntegrationTests()
        {
            this.productId = Guid.NewGuid();
            this.tenantId = Guid.NewGuid();
            this.organisationId = Guid.NewGuid();

            var customer = new CustomerReadModelDetail
            {
                Id = Guid.NewGuid(),
                PrimaryPersonId = Guid.NewGuid(),
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
                TenantId = this.tenantId,
                DisplayName = "Randy Walsh",
                OrganisationId = TenantFactory.DefaultId,
                OrganisationName = "Default Organisation",
            };

            this.configurationService.Setup(x => x.GetProductComponentConfiguration(
               It.IsAny<ReleaseContext>(), It.IsAny<WebFormAppType>()))
              .Returns(Task.FromResult(this.GetSampleProductComponentConfigurationForQuote()));

            this.mediator.Setup(s => s.Send(It.IsAny<GetCustomerByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(customer));
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        public async Task PremiumFundingDetailsAreExposedInApplicationEvents()
        {
            // Arrange
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var stack = new ApplicationStack(
                DatabaseFixture.TestConnectionStringName,
                ApplicationStackConfiguration.WithFundingService(FundingServiceName.PremiumFunding));
            var tenant = TenantFactory.Create(this.tenantId);
            stack.CreateTenant(tenant);

            var product = ProductFactory.Create(tenant.Id, this.productId);
            stack.CreateProduct(product);

            stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            stack.MockMediator.GetProductByIdOrAliasQuery(product);

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.Now().Plus(Duration.FromMinutes(1)));
            var organisationReadModelSummary = new OrganisationReadModelSummary
            {
                TenantId = organisation.TenantId,
                Id = organisation.Id,
                Alias = organisation.Alias,
                Name = organisation.Name,
                IsActive = organisation.IsActive,
                IsDeleted = organisation.IsDeleted,
                IsDefault = true,
                CreatedTimestamp = organisation.CreatedTimestamp,
                LastModifiedTimestamp = organisation.CreatedTimestamp,
            };

            var customerOwnerPerson = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, SystemClock.Instance.Now());
            await stack.PersonAggregateRepository.Save(customerOwnerPerson);

            var customerPerson = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, SystemClock.Instance.Now());
            await stack.PersonAggregateRepository.Save(customerPerson);

            var customer = CustomerAggregate.CreateNewCustomer(
                tenant.Id,
                customerPerson,
                DeploymentEnvironment.Staging,
                this.performingUserId,
                null,
                SystemClock.Instance.Now(),
                true);

            customer.AssignOwnership(customer.Id, customerOwnerPerson, this.performingUserId, SystemClock.Instance.Now());
            await stack.CustomerAggregateRepository.Save(customer);

            this.mockPersonService.Setup(s => s.GetByCustomerId(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(customerPerson);

            var quote = QuoteAggregate.CreateNewBusinessQuote(
                  tenant.Id,
                  tenant.Details.DefaultOrganisationId,
                  product.Id,
                  DeploymentEnvironment.Staging,
                  QuoteExpirySettings.Default,
                  this.performingUserId,
                  stack.Clock.Now(),
                  Guid.NewGuid(),
                  Timezones.AET);

            quote.Aggregate.RecordAssociationWithCustomer(customer, new FakePersonalDetails(), this.performingUserId, SystemClock.Instance.Now());

            await stack.QuoteAggregateRepository.Save(quote.Aggregate);
            var quoteAggregate = stack.QuoteAggregateRepository.GetById(tenant.Id, quote.Aggregate.Id);
            quote = quoteAggregate.GetQuoteOrThrow(quote.Id);
            var quoteCalculationCommand = new QuoteCalculationCommand(
                quote.ProductContext,
                quote.Id,
                null,
                quote.Type,
                quote.ProductReleaseId,
                JsonConvert.DeserializeObject<SpreadsheetCalculationDataModel>(this.GetSampleFormData()),
                null,
                true,
                true,
                quote.Aggregate.Id);

            await stack.Mediator.Send(quoteCalculationCommand);
            quoteAggregate = stack.QuoteAggregateRepository.GetById(tenant.Id, quote.Aggregate.Id);
            var quotes = quoteAggregate.GetQuoteOrThrow(quote.Id);
            var quoteId = quotes.Id;
            var fundingProposalId = quotes.LatestFundingProposalCreationResult.Proposal.InternalId;

            // Test credit card number is currently not acceptable for testing, we use bank account details instead.
            var bankAccountDetails = new BankAccountDetails("Foo", "123456", "123456");
            stack.Dispose();
            var stack2 = new ApplicationStack(
                DatabaseFixture.TestConnectionStringName,
                ApplicationStackConfiguration.WithFundingService(FundingServiceName.PremiumFunding));
            var releaseContext = new ReleaseContext(quote.ProductContext, quote.ProductReleaseId.Value);

            // Act
            await stack2.Mediator.Send(new AcceptFundingProposalCommand(releaseContext, quoteId, fundingProposalId, bankAccountDetails));

            // Assert
            stack2.Dispose();
            var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName, ApplicationStackConfiguration.WithFundingService(FundingServiceName.PremiumFunding));
            var readQuote = stack3.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
            var quoteRead = readQuote.GetQuoteOrThrow(quote.Id);
            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                ApplicationEventType.PaymentMade,
                readQuote,
                quoteRead.Id,
                0,
                "1",
                quoteRead.ProductReleaseId.Value);
            ApplicationEventViewModel viewModel = await ApplicationEventViewModel.Create(
                this.formDataPrettifier,
                applicationEvent,
                this.emailConfiguration,
                this.configurationService.Object,
                this.productConfiguration,
                this.displayableFieldDto,
                this.mockPersonService.Object,
                tenant,
                product,
                organisationReadModelSummary,
                SystemClock.Instance,
                this.mediator.Object);

            viewModel.Funding.Should().NotBeNull();
            stack3.Dispose();
        }

        [Fact]
        public async Task FundingProposalCreatedEventIsNotTriggeredOrPersistedWhenFundingServiceIsIQumulate()
        {
            // Arrange
            var stack = new ApplicationStack(
                DatabaseFixture.TestConnectionStringName,
                ApplicationStackConfiguration.WithFundingService(FundingServiceName.IQumulate));
            var tenant = TenantFactory.Create(this.tenantId);
            stack.CreateTenant(tenant);

            var product = ProductFactory.Create(tenant.Id, this.productId);
            stack.CreateProduct(product);

            stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            stack.MockMediator.GetProductByIdOrAliasQuery(product);

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.Now().Plus(Duration.FromMinutes(1)));
            var organisationReadModelSummary = new OrganisationReadModelSummary
            {
                TenantId = organisation.TenantId,
                Id = organisation.Id,
                Alias = organisation.Alias,
                Name = organisation.Name,
                IsActive = organisation.IsActive,
                IsDeleted = organisation.IsDeleted,
                IsDefault = true,
                CreatedTimestamp = organisation.CreatedTimestamp,
                LastModifiedTimestamp = organisation.CreatedTimestamp,
            };
            await stack.ProductOrganisationSettingRepository.UpdateProductSetting(
                tenant.Id, tenant.Details.DefaultOrganisationId, product.Id, true);
            stack.CreateRelease(new ReleaseContext(tenant.Id, product.Id, DeploymentEnvironment.Staging, Guid.NewGuid()));
            var quoteReadModel = await stack.Mediator.Send(new CreateNewBusinessQuoteCommand(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                null,
                product.Id,
                DeploymentEnvironment.Staging,
                true,
                null,
                null,
                null));

            // Act
            var quoteAggregate = stack.QuoteAggregateRepository.GetById(tenant.Id, quoteReadModel.AggregateId);
            var quote = quoteAggregate.GetQuoteOrThrow(quoteReadModel.Id);
            var quoteCalculationCommand = new QuoteCalculationCommand(
                quote.ProductContext,
                quote.Id,
                null,
                quote.Type,
                quote.ProductReleaseId,
                JsonConvert.DeserializeObject<SpreadsheetCalculationDataModel>(this.GetSampleFormData()),
                null,
                true,
                true,
                quote.Aggregate.Id);
            await stack.Mediator.Send(quoteCalculationCommand);
            quoteAggregate = stack.QuoteAggregateRepository.GetById(tenant.Id, quote.Aggregate.Id);

            // Get events that was triggered from quoteAggregate
            var events = quoteAggregate.GetUnstrippedEvents();
            var quotes = quoteAggregate.GetQuoteOrThrow(quote.Id);

            // Assert
            events.Where(x => x.ToString().Contains("FundingProposalCreatedEvent")).FirstOrDefault().Should().BeNull();
            quotes.LatestFundingProposalCreationResult.Should().BeNull();
        }

        private string GetSampleFormData()
        {
            var inceptionDate = SystemClock.Instance.Now().InZone(Timezones.AET).Date.PlusDays(1);
            var expiryDate = inceptionDate.PlusYears(1);
            return $@"{{
                    ""formModel"": {{
                        ""insuredName"": ""bar"",
                        ""inceptionDate"": ""{LocalDatePattern.Iso.Format(inceptionDate)}"",
                        ""expiryDate"": ""{LocalDatePattern.Iso.Format(expiryDate)}"",
                        ""contactAddressLine1"": ""Foo Street"",
                        ""contactAddressSuburb"": ""Melbourne"",
                        ""contactAddressState"": ""VIC"",
                        ""contactAddressPostcode"": ""3000"",
                        ""totalPremium"": ""3000""
                    }},
                ""questionAnswers"": [],
                ""repeatingQuestionAnswers"": []
                }}";
        }

        private IProductComponentConfiguration GetSampleProductComponentConfigurationForQuote()
        {
            Component component = new Component
            {
                Form = new Form
                {
                    TextElements = new List<TextElement>
                    {
                        new TextElement
                        {
                            Category = "Organisation",
                            Name = "Name",
                            Text = "ABC Insurance Ltd.",
                        },
                    },
                },
            };

            var mockConfig = new Mock<IProductComponentConfiguration>();
            mockConfig.Setup(x => x.Component).Returns(component);
            mockConfig.Setup(x => x.Version).Returns("2.0.0");
            mockConfig.Setup(x => x.IsVersion2OrGreater).Returns(true);
            mockConfig.Setup(x => x.IsVersion1).Returns(false);
            return mockConfig.Object;
        }
    }
}
