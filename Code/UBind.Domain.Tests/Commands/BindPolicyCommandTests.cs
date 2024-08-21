// <copyright file="BindPolicyCommandTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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
    using UBind.Application.Commands.Policy;
    using UBind.Application.Commands.Quote;
    using UBind.Application.CustomPipelines;
    using UBind.Application.CustomPipelines.BindPolicy;
    using UBind.Application.Funding;
    using UBind.Application.Funding.EFundExpress;
    using UBind.Application.Funding.Iqumulate;
    using UBind.Application.Funding.PremiumFunding;
    using UBind.Application.Payment;
    using UBind.Application.Payment.Deft;
    using UBind.Application.Queries.Services;
    using UBind.Application.Queries.Tenant;
    using UBind.Application.Services.Email;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Configuration;
    using UBind.Domain.CustomPipelines.BindPolicy;
    using UBind.Domain.Dto;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Attributes;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Infrastructure;
    using Xunit;
    using FormData = UBind.Domain.Aggregates.Quote.FormData;

    /// <summary>
    /// Unit tests for the <see cref="BindPolicyCommandTests"/>.
    /// </summary>
    public class BindPolicyCommandTests
    {
        private readonly IClock clock = SystemClock.Instance;
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly Tenant tenant = TenantFactory.Create();
        private readonly Product product = ProductFactory.Create();
        private Mock<IQuoteAggregateResolverService> mockQuoteAggregateResolverService;
        private Mock<IQuoteAggregateRepository> mockQuoteAggregateRepository;
        private Mock<IProductFeatureSettingService> mockProductFeatureSettingService;
        private Mock<IPolicyService> mockPolicyService;
        private Mock<IPolicyNumberRepository> mockPolicyNumberRepository;
        private Mock<IUBindDbContext> mockUBindDbContext;
        private Mock<IPaymentConfigurationProvider> mockPaymentConfigurationProvider;
        private Mock<IFundingConfigurationProvider> mockFundingConfigurationProvider;
        private Mock<ITenantRepository> mockTenantRepository;
        private Mock<IProductRepository> mockProductRepository;
        private Mock<IQuoteWorkflowProvider> mockQuoteWorkflowProvider;
        private Mock<IInvoiceNumberRepository> mockInvoiceNumberRepository;
        private Mock<ICreditNoteNumberRepository> mockCreditNoteNumberRepository;
        private Mock<IProductConfigurationProvider> mockProductConfigurationProvider;
        private Mock<ISavedPaymentMethodRepository> mockSavedPaymentMethodRepository;
        private Mock<IFeatureSettingRepository> mockFeatureSettingRepository;
        private Mock<IProductFeatureSettingRepository> mockProductFeatureSettingRepository;
        private Mock<ISystemAlertService> mockSystemAlertService;
        private Mock<IHttpContextPropertiesResolver> mockHttpContextPropertiesResolver;
        private Guid quoteId;
        private ICqrsMediator mediator;
        private CachingResolver cachingResolver;
        private ServiceProvider serviceProvider;

        public BindPolicyCommandTests()
        {
            this.mockQuoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
            this.mockQuoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
            this.mockProductFeatureSettingService = new Mock<IProductFeatureSettingService>();
            this.mockPolicyService = new Mock<IPolicyService>();
            this.mockPolicyService
                .Setup(s => s.IsRenewalAllowedAtTheCurrentTime(It.IsAny<IPolicyReadModelDetails>(), It.IsAny<bool>()))
                .Returns(true);
            this.mockPolicyNumberRepository = new Mock<IPolicyNumberRepository>();
            this.mockPolicyNumberRepository
                .Setup(s => s.GetAvailableForProduct(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>()))
                .Returns(new List<string> { "P00001" });
            this.mockPolicyNumberRepository
                .Setup(x => x.ConsumeForProduct(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>()))
                .Returns("P00001");
            this.mockPolicyNumberRepository
                .Setup(x => x.ConsumeAndSave(It.IsAny<IProductContext>()))
                .Returns("P00001");

            this.mockUBindDbContext = new Mock<IUBindDbContext>();
            var transactionStack = new Stack<TransactionScope>();
            this.mockUBindDbContext.Setup(s => s.TransactionStack).Returns(transactionStack);
            this.mockPaymentConfigurationProvider = new Mock<IPaymentConfigurationProvider>();
            this.mockPaymentConfigurationProvider.Setup(x => x.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(new TestDeftConfiguration() as IPaymentConfiguration)));
            this.mockFundingConfigurationProvider = new Mock<IFundingConfigurationProvider>();
            this.mockTenantRepository = new Mock<ITenantRepository>();
            this.mockTenantRepository.Setup(s => s.GetTenantById(It.IsAny<Guid>())).Returns(this.tenant);
            this.mockProductRepository = new Mock<IProductRepository>();
            this.mockProductRepository
                .Setup(s => s.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false))
                .Returns(this.product);
            this.mockFeatureSettingRepository = new Mock<IFeatureSettingRepository>();
            this.mockProductFeatureSettingRepository = new Mock<IProductFeatureSettingRepository>();
            this.mockQuoteWorkflowProvider = new Mock<IQuoteWorkflowProvider>();
            this.mockQuoteWorkflowProvider
                .Setup(provide => provide.GetConfigurableQuoteWorkflow(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(new FakeQuoteWorkflow() as IQuoteWorkflow));
            this.mockInvoiceNumberRepository = new Mock<IInvoiceNumberRepository>();
            this.mockInvoiceNumberRepository
                .Setup(s => s.ConsumeForProduct(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>()))
                .Returns("INV00012");
            this.mockInvoiceNumberRepository
                .Setup(s => s.ConsumeAndSave(It.IsAny<ProductContext>()))
                .Returns("INV00012");
            this.mockInvoiceNumberRepository
                .Setup(s => s.GetAvailableForProduct(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>()))
                .Returns(new List<string> { "INV00012", "INV00013" });
            this.mockCreditNoteNumberRepository = new Mock<ICreditNoteNumberRepository>();
            this.mockCreditNoteNumberRepository
                .Setup(s => s.ConsumeForProduct(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>()))
                .Returns("CN00012");
            this.mockCreditNoteNumberRepository
                .Setup(s => s.ConsumeAndSave(It.IsAny<ProductContext>()))
                .Returns("CN00012");
            this.mockCreditNoteNumberRepository
                .Setup(s => s.GetAvailableForProduct(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>()))
                .Returns(new List<string>() { "CN00012", "CN00013" });
            this.mockProductConfigurationProvider = new Mock<IProductConfigurationProvider>();
            this.mockProductConfigurationProvider
                .Setup(s => s.GetProductConfiguration(It.IsAny<ReleaseContext>(), It.IsAny<WebFormAppType>()))
                .Returns(Task.FromResult(new DefaultProductConfiguration() as IProductConfiguration));
            this.mockSavedPaymentMethodRepository = new Mock<ISavedPaymentMethodRepository>();
            this.mockQuoteAggregateRepository.Setup(x => x.Save(It.IsAny<QuoteAggregate>()))
                .Returns(Task.CompletedTask);
            this.mockSystemAlertService = new Mock<ISystemAlertService>();
            this.mockHttpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
            var mockedFundingServiceFactory = new FundingServiceFactory(
                this.mockQuoteAggregateResolverService.Object,
                this.clock,
                new Mock<IFundingServiceRedirectUrlHelper>().Object,
                new Mock<ICachingAccessTokenProvider>().Object,
                new Mock<ICachingResolver>().Object,
                new Mock<IIqumulateService>().Object,
                new Mock<IQuoteAggregateRepository>().Object,
                this.mockHttpContextPropertiesResolver.Object,
                new Mock<IServiceProvider>().Object);
            var mockedPaymentGatewayFactory = new PaymentGatewayFactory(
                this.clock,
                new Mock<IDeftCustomerReferenceNumberGenerator>().Object,
                new Mock<ICachingResolver>().Object,
                new Mock<IHttpContextPropertiesResolver>().Object,
                new Mock<ICqrsMediator>().Object,
                new Mock<ILogger<PaymentGatewayFactory>>().Object);
            var accountingTransactionService = new AccountingTransactionService(
                this.mockQuoteWorkflowProvider.Object,
                this.mockCreditNoteNumberRepository.Object,
                this.mockInvoiceNumberRepository.Object,
                this.mockSystemAlertService.Object,
                this.mockHttpContextPropertiesResolver.Object,
                this.mockProductFeatureSettingService.Object,
                this.clock);
            var mockAggregateLockingService = new Mock<IAggregateLockingService>();
            mockAggregateLockingService
                .Setup(a => a.CreateLockOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AggregateType>()))
                .ReturnsAsync(It.IsAny<IRedLock>());
            var services = new ServiceCollection();
            services.RegisterCustomPipelines();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BindPolicyCommand).GetTypeInfo().Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetTenantByIdQuery).GetTypeInfo().Assembly));
            services.AddTransient<IClock>(c => this.clock);
            services.AddSingleton<ICachingAccessTokenProvider>(c => new Mock<ICachingAccessTokenProvider>().Object);
            services.AddTransient<IHttpContextPropertiesResolver>(c => this.mockHttpContextPropertiesResolver.Object);
            services.AddTransient<IQuoteAggregateRepository>(c => this.mockQuoteAggregateRepository.Object);
            services.AddTransient<IQuoteAggregateResolverService>(c => this.mockQuoteAggregateResolverService.Object);
            services.AddTransient<IProductFeatureSettingService>(c => this.mockProductFeatureSettingService.Object);
            services.AddTransient<IPolicyService>(c => this.GetPolicyService());
            services.AddTransient<IQuoteWorkflowProvider>(c => this.mockQuoteWorkflowProvider.Object);
            services.AddTransient<ICachingResolver>(c => this.cachingResolver);
            services.AddTransient<IPolicyNumberRepository>(c => this.mockPolicyNumberRepository.Object);
            services.AddTransient<IUBindDbContext>(_ => this.mockUBindDbContext.Object);
            services.AddTransient<IPaymentConfigurationProvider>(c => this.mockPaymentConfigurationProvider.Object);
            services.AddTransient<IFundingConfigurationProvider>(c => this.mockFundingConfigurationProvider.Object);
            services.AddTransient<ITenantRepository>(c => this.mockTenantRepository.Object);
            services.AddTransient<IProductRepository>(c => this.mockProductRepository.Object);
            services.AddTransient<IInvoiceNumberRepository>(c => this.mockInvoiceNumberRepository.Object);
            services.AddTransient<ICreditNoteNumberRepository>(c => this.mockCreditNoteNumberRepository.Object);
            services.AddSingleton<PaymentGatewayFactory>(c => mockedPaymentGatewayFactory);
            services.AddSingleton<FundingServiceFactory>(c => mockedFundingServiceFactory);
            services.AddTransient<IProductConfigurationProvider>(c => this.mockProductConfigurationProvider.Object);
            services.AddTransient<IPolicyTransactionTimeOfDayScheme>(c => new DefaultPolicyTransactionTimeOfDayScheme());
            services.AddTransient<ISavedPaymentMethodRepository>(c => this.mockSavedPaymentMethodRepository.Object);
            services.AddTransient<ISystemAlertService>(c => this.mockSystemAlertService.Object);
            services.AddTransient<IAdditionalPropertyValueService>(c => new Mock<IAdditionalPropertyValueService>().Object);
            services.AddTransient<IPersonAggregateRepository>(c => new Mock<IPersonAggregateRepository>().Object);
            services.AddTransient<IUnitOfWork>(_ => new UnitOfWork(this.mockUBindDbContext.Object));
            services.AddTransient<IAccountingTransactionService>(c => accountingTransactionService);
            services.AddTransient<IErrorNotificationService>(c => new Mock<IErrorNotificationService>().Object);
            services.AddScoped<IAggregateLockingService>(c => mockAggregateLockingService.Object);
            services.AddSingleton<ICqrsMediator, CqrsMediator>();
            services.AddSingleton<ICqrsRequestContext>(_ => new CqrsRequestContext());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AcceptFundingProposalCommandHandler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IssueInvoiceCommandHandler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IssueCreditNoteCommandHandler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CompletePolicyTransactionCommandHandler).Assembly));
            services.AddLogging(loggingBuilder => loggingBuilder.AddDebug());
            this.serviceProvider = services.BuildServiceProvider();
            this.mediator = this.serviceProvider.GetService<ICqrsMediator>();
            this.cachingResolver = new CachingResolver(
                this.mediator,
                this.mockTenantRepository.Object,
                this.mockProductRepository.Object,
                this.mockFeatureSettingRepository.Object,
                this.mockProductFeatureSettingRepository.Object);

            foreach (var service in services)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"{service.ServiceType.FullName},{service.ImplementationType?.FullName}");
            }
        }

        [Fact]
        public void Mediator_Should_Resolve_Handlers()
        {
            var handlers = this.serviceProvider
                .GetServices<IPipelineBehavior<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>>()
                .ToList();

            handlers.Count.Should().Be(4);
            var handlerTypes = handlers.Select(handler => handler.GetType());
            handlerTypes.Should().Contain(typeof(ValidateBindPolicyCommandHandler<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>));
            handlerTypes.Should().Contain(typeof(AcceptFundingProposalCommandHandler));
            handlerTypes.Should().Contain(typeof(CardPaymentCommandHandler));
            handlerTypes.Should().Contain(typeof(SaveBindCommandHandler<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>));

            var command = this.serviceProvider
                .GetServices<IRequestHandler<AcceptFundingProposalCommand, Unit>>()
                .ToList();
            command.Should().HaveCount(1);
            var commandHandlers = command.Select(handler => handler.GetType());
            commandHandlers.Should().Contain(typeof(AcceptFundingProposalCommandHandler));

            var handlers2 = this.serviceProvider
                .GetServices<IRequestHandler<GetTenantByIdQuery, Domain.Tenant>>()
                .ToList();
            handlers2.Count.Should().Be(1);
            handlers2.Select(handler => handler.GetType())
                .Should().Contain(typeof(GetTenantByIdQueryHandler));
        }

        [SkipDuringLeapDay]
        public async Task Bind_ShouldSucceed_WhenQuoteIsBindable()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            productFeature.UpdateProductFeatureRenewalSetting(true, Duration.FromDays(10));
            this.mockProductFeatureSettingService
                .Setup(x => x.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeature);

            var twelveMonthsAgo = SystemClock.Instance.Today().PlusMonths(-12);
            var quoteAggregate = QuoteFactory.CreateNewPolicy(
            formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(
                inceptionDate: twelveMonthsAgo,
                durationInMonths: 12),
            calculationResultJson: CalculationResultJsonFactory.Create(startDate: twelveMonthsAgo));
            var cachingResolver = new Mock<ICachingResolver>().Object;
            var renewalQuote = quoteAggregate.WithRenewalQuote();
            quoteAggregate = QuoteFactory.WithCalculationResult(QuoteFactory.WithCustomer(quoteAggregate), renewalQuote.Id);
            this.mockQuoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockQuoteAggregateRepository
                .Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);

            var quote = quoteAggregate.GetQuoteOrThrow(renewalQuote.Id);
            var requirements = new BindRequirementDto(quote.LatestCalculationResult.Id);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> action = async () => await this.mediator.Send(
                BindPolicyCommand.CreateForBindingWithQuote(releaseContext, quote.Id, requirements));

            // Assert
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public void Bind_ShouldNotIssueCreditNote_WhenPayableIsGreaterThanZero()
        {
            // Arrange
            var payable = "100";
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            productFeature.UpdateProductFeatureRenewalSetting(true, Duration.FromDays(10));
            this.mockProductFeatureSettingService
                .Setup(x => x.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeature);

            var twoMonthsAgo = SystemClock.Instance.Today().PlusMonths(2);
            var quoteAggregate = QuoteFactory.CreateNewPolicy(
            formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(
                inceptionDate: twoMonthsAgo,
                durationInMonths: 12),
            calculationResultJson: CalculationResultJsonFactory.Create(startDate: twoMonthsAgo));
            var cachingResolver = new Mock<ICachingResolver>().Object;
            var cancellationQuote = quoteAggregate.WithCancellationQuote();
            var calculationResultJson = CalculationResultJsonFactory.CreateWithNewBreakdown(true, payable);
            var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();

            quoteAggregate.WithCalculationResult(cancellationQuote.Id, formData, calculationResultJson);

            this.mockQuoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockQuoteAggregateRepository
                .Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);

            var quote = quoteAggregate.GetQuoteOrThrow(cancellationQuote.Id);

            var requirements = new BindRequirementDto(quote.LatestCalculationResult.Id);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            var action = this.mediator.Send(BindPolicyCommand.CreateForBindingWithQuote(releaseContext, quote.Id, requirements));

            // Assert
            quote.CreditNoteIssued.Should().BeFalse();
            quote.CreditNote.Should().BeNull();
        }

        [Fact]
        public void Bind_ShouldIssueCreditNote_WhenPayableIsLessThanZero()
        {
            // Arrange
            var payable = "-100";
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            productFeature.UpdateProductFeatureRenewalSetting(true, Duration.FromDays(10));
            this.mockProductFeatureSettingService
                .Setup(x => x.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeature);

            var twoMonthsAgo = SystemClock.Instance.Today().PlusMonths(2);
            var quoteAggregate = QuoteFactory.CreateNewPolicy(
            formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(
                inceptionDate: twoMonthsAgo,
                durationInMonths: 12),
            calculationResultJson: CalculationResultJsonFactory.Create(startDate: twoMonthsAgo));
            var cachingResolver = new Mock<ICachingResolver>().Object;
            var cancellationQuote = quoteAggregate.WithCancellationQuote();
            var calculationResultJson = CalculationResultJsonFactory.CreateWithNewBreakdown(true, payable);
            var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();

            quoteAggregate.WithCalculationResult(cancellationQuote.Id, formData, calculationResultJson);

            this.mockQuoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockQuoteAggregateRepository
                .Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);

            var quote = quoteAggregate.GetQuoteOrThrow(cancellationQuote.Id);

            var requirements = new BindRequirementDto(quote.LatestCalculationResult.Id);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            var action = this.mediator.Send(BindPolicyCommand.CreateForBindingWithQuote(releaseContext, quote.Id, requirements));

            // Assert
            quote.CreditNoteIssued.Should().BeTrue();
            quote.CreditNote.CreditNoteNumber.Should().NotBeNull();
        }

        [SkipDuringLeapDay]
        public async Task Bind_ShouldFail_WhenQuoteLatestCalculationResultIsNotSameAsRequest()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            productFeature.UpdateProductFeatureRenewalSetting(true, Duration.FromDays(10));
            this.mockProductFeatureSettingService
                .Setup(x => x.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeature);
            var twelveMonthsAgo = SystemClock.Instance.Today().PlusMonths(-12);
            var quoteAggregate = QuoteFactory.CreateNewPolicy(
            formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(
                inceptionDate: twelveMonthsAgo,
                durationInMonths: 12),
            calculationResultJson: CalculationResultJsonFactory.Create(startDate: twelveMonthsAgo));
            var cachingResolver = new Mock<ICachingResolver>().Object;
            var renewalQuote = quoteAggregate.WithRenewalQuote();
            quoteAggregate = QuoteFactory.WithCalculationResult(QuoteFactory.WithCustomer(quoteAggregate), renewalQuote.Id);
            this.mockQuoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockQuoteAggregateRepository
                .Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            var quote = quoteAggregate.GetQuoteOrThrow(renewalQuote.Id);
            var calculationResultIdOld = quote.LatestCalculationResult.Id;
            var requirements = new BindRequirementDto(calculationResultIdOld);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            // create a new calculation result
            quoteAggregate = QuoteFactory.WithCalculationResult(QuoteFactory.WithCustomer(quoteAggregate), renewalQuote.Id);
            var calculationResultIdNew = quote.LatestCalculationResult.Id;

            // bind with old calculation result id
            Func<Task> action = async () => await this.mediator.Send(
                BindPolicyCommand.CreateForBindingWithQuote(releaseContext, quote.Id, requirements));

            // Assert
            calculationResultIdNew.Should().NotBe(calculationResultIdOld);
            (await action.Should().ThrowAsync<ErrorException>()).And.Error.Code.Should().Be("operation.bind.not.permitted");
        }

        [SkipDuringLeapDay]
        public async Task Bind_ThrowErrorException_WhenPolicyIsExpiredAndRenewalAfterExpiryIsDisabled()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            this.mockProductFeatureSettingService.Setup(e => e.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeature);

            productFeature.UpdateProductFeatureRenewalSetting(false, Duration.FromDays(0));
            var policyId = Guid.NewGuid();
            var twelveMonthsAgo = SystemClock.Instance.Today().PlusMonths(-12);

            var quoteAggregate = QuoteFactory.CreateNewPolicy(
            formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(
                inceptionDate: twelveMonthsAgo,
                durationInMonths: 11),
            calculationResultJson: CalculationResultJsonFactory.Create(startDate: twelveMonthsAgo));

            var renewalQuote = quoteAggregate.WithRenewalQuote(
                createdTimestamp: this.clock.Today().PlusDays(-1).GetInstantAt4pmAet());
            var quote = quoteAggregate.GetQuoteOrThrow(renewalQuote.Id);
            var formData = FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDates(
                inceptionDate: SystemClock.Instance.Today().PlusMonths(-1),
                effectivedateOffsetInMonths: 0);
            quoteAggregate = QuoteFactory.WithCalculationResult(
                QuoteFactory.WithCustomer(quoteAggregate), renewalQuote.Id, formData);
            this.mockQuoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockQuoteAggregateRepository
                .Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            var requirements = new BindRequirementDto(quote.LatestCalculationResult.Id);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> act = async () => await this.mediator.Send(BindPolicyCommand.CreateForBindingWithQuote(
                releaseContext, quote.Id, requirements));

            // Assert that it throws exception
            (await act.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("expired.policy.renewal.not.allowed");
        }

        [Fact]
        public async Task Bind_ThrowErrorException_WhenInceptionDateIsInvalid()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            this.mockProductFeatureSettingService.Setup(e => e.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeature);

            productFeature.UpdateProductFeatureRenewalSetting(false, Duration.FromDays(0));
            var policyId = Guid.NewGuid();
            var thirteenMonthsAgo = SystemClock.Instance.Today().PlusMonths(-13);
            var cachingResolver = new Mock<ICachingResolver>().Object;

            var quoteAggregate = QuoteFactory.CreateNewPolicy(
            formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(
                inceptionDate: thirteenMonthsAgo,
                durationInMonths: 12),
            calculationResultJson: CalculationResultJsonFactory.Create(startDate: thirteenMonthsAgo));

            var renewQuote = quoteAggregate.WithRenewalQuote();
            var quote = quoteAggregate.GetQuoteOrThrow(renewQuote.Id);
            quoteAggregate = QuoteFactory.WithCalculationResult(
                QuoteFactory.WithCustomer(quoteAggregate), renewQuote.Id, FormDataJsonFactory.GetSampleWithStartAndEndDates());
            this.mockQuoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockQuoteAggregateRepository
                .Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            var requirements = new BindRequirementDto(quote.LatestCalculationResult.Id);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> act = async () => await this.mediator.Send(BindPolicyCommand.CreateForBindingWithQuote(
                releaseContext, quote.Id, requirements));

            // Assert that it throws exception
            (await act.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("policy.renewal.date.time.must.match.expiry");
        }

        [Fact]
        public async Task Bind_ThrowErrorException_WhenPolicyIsExpiredAndExpiryDateIsNotWithInAllowableRenewalPeriod()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            productFeature.UpdateProductFeatureRenewalSetting(false, Duration.FromDays(10));
            this.mockProductFeatureSettingService.Setup(e => e.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeature);

            var today = new LocalDate(2024, 03, 28);
            var thirteenMonthsAgo = today.PlusMonths(-13);
            var quoteAggregate = QuoteFactory.CreateNewPolicy(
                formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(
                    inceptionDate: thirteenMonthsAgo,
                    durationInMonths: 12),
                calculationResultJson: CalculationResultJsonFactory.Create(startDate: thirteenMonthsAgo));
            var renewalQuote = quoteAggregate.WithRenewalQuote();
            var formData = FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDates(
                        inceptionDate: today.PlusMonths(-1),
                        effectivedateOffsetInMonths: 0);
            quoteAggregate = QuoteFactory.WithCalculationResult(
                QuoteFactory.WithCustomer(quoteAggregate), renewalQuote.Id, formData);
            this.mockQuoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockQuoteAggregateRepository
                .Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            var quote = quoteAggregate.GetQuoteOrThrow(renewalQuote.Id);
            var requirements = new BindRequirementDto(quote.LatestCalculationResult.Id);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> act = async () => await this.mediator.Send(BindPolicyCommand.CreateForBindingWithQuote(
                releaseContext, quote.Id, requirements));

            // Assert that it throws exception
            (await act.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("expired.policy.renewal.not.allowed");
        }

        [Fact]
        public async Task Bind_DoesNotThrowErrorException_WhenPolicyExpiryDoesntMatchInceptionDueToLeapYearDifference()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            productFeature.UpdateProductFeatureRenewalSetting(false, Duration.FromDays(10));
            this.mockProductFeatureSettingService.Setup(e => e.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeature);

            var inceptionDate = new LocalDate(2024, 02, 29);
            var quoteAggregate = QuoteFactory.CreateNewPolicy(
                formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(
                    inceptionDate: inceptionDate,
                    durationInMonths: 12),
                calculationResultJson: CalculationResultJsonFactory.Create(startDate: inceptionDate));
            var renewalQuote = quoteAggregate.WithRenewalQuote();
            var formData = FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDates(
                        inceptionDate: inceptionDate.PlusMonths(12),
                        effectivedateOffsetInMonths: 0);
            quoteAggregate = QuoteFactory.WithCalculationResult(
                QuoteFactory.WithCustomer(quoteAggregate), renewalQuote.Id, formData);
            this.mockQuoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockQuoteAggregateRepository
                .Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            var quote = quoteAggregate.GetQuoteOrThrow(renewalQuote.Id);
            var requirements = new BindRequirementDto(quote.LatestCalculationResult.Id);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> act = async () => await this.mediator.Send(BindPolicyCommand.CreateForBindingWithQuote(
                releaseContext, quote.Id, requirements));

            // Assert that it throws exception
            await act.Should().NotThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task Bind_Fails_WhenSettlementIsRequiredAndNoPaymentDetailsArePresent()
        {
            var tenant = TenantFactory.Create();
            var productId = Guid.NewGuid();
            var quoteAggregate = this.CreateQuote(tenant.Id, productId)
                .WithCalculationResult(this.quoteId, FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            this.mockQuoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockQuoteAggregateRepository
                .Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            var quoteWorkflow = new FakeQuoteWorkflow()
            {
                IsSettlementRequired = true,
            };
            this.mockQuoteWorkflowProvider
                .Setup(provide => provide.GetConfigurableQuoteWorkflow(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(quoteWorkflow as IQuoteWorkflow));
            var requirements = new BindRequirementDto(quote.LatestCalculationResult.Id);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> action = async () => await this.mediator.Send(
                BindPolicyCommand.CreateForBindingWithQuote(releaseContext, quote.Id, requirements));

            // Assert
            (await action.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("operation.bind.settlement.required");
        }

        [Fact]
        public async Task Bind_FailsWhenCalculationResultIsNotBindable()
        {
            var quoteAggregate = this.CreateQuote(this.tenant.Id, this.product.Id);
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var formData = new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            Guid formUpdateId = default;
            formUpdateId = quote.UpdateFormData(formData, this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create(false));
            formUpdateId = quote.UpdateFormData(formData, this.performingUserId, this.clock.Now());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formUpdateId;
            quote.RecordCalculationResult(
                calculationResult,
                calculationData,
                SystemClock.Instance.GetCurrentInstant(),
                new FormDataSchema(new JObject()),
                false,
                this.performingUserId);
            var requirements = new BindRequirementDto(quote.LatestCalculationResult.Id);
            var paymentMethodDetails = new CreditCardDetails("4111111111111111", "John Doe", "Dec", 2020, "111");
            this.mockQuoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockQuoteAggregateRepository
                .Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> action = async () => await this.mediator.Send(BindPolicyCommand.CreateForBindingWithQuote(
                releaseContext, quote.Id, requirements, null, paymentMethodDetails));

            // Assert
            (await action.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("operation.bind.invalid.calculation.state");
        }

        [SkipDuringLeapDay]
        public async Task Bind_ThrowErrorException_WhenThereAreResourceNumbersAreExhausted()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            productFeature.UpdateProductFeatureRenewalSetting(true, Duration.FromDays(10));
            this.mockProductFeatureSettingService
                .Setup(x => x.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeature);

            var twelveMonthsAgo = SystemClock.Instance.Today().PlusMonths(-12);
            var quoteAggregate = QuoteFactory.CreateNewPolicy(
            formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(
                inceptionDate: twelveMonthsAgo,
                durationInMonths: 12),
            calculationResultJson: CalculationResultJsonFactory.Create(startDate: twelveMonthsAgo));
            var cachingResolver = new Mock<ICachingResolver>().Object;
            var renewalQuote = quoteAggregate.WithRenewalQuote();
            quoteAggregate = QuoteFactory.WithCalculationResult(QuoteFactory.WithCustomer(quoteAggregate), renewalQuote.Id);
            this.mockQuoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockQuoteAggregateRepository
                .Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockInvoiceNumberRepository
                .Setup(s => s.GetAvailableForProduct(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>()))
                .Returns(new List<string>());

            var quote = quoteAggregate.GetQuoteOrThrow(renewalQuote.Id);
            var requirements = new BindRequirementDto(quote.LatestCalculationResult.Id);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> action = async () => await this.mediator.Send(
                BindPolicyCommand.CreateForBindingWithQuote(releaseContext, quote.Id, requirements));

            // Assert
            (await action.Should().ThrowAsync<ReferenceNumberUnavailableException>())
                .And.Error.Code.Should().Be("number.pool.none.available");
        }

        [Fact]
        public async Task Bind_ThrowErrorException_WhenPaymentFailed()
        {
            var tenant = TenantFactory.Create();
            var productId = Guid.NewGuid();
            var quoteAggregate = this.CreateQuote(tenant.Id, productId)
                .WithCalculationResult(this.quoteId, FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            this.mockQuoteAggregateResolverService
                .Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.mockQuoteAggregateRepository
                .Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            var quoteWorkflow = new FakeQuoteWorkflow()
            {
                IsSettlementRequired = true,
            };
            this.mockQuoteWorkflowProvider
                .Setup(provide => provide.GetConfigurableQuoteWorkflow(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(quoteWorkflow as IQuoteWorkflow));
            var requirements = new BindRequirementDto(quote.LatestCalculationResult.Id);

            var cardDetails = new CreditCardDetails(
                "4283144693700003",
                "John Smith",
                "11",
                10,
                "123");
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                this.product.Id,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> action = async () => await this.mediator.Send(
                BindPolicyCommand.CreateForBindingWithQuote(releaseContext, quote.Id, requirements, null, cardDetails));

            // Assert
            (await action.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("payment.card.payment.failed");
            var failedQuote = quoteAggregate.GetQuoteOrThrow(this.quoteId);

            failedQuote.LatestPaymentAttemptResult.IsSuccess.Should().BeFalse();
        }

        private IPolicyService GetPolicyService()
        {
            var mockTenantRepository = new Mock<ITenantRepository>();
            var tenant = new Tenant(TenantFactory.DefaultId, "foo", "bar", null, default, default, this.clock.Now());
            mockTenantRepository.Setup(e => e.GetTenantById(TenantFactory.DefaultId)).Returns(tenant);

            var mockcachingResolver = new Mock<ICachingResolver>();
            var mockProductRepository = new Mock<IProductRepository>();
            var product = new Product(TenantFactory.DefaultId, ProductFactory.DefaultId, "bar", "bar", this.clock.Now());
            mockProductRepository.Setup(e => e.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(product);
            mockcachingResolver.Setup(e => e.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(e => e.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(e => e.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(e => e.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            var quoteAggregate = QuoteFactory.CreateNewPolicy();
            var adjustmentQuote = quoteAggregate.WithAdjustmentQuote();

            ////this.mockQuoteAggregateResolverService.Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(quoteAggregate);
            var mockUBindDbContext = new Mock<IUBindDbContext>();
            var transactionStack = new Stack<TransactionScope>();
            mockUBindDbContext.Setup(s => s.TransactionStack).Returns(transactionStack);
            var policyService = new PolicyService(
                this.mockQuoteAggregateRepository.Object,
                this.clock,
                mockcachingResolver.Object,
                new Mock<IClaimReadModelRepository>().Object,
                this.mockProductConfigurationProvider.Object,
                new Mock<IUniqueIdentifierService>().Object,
                new Mock<IQuoteDocumentReadModelRepository>().Object,
                new Mock<IPolicyNumberRepository>().Object,
                new Mock<IPolicyReadModelRepository>().Object,
                new Mock<ISystemAlertService>().Object,
                new Mock<IQuoteReferenceNumberGenerator>().Object,
                new Mock<IHttpContextPropertiesResolver>().Object,
                this.mockProductFeatureSettingService.Object,
                this.mockQuoteWorkflowProvider.Object,
                new Mock<IPolicyTransactionTimeOfDayScheme>().Object,
                mockUBindDbContext.Object);
            return policyService;
        }
        private QuoteAggregate CreateQuote(Guid tenantId, Guid productId)
        {
            var formDataSchema = new FormDataSchema(new JObject());
            var timestamp = SystemClock.Instance.GetCurrentInstant();
            var organisationId = Guid.NewGuid();
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                tenantId,
                organisationId,
                productId,
                DeploymentEnvironment.Staging,
                QuoteExpirySettings.Default,
                this.performingUserId,
                timestamp,
                Guid.NewGuid());
            this.quoteId = quote.Id;
            var quoteAggregate = quote.Aggregate;
            var formData = new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var personalDetails = new FakePersonalDetails();
            var person = PersonAggregate.CreatePersonFromPersonalDetails(
                tenantId,
                organisationId,
                personalDetails,
                null,
                timestamp);
            var customer = CustomerAggregate.CreateNewCustomer(
                TenantFactory.DefaultId,
                person,
                DeploymentEnvironment.Staging,
                this.performingUserId,
                null,
                timestamp);
            var formDataUpdateId = quote.UpdateFormData(formData, this.performingUserId, timestamp);
            quoteAggregate.UpdateCustomerDetails(personalDetails, this.performingUserId, timestamp, quote.Id);
            quoteAggregate.RecordAssociationWithCustomer(customer, personalDetails, this.performingUserId, timestamp);
            quote.AssignQuoteNumber("ABCDEF", this.performingUserId, timestamp);
            var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataUpdateId;
            quote.RecordCalculationResult(
                calculationResult,
                calculationData,
                timestamp,
                formDataSchema,
                false,
                this.performingUserId);
            return quoteAggregate;
        }

        private class FakeQuoteWorkflow : DefaultQuoteWorkflow, IQuoteWorkflow
        {
            public new bool IsSettlementRequired { get; set; }

            public new decimal? PremiumThresholdRequiringSettlement => 1000.0m;
        }
    }
}
