// <copyright file="IssueInvoiceCommandTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Transactions;
    using FluentAssertions;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using RedLockNet;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Releases;
    using UBind.Application.Services.Email;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Infrastructure;
    using Xunit;
    using IClock = NodaTime.IClock;

    public class IssueInvoiceCommandTests
    {
        private readonly Tenant tenant = TenantFactory.Create();
        private readonly Product product;
        private readonly IClock clock = new TestClock();
        private readonly IAccountingTransactionService accountingTransactionService;
        private Mock<IProductRepository> productRepository;
        private Mock<IProductFeatureSettingRepository> productFeatureRepository;
        private Mock<IInvoiceNumberRepository> invoiceNumberRepository;
        private Mock<ICachingResolver> cachingResolver = new Mock<ICachingResolver>();

        public IssueInvoiceCommandTests()
        {
            this.productRepository = new Mock<IProductRepository>();
            this.productFeatureRepository = new Mock<IProductFeatureSettingRepository>();
            this.invoiceNumberRepository = new Mock<IInvoiceNumberRepository>();
            this.invoiceNumberRepository
                .Setup(x => x.ConsumeAndSave(It.IsAny<ProductContext>()))
                .Returns("INV-0021");
            var productFeatureSettingService = new ProductFeatureSettingService(
                this.productFeatureRepository.Object,
                this.clock,
                this.cachingResolver.Object);
            this.accountingTransactionService = new AccountingTransactionService(
                new DefaultQuoteWorkflowProvider(),
                new Mock<ICreditNoteNumberRepository>().Object,
                this.invoiceNumberRepository.Object,
                new Mock<ISystemAlertService>().Object,
                new Mock<IHttpContextPropertiesResolver>().Object,
                productFeatureSettingService,
                this.clock);
            this.product = ProductFactory.Create(this.tenant.Id);
        }

        [Fact]
        public async Task IssueInvoiceCommand_ShouldSucceed_WhenProductFeatureEnabled()
        {
            // Arrange
            var quoteAggregate = QuoteFactory.CreateNewPolicy(this.tenant.Id);
            var adjustmentQuote = quoteAggregate.WithAdjustmentQuote();
            adjustmentQuote.Aggregate.WithCalculationResult(adjustmentQuote.Id);
            var productFeature = new ProductFeatureSetting(this.tenant.Id, quoteAggregate.ProductId, this.clock.Now());
            productFeature.Enable(ProductFeatureSettingItem.NewBusinessPolicyTransactions);
            productFeature.Enable(ProductFeatureSettingItem.AdjustmentPolicyTransactions);
            this.cachingResolver
                .Setup(e => e.GetProductSettingOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(productFeature);
            var mediator = this.GetMediator(quoteAggregate);

            // Act
            var quoteReadModel = await mediator.Send(
                new IssueInvoiceCommand(
                    this.tenant.Id,
                    adjustmentQuote.Id));

            // Assert
            adjustmentQuote.InvoiceNumber.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task IssueInvoiceCommand_ShouldNotThrowErrorException_WhenProductFeatureDisabled()
        {
            // Arrange
            var quoteAggregate = QuoteFactory.CreateNewPolicy(this.tenant.Id, this.product.Id);
            var adjustmentQuote = quoteAggregate.WithAdjustmentQuote();
            var productFeature = new ProductFeatureSetting(this.tenant.Id, quoteAggregate.ProductId, this.clock.Now());
            productFeature.Disable(ProductFeatureSettingItem.AdjustmentPolicyTransactions);
            this.cachingResolver
                .Setup(e => e.GetProductSettingOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(productFeature);
            var mediator = this.GetMediator(quoteAggregate);

            // Act
            Func<Task> act = async () => await mediator.Send(
                new IssueInvoiceCommand(
                    this.tenant.Id,
                    adjustmentQuote.Id));

            // Assert
            await act.Should().NotThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task IssueInvoiceCommand_ShouldThrowException_WhenThereAreNoMoreResourceNumbersAvailable()
        {
            // Arrange
            var quoteAggregate = QuoteFactory.CreateNewPolicy(this.tenant.Id);
            var adjustmentQuote = quoteAggregate.WithAdjustmentQuote();
            var productFeature = new ProductFeatureSetting(this.tenant.Id, quoteAggregate.ProductId, this.clock.Now());
            productFeature.Enable(ProductFeatureSettingItem.NewBusinessPolicyTransactions);
            productFeature.Enable(ProductFeatureSettingItem.AdjustmentPolicyTransactions);
            this.cachingResolver
                .Setup(e => e.GetProductSettingOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(productFeature);
            this.invoiceNumberRepository
                .Setup(x => x.ConsumeAndSave(It.IsAny<ProductContext>()))
                .Throws(new ReferenceNumberUnavailableException(Errors.NumberPool.NoneAvailable(quoteAggregate.TenantId.ToString(), quoteAggregate.ProductId.ToString(), "invoice")));
            var mediator = this.GetMediator(quoteAggregate);

            // Act
            Func<Task> act = async () => await mediator.Send(
               new IssueInvoiceCommand(
                   this.tenant.Id,
                   adjustmentQuote.Id));

            // Assert
            (await act.Should().ThrowAsync<ReferenceNumberUnavailableException>())
               .And.Error.Code.Should().Be("number.pool.none.available");
        }

        private ICqrsMediator GetMediator(QuoteAggregate aggregate)
        {
            var quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
            quoteAggregateRepository.Setup(x => x.GetById(this.tenant.Id, It.IsAny<Guid>()))
                .Returns(aggregate);
            var quoteAggregateResolver = new Mock<IQuoteAggregateResolverService>();
            quoteAggregateResolver.Setup(x => x.GetQuoteAggregateIdForQuoteId(It.IsAny<Guid>()))
                .Returns(aggregate.Id);
            var mockReleaseQueryService = new Mock<IReleaseQueryService>();
            var releaseContext = new ReleaseContext(
                aggregate.TenantId,
                aggregate.ProductId,
                aggregate.Environment,
                Guid.NewGuid());
            mockReleaseQueryService.Setup(e => e.GetReleaseContextForReleaseOrDefaultRelease(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>())).Returns(releaseContext);
            this.productRepository.Setup(e => e.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(this.product);
            this.cachingResolver.Setup(s => s.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(this.product);

            var mockAggregateLockingService = new Mock<IAggregateLockingService>();
            mockAggregateLockingService
                .Setup(a => a.CreateLockOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AggregateType>()))
                .ReturnsAsync(It.IsAny<IRedLock>());

            var mediator = new Mock<ICqrsMediator>();
            var transactionScope = new Stack<TransactionScope>();
            var dbContext = new Mock<IUBindDbContext>();
            dbContext.Setup(s => s.TransactionStack).Returns(transactionScope);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IQuoteAggregateRepository>(c => quoteAggregateRepository.Object);
            serviceCollection.AddTransient<IClock>(c => this.clock);
            serviceCollection.AddTransient<IInvoiceNumberRepository>(c => this.invoiceNumberRepository.Object);
            serviceCollection.AddTransient<ISystemAlertService>(c => new Mock<ISystemAlertService>().Object);
            serviceCollection.AddTransient<IHttpContextPropertiesResolver>(c => new Mock<IHttpContextPropertiesResolver>().Object);
            serviceCollection.AddTransient<IQuoteAggregateResolverService>(c => quoteAggregateResolver.Object);
            serviceCollection.AddTransient<IProductFeatureSettingService>(c => this.GetProductFeatureService());
            serviceCollection.AddTransient<IQuoteWorkflowProvider>(c => new DefaultQuoteWorkflowProvider());
            serviceCollection.AddTransient<IAccountingTransactionService>(c => this.accountingTransactionService);
            serviceCollection.AddTransient<IReleaseQueryService>(c => mockReleaseQueryService.Object);
            serviceCollection.AddTransient<IProductRepository>(c => this.productRepository.Object);
            serviceCollection.AddTransient<IUBindDbContext>(c => dbContext.Object);
            serviceCollection.AddTransient<IUnitOfWork>(c => new UnitOfWork(dbContext.Object));
            serviceCollection.AddTransient<IErrorNotificationService>(c => new Mock<IErrorNotificationService>().Object);
            serviceCollection.AddSingleton<ICqrsMediator, CqrsMediator>();
            serviceCollection.AddSingleton<ICqrsRequestContext>(_ => new CqrsRequestContext());
            serviceCollection.AddScoped<IAggregateLockingService>(c => mockAggregateLockingService.Object);
            serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IssueInvoiceCommandHandler).Assembly));
            serviceCollection.AddLogging(loggingBuilder => loggingBuilder.AddDebug());
            var services = serviceCollection.BuildServiceProvider();
            return services.GetService<ICqrsMediator>();
        }

        private ProductFeatureSettingService GetProductFeatureService()
        {
            var tenant = TenantFactory.Create(this.tenant.Id);
            Mock<DevRelease> devRelease = new Mock<DevRelease>();
            this.productRepository.Setup(e => e.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(this.product);
            var productSummary = new Mock<IProductSummary>();
            var deploymentSetting = new ProductDeploymentSetting();
            deploymentSetting.Development = new List<string> { DeploymentEnvironment.Development.ToString() };
            var productDetails = new ProductDetails(this.product.Details.Name, this.product.Details.Alias, false, false, this.clock.GetCurrentInstant(), deploymentSetting);
            productSummary.Setup(p => p.Details).Returns(productDetails);
            productSummary.Setup(p => p.Id).Returns(this.product.Id);
            IEnumerable<IProductSummary> productSummaries = new List<IProductSummary>() { productSummary.Object };
            this.productRepository.Setup(p => p.GetAllActiveProductSummariesForTenant(this.tenant.Id)).Returns(productSummaries);
            var productFeatureService = new ProductFeatureSettingService(
                this.productFeatureRepository.Object,
                this.clock,
                this.cachingResolver.Object);
            return productFeatureService;
        }
    }
}
