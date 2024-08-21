// <copyright file="CloneQuoteFromExpiredQuoteCommandHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands.Quote
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using RedLockNet;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Releases;
    using UBind.Application.Tests.Fakes;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class CloneQuoteFromExpiredQuoteCommandHandlerTests
    {
        private readonly Mock<ICachingResolver> cachingResolverMock = new Mock<ICachingResolver>();
        private readonly Mock<IProductFeatureSettingService> productFeatureSettingServiceMock
            = new Mock<IProductFeatureSettingService>();

        private readonly Mock<IQuoteExpirySettingsProvider> quoteExpirySettingsProviderMock
            = new Mock<IQuoteExpirySettingsProvider>();

        private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolverMock
            = new Mock<IHttpContextPropertiesResolver>();

        private readonly Mock<IQuoteAggregateRepository> quoteAggregateRepositoryMock
            = new Mock<IQuoteAggregateRepository>();

        private readonly Mock<UBind.Domain.Services.IPolicyService> policyServiceMock
            = new Mock<UBind.Domain.Services.IPolicyService>();

        private readonly Mock<IQuoteWorkflowProvider> quoteWorkflowProviderMock
            = new Mock<IQuoteWorkflowProvider>();

        private readonly Mock<IProductConfigurationProvider> productConfigurationProviderMock
            = new Mock<IProductConfigurationProvider>();

        private readonly Mock<IQuoteReadModelRepository> quoteReadModelRepositoryMock = new Mock<IQuoteReadModelRepository>();
        private readonly Mock<ICustomerAggregateRepository> customerAggregateRepositoryMock = new Mock<ICustomerAggregateRepository>();
        private readonly Mock<IUserReadModelRepository> userReadModelRepositoryMock = new Mock<IUserReadModelRepository>();
        private readonly Mock<IPersonAggregateRepository> personAggregateRepositoryMock = new Mock<IPersonAggregateRepository>();
        private readonly Mock<IQuoteAggregateResolverService> quoteAggregateResolverServiceMock
            = new Mock<IQuoteAggregateResolverService>();

        private readonly Mock<DefaultPolicyTransactionTimeOfDayScheme> defaultPolicyTransactionTimeOfDaySchemeMock
            = new Mock<DefaultPolicyTransactionTimeOfDayScheme>();

        private readonly Mock<IAggregateLockingService> aggregateLockingService = new Mock<IAggregateLockingService>();

        private readonly Mock<IReleaseQueryService> releaseQueryService = new Mock<IReleaseQueryService>();
        private readonly IQuoteWorkflow quoteWorkflow = new DefaultQuoteWorkflow();
        private readonly Guid performingUserId;
        private readonly IQuoteExpirySettings expirySettings;
        private IClock clock;

        public CloneQuoteFromExpiredQuoteCommandHandlerTests()
        {
            this.clock = new TestClock();
            this.cachingResolverMock.Setup(s => s.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Domain.Product.Product(
                    TenantFactory.DefaultId,
                    ProductFactory.DefaultId,
                    TenantFactory.DefaultName,
                    "test-product",
                    this.clock.Now())));
            this.cachingResolverMock.Setup(s => s.GetTenantOrThrow(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Tenant(
                    TenantFactory.DefaultId,
                    "Test tenant",
                    "test-tenant",
                    null,
                    default,
                    default,
                    this.clock.Now())));
            this.quoteExpirySettingsProviderMock.Setup(s => s.Retrieve(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(QuoteExpirySettings.Default);
            this.quoteWorkflowProviderMock.Setup(s => s.GetConfigurableQuoteWorkflow(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(this.quoteWorkflow));
            var questionKey = "CloneNewBusinessQuoteField";
            var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSet(questionKey, "true");
            this.productConfigurationProviderMock
                .Setup(s => s.GetFormDataSchema(It.IsAny<ProductContext>(), WebFormAppType.Quote))
                .Returns(new FormDataSchema(JObject.Parse(baseConfiguration)));
            var releaseContext = new ReleaseContext(TenantFactory.DefaultId, ProductFactory.DefaultId, DeploymentEnvironment.Development, Guid.NewGuid());
            this.releaseQueryService
                .Setup(s => s.GetDefaultReleaseContextOrNull(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>()))
                .Returns(releaseContext);

            this.policyServiceMock.Setup(a => a.GenerateQuoteNumber(It.IsAny<ReleaseContext>()).Result).Returns("TEST-QUOTE-NUMBER-NEW");
            this.performingUserId = Guid.NewGuid();
            this.httpContextPropertiesResolverMock.Setup(a => a.PerformingUserId).Returns(this.performingUserId);
            var productFeatureSettings = new ProductFeatureSetting(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                this.clock.Now());
            this.productFeatureSettingServiceMock
                .Setup(s => s.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeatureSettings);
            this.expirySettings = QuoteExpirySettings.Default;

            this.aggregateLockingService.Setup(s => s.CreateLockOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>(), AggregateType.Quote))
                .ReturnsAsync(It.IsAny<IRedLock>());
        }

        [Fact]
        public async Task Handle_Succeeds_WhenTheExpiredQuoteIsANewBusinessQuote()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                this.expirySettings,
                default,
                false);
            quote.Aggregate.WithQuoteNumber(quote.Id, "NEW-BUSINESS-QUOTE-NUMBER");
            this.SetupQuoteProperties(quote.Aggregate, quote.Id);
            this.SetupMockups(quote.Aggregate, quote.Id);
            var command = new CloneQuoteFromExpiredQuoteCommand(
                TenantFactory.DefaultId,
                quote.Id,
                DeploymentEnvironment.Development);
            var handler = this.CreateCommandHandler();

            // Act
            Func<Task<NewQuoteReadModel>> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Handle_Succeeds_WhenTheExpiredQuoteIsARenewalQuote()
        {
            // Arrange
            var aggregate = QuoteFactory.CreateNewPolicy();
            var quote = aggregate.WithRenewalQuote(this.clock.Now(), "RENEWAL-QUOTE-NUMBER");
            this.SetupQuoteProperties(aggregate, quote.Id);
            this.SetupMockups(aggregate, quote.Id);
            var command = new CloneQuoteFromExpiredQuoteCommand(
                TenantFactory.DefaultId,
                quote.Id,
                DeploymentEnvironment.Development);
            var handler = this.CreateCommandHandler();

            // Act
            Func<Task<NewQuoteReadModel>> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Handle_Succeeds_WhenTheExpiredQuoteIsAnAdjustmentQuote()
        {
            // Arrange
            var aggregate = QuoteFactory.CreateNewPolicy();
            var quote = aggregate.WithAdjustmentQuote(this.clock.Now(), "ADJUSTMENT-QUOTE-NUMBER");
            this.SetupQuoteProperties(aggregate, quote.Id);
            this.SetupMockups(aggregate, quote.Id);
            var command = new CloneQuoteFromExpiredQuoteCommand(
                TenantFactory.DefaultId,
                quote.Id,
                DeploymentEnvironment.Development);
            var handler = this.CreateCommandHandler();

            // Act
            Func<Task<NewQuoteReadModel>> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Handle_ThrowsAnErrorException_WhenTheQuoteIsNotFound()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewBusinessQuote();
            this.SetupQuoteProperties(quote.Aggregate, quote.Id);
            var command = new CloneQuoteFromExpiredQuoteCommand(
                TenantFactory.DefaultId,
                quote.Id,
                DeploymentEnvironment.Development);
            var handler = this.CreateCommandHandler();

            // Act
            Func<Task<NewQuoteReadModel>> sut = () => handler.Handle(command, CancellationToken.None);

            // Assert
            (await sut.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("record.not.found");
        }

        [Fact]
        public async Task Handle_ThrowsAnInvalidOperationException_WhenTheQuoteIsNotExpired()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewBusinessQuote();
            this.SetupMockups(quote.Aggregate, quote.Id);
            var command = new CloneQuoteFromExpiredQuoteCommand(
                TenantFactory.DefaultId,
                quote.Id,
                DeploymentEnvironment.Development);
            var handler = this.CreateCommandHandler();

            // Act
            Func<Task<NewQuoteReadModel>> sut = () => handler.Handle(command, CancellationToken.None);

            // Assert
            (await sut.Should().ThrowAsync<InvalidOperationException>())
                .Which.Message.Should().Be("Quote should be expired to be able to replicate from the quote");
        }

        private CloneQuoteFromExpiredQuoteCommandHandler CreateCommandHandler()
        {
            return new CloneQuoteFromExpiredQuoteCommandHandler(
                this.quoteAggregateRepositoryMock.Object,
                this.quoteReadModelRepositoryMock.Object,
                this.customerAggregateRepositoryMock.Object,
                this.userReadModelRepositoryMock.Object,
                this.personAggregateRepositoryMock.Object,
                this.productConfigurationProviderMock.Object,
                this.quoteWorkflowProviderMock.Object,
                this.quoteExpirySettingsProviderMock.Object,
                this.httpContextPropertiesResolverMock.Object,
                this.clock,
                this.quoteAggregateResolverServiceMock.Object,
                this.productFeatureSettingServiceMock.Object,
                this.policyServiceMock.Object,
                this.defaultPolicyTransactionTimeOfDaySchemeMock.Object,
                this.releaseQueryService.Object,
                this.aggregateLockingService.Object);
        }

        private void SetupQuoteProperties(QuoteAggregate aggregate, Guid quoteId)
        {
            aggregate.WithFormData(quoteId);
            aggregate.WithCalculationResult(quoteId);
            var expiryTimestamp = Instant.MinValue;
            aggregate.SetQuoteExpiryTime(quoteId, expiryTimestamp, this.performingUserId, expiryTimestamp);
        }

        private void SetupMockups(QuoteAggregate aggregate, Guid quoteId)
        {
            var quote = aggregate.GetQuoteOrThrow(quoteId);
            var userPersonAggregate = PersonAggregate.CreatePerson(
                aggregate.TenantId,
                aggregate.OrganisationId,
                this.performingUserId,
                this.clock.Now());
            var userAggregate = UserAggregate.CreateUser(
                aggregate.TenantId,
                this.performingUserId,
                UserType.Client,
                userPersonAggregate,
                this.performingUserId,
                null,
                this.clock.Now());
            var customerPersonAggregate = PersonAggregate.CreatePerson(
                aggregate.TenantId,
                aggregate.OrganisationId,
                this.performingUserId,
                this.clock.Now());
            var customerAggregate = CustomerAggregate.CreateNewCustomer(
                aggregate.TenantId,
                customerPersonAggregate,
                DeploymentEnvironment.Development,
                this.performingUserId,
                null,
                this.clock.Now());
            aggregate.WithCustomer(customerAggregate);
            aggregate.WithCustomerDetails(quoteId, customerPersonAggregate);
            this.userReadModelRepositoryMock
                .Setup(a => a.GetUser(It.IsAny<Guid>(), userAggregate.Id))
                .Returns(new Domain.ReadModel.User.UserReadModel(
                    userAggregate.Id,
                    new PersonData(userPersonAggregate),
                    null,
                    null,
                    this.clock.Now(),
                    UserType.Client,
                    DeploymentEnvironment.Development));
            this.personAggregateRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>(), userPersonAggregate.Id)).Returns(userPersonAggregate);
            this.customerAggregateRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>(), customerAggregate.Id)).Returns(customerAggregate);
            this.personAggregateRepositoryMock
                .Setup(a => a.GetById(It.IsAny<Guid>(), customerPersonAggregate.Id))
                .Returns(customerPersonAggregate);
            var quoteSummary = new FakeQuoteReadModelSummary(quote);
            this.quoteReadModelRepositoryMock
                .Setup(a => a.GetQuoteSummary(TenantFactory.DefaultId, quote.Id))
                .Returns(quoteSummary);
            this.quoteAggregateResolverServiceMock
                .Setup(s => s.GetQuoteAggregateIdForQuoteId(quote.Id))
                .Returns(aggregate.Id);
            this.quoteAggregateRepositoryMock
                .Setup(s => s.GetById(aggregate.TenantId, aggregate.Id))
                .Returns(aggregate);
        }
    }
}
