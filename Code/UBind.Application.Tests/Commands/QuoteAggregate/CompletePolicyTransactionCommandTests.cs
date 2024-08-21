// <copyright file="CompletePolicyTransactionCommandTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands.QuoteAggregate
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
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using RedLockNet;
    using UBind.Application.Commands.Policy;
    using UBind.Application.Releases;
    using UBind.Application.Services;
    using UBind.Application.Services.Email;
    using UBind.Application.Tests.Fakes;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Commands.Policy;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Infrastructure;
    using Xunit;
    using FormData = UBind.Domain.Aggregates.Quote.FormData;

    public class CompletePolicyTransactionCommandTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly TestClock clock = new TestClock();
        private readonly Tenant tenant = TenantFactory.Create();
        private Guid quoteId;

        public CompletePolicyTransactionCommandTests()
        {
        }

        [Fact]
        public async Task CompletePolicyTransaction_ForNewBusiness_Succeeds()
        {
            // Arrange
            var quoteAggregate = this.CreateNewBusinessPolicy(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var quote = quoteAggregate.GetLatestQuote();
            var command = new CompletePolicyTransactionCommand(
                this.tenant.Id,
                this.quoteId,
                quote.LatestCalculationResult.Id,
                quote.LatestFormData.Data);
            var mediator = this.GetMediator(quoteAggregate);

            // Act
            var quoteReadModel = await mediator.Send(command);
            var latestQuote = quoteAggregate.GetLatestQuote();

            // Assert
            latestQuote.TransactionCompleted.Should().BeTrue();
            latestQuote.QuoteNumber.Should().Be("TestFoo");
            quoteAggregate.Policy.Should().NotBeNull();
        }

        [Fact]
        public async Task CompletePolicyTransaction_ForAdjustment_Succeeds()
        {
            // Arrange
            var quoteAggregate = QuoteFactory.CreateNewPolicy(this.tenant.Id);
            var adjustmentQuote = quoteAggregate.WithAdjustmentQuote();
            var formDataSchema = new FormDataSchema(new JObject());
            var quote = quoteAggregate.GetQuoteOrThrow(adjustmentQuote.Id);
            var formData = new FormData(FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDates());
            var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create(includeEffectiveDate: true));
            var formDataId = quote.UpdateFormData(formData, this.performingUserId, SystemClock.Instance.Now());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataId;
            quote.RecordCalculationResult(
                calculationResult,
                calculationData,
                SystemClock.Instance.Now(),
                formDataSchema,
                false,
                this.performingUserId);

            var mediator = this.GetMediator(quoteAggregate);
            var command = new CompletePolicyTransactionCommand(
                this.tenant.Id,
                quote.Id,
                quote.LatestCalculationResult.Id,
                formData);

            // Act
            var quoteReadModel = await mediator.Send(command);

            // Assert
            var today = this.clock.GetCurrentInstant().InZone(Timezones.AET).Date;
            var expectedEffectiveDate = today.PlusMonths(6);
            quoteAggregate.Policy.AdjustmentEffectiveDateTime?.Date.Should().Be(expectedEffectiveDate);
            quoteAggregate.GetLatestQuote().TransactionCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task CompletePolicyTransaction_ForAdjustment_FailsForExpiredPolicy()
        {
            // Arrange
            var quoteAggregate = this.CreateNewBusinessPolicy(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var adjustmentQuote = quoteAggregate
                .WithPolicy(quoteAggregate.GetLatestQuote().Id)
                .WithAdjustmentQuote();
            var quote = quoteAggregate.GetQuoteOrThrow(adjustmentQuote.Id);
            quoteAggregate.WithCalculationResult(quote.Id);

            var today = SystemClock.Instance.Now().InZone(Timezones.AET).Date;
            var dayAfterExpiry = today.PlusYears(1).PlusDays(1);
            this.clock.SetAetTime(Timezones.AET.AtStartOfDay(dayAfterExpiry).LocalDateTime);
            var command = new CompletePolicyTransactionCommand(
                this.tenant.Id,
                quote.Id,
                quote.LatestCalculationResult.Id,
                quote.LatestFormData.Data);
            var mediator = this.GetMediator(quoteAggregate);

            // Act
            Func<Task> act = async () => await mediator.Send(command);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("policy.adjustment.policy.has.expired");
        }

        [Fact]
        public async Task CompletePolicyTransaction_ForCancellation_Succeeds()
        {
            var quoteAggregate = QuoteFactory.CreateNewPolicy(this.tenant.Id);
            var cancellationQuote = quoteAggregate.WithCancellationQuote();
            var quote = quoteAggregate.GetQuoteOrThrow(cancellationQuote.Id);
            var formDataSchema = new FormDataSchema(new JObject());
            var formData = new FormData(FormDataJsonFactory.GetSampleWithStartEndAndEffectiveAndCancellationDatesInDays());
            var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create(includeEffectiveDate: true));
            var formDataId = quote.UpdateFormData(formData, this.performingUserId, SystemClock.Instance.Now());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataId;
            quote.RecordCalculationResult(
                calculationResult,
                calculationData,
                SystemClock.Instance.Now(),
                formDataSchema,
                false,
                this.performingUserId);

            var mediator = this.GetMediator(quoteAggregate);
            var command = new CompletePolicyTransactionCommand(
                this.tenant.Id,
                quote.Id,
                quote.LatestCalculationResult.Id,
                formData);

            // Act
            var quoteReadModel = await mediator.Send(command);

            // assert
            var expectedEffectiveDate = this.clock.GetCurrentInstant().InZone(Timezones.AET).Date;
            quoteAggregate.Policy.LatestPolicyPeriodStartDateTime.Date.Should().Be(expectedEffectiveDate);
            quoteAggregate.Policy.CancellationEffectiveDateTime.HasValue.Should().BeTrue();
        }

        [Fact]
        public async Task CompletePolicyTransaction_ForCancellation_FailsForExpiredPolicy()
        {
            // Arrange
            var quoteAggregate = this.CreateNewBusinessPolicy(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var cancellationQuote = quoteAggregate.WithPolicy(quoteAggregate.GetLatestQuote().Id)
                .WithCancellationQuote();
            quoteAggregate.WithCalculationResult(cancellationQuote.Id, formDataJson: FormDataJsonFactory.GetSampleWithStartEndAndEffectiveAndCancellationDatesInDays());
            var dayAfterExpiry = this.clock.GetCurrentInstant().InZone(Timezones.AET).Date.PlusYears(1).PlusDays(1);
            this.clock.SetAetTime(Timezones.AET.AtStartOfDay(dayAfterExpiry).LocalDateTime);

            var mediator = this.GetMediator(quoteAggregate);
            var command = new CompletePolicyTransactionCommand(
                this.tenant.Id,
                cancellationQuote.Id,
                cancellationQuote.LatestCalculationResult.Id,
                cancellationQuote.LatestFormData.Data);

            // Act
            Func<Task> act = async () => await mediator.Send(command);

            // Assert
            (await act.Should().ThrowAsync<InvalidOperationException>())
                .And.Message.Should().Be("Cannot adjust an expired or cancelled policy.");
        }

        /// <summary>
        /// The CreateNewBusinessPolicy.
        /// </summary>
        /// <param name="formDataJson">The formDataJson<see cref="string"/>.</param>
        /// <returns>The <see cref="Task{QuoteAggregate}"/>.</returns>
        private QuoteAggregate CreateNewBusinessPolicy(string formDataJson)
        {
            var formData = new FormData(formDataJson);
            DeploymentEnvironment env = DeploymentEnvironment.Development;
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                env,
                QuoteExpirySettings.Default,
                this.performingUserId,
                this.clock.GetCurrentInstant(),
                Guid.NewGuid(),
                Timezones.AET);
            var quoteAggregate = quote.Aggregate.WithCustomer();
            quote.UpdateFormData(formData, this.performingUserId, this.clock.GetCurrentInstant());
            var customerDetails = new Mock<IPersonalDetails>().SetupAllProperties();
            quoteAggregate.UpdateCustomerDetails(customerDetails.Object, this.performingUserId, this.clock.GetCurrentInstant(), quote.Id);
            var formDataSchema = new FormDataSchema(new JObject());

            var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create());
            var formDataId = quote.UpdateFormData(formData, this.performingUserId, this.clock.GetCurrentInstant());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataId;
            quote.RecordCalculationResult(
                calculationResult,
                calculationData,
                this.clock.GetCurrentInstant(),
                formDataSchema,
                false,
                this.performingUserId);
            quote.AssignQuoteNumber(
                "TestFoo", this.performingUserId, this.clock.GetCurrentInstant());
            quoteAggregate.CreateVersion(this.performingUserId, this.clock.GetCurrentInstant(), quote.Id);
            this.quoteId = quote.Id;
            return quoteAggregate;
        }

        private ICqrsMediator GetMediator(QuoteAggregate quoteAggregate)
        {
            var policyReadModelRepository = new Mock<IPolicyReadModelRepository>();
            var productFeatureSettingService = new Mock<IProductFeatureSettingService>();
            var quoteReadModelRepository = new Mock<IQuoteReadModelRepository>();
            var userRepository = new Mock<IUserAggregateRepository>();
            var tenantRepository = new Mock<ITenantRepository>();
            var policyNumberSource = new Mock<IPolicyNumberRepository>();
            var systemAlertService = new Mock<ISystemAlertService>();
            var quoteDocumentRepository = new Mock<IQuoteDocumentReadModelRepository>();
            var applicationQuoteService = new Mock<IApplicationQuoteService>();
            var claimReadModelRepository = new Mock<IClaimReadModelRepository>();
            var productConfigurationProvider = new DefaultProductConfigurationProvider();
            var personAggregateRepository = new Mock<IPersonAggregateRepository>();
            var organisationService = new Mock<IOrganisationService>();
            var additionalPropertyValueServiceMock = new Mock<IAdditionalPropertyValueService>();
            var mediator = new Mock<ICqrsMediator>();
            var transactionScope = new Stack<TransactionScope>();
            var mockDbContext = new Mock<IUBindDbContext>();
            mockDbContext.Setup(s => s.TransactionStack).Returns(transactionScope);
            var defaultQuoteWorkflow = new DefaultQuoteWorkflow() as IQuoteWorkflow;
            var cachingResolver = new Mock<ICachingResolver>();
            cachingResolver.Setup(x => x.GetTenantAliasOrThrowAsync(this.tenant.Id)).Returns(Task.FromResult(this.tenant.Details.Alias));
            var contextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
            contextPropertiesResolver.Setup(x => x.PerformingUserId).Returns(this.performingUserId);
            var quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
            quoteAggregateRepository
                .Setup(r => r.GetById(quoteAggregate.TenantId, quoteAggregate.Id))
                .Returns(quoteAggregate);
            var quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
            quoteAggregateResolverService.Setup(s => s.GetQuoteAggregateIdForQuoteId(It.IsAny<Guid>())).Returns(quoteAggregate.Id);
            var quoteWorkflowProvider = new Mock<IQuoteWorkflowProvider>();
            quoteWorkflowProvider.Setup(x => x.GetConfigurableQuoteWorkflow(It.IsAny<ReleaseContext>())).Returns(Task.FromResult(defaultQuoteWorkflow));

            var policyService = new PolicyService(
                quoteAggregateRepository.Object,
                this.clock,
                new Mock<ICachingResolver>().Object,
                new Mock<IClaimReadModelRepository>().Object,
                productConfigurationProvider,
                new Mock<IUniqueIdentifierService>().Object,
                quoteDocumentRepository.Object,
                policyNumberSource.Object,
                policyReadModelRepository.Object,
                systemAlertService.Object,
                new Mock<IQuoteReferenceNumberGenerator>().Object,
                contextPropertiesResolver.Object,
                productFeatureSettingService.Object,
                quoteWorkflowProvider.Object,
                new Mock<IPolicyTransactionTimeOfDayScheme>().Object,
                mockDbContext.Object);
            var mockAggregateLockingService = new Mock<IAggregateLockingService>();
            mockAggregateLockingService
                .Setup(a => a.CreateLockOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AggregateType>()))
                .ReturnsAsync(It.IsAny<IRedLock>());
            var services = new ServiceCollection();
            services.AddTransient<ICachingResolver>(c => cachingResolver.Object);
            services.AddTransient<IHttpContextPropertiesResolver>(c => contextPropertiesResolver.Object);
            services.AddTransient<IQuoteAggregateResolverService>(c => quoteAggregateResolverService.Object);
            services.AddTransient<IQuoteAggregateRepository>(c => quoteAggregateRepository.Object);
            services.AddTransient<IProductConfigurationProvider>(c => productConfigurationProvider);
            services.AddTransient<IQuoteWorkflowProvider>(c => quoteWorkflowProvider.Object);
            services.AddTransient<IPolicyTransactionTimeOfDayScheme>(c => new Mock<IPolicyTransactionTimeOfDayScheme>().Object);
            services.AddTransient<IPolicyService>(c => policyService);
            services.AddTransient<IAdditionalPropertyValueService>(c => new Mock<IAdditionalPropertyValueService>().Object);
            services.AddTransient<IUBindDbContext>(c => mockDbContext.Object);
            services.AddTransient<IReleaseQueryService>(c => new Mock<IReleaseQueryService>().Object);
            services.AddTransient<IUBindDbContext>(c => mockDbContext.Object);
            services.AddScoped<IAggregateLockingService>(c => mockAggregateLockingService.Object);
            services.AddSingleton<IClock>(c => this.clock);
            services.AddTransient<IErrorNotificationService>(c => new Mock<IErrorNotificationService>().Object);
            services.AddSingleton<ICqrsMediator, CqrsMediator>();
            services.AddSingleton<ICqrsRequestContext>(_ => new CqrsRequestContext());
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CompletePolicyTransactionCommandHandler).Assembly));
            services.AddLogging(loggingBuilder => loggingBuilder.AddDebug());
            var dependencyProvider = services.BuildServiceProvider();
            return dependencyProvider.GetService<ICqrsMediator>();
        }
    }
}
