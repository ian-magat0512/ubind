// <copyright file="ApplicationEventViewModelTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Export.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NodaTime;
    using NodaTime.Extensions;
    using UBind.Application.Export.ViewModels;
    using UBind.Application.Person;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.Services;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class ApplicationEventViewModelTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly Mock<IConfigurationService> configurationService = new Mock<IConfigurationService>();
        private readonly IEmailInvitationConfiguration emailConfiguration = new Mock<IEmailInvitationConfiguration>().Object;
        private readonly IPersonService personService = new Mock<IPersonService>().Object;
        private readonly Mock<IOrganisationService> organisationService = new Mock<IOrganisationService>();
        private readonly IProductConfiguration productConfiguration = new Mock<IProductConfiguration>().Object;
        private readonly Mock<ICqrsMediator> mediator = new Mock<ICqrsMediator>();
        private DisplayableFieldDto displayableFieldDto = new DisplayableFieldDto(new List<string>(), new List<string>(), true, true);
        private IFormDataPrettifier formDataPrettifier = new FormDataPrettifier(new FormDataFieldFormatterFactory(), NullLogger<FormDataPrettifier>.Instance);
        private Guid tenantId = Guid.NewGuid();

        public ApplicationEventViewModelTests()
        {
            this.configurationService.Setup(x => x.GetProductComponentConfiguration(It.IsAny<ReleaseContext>(), It.IsAny<WebFormAppType>()))
                .Returns(Task.FromResult(this.GetSampleProductComponentConfigurationForQuote()));

            var personDetails = new FakePersonalDetails();
            var personAggregate = PersonAggregate.CreatePersonFromPersonalDetails(
                this.tenantId,
                Guid.NewGuid(),
                personDetails,
                this.performingUserId,
                default);
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

            this.mediator.Setup(s => s.Send(It.IsAny<GetCustomerByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(customer));
        }

        [Fact]
        public async Task Calculation_IncludesPriceAndRefundBreakDowns()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var product = new Product(tenant.Id, Guid.NewGuid(), "test", "test", SystemClock.Instance.Now());
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
            var personService = new Mock<IPersonService>();
            var quote = QuoteFactory.CreateNewBusinessQuote(TenantFactory.DefaultId);
            var quoteAggregate = quote.Aggregate
                  .WithCustomerDetails(quote.Id)
                  .WithFormData(quote.Id)
                  .WithCalculationResult(quote.Id)
                  .WithCustomer()
                  .WithQuoteNumber(quote.Id);
            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                ApplicationEventType.PaymentMade,
                quoteAggregate,
                quote.Id,
                0,
                "0",
                quote.ProductReleaseId.Value);
            Task<ReleaseProductConfiguration> task = Task.Run(() => new ReleaseProductConfiguration
            {
                ConfigurationJson = "{ \"baseConfiguration\": { \"textElements\": { } } }",
                ProductReleaseId = default,
            });
            this.organisationService
                .Setup(o => o.GetOrganisationSummaryForTenantIdAndOrganisationId(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new OrganisationReadModelSummary
                {
                    Alias = "sample",
                });

            var sut = await ApplicationEventViewModel.Create(
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

            // Act
            var payable = sut.Calculation["payment.payableComponents.totalPayable"];
            var refund = sut.Calculation["payment.refundComponents.totalPayable"];

            // Assert
            ((string)payable).Should().Be("$125.84");
            ((string)refund).Should().Be("$0.00");
        }

        [Fact]
        public async Task Quote_IncludesExpiryDateAndTime_WhenQuoteExpiryIsEnabled()
        {
            // Arrange
            TestClock testClock = new TestClock(false);
            var time = new DateTime(1999, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToInstant();
            testClock.SetToInstant(time);
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = new Tenant(tenantId, "tenant name", tenantId.ToString(), null, default, default, testClock.Now());
            var product = new Product(tenant.Id, productId, productId.ToString(), productId.ToString(), testClock.Now());
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
            var quoteExpirySettings = new QuoteExpirySettings(1, true);
            testClock.Increment(Duration.FromSeconds(10));
            product.Update(new ProductDetails(product.Details.Name, product.Details.Alias, false, false, testClock.Now(), null, quoteExpirySettings));
            QuoteFactory.Clock = testClock;
            Mock<IQuoteExpirySettingsProvider> quoteExpirySettingsProvider = new Mock<IQuoteExpirySettingsProvider>();
            quoteExpirySettingsProvider.Setup(x => x.Retrieve(It.IsAny<QuoteAggregate>())).ReturnsAsync(quoteExpirySettings);
            QuoteFactory.QuoteExpirySettingsProvider = quoteExpirySettingsProvider.Object;
            var expiryDateToUse = new DateTime(1999, 1, 30, 6, 0, 0);
            expiryDateToUse = DateTime.SpecifyKind(expiryDateToUse, DateTimeKind.Utc);
            testClock.Increment(Duration.FromSeconds(10));
            Quote quote = QuoteFactory.CreateNewBusinessQuote(
                tenant.Id,
                product.Id,
                quoteExpirySettings: quoteExpirySettings);
            QuoteAggregate quoteAggregate = quote.Aggregate
                .WithCustomerDetails(quote.Id)
                .WithFormData(quote.Id)
                .WithCalculationResult(quote.Id)
                .WithCustomer()
                .WithQuoteNumber(quote.Id)
                .WithExpiryTime(Instant.FromDateTimeUtc(expiryDateToUse.ToUniversalTime()), quote.Id);
            this.organisationService
               .Setup(o => o.GetOrganisationSummaryForTenantIdAndOrganisationId(It.IsAny<Guid>(), It.IsAny<Guid>()))
               .ReturnsAsync(new OrganisationReadModelSummary
               {
                   Alias = "sample",
               });

            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                ApplicationEventType.PaymentMade,
                quoteAggregate,
                quote.Id,
                0,
                "0",
                quote.ProductReleaseId.Value);
            var sut = await ApplicationEventViewModel.Create(
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
                testClock,
                this.mediator.Object);

            // Act
            var expiryDate = sut.Quote["expiryDate"] as string;
            var expiryTime = sut.Quote["expiryTimeOfDay"] as string;

            // Assert
            expiryDate.Should().Be("02 Jan 1999");
            expiryTime.Should().Be("5:00 PM");
        }

        [Fact]
        public async Task Quote_HasEmptyExpiryDateAndTime_WhenQuoteExpiryIsDisabled()
        {
            // Arrange
            TestClock testClock = new TestClock(false);
            testClock.SetToInstant(Instant.MinValue);
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = new Tenant(tenantId, "tenant name", tenantId.ToString(), null, default, default, SystemClock.Instance.Now());
            var product = new Product(tenant.Id, productId, productId.ToString(), productId.ToString(), SystemClock.Instance.Now());
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
            var quoteExpirySettings = new QuoteExpirySettings(1, false);
            product.Update(new ProductDetails(product.Details.Name, product.Details.Alias, false, false, SystemClock.Instance.Now(), null, quoteExpirySettings));
            var expiryDateToUse = SystemClock.Instance.Now().ToLocalDateInAet().PlusDays(1).ToDateTimeUnspecified();
            this.organisationService
               .Setup(o => o.GetOrganisationSummaryForTenantIdAndOrganisationId(It.IsAny<Guid>(), It.IsAny<Guid>()))
               .ReturnsAsync(new OrganisationReadModelSummary
               {
                   Alias = "sample",
               });
            QuoteFactory.Clock = testClock;
            Mock<IQuoteExpirySettingsProvider> quoteExpirySettingsProvider = new Mock<IQuoteExpirySettingsProvider>();
            quoteExpirySettingsProvider.Setup(x => x.Retrieve(It.IsAny<QuoteAggregate>())).ReturnsAsync(quoteExpirySettings);
            QuoteFactory.QuoteExpirySettingsProvider = quoteExpirySettingsProvider.Object;
            Quote quote = QuoteFactory.CreateNewBusinessQuote(tenant.Id, product.Id);
            QuoteAggregate quoteAggregate = quote.Aggregate
                .WithCustomerDetails(quote.Id)
                .WithFormData(quote.Id)
                .WithCalculationResult(quote.Id)
                .WithCustomer()
                .WithQuoteNumber(quote.Id)
                .WithExpiryTime(Instant.FromDateTimeUtc(expiryDateToUse.ToUniversalTime()), quote.Id);
            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                ApplicationEventType.PaymentMade,
                quoteAggregate,
                quote.Id,
                0,
                "0",
                quote.ProductReleaseId.Value);
            var sut = await ApplicationEventViewModel.Create(
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

            // Act
            var expiryDate = sut.Quote["expiryDate"] as string;
            var expiryTime = sut.Quote["expiryTimeOfDay"] as string;

            // Assert
            expiryDate.Should().BeNullOrEmpty();
            expiryTime.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task Constructor_SuccessAddingExtractedCustomerPhonesAndEmailsToViewModel()
        {
            // Arrange
            TestClock testClock = new TestClock(false);
            var time = new DateTime(1999, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToInstant();
            testClock.SetToInstant(time);
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = new Tenant(tenantId, "tenant name", tenantId.ToString(), null, default, default, testClock.Now());
            var product = new Product(tenant.Id, productId, productId.ToString(), productId.ToString(), testClock.Now());

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
            this.organisationService
                .Setup(o => o.GetOrganisationSummaryForTenantIdAndOrganisationId(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new OrganisationReadModelSummary
                {
                    Alias = "sample",
                });

            var quote = QuoteFactory.CreateNewBusinessQuote(tenant.Id, product.Id);
            var quoteAggregate = quote.Aggregate
                .WithCustomerDetails(quote.Id)
                .WithFormData(quote.Id)
                .WithCalculationResult(quote.Id)
                .WithCustomer()
                .WithQuoteNumber(quote.Id);
            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                ApplicationEventType.PaymentMade,
                quoteAggregate,
                quote.Id,
                0,
                "0",
                quote.ProductReleaseId.Value);
            ReleaseProductConfiguration config = new ReleaseProductConfiguration();
            config.ConfigurationJson = "{ \"baseConfiguration\": { \"textElements\": { } } }";
            Task<ReleaseProductConfiguration> task = Task.Run(() => { return config; });
            var releaseContext = new ReleaseContext(
                applicationEvent.Aggregate.TenantId,
                applicationEvent.Aggregate.ProductId,
                applicationEvent.Aggregate.Environment,
                quote.ProductReleaseId.Value);
            this.configurationService.Setup(x => x.GetConfigurationAsync(releaseContext, WebFormAppType.Quote)).Returns(task);

            // Act
            var sut = await ApplicationEventViewModel.Create(
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
                testClock,
                this.mediator.Object);
            var homePhone = sut.Customer["homePhone"] as string;
            var workPhone = sut.Customer["workPhone"] as string;
            var mobilePhone = sut.Customer["mobilePhone"] as string;
            var email = sut.Customer["email"] as string;
            var alternativeEmail = sut.Customer["alternativeEmail"] as string;

            // Assert
            homePhone.Should().Be("0412341234");
            workPhone.Should().Be("0412341235");
            mobilePhone.Should().Be("0412341236");
            email.Should().Be("test1@email.com");
            alternativeEmail.Should().Be("test2@email.com");
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
    }
}
