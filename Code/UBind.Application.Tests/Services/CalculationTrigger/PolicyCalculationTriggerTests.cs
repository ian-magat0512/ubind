// <copyright file="PolicyCalculationTriggerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    using UBind.Domain.Aggregates.Customer;
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
    using UBind.Domain.ReadWriteModel.CalculationTrigger;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Infrastructure;
    using Xunit;

    public class PolicyCalculationTriggerTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly IClock clock = SystemClock.Instance;
        private readonly DeploymentEnvironment environment = DeploymentEnvironment.Development;
        private readonly Tenant tenant = TenantFactory.Create();
        private Guid quoteId;

        [Fact]
        public async Task CreatePolicyWithSoftReferralTrigger()
        {
            QuoteAggregate quoteAggregates = this.CreateQuoteAggregateFromCalculationResultJson(CalculationResultJsonFactory.SampleWithSoftReferral);
            var quote = quoteAggregates.GetQuoteOrThrow(this.quoteId);
            var command = new CompletePolicyTransactionCommand(
                this.tenant.Id,
                quote.Id,
                quote.LatestCalculationResult.Id,
                new FormData("{}"));
            var mediator = this.GetMediator(quoteAggregates);

            // Act
            Func<Task> act = async () => await mediator.Send(command);

            // Assert
            var exception = await act.Should().ThrowAsync<InvalidCalculationTriggerException>();
            exception.Which.Message.Should().Be(
                "Policy could not be issued because the associated calculation result contained a soft referral trigger");
            quote.LatestCalculationResult.Data.Triggers.First().Should().BeOfType<SoftReferralCalculationTrigger>();
        }

        [Fact]
        public async Task CreatePolicyWithHardReferralTrigger()
        {
            QuoteAggregate quoteAggregates = this.CreateQuoteAggregateFromCalculationResultJson(CalculationResultJsonFactory.SampleWithHardReferral);
            var quote = quoteAggregates.GetQuoteOrThrow(this.quoteId);
            var command = new CompletePolicyTransactionCommand(
                this.tenant.Id,
                quote.Id,
                quote.LatestCalculationResult.Id,
                new FormData(FormDataJsonFactory.Sample));
            var mediator = this.GetMediator(quoteAggregates);

            // Act
            Func<Task> act = async () => await mediator.Send(command);

            // Assert
            var exception = await act.Should().ThrowAsync<InvalidCalculationTriggerException>();
            exception.Which.Message.Should().Be(
                "Policy could not be issued because the associated calculation result contained a hard referral trigger");
            quote.LatestCalculationResult.Data.Triggers.First().Should().BeOfType<HardReferralCalculationTrigger>();
        }

        [Fact]
        public async Task CreatePolicyWithDeclinedTrigger()
        {
            QuoteAggregate quoteAggregates = this.CreateQuoteAggregateFromCalculationResultJson(CalculationResultJsonFactory.SampleWithDeclined);
            var quote = quoteAggregates.GetQuoteOrThrow(this.quoteId);
            var command = new CompletePolicyTransactionCommand(
                this.tenant.Id,
                quote.Id,
                quote.LatestCalculationResult.Id,
                new FormData(FormDataJsonFactory.Sample));
            var mediator = this.GetMediator(quoteAggregates);

            // Act
            Func<Task> act = async () => await mediator.Send(command);

            // Assert
            var exception = await act.Should().ThrowAsync<InvalidCalculationTriggerException>();
            exception.Which.Message.Should().Be(
                "Policy could not be issued because the associated calculation result contained a decline trigger");
            quote.LatestCalculationResult.Data.Triggers.First().Should().BeOfType<DeclinedCalculationTrigger>();
        }

        [Fact]
        public async Task CreatePolicyWithErrorTrigger()
        {
            QuoteAggregate quoteAggregate = this.CreateQuoteAggregateFromCalculationResultJson(CalculationResultJsonFactory.SampleWithError);
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var command = new CompletePolicyTransactionCommand(
                this.tenant.Id,
                quote.Id,
                quote.LatestCalculationResult.Id,
                new FormData(FormDataJsonFactory.Sample));
            var mediator = this.GetMediator(quoteAggregate);

            // Act
            Func<Task> act = async () => await mediator.Send(command);

            // Assert
            var exception = await act.Should().ThrowAsync<InvalidCalculationTriggerException>();
            exception.Which.Message.Should().Be(
                "Policy could not be issued because the associated calculation result contained an error trigger");
            quote.LatestCalculationResult.Data.Triggers.First().Should().BeOfType<ErrorCalculationTrigger>();
        }

        [Fact]
        public async Task CreatePolicyWithNoCalculationTrigger()
        {
            QuoteAggregate quoteAggregate = this.CreateQuoteAggregateFromCalculationResultJson(CalculationResultJsonFactory.Create());
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var command = new CompletePolicyTransactionCommand(
                this.tenant.Id,
                quote.Id,
                quote.LatestCalculationResult.Id,
                quote.LatestFormData.Data);
            var mediator = this.GetMediator(quoteAggregate);

            // Act
            var quoteReadModel = await mediator.Send(command);

            // Assert
            quote.LatestCalculationResult.Data.Triggers.Should().BeEmpty();
            quote.Aggregate.Policy.Should().NotBeNull();
        }

        private QuoteAggregate CreateQuoteAggregateFromCalculationResultJson(string calculationResultJson)
        {
            Instant currentTime = this.clock.GetCurrentInstant();
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                ProductFactory.DefaultId,
                this.environment,
                QuoteExpirySettings.Default,
                this.performingUserId,
                currentTime,
                Guid.NewGuid(),
                Timezones.AET);
            quote.UpdateFormData(new Domain.Aggregates.Quote.FormData("{}"), this.performingUserId, currentTime);
            var personAggregate = PersonAggregate.CreatePerson(
                this.tenant.Id, this.tenant.Details.DefaultOrganisationId, this.performingUserId, this.clock.Now());
            var customerAggregate = CustomerAggregate.CreateNewCustomer(
                this.tenant.Id, personAggregate, this.environment, this.performingUserId, null, currentTime);
            quote.Aggregate.RecordAssociationWithCustomer(customerAggregate, personAggregate, this.performingUserId, currentTime);
            var person = new PersonCommonProperties()
            {
                FullName = "Foo",
                MobilePhoneNumber = "04 1234 1234",
                Email = "foo@example.com",
            };

            var personDetails = new PersonalDetails(this.tenant.Id, person);
            personAggregate.Update(personDetails, this.performingUserId, this.clock.Now());

            var formDataSchema = new FormDataSchema(new JObject());

            var customerDetails = new PersonalDetails(personAggregate);
            quote.Aggregate.UpdateCustomerDetails(customerDetails, this.performingUserId, this.clock.Now(), quote.Id);
            var formData = new Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var calculationData = new CachingJObjectWrapper(calculationResultJson);
            var formDataUpdateId = quote.UpdateFormData(formData, this.performingUserId, currentTime);
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataUpdateId;
            quote.RecordCalculationResult(
                calculationResult,
                calculationData,
                currentTime,
                formDataSchema,
                false,
                this.performingUserId);
            quote.AssignQuoteNumber("TestFoo", this.performingUserId, currentTime);
            quote.Aggregate.CreateVersion(this.performingUserId, currentTime, quote.Id);
            this.quoteId = quote.Id;
            return quote.Aggregate;
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
            quoteWorkflowProvider.Setup(x => x.GetConfigurableQuoteWorkflow(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(defaultQuoteWorkflow));

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
            services.AddTransient<IReleaseQueryService>(c => new Mock<IReleaseQueryService>().Object);
            services.AddTransient<IUBindDbContext>(c => mockDbContext.Object);
            services.AddTransient<IUBindDbContext>(c => mockDbContext.Object);
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAggregateLockingService>(c => mockAggregateLockingService.Object);
            services.AddSingleton<IClock>(c => SystemClock.Instance);
            services.AddTransient<IErrorNotificationService>(c => new Mock<IErrorNotificationService>().Object);
            services.AddSingleton<ICqrsMediator, CqrsMediator>();
            services.AddSingleton<ICqrsRequestContext>(_ => new CqrsRequestContext());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CompletePolicyTransactionCommandHandler).Assembly));
            services.AddLogging(loggingBuilder => loggingBuilder.AddDebug());
            var dependencyProvider = services.BuildServiceProvider();
            return dependencyProvider.GetService<ICqrsMediator>();
        }
    }
}
