// <copyright file="PaymentIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Moq;
    using NodaTime;
    using UBind.Application;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Export.ViewModels;
    using UBind.Application.Person;
    using UBind.Application.Releases;
    using UBind.Application.Tests.Payment.Stripe;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Configuration;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;
    using FormData = UBind.Domain.Aggregates.Quote.FormData;

    [Collection(DatabaseCollection.Name)]
    public class PaymentIntegrationTests
    {
        private readonly IEmailInvitationConfiguration emailConfiguration = new Mock<IEmailInvitationConfiguration>().Object;
        private readonly Mock<IConfigurationService> configurationService = new Mock<IConfigurationService>();
        private readonly IProductConfiguration productConfiguration = new Mock<IProductConfiguration>().Object;
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly Mock<ICqrsMediator> mediator = new Mock<ICqrsMediator>();
        private IPersonService personService = new Mock<IPersonService>().Object;
        private DisplayableFieldDto displayableFieldDto = new DisplayableFieldDto(new List<string>(), new List<string>(), true, true);
        private IFormDataPrettifier formDataPrettifier = new Mock<IFormDataPrettifier>().Object;

        private Guid tenantId;
        private Guid productId;
        private Guid organisationId;

        public PaymentIntegrationTests()
        {
            this.productId = Guid.NewGuid();
            this.tenantId = Guid.NewGuid();
            this.organisationId = Guid.NewGuid();

            this.configurationService.Setup(x => x.GetProductComponentConfiguration(
                It.IsAny<ReleaseContext>(), It.IsAny<WebFormAppType>()))
               .Returns(Task.FromResult(this.GetSampleProductComponentConfigurationForQuote()));
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task StripePaymentDetailsAreExposedInApplicationEvents()
        {
            // Arrange
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var tenant = TenantFactory.Create(this.tenantId);
            stack.CreateTenant(tenant);

            var product = ProductFactory.Create(tenant.Id, this.productId);
            stack.CreateProduct(product);

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

            await stack.OrganisationAggregateRepository.Save(organisation);

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
                SystemClock.Instance.Now());

            customer.AssignOwnership(customer.Id, customerOwnerPerson, this.performingUserId, SystemClock.Instance.Now());

            await stack.CustomerAggregateRepository.Save(customer);

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

            var quoteAggregate = quote.Aggregate
                .WithCustomer(customer)
                .WithCustomerDetails(quote.Id, new FakePersonalDetails())
                .WithFormData(quote.Id)
                .WithCalculationResult(quote.Id)
                .WithQuoteNumber(quote.Id);

            await stack.QuoteAggregateRepository.Save(quoteAggregate);
            stack.DbContext.Dispose();
            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var q2 = stack2.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
            var token = await new TestStripeTokenProvider().GetTokenForValidCard();
            var latestQuote = q2.GetQuoteOrThrow(quote.Id);
            var cqrsMediator = stack2.GetPaymentComamndMediator(PaymentGatewayName.Stripe);
            var releaseContext = new ReleaseContext(quote.ProductContext, quote.ProductReleaseId.Value);

            // Act
            await cqrsMediator.Send(new CardPaymentCommand(
                releaseContext,
                quote.Id,
                latestQuote.LatestCalculationResult.Id,
                new FormData("{}"),
                token));

            // Assert
            stack2.DbContext.Dispose();
            var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var readQuote = stack3.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
            var quoteRead = readQuote.GetQuoteOrThrow(latestQuote.Id);

            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                ApplicationEventType.PaymentMade,
                readQuote,
                quoteRead.Id,
                0,
                "1",
                quoteRead.ProductReleaseId.Value);
            var viewModel = await ApplicationEventViewModel.Create(
                this.formDataPrettifier,
                applicationEvent,
                this.emailConfiguration,
                this.configurationService.Object,
                this.productConfiguration,
                this.displayableFieldDto,
                this.personService,
                tenant,
                product,
                organisationReadModelSummary,
                SystemClock.Instance,
                stack3.Mediator);
            Assert.NotNull(viewModel.Payment);
            var reference = viewModel.Payment["reference"];
            var requestAmount = viewModel.Payment.Request["Amount"];
            var last4digits = viewModel.Payment.Response["$.payment_method_details.card.last4"];
            Assert.NotNull(reference);
            Assert.Equal("12584", requestAmount);
            Assert.Equal("4242", last4digits);
            stack3.DbContext.Dispose();
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task DeftPaymentDetailsAreExposedInApplicationEvents()
        {
            // Arrange
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var tenant = TenantFactory.Create(this.tenantId);
            stack.CreateTenant(tenant);

            var product = ProductFactory.Create(tenant.Id, this.productId);
            stack.CreateProduct(product);

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

            var quoteAggregate = quote.Aggregate
                .WithFormData(quote.Id)
                .WithCalculationResult(quote.Id);
            await stack.QuoteAggregateRepository.Save(quoteAggregate);
            stack.DbContext.Dispose();
            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var q2 = stack2.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
            var creditCardDetails = new CreditCardDetails("4444333322221111", "John Smith", "11", this.GetCurrentYear() + 1, "123");
            var latestQuote = quoteAggregate.GetQuoteOrThrow(quote.Id);
            var cqrsMediator = stack2.GetPaymentComamndMediator(PaymentGatewayName.Deft);
            var releaseContext = new ReleaseContext(quote.ProductContext, quote.ProductReleaseId.Value);

            // Act
            await cqrsMediator.Send(new CardPaymentCommand(
                releaseContext,
                quote.Id,
                latestQuote.LatestCalculationResult.Id,
                new FormData("{}"),
                cardDetails: creditCardDetails));

            // Assert
            var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var readQuote = stack3.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
            var quoteRead = readQuote.GetQuoteOrThrow(latestQuote.Id);

            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                ApplicationEventType.PaymentMade,
                readQuote,
                quoteRead.Id,
                0,
                "1",
                quoteRead.ProductReleaseId.Value);
            var viewModel = await ApplicationEventViewModel.Create(
                this.formDataPrettifier,
                applicationEvent,
                this.emailConfiguration,
                this.configurationService.Object,
                this.productConfiguration,
                this.displayableFieldDto,
                this.personService,
                tenant,
                product,
                organisationReadModelSummary,
                SystemClock.Instance,
                this.mediator.Object);
            Assert.NotNull(viewModel.Payment);
            var cardNumber = viewModel.Payment.Request["$.cardDetails.cardNumber"];
            Assert.Equal("************1111", cardNumber);
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task EwayPaymentDetailsAreExposedInApplicationEvents()
        {
            // Arrange
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var tenant = TenantFactory.Create(this.tenantId);
            stack.CreateTenant(tenant);

            var product = ProductFactory.Create(tenant.Id, this.productId);
            stack.CreateProduct(product);

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

            var quoteAggregate = quote.Aggregate
                .WithFormData(quote.Id)
                .WithCalculationResult(quote.Id);
            await stack.QuoteAggregateRepository.Save(quoteAggregate);
            stack.DbContext.Dispose();
            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var q2 = stack2.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
            var creditCardDetails = new CreditCardDetails("4444333322221111", "John Smith", "12", this.GetCurrentYear() + 1, "123");
            var latestQuote = q2.GetQuoteOrThrow(quote.Id);
            var cqrsMediator = stack2.GetPaymentComamndMediator(PaymentGatewayName.EWay);
            var releaseContext = new ReleaseContext(quote.ProductContext, quote.ProductReleaseId.Value);

            // Act
            await cqrsMediator.Send(new CardPaymentCommand(
                releaseContext,
                quote.Id,
                latestQuote.LatestCalculationResult.Id,
                new FormData("{}"),
                cardDetails: creditCardDetails));

            // Assert
            var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var readQuote = stack3.QuoteAggregateRepository.GetById(tenant.Id, quoteAggregate.Id);
            var quoteRead = readQuote.GetQuoteOrThrow(latestQuote.Id);

            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                ApplicationEventType.PaymentMade,
                readQuote,
                quoteRead.Id,
                0,
                "1",
                quoteRead.ProductReleaseId.Value);
            var viewModel = await ApplicationEventViewModel.Create(
                this.formDataPrettifier,
                applicationEvent,
                this.emailConfiguration,
                this.configurationService.Object,
                this.productConfiguration,
                this.displayableFieldDto,
                this.personService,
                tenant,
                product,
                organisationReadModelSummary,
                SystemClock.Instance,
                this.mediator.Object);
            Assert.NotNull(viewModel.Payment);
            var totalAmountInCents = viewModel.Payment.Request["$.PaymentDetails.TotalAmount"];
            Assert.Equal("12584", totalAmountInCents);
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
                        new TextElement
                        {
                            Category = "Product",
                            Name = "Title",
                            Text = "General Liability",
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

        private int GetCurrentYear()
        {
            return SystemClock.Instance.Now().InUtc().Year;
        }
    }
}
