// <copyright file="CardPaymentCommandTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using System.Transactions;
    using CSharpFunctionalExtensions;
    using FluentAssertions;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using RedLockNet;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Payment;
    using UBind.Application.Payment.Deft;
    using UBind.Application.Queries.Person;
    using UBind.Application.Services.Email;
    using UBind.Application.Tests.Payment;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Configuration;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using FormData = UBind.Domain.Aggregates.Quote.FormData;

    public class CardPaymentCommandTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly Mock<IHttpContextPropertiesResolver> currentUserIdentification = new Mock<IHttpContextPropertiesResolver>();

        private IClock clock;
        private Mock<IProductFeatureSettingService> productFeatureSettingService = new Mock<IProductFeatureSettingService>();
        private Mock<ICachingResolver> cacheResolver = new Mock<ICachingResolver>();
        private Mock<IProductConfigurationProvider> productConfigurationProvider;
        private Mock<IQuoteAggregateRepository> quoteAggregateRepository;
        private Mock<IQuoteAggregateResolverService> quoteAggregateResolverService;
        private ICqrsMediator mediator;
        private Guid quoteId;
        private ServiceCollection services = new ServiceCollection();

        public CardPaymentCommandTests()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.currentUserIdentification.Setup(e => e.PerformingUserId).Returns(this.performingUserId);
            this.quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
            this.quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
            var quoteWorkflowProvider = new Mock<IQuoteWorkflowProvider>();
            quoteWorkflowProvider.Setup(provide => provide.GetConfigurableQuoteWorkflow(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(new DefaultQuoteWorkflow() as IQuoteWorkflow));
            this.productConfigurationProvider = new Mock<IProductConfigurationProvider>();
            this.productConfigurationProvider
                .Setup(s => s.GetProductConfiguration(It.IsAny<ReleaseContext>(), It.IsAny<WebFormAppType>()))
                .Returns(Task.FromResult(new DefaultProductConfiguration() as IProductConfiguration));

            var mockUBindDbContext = new Mock<IUBindDbContext>();
            var transactionStack = new Stack<TransactionScope>();
            mockUBindDbContext.Setup(s => s.TransactionStack).Returns(transactionStack);
            var mockAggregateLockingService = new Mock<IAggregateLockingService>();
            mockAggregateLockingService
                .Setup(a => a.CreateLockOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AggregateType>()))
                .ReturnsAsync(It.IsAny<IRedLock>());
            this.clock = new TestClock();
            this.services.AddTransient<IClock>(c => this.clock);
            this.services.AddTransient<ISavedPaymentMethodRepository>(c => new Mock<ISavedPaymentMethodRepository>().Object);
            this.services.AddTransient<IProductConfigurationProvider>(c => this.productConfigurationProvider.Object);
            this.services.AddTransient<IProductFeatureSettingService>(c => this.productFeatureSettingService.Object);
            this.services.AddTransient<IHttpContextPropertiesResolver>(c => this.currentUserIdentification.Object);
            this.services.AddTransient<IQuoteWorkflowProvider>(c => quoteWorkflowProvider.Object);
            this.services.AddTransient<IQuoteAggregateRepository>(c => this.quoteAggregateRepository.Object);
            this.services.AddTransient<IQuoteAggregateResolverService>(c => this.quoteAggregateResolverService.Object);
            this.services.AddTransient<IUBindDbContext>(_ => mockUBindDbContext.Object);
            this.services.AddTransient<IErrorNotificationService>(c => new Mock<IErrorNotificationService>().Object);
            this.services.AddScoped<IAggregateLockingService>(c => mockAggregateLockingService.Object);
            this.services.AddSingleton<ICqrsMediator, CqrsMediator>();
            this.services.AddSingleton<ICqrsRequestContext>(_ => new CqrsRequestContext());
            this.services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(GetPrimaryPersonForCustomerQuery).Assembly));
            this.services.AddLogging(loggingBuilder => loggingBuilder.AddDebug());
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task CardPaymentCommand_Succeeds_UsingEwayPaymentGateway()
        {
            // Arrange
            var quoteAggregate = this.CreateQuote();
            var logger = new Mock<ILogger<PaymentGatewayFactory>>();
            var paymentConfigurationProvider = new Mock<IPaymentConfigurationProvider>();
            var crnGenerator = new Mock<IDeftCustomerReferenceNumberGenerator>();
            var ewayConfiguration = TestEwayConfigurationFactory.CreateWithValidCredentials();
            paymentConfigurationProvider
                .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(ewayConfiguration)));
            var paymentGatewayFactory = new PaymentGatewayFactory(
                this.clock, crnGenerator.Object, this.cacheResolver.Object, this.currentUserIdentification.Object, new Mock<ICqrsMediator>().Object, logger.Object);
            this.quoteAggregateRepository
                .Setup(r => r.GetById(quoteAggregate.TenantId, quoteAggregate.Id))
                .Returns(quoteAggregate);
            this.quoteAggregateResolverService
                .Setup(r => r.GetQuoteAggregateIdForQuoteId(It.IsAny<Guid>()))
                .Returns(quoteAggregate.Id);

            this.services.AddSingleton<PaymentGatewayFactory>(c => paymentGatewayFactory);
            this.services.AddTransient<IPaymentConfigurationProvider>(c => paymentConfigurationProvider.Object);
            var serviceProvider = this.services.BuildServiceProvider();
            this.mediator = serviceProvider.GetRequiredService<ICqrsMediator>();

            var creditCardDetails = new CreditCardDetails(
                "4444333322221111",
                "John Smith",
                "12",
                DateTime.Today.Year + 1,
                "123");
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var releaseContext = new ReleaseContext(
                quote.Aggregate.TenantId,
                quote.Aggregate.ProductId,
                quote.Aggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            await this.mediator.Send(new CardPaymentCommand(
                releaseContext,
                quote.Id,
                quote.LatestCalculationResult.Id,
                new FormData("{}"),
                cardDetails: creditCardDetails));

            // Assert
            var updatedQuote = quoteAggregate.GetQuoteOrThrow(quote.Id);
            updatedQuote.LatestPaymentAttemptResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task CardPaymentCommand_WithEnabledProductFeatureSettings_Succeed()
        {
            // Arrange
            var logger = new Mock<ILogger<PaymentGatewayFactory>>();
            var paymentConfigurationProvider = new Mock<IPaymentConfigurationProvider>();
            var ewayConfiguration = TestEwayConfigurationFactory.CreateWithValidCredentials();
            paymentConfigurationProvider
                .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(ewayConfiguration)));
            var crnGenerator = new Mock<IDeftCustomerReferenceNumberGenerator>();
            var paymentGatewayFactory = new PaymentGatewayFactory(
                this.clock, crnGenerator.Object, this.cacheResolver.Object, this.currentUserIdentification.Object, this.mediator, logger.Object);

            var quoteAggregate = this.CreateQuote();
            this.quoteAggregateRepository
                .Setup(r => r.GetById(quoteAggregate.TenantId, quoteAggregate.Id))
                .Returns(quoteAggregate);
            this.quoteAggregateResolverService
                .Setup(r => r.GetQuoteAggregateIdForQuoteId(It.IsAny<Guid>()))
                .Returns(quoteAggregate.Id);
            this.services.AddSingleton<PaymentGatewayFactory>(c => paymentGatewayFactory);
            this.services.AddTransient<IPaymentConfigurationProvider>(c => paymentConfigurationProvider.Object);
            var serviceProvider = this.services.BuildServiceProvider();
            this.mediator = serviceProvider.GetRequiredService<ICqrsMediator>();
            var creditCardDetails = new CreditCardDetails(
                "4444333322221111",
                "John Smith",
                "12",
                DateTime.Today.Year + 1,
                "123");
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var releaseContext = new ReleaseContext(
                quote.Aggregate.TenantId,
                quote.Aggregate.ProductId,
                quote.Aggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            await this.mediator.Send(new CardPaymentCommand(
                releaseContext,
                quote.Id,
                quote.LatestCalculationResult.Id,
                new FormData("{}"),
                cardDetails: creditCardDetails));

            // Assert
            var updatedQuote = quoteAggregate.GetQuoteOrThrow(quote.Id);
            updatedQuote.LatestPaymentAttemptResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePaymentAsync_Succeeds_ForTestQuote_UsingEwayPaymentGateway()
        {
            // Arrange
            var logger = new Mock<ILogger<PaymentGatewayFactory>>();
            var paymentConfigurationProvider = new Mock<IPaymentConfigurationProvider>();
            var crnGenerator = new Mock<IDeftCustomerReferenceNumberGenerator>();
            var paymentGatewayFactory = new PaymentGatewayFactory(
                this.clock, crnGenerator.Object, this.cacheResolver.Object, this.currentUserIdentification.Object, this.mediator, logger.Object);

            var ewayConfiguration = TestEwayConfigurationFactory.CreateWithValidCredentials();
            paymentConfigurationProvider
                .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(ewayConfiguration)));
            var quoteAggregate = this.CreateTestQuote();
            this.quoteAggregateRepository
                .Setup(r => r.GetById(quoteAggregate.TenantId, quoteAggregate.Id))
                .Returns(quoteAggregate);
            this.quoteAggregateResolverService
                .Setup(r => r.GetQuoteAggregateIdForQuoteId(It.IsAny<Guid>()))
                .Returns(quoteAggregate.Id);
            this.services.AddSingleton<PaymentGatewayFactory>(c => paymentGatewayFactory);
            this.services.AddTransient<IPaymentConfigurationProvider>(c => paymentConfigurationProvider.Object);
            var serviceProvider = this.services.BuildServiceProvider();
            this.mediator = serviceProvider.GetService<ICqrsMediator>();
            var creditCardDetails = new CreditCardDetails(
                "4444333322221111",
                "John Smith",
                "12",
                DateTime.Today.Year + 1,
                "123");
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var releaseContext = new ReleaseContext(
                quote.Aggregate.TenantId,
                quote.Aggregate.ProductId,
                quote.Aggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            await this.mediator.Send(new CardPaymentCommand(
                releaseContext,
                quote.Id,
                quote.LatestCalculationResult.Id,
                new FormData("{}"),
                cardDetails: creditCardDetails));

            // Assert
            var updatedQuote = quoteAggregate.GetQuoteOrThrow(quote.Id);
            updatedQuote.LatestPaymentAttemptResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePaymentAsync_Succeeds_UsingDeftPaymentGateway()
        {
            // Arrange
            var logger = new Mock<ILogger<PaymentGatewayFactory>>();
            var paymentConfigurationProvider = new Mock<IPaymentConfigurationProvider>();
            var productFeatureSettingService = new Mock<IProductFeatureSettingService>();
            var crnGenerator = new Mock<IDeftCustomerReferenceNumberGenerator>();
            crnGenerator
                .Setup(g => g.GenerateDeftCrnNumber(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<CrnGenerationConfiguration>()))
                .Returns("1234567890");
            var paymentGatewayFactory = new PaymentGatewayFactory(
                this.clock, crnGenerator.Object, this.cacheResolver.Object, this.currentUserIdentification.Object, this.mediator, logger.Object);

            var deftConfiguration = new TestDeftConfiguration();
            paymentConfigurationProvider
                .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(deftConfiguration)));
            var quoteAggregate = this.CreateQuote();
            this.quoteAggregateRepository
                .Setup(r => r.GetById(quoteAggregate.TenantId, quoteAggregate.Id))
                .Returns(quoteAggregate);
            this.quoteAggregateResolverService
                .Setup(r => r.GetQuoteAggregateIdForQuoteId(It.IsAny<Guid>()))
                .Returns(quoteAggregate.Id);
            this.services.AddSingleton<PaymentGatewayFactory>(c => paymentGatewayFactory);
            this.services.AddTransient<IPaymentConfigurationProvider>(c => paymentConfigurationProvider.Object);
            var serviceProvider = this.services.BuildServiceProvider();
            this.mediator = serviceProvider.GetService<ICqrsMediator>();
            var creditCardDetails = new CreditCardDetails(
                "4283144693700003",
                "John Smith",
                "11",
                DateTime.Today.Year + 1,
                "123");
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var releaseContext = new ReleaseContext(
                quote.Aggregate.TenantId,
                quote.Aggregate.ProductId,
                quote.Aggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            await this.mediator.Send(new CardPaymentCommand(
                releaseContext,
                quote.Id,
                quote.LatestCalculationResult.Id,
                new FormData("{}"),
                cardDetails: creditCardDetails));

            // Assert
            var updatedQuote = quoteAggregate.GetQuoteOrThrow(quote.Id);
            updatedQuote.LatestPaymentAttemptResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePaymentAsync_Succeeds_ForTestQuote_UsingDeftPaymentGateway()
        {
            // Arrange
            var logger = new Mock<ILogger<PaymentGatewayFactory>>();
            var deftConfiguration = new TestDeftConfiguration();
            var paymentConfigurationProvider = new Mock<IPaymentConfigurationProvider>();
            paymentConfigurationProvider
                .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(deftConfiguration)));

            var productFeatureSettingService = new Mock<IProductFeatureSettingService>();
            var crnGenerator = new Mock<IDeftCustomerReferenceNumberGenerator>();
            crnGenerator
                .Setup(g => g.GenerateDeftCrnNumber(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<CrnGenerationConfiguration>()))
                .Returns("1234567890");

            var accessTokenProvider = new DeftAccessTokenProvider(deftConfiguration, this.clock);
            var deftPaymentGateway = new DeftPaymentGateway(deftConfiguration, accessTokenProvider, crnGenerator.Object, this.clock);
            var paymentGatewayFactory = new PaymentGatewayFactory(
                this.clock, crnGenerator.Object, this.cacheResolver.Object, this.currentUserIdentification.Object, this.mediator, logger.Object);

            var quoteAggregate = this.CreateQuote();
            this.quoteAggregateRepository
                .Setup(r => r.GetById(quoteAggregate.TenantId, quoteAggregate.Id))
                .Returns(quoteAggregate);
            this.quoteAggregateResolverService
                .Setup(r => r.GetQuoteAggregateIdForQuoteId(It.IsAny<Guid>()))
                .Returns(quoteAggregate.Id);
            this.services.AddSingleton<PaymentGatewayFactory>(c => paymentGatewayFactory);
            this.services.AddTransient<IPaymentConfigurationProvider>(c => paymentConfigurationProvider.Object);
            var serviceProvider = this.services.BuildServiceProvider();
            this.mediator = serviceProvider.GetService<ICqrsMediator>();
            var creditCardDetails = new CreditCardDetails(
                "4283144693700003",
                "John Smith",
                "11",
                DateTime.Today.Year + 1,
                "123");
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var releaseContext = new ReleaseContext(
                quote.Aggregate.TenantId,
                quote.Aggregate.ProductId,
                quote.Aggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            await this.mediator.Send(new CardPaymentCommand(
                releaseContext,
                quote.Id,
                quote.LatestCalculationResult.Id,
                new FormData("{}"),
                cardDetails: creditCardDetails));

            // Assert
            var updatedQuote = quoteAggregate.GetQuoteOrThrow(quote.Id);
            updatedQuote.LatestPaymentAttemptResult.IsSuccess.Should().BeTrue();
        }

        private QuoteAggregate CreateQuote()
        {
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                DeploymentEnvironment.Staging,
                QuoteExpirySettings.Default,
                this.currentUserIdentification.Object.PerformingUserId,
                this.clock.Now(),
                Guid.NewGuid(),
                Timezones.AET);
            quote.UpdateFormData(new Domain.Aggregates.Quote.FormData("{}"), this.currentUserIdentification.Object.PerformingUserId, this.clock.GetCurrentInstant());
            this.quoteId = quote.Id;
            var calculationResultJson = @"{
    ""payment"": {
        ""currencyCode"": ""AUD"",
        ""total"": {
            ""premium"": ""$82.64"",
            ""esl"": ""$0"",
            ""gst"": ""$8.26"",
            ""stampDuty"": ""$9.09"",
            ""serviceFees"": ""$0"",
            ""interest"": ""$0"",
            ""merchantFees"": ""$0.39"",
            ""transactionCosts"": ""$0"",
            ""payable"": ""$101.48""
        }
    }
}";

            var formnDataSchema = new FormDataSchema(new JObject());
            var formData = new Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var calculationData = new CachingJObjectWrapper(calculationResultJson);
            quote.UpdateFormData(formData, this.performingUserId, this.clock.Now());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            quote.RecordCalculationResult(
                CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver),
                calculationData,
                this.clock.Now(),
                formnDataSchema,
                false,
                this.currentUserIdentification.Object.PerformingUserId);
            return quote.Aggregate;
        }

        private QuoteAggregate CreateTestQuote()
        {
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                DeploymentEnvironment.Staging,
                QuoteExpirySettings.Default,
                this.currentUserIdentification.Object.PerformingUserId,
                this.clock.GetCurrentInstant(),
                Guid.NewGuid(),
                Timezones.AET,
                false,
                null,
                true);
            quote.UpdateFormData(new Domain.Aggregates.Quote.FormData("{}"), this.performingUserId, this.clock.GetCurrentInstant());
            this.quoteId = quote.Id;
            var calculationResultJson = @"{
    ""payment"": {
        ""currencyCode"": ""AUD"",
        ""total"": {
            ""premium"": ""$82.64"",
            ""esl"": ""$0"",
            ""gst"": ""$8.26"",
            ""stampDuty"": ""$9.09"",
            ""serviceFees"": ""$0"",
            ""interest"": ""$0"",
            ""merchantFees"": ""$1.49"",
            ""transactionCosts"": ""$0"",
            ""payable"": ""$101.48""
        }
    }
}";

            var formnDataSchema = new FormDataSchema(new JObject());
            var formData = new Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var calculationData = new CachingJObjectWrapper(calculationResultJson);
            quote.UpdateFormData(formData, this.performingUserId, this.clock.Now());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            quote.RecordCalculationResult(
                CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver),
                calculationData,
                this.clock.Now(),
                formnDataSchema,
                false,
                this.performingUserId);
            return quote.Aggregate;
        }
    }
}
