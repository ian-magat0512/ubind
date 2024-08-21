// <copyright file="ApplicationStack.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using System.Transactions;
    using CSharpFunctionalExtensions;
    using Hangfire;
    using Hangfire.SqlServer;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NodaTime;
    using RazorEngine.Templating;
    using RedLockNet;
    using StackExchange.Redis;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.Automation;
    using UBind.Application.Commands.Customer.Merge;
    using UBind.Application.Commands.Policy;
    using UBind.Application.Commands.QuoteCalculation;
    using UBind.Application.Commands.User;
    using UBind.Application.CustomPipelines;
    using UBind.Application.CustomPipelines.BindPolicy;
    using UBind.Application.Export;
    using UBind.Application.FlexCel;
    using UBind.Application.Funding;
    using UBind.Application.Funding.Iqumulate;
    using UBind.Application.Funding.PremiumFunding;
    using UBind.Application.MicrosoftGraph;
    using UBind.Application.Payment;
    using UBind.Application.Payment.Deft;
    using UBind.Application.Person;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Queries.Product;
    using UBind.Application.Queries.Services;
    using UBind.Application.Queries.Tenant;
    using UBind.Application.Queries.User;
    using UBind.Application.Quote;
    using UBind.Application.Releases;
    using UBind.Application.Report;
    using UBind.Application.Services;
    using UBind.Application.Services.Email;
    using UBind.Application.Services.Import;
    using UBind.Application.Services.Imports;
    using UBind.Application.Services.Messaging;
    using UBind.Application.Services.PolicyDataPatcher;
    using UBind.Application.Services.Search;
    using UBind.Application.Services.SystemEmail;
    using UBind.Application.SystemEvents;
    using UBind.Application.Tests;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Fakes;
    using UBind.Application.Tests.Funding.EFundExpress;
    using UBind.Application.Tests.Funding.PremiumFunding;
    using UBind.Application.Tests.Payment;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Accounting;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Accounting;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Claim.Workflow;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Aggregates.Report;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Clients.DVA.Perils.Interfaces;
    using UBind.Domain.Configuration;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Funding;
    using UBind.Domain.Loggers;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Accounting;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Person.Fields;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Reduction;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Repositories.Redis;
    using UBind.Domain.Search;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyDefinition;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Services.Pricing;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence;
    using UBind.Persistence.Aggregates;
    using UBind.Persistence.Clients.DVA.Migrations;
    using UBind.Persistence.Clients.DVA.Perils.Respositories;
    using UBind.Persistence.Configuration;
    using UBind.Persistence.Entities;
    using UBind.Persistence.Infrastructure;
    using UBind.Persistence.ReadModels;
    using UBind.Persistence.ReadModels.Claim;
    using UBind.Persistence.ReadModels.Organisation;
    using UBind.Persistence.ReadModels.Portal;
    using UBind.Persistence.ReadModels.Quote;
    using UBind.Persistence.ReadModels.User;
    using UBind.Persistence.Reduction;
    using UBind.Persistence.Search;

    /// <summary>
    /// Application stack for use in integration tests.
    /// </summary>
    public class ApplicationStack : IDisposable
    {
        private readonly IConnectionConfiguration connectionConfiguration;
        private ServiceCollection mediatorDependencyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationStack"/> class.
        /// </summary>
        /// <param name="connectionStringName">The connection string.</param>
        public ApplicationStack(string connectionStringName)
            : this(connectionStringName, ApplicationStackConfiguration.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationStack"/> class.
        /// </summary>
        /// <param name="connectionStringName">The connection string.</param>
        /// <param name="configuration">Configuration specifying optional stack configuration.</param>
        public ApplicationStack(string connectionStringName, ApplicationStackConfiguration configuration)
        {
            this.connectionConfiguration = new ConnectionStrings();

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            this.connectionConfiguration.UBind = config.GetConnectionString(connectionStringName);

            this.PasswordComplexityValidator = UBind.Domain.Authentication.PasswordComplexityValidator.Default;
            this.PasswordReuseValidator = UBind.Domain.Authentication.PasswordReuseValidator.Default;
            this.Clock = new TestClock();
            this.DbContext = new TestUBindDbContext(DatabaseFixture.TestConnectionString);
            this.EventRecordRepository = new EventRecordRepository(this.DbContext, this.connectionConfiguration);
            this.LuceneIndexCache = new LuceneIndexCache();

            this.AutomationPeriodicTriggerSchedulerMock = new Mock<IAutomationPeriodicTriggerScheduler>();

            // application policy
            this.DefaultTimeOfDayScheme = new DefaultPolicyTransactionTimeOfDayScheme();

            // Funding
            var fundingConfigurationProvider = new Mock<IFundingConfigurationProvider>();
            IFundingConfiguration fundingConfiguration =
                configuration.FundingServiceName == FundingServiceName.PremiumFunding ? new TestPremiumFundingConfiguration() :
                configuration.FundingServiceName == FundingServiceName.EFundExpress ? new TestEFundExpressProductConfiguration() :
                (IFundingConfiguration)null;

            var accessTokenProvider = new AccessTokenProvider();
            var cachingAccessTokenProvider = new CachingAccessTokenProvider(accessTokenProvider, this.Clock);
            var differentialPriceCalculationService = new Mock<IDifferentialPriceCalculationService>();

            var mockReleaseQueryService = new Mock<IReleaseQueryService>();
            var mockdevReleaseRepository = new Mock<IDevReleaseRepository>();
            var mockProductReleaseService = new Mock<IProductReleaseService>();
            var activeDeployedRelease = new Mock<ActiveDeployedRelease>();
            var fakeDevRelease = FakeReleaseBuilder
                .CreateForProduct(Guid.NewGuid(), Guid.NewGuid())
                .BuildDevRelease();
            var cachedRelease = new ActiveDeployedRelease(fakeDevRelease, DeploymentEnvironment.Development, null);
            mockReleaseQueryService.Setup(e => e.GetRelease(It.IsAny<ReleaseContext>())).Returns(cachedRelease);
            mockdevReleaseRepository.Setup(e => e.GetDevReleaseForProductWithoutAssets(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(fakeDevRelease);
            this.ReleaseQueryService = mockReleaseQueryService.Object;
            this.DevReleaseRepository = mockdevReleaseRepository.Object;
            this.ProductReleaseService = mockProductReleaseService.Object;
            this.EntitySettingsRepository = new EntitySettingsRepository(this.DbContext, this.Clock);

            // Calculation
            var calculationService = new Mock<ICalculationService>();
            calculationService
                .Setup(s => s.GetQuoteCalculation(
                    It.IsAny<ReleaseContext>(),
                    It.IsAny<SpreadsheetCalculationDataModel>(),
                    It.IsAny<IAdditionalRatingFactors>()))
                .Returns(new ReleaseCalculationOutput
                {
                    CalculationJson = CalculationResultJsonFactory.Sample,
                    ReleaseId = default,
                });

            // Mediator
            this.MockMediator = new Mock<ICqrsMediator>();

            // Mediator
            this.mediatorDependencyProvider = new ServiceCollection();
            this.mediatorDependencyProvider.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BaseUpdaterJob).Assembly));
            this.mediatorDependencyProvider.AddTransient<ILogger<QuoteCalculationCommand>>(c => (ILogger<QuoteCalculationCommand>)NullLogger<QuoteCalculationCommand>.Instance);
            this.mediatorDependencyProvider.AddTransient<NodaTime.IClock>(c => this.Clock);
            this.mediatorDependencyProvider.AddTransient<ILogger<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler>>(c => (ILogger<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler>)NullLogger<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler>.Instance);
            this.mediatorDependencyProvider.AddTransient<IProductRepository>(c => this.ProductRepository);
            this.mediatorDependencyProvider.AddTransient<IProductFeatureSettingRepository>(c => this.ProductFeatureSettingRepository);
            this.mediatorDependencyProvider.AddTransient<IReleaseQueryService>(c => this.ReleaseQueryService);
            this.mediatorDependencyProvider.AddTransient<IHttpContextPropertiesResolver>(c => this.HttpContextPropertiesResolver);
            this.mediatorDependencyProvider.AddTransient<ICachingResolver>(c => this.CachingResolver);
            this.mediatorDependencyProvider.AddTransient<ICustomerAggregateRepository>(c => this.CustomerAggregateRepository);
            this.mediatorDependencyProvider.AddTransient<IUserAggregateRepository>(c => this.UserAggregateRepository);
            this.mediatorDependencyProvider.AddTransient<IPersonAggregateRepository>(c => this.PersonAggregateRepository);
            this.mediatorDependencyProvider.AddTransient<IRoleRepository>(c => this.RoleRepository);
            this.mediatorDependencyProvider.AddTransient<IClaimAggregateRepository>(c => this.ClaimAggregateRepository);
            this.mediatorDependencyProvider.AddTransient<IQuoteAggregateRepository>(c => this.QuoteAggregateRepository);
            this.mediatorDependencyProvider.AddTransient<IQuoteAggregateResolverService>(c => this.QuoteAggregateResolverService);
            this.mediatorDependencyProvider.AddTransient<IApplicationFundingService>(c => this.ApplicationFundingService);
            this.mediatorDependencyProvider.AddTransient<IFundingConfigurationProvider>(c => fundingConfigurationProvider.Object);
            this.mediatorDependencyProvider.AddTransient<IConfigurationService>(c => new Mock<IConfigurationService>().Object);
            this.mediatorDependencyProvider.AddTransient<IPaymentService>(c => this.GetPaymentService(PaymentGatewayName.Deft));
            this.mediatorDependencyProvider.AddTransient<IPerilsRepository>(c => this.PerilsRepository);
            this.mediatorDependencyProvider.AddTransient<IProductConfigurationProvider>(c => this.ProductConfigurationProvider);
            this.mediatorDependencyProvider.AddTransient<IPolicyTransactionTimeOfDayScheme>(c => this.PolicyTransactionTimeOfDayScheme);
            this.mediatorDependencyProvider.AddTransient<IClaimReadModelRepository>(c => this.ClaimReadModelRepository);
            this.mediatorDependencyProvider.AddTransient<IQuoteExpirySettingsProvider>(c => this.QuoteExpirySettingsProvider);
            this.mediatorDependencyProvider.AddTransient<IQuoteWorkflowProvider>(c => this.DefaultWorkflowProvider);
            this.mediatorDependencyProvider.AddTransient<IDifferentialPriceCalculationService>(c => differentialPriceCalculationService.Object);
            this.mediatorDependencyProvider.AddTransient<ICustomerService>(c => this.CustomerService);
            this.mediatorDependencyProvider.AddTransient<Application.User.IUserService>(c => this.UserService);
            this.mediatorDependencyProvider.AddTransient<ICalculationService>(c => calculationService.Object);
            this.mediatorDependencyProvider.AddTransient<ICustomerReadModelRepository>(c => this.CustomerReadModelRepository);
            this.mediatorDependencyProvider.AddTransient<IPasswordComplexityValidator>(c => this.PasswordComplexityValidator);
            this.mediatorDependencyProvider.AddTransient<IPasswordHashingService>(c => this.PasswordHashingService);
            this.mediatorDependencyProvider.AddTransient<IProductFeatureSettingService>(c => this.ProductFeatureSettingService);
            this.mediatorDependencyProvider.AddTransient<IPolicyService>(c => this.PolicyService);
            this.mediatorDependencyProvider.AddTransient<IPolicyReadModelRepository>(c => this.PolicyReadModelRepository);
            this.mediatorDependencyProvider.AddTransient<IProductOrganisationSettingRepository>(c => this.ProductOrganisationSettingRepository);
            this.mediatorDependencyProvider.AddTransient<IProductPortalSettingRepository>(c => this.ProductPortalSettingRepository);
            this.mediatorDependencyProvider.AddTransient<IOrganisationService>(c => this.OrganisationService);
            this.mediatorDependencyProvider.AddTransient<IPortalReadModelRepository>(c => this.PortalReadModelRepository);
            this.mediatorDependencyProvider.AddTransient<IEnvironmentSetting<ProductSetting>>(c => new EnvironmentSetting<ProductSetting>());
            this.mediatorDependencyProvider.AddTransient<ProductSetting>(c => new ProductSetting());
            this.mediatorDependencyProvider.AddTransient<Domain.Services.IProductService>(c => this.DomainProductService);
            this.mediatorDependencyProvider.AddTransient<ITenantRepository>(c => this.TenantRepository);
            this.mediatorDependencyProvider.AddTransient<IQuoteReadModelRepository>(c => this.QuoteReadModelRepository);
            this.mediatorDependencyProvider.AddTransient<IPolicyTransactionReadModelRepository>(c => this.PolicyTransactionReadModelRepository);
            this.mediatorDependencyProvider.AddTransient<IUniqueIdentifierService>(c => this.UniqueIdentifierService);
            this.mediatorDependencyProvider.AddTransient<IQuoteReferenceNumberGenerator>(c => this.QuoteNumberGenerator);
            this.mediatorDependencyProvider.AddTransient<IUserSessionService>(c => this.UserSessionService);
            this.mediatorDependencyProvider.AddTransient<IUserSessionRepository>(c => this.UserSessionRepositoryMock.Object);
            this.mediatorDependencyProvider.AddTransient<ILoginAttemptTrackingService>(c => this.LoginAttemptTrackingService);
            this.mediatorDependencyProvider.AddTransient<IUserPasswordResetInvitationService>(c => this.UserPasswordResetInvitationService);
            this.mediatorDependencyProvider.AddTransient<IPasswordResetTrackingService>(c => this.PasswordResetTrackingService);
            this.mediatorDependencyProvider.AddTransient<ISystemEmailTemplateRepository>(c => this.SystemEmailTemplateRepository);
            this.mediatorDependencyProvider.AddTransient<IPersonReadModelRepository>(c => this.PersonReadModelRepository);
            this.mediatorDependencyProvider.AddTransient<IPersonService>(c => this.PersonService);
            this.mediatorDependencyProvider.AddTransient<IUserReadModelRepository>(c => this.UserReadModelRepository);
            this.mediatorDependencyProvider.AddTransient<IOrganisationAggregateRepository>(c => this.OrganisationAggregateRepository);
            this.mediatorDependencyProvider.AddTransient<IEmailInvitationConfiguration>(c => this.EmailInvitationConfiguration);
            this.mediatorDependencyProvider.AddTransient<ISystemEmailService>(c => this.SystemEmailService);
            this.mediatorDependencyProvider.AddTransient<IHttpContextAccessor>(c => this.ContextAccessor);
            this.mediatorDependencyProvider.AddTransient<IUserLoginEmailRepository>(c => this.UserLoginEmailRepository);
            this.mediatorDependencyProvider.AddTransient<IQuoteService>(c => this.QuoteService);
            this.mediatorDependencyProvider.AddTransient<IClaimService>(c => this.ClaimService);
            this.mediatorDependencyProvider.AddTransient<IOrganisationReadModelRepository>(c => this.OrganisationReadModelRepository);
            this.mediatorDependencyProvider.AddTransient<IUserActivationInvitationService>(c => this.UserActivationInvitationService);
            this.mediatorDependencyProvider.AddTransient<IAutomationService>(c => new Mock<IAutomationService>().Object);
            this.mediatorDependencyProvider.AddTransient<IAutomationExtensionPointService>(c => new Mock<IAutomationExtensionPointService>().Object);
            this.mediatorDependencyProvider.AddTransient<ICqrsRequestContext>(_ => new CqrsRequestContext());
            this.mediatorDependencyProvider.AddTransient<ISmsRepository>(c => this.SmsRepository);
            this.mediatorDependencyProvider.AddTransient<IEmailRepository>(c => this.SecondEmailRepository);
            this.mediatorDependencyProvider.AddTransient<IAdditionalPropertyValueService>(c => this.AdditionalPropertyValueService.Object);
            this.mediatorDependencyProvider.AddScoped<IBackgroundJobClient>(c => this.BackgroundJobClient);
            this.mediatorDependencyProvider.AddTransient<IPortalService>(c => this.PortalService);
            this.mediatorDependencyProvider.AddTransient<IFilesystemFileRepository>(c => this.FilesystemFileRepositoryMock.Object);
            this.mediatorDependencyProvider.AddTransient<IFilesystemStoragePathService>(c => this.FilesystemStoragePathService);
            this.mediatorDependencyProvider.AddTransient<UBind.Application.User.IUserService>(c => this.UserService);
            this.mediatorDependencyProvider.AddTransient<UBind.Application.IRoleService>(c => this.RoleService);
            this.mediatorDependencyProvider.AddTransient<IAutomationPeriodicTriggerScheduler>(c => this.AutomationPeriodicTriggerSchedulerMock.Object);
            this.mediatorDependencyProvider.AddTransient<IFeatureSettingRepository>(c => this.FeatureSettingRepository);
            this.mediatorDependencyProvider.AddTransient<IPortalAggregateRepository>(c => this.PortalAggregateRepository);
            this.mediatorDependencyProvider.AddTransient<ITenantService>(c => this.TenantService);
            this.mediatorDependencyProvider.AddTransient<IAdditionalPropertyTransformHelper>(c => this.AdditionalPropertyTransformHelper);
            this.mediatorDependencyProvider.AddTransient<IUserSystemEventEmitter>(c => this.UserSystemEventEmitter);
            this.mediatorDependencyProvider.AddSingleton<FundingServiceFactory>(c => this.FundingFactory);
            this.mediatorDependencyProvider.AddSingleton<IAuthenticationMethodService>(c => this.AuthenticationMethodService);
            this.mediatorDependencyProvider.AddTransient<IErrorNotificationService>(c => new Mock<IErrorNotificationService>().Object);
            this.mediatorDependencyProvider.AddSingleton<ICqrsMediator, CqrsMediator>();
            this.mediatorDependencyProvider.AddSingleton<IUBindDbContext>(_ => this.DbContext);
            this.mediatorDependencyProvider.AddSingleton<IConnectionConfiguration>(_ => this.connectionConfiguration);
            this.mediatorDependencyProvider.AddTransient<IUnitOfWork>(_ => new UnitOfWork(this.DbContext));
            this.mediatorDependencyProvider.RegisterCustomPipelines();
            this.mediatorDependencyProvider.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ActivateUserCommandHandler).Assembly));
            this.mediatorDependencyProvider.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetTenantByIdQuery).Assembly));
            this.mediatorDependencyProvider.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetTenantByAliasQuery).Assembly));
            this.mediatorDependencyProvider.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductByAliasQuery).Assembly));
            this.mediatorDependencyProvider.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));
            this.mediatorDependencyProvider.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CustomerHasUserAccountQuery).Assembly));
            this.mediatorDependencyProvider.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetCustomerByIdQuery).Assembly));
            this.mediatorDependencyProvider.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetNextAvailableUserForCustomerQuery).Assembly));
            this.mediatorDependencyProvider.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AcceptFundingProposalCommandHandler).Assembly));
            this.mediatorDependencyProvider.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CompletePolicyTransactionCommandHandler).Assembly));
            this.mediatorDependencyProvider.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetPortalUrlQuery).Assembly));
            this.mediatorDependencyProvider.AddLogging(loggingBuilder => loggingBuilder.AddDebug());
            this.mediatorDependencyProvider.AddTransient<IReleaseService>(c => this.ReleaseService);
            this.mediatorDependencyProvider.AddTransient<IDevReleaseRepository>(c => this.DevReleaseRepository);
            this.mediatorDependencyProvider.AddTransient<IReleaseRepository>(c => this.ReleaseRepository);
            this.mediatorDependencyProvider.AddTransient<IEntitySettingsRepository>(c => this.EntitySettingsRepository);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotRepository>(c => this.AggregateSnapshotRepository!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<QuoteAggregate>>(c => this.QuoteAggregateSnapshotService!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<PersonAggregate>>(c => this.PersonAggregateSnapshotService!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<UserAggregate>>(c => this.UserAggregateSnapshotService!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<CustomerAggregate>>(c => this.CustomerAggregateSnapshotService!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<ClaimAggregate>>(c => this.ClaimAggregateSnapshotService!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<Organisation>>(c => this.OrganisationAggregateSnapshotService!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<ReportAggregate>>(c => this.ReportAggregateSnapshotService!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<TextAdditionalPropertyValue>>(c => this.TextAdditionalPropertyAggregateSnapshotService!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<AdditionalPropertyDefinition>>(c => this.AdditionalPropertyDefinitionAggregateSnapshotService!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<FinancialTransactionAggregate<Domain.Accounting.Invoice>>>(c => this.InvoiceAggregateSnapshotService!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<FinancialTransactionAggregate<Domain.Accounting.CreditNote>>>(c => this.CreditNoteAggregateSnapshotService!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<PortalAggregate>>(c => this.PortalAggregateSnapshotService!);
            this.mediatorDependencyProvider.AddTransient<IAggregateSnapshotService<StructuredDataAdditionalPropertyValue>>(c => this.StructuredDataAdditionalPropertyValue!);
            this.mediatorDependencyProvider.AddScoped<IAggregateLockingService>(c => this.AggregateLockingService);
            var serviceProvider = this.mediatorDependencyProvider.BuildServiceProvider();
            this.Mediator = serviceProvider.GetService<ICqrsMediator>();

            // Tenant + Product
            this.TenantRepository = new TenantRepository(this.DbContext);
            this.ProductRepository = new ProductRepository(this.DbContext);
            this.FeatureSettingRepository = new FeatureSettingRepository(this.DbContext, this.Clock);
            this.CachingResolver = new CachingResolver(
                this.Mediator, this.TenantRepository, this.ProductRepository, this.FeatureSettingRepository, this.ProductFeatureSettingRepository);

            var httpContextPropertiesResolverMock = new Mock<IHttpContextPropertiesResolver>();
            httpContextPropertiesResolverMock.Setup(s => s.ClientIpAddress).Returns(new System.Net.IPAddress(0x4435473));
            this.HttpContextPropertiesResolver = httpContextPropertiesResolverMock.Object;

            var productFeatureSettingMock = new Mock<IProductFeatureSettingService>();

            var productFeatureSettings = new ProductFeatureSetting(TenantFactory.DefaultId, ProductFactory.DefaultId, this.Clock.Now());
            productFeatureSettings.Enable(ProductFeatureSettingItem.AdjustmentQuotes);
            productFeatureSettings.Enable(ProductFeatureSettingItem.RenewalQuotes);
            productFeatureSettings.Enable(ProductFeatureSettingItem.CancellationQuotes);
            productFeatureSettingMock.Setup(p => p.GetProductFeature(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(productFeatureSettings);
            this.ProductFeatureSettingService = productFeatureSettingMock.Object;

            this.AggregateSnapshotRepository = new AggregateSnapshotRepository(this.DbContext, this.connectionConfiguration);
            var mockAggregateLockingService = new Mock<IAggregateLockingService>();
            mockAggregateLockingService
                .Setup(a => a.CreateLockOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AggregateType>()))
                .ReturnsAsync(It.IsAny<IRedLock>());
            this.AggregateLockingService = mockAggregateLockingService.Object;

            var textAdditionalPropertyValueReadModelWriter = new TextAdditionalPropertyValueReadModelWriter(
                new ReadModelUpdateRepository<TextAdditionalPropertyValueReadModel>(this.DbContext));
            var additionalPropertyDefinitionReadModelWriter = new AdditionalPropertyDefinitionReadModelWriter(
                  new ReadModelUpdateRepository<AdditionalPropertyDefinitionReadModel>(this.DbContext));
            var additionalPropertyDefinitionRepository = new AdditionalPropertyDefinitionRepository(this.DbContext);
            this.StructuredDataAdditionalPropertyValue = new AggregateSnapshotService<StructuredDataAdditionalPropertyValue>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.TextAdditionalPropertyAggregateSnapshotService = new AggregateSnapshotService<TextAdditionalPropertyValue>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.TextAdditionalPropertyValueAggregateRepository = new TextAdditionalPropertyValueAggregateRepository(
                this.DbContext,
                this.EventRecordRepository,
                textAdditionalPropertyValueReadModelWriter,
                this.TextAdditionalPropertyAggregateSnapshotService,
                this.Clock,
                NullLogger<TextAdditionalPropertyValueAggregateRepository>.Instance,
                serviceProvider);
            this.AdditionalPropertyDefinitionAggregateSnapshotService = new AggregateSnapshotService<AdditionalPropertyDefinition>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.AdditionalPropertyDefinitionAggregateRepository = new AdditionalPropertyDefinitionAggregateRepository(
                this.DbContext,
                this.EventRecordRepository,
                additionalPropertyDefinitionReadModelWriter,
                this.AdditionalPropertyDefinitionAggregateSnapshotService,
                this.Clock,
                NullLogger<AdditionalPropertyDefinitionAggregateRepository>.Instance,
                serviceProvider);

            var claimVersionReadModelRepository = new ClaimVersionReadModelRepository(this.DbContext);
            var quoteVersionReadModelRepository = new QuoteVersionReadModelRepository(this.DbContext);
            var additionalPropertyFilterResolver = new AdditionalPropertyDefinitionFilterResolver(
                this.QuoteReadModelRepository, this.ClaimReadModelRepository, this.PolicyReadModelRepository, this.PolicyTransactionReadModelRepository, claimVersionReadModelRepository, quoteVersionReadModelRepository);

            this.TextAdditionalPropertyValueReadModelRepository = new TextAdditionalPropertyValueReadModelRepository(
                this.DbContext,
                additionalPropertyDefinitionRepository,
                additionalPropertyFilterResolver);
            var dictionary = new Dictionary<AdditionalPropertyDefinitionType, IAdditionalPropertyValueProcessor>
            {
                {
                    AdditionalPropertyDefinitionType.Text,
                    new TextAdditionalPropertyValueProcessor(
                        new TextAdditionalPropertyValueReadModelRepository(
                            this.DbContext,
                            additionalPropertyDefinitionRepository,
                            additionalPropertyFilterResolver),
                        this.Clock,
                        new TextAdditionalPropertyValueAggregateRepository(
                            this.DbContext,
                            this.EventRecordRepository,
                            textAdditionalPropertyValueReadModelWriter,
                            this.TextAdditionalPropertyAggregateSnapshotService,
                            this.Clock,
                            NullLogger<TextAdditionalPropertyValueAggregateRepository>.Instance,
                            serviceProvider),
                        new ReadModelUpdateRepository<TextAdditionalPropertyValueReadModel>(this.DbContext))
                },
            };
            var propertyTypeEvaluatorService = new PropertyTypeEvaluatorService(dictionary);

            // Hangfire JobStorage
            this.BackgroundJobStorageProvider = new SqlServerStorage(DatabaseFixture.TestConnectionString);
            /*
            var mockIStorageConnection = new Mock<IStorageConnection>();
            mockSqlServerStorage.Setup(x => x.GetConnection()).Returns(mockIStorageConnection.Object);
            */
            JobStorage.Current = this.BackgroundJobStorageProvider;

            // Quote deletion
            this.ProgressLogger = new Mock<IProgressLogger>().Object;
            this.QuoteReductor = new QuoteDeletionManager(DatabaseFixture.TestConnectionString);
            this.MiniProfilerReductor = new MiniProfilerDeletionManager(DatabaseFixture.TestConnectionString);

            // Tenant + Product
            this.TenantRepository = new TenantRepository(this.DbContext);
            this.ProductRepository = new ProductRepository(this.DbContext);
            this.FeatureSettingRepository = new FeatureSettingRepository(this.DbContext, this.Clock);
            this.ProductFeatureSettingRepository = new ProductFeatureSettingRepository(this.DbContext);

            this.CachingResolver = new CachingResolver(
                this.Mediator, this.TenantRepository, this.ProductRepository, this.FeatureSettingRepository, this.ProductFeatureSettingRepository);

            this.ProductFeatureSettingRepository = new ProductFeatureSettingRepository(this.DbContext);
            this.PortalReadModelRepository = new PortalReadModelRepository(this.DbContext, this.CachingResolver);
            this.QuoteExpirySettingsProvider = configuration.UseDefaultQuoteExpirySettingsProvider
                ? (IQuoteExpirySettingsProvider)new DefaultExpirySettingsProvider()
                : new QuoteExpirySettingsProvider(this.CachingResolver);
            this.DefaultWorkflowProvider = new DefaultQuoteWorkflowProvider();
            this.DefaultQuoteWorkflow = new DefaultQuoteWorkflow();
            this.ClaimWorkflowProvider = new DefaultClaimWorkflowProvider();

            this.ProductConfigurationProvider = new DefaultProductConfigurationProvider();
            this.PolicyTransactionTimeOfDayScheme = new DefaultPolicyTransactionTimeOfDayScheme();
            this.UniqueIdentifierService = new UniqueIdentifierService(this.DbContext, this.CachingResolver, this.Clock);
            this.UniqueNumberSequenceGenerator = new UniqueNumberSequenceGenerator(DatabaseFixture.TestConnectionString);
            this.QuoteNumberGenerator = new QuoteReferenceNumberGenerator(this.UniqueNumberSequenceGenerator);

            var storageConfig = new Mock<IFilesystemStorageConfiguration>();
            storageConfig.Setup(x => x.StorageProvider).Returns("Local");
            storageConfig.Setup(x => x.UBindFolderName).Returns("UBindTest");
            this.FilesystemStoragePathService = new FilesystemStoragePathService(storageConfig.Object);

            this.ProductService = new UBind.Application.ProductService(
                this.TenantRepository,
                this.ProductRepository,
                new Mock<ICachingAuthenticationTokenProvider>().Object,
                new Mock<IFilesystemFileRepository>().Object,
                this.FilesystemStoragePathService,
                new Mock<IJobClient>().Object,
                this.Clock,
                new Mock<IInvoiceNumberRepository>().Object,
                new Mock<IClaimNumberRepository>().Object,
                new Mock<IPolicyNumberRepository>().Object,
                new Mock<ICreditNoteNumberRepository>().Object,
                this.QuoteService,
                new Mock<IProductFeatureSettingService>().Object,
                this.AutomationPeriodicTriggerSchedulerMock.Object,
                NullLogger<UBind.Application.ProductService>.Instance,
                this.MockMediator.Object,
                this.HttpContextPropertiesResolver,
                new Mock<IProductPortalSettingRepository>().Object,
                this.CachingResolver);

            var mockTenantSystemEventEmitter = new Mock<ITenantSystemEventEmitter>();
            this.EmailTemplateService = new EmailTemplateService(
                this.SystemEmailTemplateRepository,
                this.CachingResolver,
                mockTenantSystemEventEmitter.Object,
                this.Clock);

            this.CustomerReadModelRepository = new CustomerReadModelRepository(this.DbContext);

            this.SystemEmailService = new SystemEmailService(
                new Mock<IEmailService>().Object,
                new Mock<ISmtpClientConfiguration>().Object,
                this.EmailTemplateService,
                new MailClientFactory(),
                new Mock<IJobClient>().Object,
                NullLogger<SystemEmailService>.Instance,
                this.Mediator,
                this.Clock,
                this.FileContentRepository);

            this.EmailInvitationConfiguration = new EmailInvitationConfiguration
            {
                InvitationLinkHost = "https://localhost:4366",
            };

            this.OrganisationReadModelRepository = new OrganisationReadModelRepository(this.DbContext);

            // Quotes
            Mock<ILuceneDirectoryConfiguration> luceneDirectory = new Mock<ILuceneDirectoryConfiguration>();
            luceneDirectory.Setup(x => x.Quote).Returns(new LuceneIndexGenerationConfig()
            {
                IndexGenerationCronExpression = "generate-expression-test",
                IndexGenerationStartupDelayInSeconds = 180,
                IndexRegenerationCronExpression = "regenerate-expression-test",
                IndexRegenerationStartupDelayInSeconds = 0,
            });
            luceneDirectory.Setup(x => x.Policy).Returns(new LuceneIndexGenerationConfig()
            {
                IndexGenerationCronExpression = "generate-expression-test",
                IndexGenerationStartupDelayInSeconds = 180,
                IndexRegenerationCronExpression = "regenerate-expression-test",
                IndexRegenerationStartupDelayInSeconds = 0,
            });
            luceneDirectory.Setup(x => x.BaseLuceneDirectory).Returns($"{System.IO.Directory.GetCurrentDirectory()}\\LuceneTest");

            var product = ProductFactory.Create(TenantFactory.DefaultId, ProductFactory.DefaultId);
            var productsForLucene = new List<Product>
            {
                product,
            };

            this.QuoteLuceneDocumentBuilder = new QuoteLuceneDocumentBuilder();
            this.QuoteReadModelRepository = new QuoteReadModelRepository(this.DbContext, this.connectionConfiguration, this.Clock);
            this.LuceneQuoteRepository = new LuceneQuoteRepository(
                luceneDirectory.Object,
                this.QuoteLuceneDocumentBuilder,
                this.Clock,
                this.LuceneIndexCache,
                this.CachingResolver,
                NullLogger<LuceneQuoteRepository>.Instance);

            this.PolicyLuceneDocumentBuilder = new PolicyLuceneDocumentBuilder();
            this.PolicyReadModelRepository = new PolicyReadModelRepository(this.DbContext, this.connectionConfiguration, this.Clock);
            this.LucenePolicyRepository = new LucenePolicyRepository(
                luceneDirectory.Object,
                this.PolicyLuceneDocumentBuilder,
                this.Clock,
                this.LuceneIndexCache,
                this.CachingResolver,
                NullLogger<LucenePolicyRepository>.Instance);

            var personReadModelUpdateRepository = new ReadModelUpdateRepository<PersonReadModel>(this.DbContext);
            this.QuoteDocumentReadModelRepository = new QuoteDocumentReadModelRepository(this.DbContext);
            this.PolicyReadModelRepository = new PolicyReadModelRepository(this.DbContext, this.connectionConfiguration, this.Clock);
            var organisationSystemEventEmitter = new Mock<IOrganisationSystemEventEmitter>();
            this.ProductOrganisationSettingRepository =
                new ProductOrganisationSettingRepository(this.DbContext, this.Clock, organisationSystemEventEmitter.Object, this.HttpContextPropertiesResolver);
            this.ProductPortalSettingRepository = new ProductPortalSettingRepository(this.DbContext, this.Clock);
            this.QuoteReadModelUpdateRepository = new ReadModelUpdateRepository<NewQuoteReadModel>(this.DbContext);
            this.ClaimReadModelRepository = new ClaimReadModelRepository(this.DbContext, this.CachingResolver, this.Clock, this.connectionConfiguration);
            this.QuoteReadModelUpdateRepository = new ReadModelDeferredUpdateRepository<NewQuoteReadModel>(this.DbContext);
            this.PolicyReadModelUpdateRepository = new ReadModelDeferredUpdateRepository<PolicyReadModel>(this.DbContext);
            var customerReadModelRepository = new ReadModelDeferredUpdateRepository<CustomerReadModel>(this.DbContext);
            var quoteReadModelWriter = new QuoteReadModelWriter(
                this.QuoteReadModelUpdateRepository,
                this.PolicyReadModelUpdateRepository,
                new ReadModelDeferredUpdateRepository<PolicyTransaction>(this.DbContext),
                this.QuoteFileAttachmentReadModelUpdateRepository,
                customerReadModelRepository,
                personReadModelUpdateRepository,
                this.ProductRepository,
                new QuoteReadModelRepository(this.DbContext, this.connectionConfiguration, this.Clock),
                propertyTypeEvaluatorService,
                new Mock<IQuoteFileAttachmentRepository>().Object,
                this.TenantRepository,
                this.Clock,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                new ProductConfigurationProvider(this.ReleaseQueryService),
                this.ProductReleaseService);
            var quoteIntegrationEventScheduler = new Mock<IQuoteEventIntegrationScheduler>();
            var quoteDocumentReadModelRepository = new ReadModelUpdateRepository<QuoteDocumentReadModel>(this.DbContext);
            var quoteDocumentReadModelWriter = new QuoteDocumentReadModelWriter(quoteDocumentReadModelRepository, this.PolicyReadModelUpdateRepository, this.QuoteReadModelUpdateRepository);
            this.QuoteVersionReadModelUpdateRepository = new ReadModelUpdateRepository<QuoteVersionReadModel>(this.DbContext);
            this.QuoteVersionReadModelRepository = new QuoteVersionReadModelRepository(this.DbContext);
            var quoteVersionReadModelWriter = new QuoteVersionReadModelWriter(
                this.QuoteVersionReadModelUpdateRepository,
                this.QuoteVersionReadModelRepository,
                new QuoteReadModelRepository(this.DbContext, this.connectionConfiguration, this.Clock),
                this.QuoteReadModelUpdateRepository);
            var luceneRepository = new Mock<ILuceneRepository<IPolicySearchIndexWriteModel, IPolicySearchResultItemReadModel, PolicyReadModelFilters>>();
            var quoteSystemEventEmitterMock = new Mock<IQuoteSystemEventEmitter>();
            var policyReadModelWriter = new PolicyReadModelWriter(
                this.QuoteReadModelUpdateRepository,
                this.PolicyReadModelUpdateRepository,
                new ReadModelDeferredUpdateRepository<Domain.ReadModel.Policy.PolicyTransaction>(this.DbContext),
                this.TenantRepository,
                luceneRepository.Object,
                this.Clock);
            var quoteEventAggregator = new QuoteEventAggregator(
                quoteVersionReadModelWriter,
                quoteReadModelWriter,
                policyReadModelWriter,
                quoteDocumentReadModelWriter,
                quoteIntegrationEventScheduler.Object,
                quoteSystemEventEmitterMock.Object,
                textAdditionalPropertyValueReadModelWriter,
                new Mock<IClaimReadModelWriter>().Object);
            this.QuoteAggregateSnapshotService = new AggregateSnapshotService<QuoteAggregate>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.QuoteAggregateRepository = new QuoteAggregateRepository(
                this.DbContext,
                this.EventRecordRepository,
                quoteEventAggregator,
                this.QuoteAggregateSnapshotService,
                this.Clock,
                NullLogger<QuoteAggregateRepository>.Instance,
                serviceProvider);

            this.QuoteAggregateResolverService = new QuoteAggregateResolverService(
                this.QuoteReadModelRepository,
                this.PolicyReadModelRepository,
                this.QuoteAggregateRepository);

            this.FundingFactory = new FundingServiceFactory(
                this.QuoteAggregateResolverService,
                this.Clock,
                null,
                cachingAccessTokenProvider,
                this.CachingResolver,
                new Mock<IIqumulateService>().Object,
                this.QuoteAggregateRepository,
                this.HttpContextPropertiesResolver,
                serviceProvider);

            var organisationReadModelUpdateRepository = new ReadModelUpdateRepository<OrganisationReadModel>(this.DbContext);
            var organisationReadModelWriter = new OrganisationReadModelWriter(
                organisationReadModelUpdateRepository,
                propertyTypeEvaluatorService);
            var authenticationMethodReadModelWriter = new AuthenticationMethodReadModelWriter(
                new ReadModelUpdateRepository<AuthenticationMethodReadModelSummary>(this.DbContext));
            var organisationLinkedIdentityReadModelWriter = new OrganisationLinkedIdentityReadModelWriter(
                this.DbContext,
                this.AuthenticationMethodReadModelRepository);
            var organisationEventAggregator = new OrganisationEventAggregator(
                organisationReadModelWriter,
                authenticationMethodReadModelWriter,
                organisationSystemEventEmitter.Object,
                organisationLinkedIdentityReadModelWriter);
            this.OrganisationAggregateSnapshotService = new AggregateSnapshotService<Organisation>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.OrganisationAggregateRepository = new OrganisationAggregateRepository(
                this.DbContext,
                this.EventRecordRepository,
                organisationEventAggregator,
                this.OrganisationAggregateSnapshotService,
                this.Clock,
                NullLogger<OrganisationAggregateRepository>.Instance,
                serviceProvider);
            this.AuthenticationMethodReadModelRepository = new AuthenticationMethodReadModelRepository(this.DbContext);
            this.AuthenticationMethodService = new AuthenticationMethodService(
                this.OrganisationAggregateRepository,
                this.HttpContextPropertiesResolver,
                this.Clock);
            this.ReleaseRepository = new ReleaseRepository(this.DbContext);

            this.ReleaseService = new ReleaseService(
                this.ReleaseRepository,
                new Mock<IDevReleaseRepository>().Object,
                this.ProductRepository,
                new Mock<IFilesystemFileRepository>().Object,
                this.FilesystemStoragePathService,
                this.CachingResolver,
                this.Clock,
                this.DbContext);

            this.ConfigurationService = new ConfigurationService(
                new Mock<ICachingAuthenticationTokenProvider>().Object,
                new Mock<IFilesystemFileRepository>().Object,
                this.FilesystemStoragePathService,
                new Mock<IReleaseQueryService>().Object,
                this.ProductRepository,
                this.ReleaseRepository,
                this.CachingResolver,
                this.ReleaseService);

            // hashing
            this.PasswordHashingService = new PasswordHashingService();

            this.SystemAlertRepository = new SystemAlertRepository(this.DbContext);

            // alert
            this.SystemAlertService = new SystemAlertService(
                new Mock<ISystemAlertRepository>().Object,
                this.Clock,
                new Mock<ITenantService>().Object,
                new Mock<IEmailComposer>().Object,
                new Mock<IMessagingService>().Object,
                this.CachingResolver,
                new Mock<IESystemAlertConfiguration>().Object,
                mockTenantSystemEventEmitter.Object,
                this.JobClient,
                new Mock<IPolicyNumberRepository>().Object,
                new Mock<IClaimNumberRepository>().Object,
                new Mock<IInvoiceNumberRepository>().Object,
                new Mock<INumberPoolCountLastCheckedTimestampRepository>().Object);

            // Claims
            var uniqueClaimIdentifierService = new UniqueIdentifierService(this.DbContext, this.CachingResolver, this.Clock);
            var uniqueClaimNumberSequenceGenerator = new UniqueNumberSequenceGenerator(DatabaseFixture.TestConnectionString);
            this.ClaimNumberGenerator = new ClaimReferenceNumberGenerator(uniqueClaimNumberSequenceGenerator);
            this.ClaimNumberRepository = new ClaimNumberRepository(this.DbContext, this.connectionConfiguration, this.Clock);
            this.ClaimVersionReadModelRepository = new ClaimVersionReadModelRepository(this.DbContext);
            this.ClaimReadModelUpdateRepository = new ReadModelUpdateRepository<ClaimReadModel>(this.DbContext);
            this.ClaimAttachmentReadModelRepository = new ReadModelUpdateRepository<ClaimAttachmentReadModel>(this.DbContext);
            this.ClaimVersionReadModelUpdateRepository = new ReadModelUpdateRepository<ClaimVersionReadModel>(this.DbContext);
            var claimReadModelWriter = new ClaimReadModelWriter(
                this.ClaimReadModelUpdateRepository,
                this.ClaimVersionReadModelUpdateRepository,
                this.PolicyReadModelRepository,
                null);
            var claimAttachmentReadModelWriter = new ClaimAttachmentReadModelWriter(this.ClaimAttachmentReadModelRepository, this.ClaimReadModelUpdateRepository);
            var claimVersionReadModelWriter = new ClaimVersionReadModelWriter(this.ClaimReadModelUpdateRepository, this.ClaimVersionReadModelUpdateRepository);
            var claimSystemEventObserver = new Mock<IClaimSystemEventEmitter>();
            var claimEventAggregator = new ClaimEventAggregator(claimReadModelWriter, claimAttachmentReadModelWriter, claimVersionReadModelWriter, claimSystemEventObserver.Object);
            this.ClaimAggregateSnapshotService = new AggregateSnapshotService<ClaimAggregate>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.ClaimAggregateRepository = new ClaimAggregateRepository(
                this.DbContext,
                this.EventRecordRepository,
                claimEventAggregator,
                this.ClaimAggregateSnapshotService,
                this.Clock,
                NullLogger<ClaimAggregateRepository>.Instance,
                serviceProvider);

            // People
            var userReadModelUpdateRepository = new ReadModelUpdateRepository<UserReadModel>(this.DbContext);
            var userLoginEmailUpdateRepository = new ReadModelUpdateRepository<UserLoginEmail>(this.DbContext);
            this.UserLoginEmailRepository = new UserLoginEmailRepository(this.DbContext);
            var personReadModelRepository = new ReadModelUpdateRepository<PersonReadModel>(this.DbContext);
            var roleRepository = new RoleRepository(this.DbContext);
            var userReadModelWriter = new UserReadModelWriter(
                userReadModelUpdateRepository, userLoginEmailUpdateRepository, this.UserLoginEmailRepository, roleRepository, propertyTypeEvaluatorService);
            var customerReadModelUpdateRepository = new ReadModelUpdateRepository<CustomerReadModel>(this.DbContext);
            var customerReadModelWriter = new CustomerReadModelWriter(
                customerReadModelUpdateRepository,
                personReadModelRepository,
                userReadModelUpdateRepository,
                this.PolicyReadModelRepository,
                this.QuoteReadModelRepository,
                new EmailRepository(this.DbContext, this.Clock),
                propertyTypeEvaluatorService);
            var emailReadModelrepository = new ReadModelUpdateRepository<EmailAddressReadModel>(this.DbContext);
            var phoneReadModelrepository = new ReadModelUpdateRepository<PhoneNumberReadModel>(this.DbContext);
            var addressReadModelrepository = new ReadModelUpdateRepository<StreetAddressReadModel>(this.DbContext);
            var webAddressReadModelrepository = new ReadModelUpdateRepository<WebsiteAddressReadModel>(this.DbContext);
            var socialReadModelrepository = new ReadModelUpdateRepository<SocialMediaIdReadModel>(this.DbContext);
            var messengerReadModelrepository = new ReadModelUpdateRepository<MessengerIdReadModel>(this.DbContext);
            var personReadModelWriter = new PersonReadModelWriter(
                personReadModelUpdateRepository,
                customerReadModelUpdateRepository,
                emailReadModelrepository,
                phoneReadModelrepository,
                addressReadModelrepository,
                webAddressReadModelrepository,
                messengerReadModelrepository,
                socialReadModelrepository,
                propertyTypeEvaluatorService);
            var personEventObserver = new PersonEventAggregator(
                userReadModelWriter, customerReadModelWriter, quoteReadModelWriter, policyReadModelWriter, claimReadModelWriter, personReadModelWriter);
            this.PersonAggregateSnapshotService = new AggregateSnapshotService<PersonAggregate>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.PersonAggregateRepository = new PersonAggregateRepository(
                this.DbContext,
                this.EventRecordRepository,
                personEventObserver,
                this.PersonAggregateSnapshotService,
                this.Clock,
                NullLogger<PersonAggregateRepository>.Instance,
                serviceProvider);
            this.UserReadModelRepository = new UserReadModelRepository(this.DbContext);
            this.PersonReadModelRepository = new PersonReadModelRepository(this.DbContext, this.PolicyReadModelRepository, this.Clock);

            // Users
            var userLinkedIdentityReadModelWriter = new UserLinkedIdentityReadModelWriter(
                this.DbContext,
                this.AuthenticationMethodReadModelRepository);

            var mockUserSystemEventEmitter = new Mock<IUserSystemEventEmitter>();
            this.UserSystemEventEmitter = mockUserSystemEventEmitter.Object;
            var userEventAggregator = new UserEventAggregator(
                userReadModelWriter,
                customerReadModelWriter,
                personReadModelWriter,
                userLinkedIdentityReadModelWriter,
                this.UserSystemEventEmitter);
            this.UserAggregateSnapshotService = new AggregateSnapshotService<UserAggregate>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.UserAggregateRepository = new UserAggregateRepository(
                this.DbContext,
                this.EventRecordRepository,
                userEventAggregator,
                this.UserAggregateSnapshotService,
                this.Clock,
                NullLogger<UserAggregateRepository>.Instance,
                serviceProvider);

            this.EventActionRepository = new SystemEventRepository(this.DbContext, this.connectionConfiguration);

            // Portals
            this.PortalAggregateSnapshotService = new AggregateSnapshotService<PortalAggregate>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.PortalAggregateRepository = new PortalAggregateRepository(
                this.DbContext,
                this.EventRecordRepository,
                new PortalEventAggregator(
                    new PortalReadModelWriter(new ReadModelUpdateRepository<PortalReadModel>(this.DbContext)),
                    new PortalSignInMethodReadModelWriter(
                        new ReadModelUpdateRepository<PortalSignInMethodReadModel>(this.DbContext),
                        this.DbContext,
                        this.AuthenticationMethodReadModelRepository),
                    new Mock<IPortalSystemEventEmitter>().Object),
                this.PortalAggregateSnapshotService,
                this.Clock,
                NullLogger<PortalAggregateRepository>.Instance,
                serviceProvider);

            // Additional property services
            this.AdditionalPropertyValueService = new Mock<IAdditionalPropertyValueService>();

            // Organisation
            var httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
            this.OrganisationService = new OrganisationService(
                this.OrganisationAggregateRepository,
                this.OrganisationReadModelRepository,
                this.TenantRepository,
                this.AdditionalPropertyValueService.Object,
                httpContextPropertiesResolver.Object,
                this.Mediator,
                this.CachingResolver,
                this.Clock,
                this.ProductRepository,
                this.ProductFeatureSettingRepository,
                this.ProductOrganisationSettingRepository);

            this.DomainProductService = new UBind.Domain.Services.ProductService(
                this.ProductFeatureSettingService,
                this.ProductOrganisationSettingRepository,
                this.OrganisationService,
                this.ProductPortalSettingRepository,
                this.PortalReadModelRepository,
                this.ProductConfigurationProvider);

            this.MediatorMock = new Mock<ICqrsMediator>();

            // Customers
            this.CustomerAggregateSnapshotService = new AggregateSnapshotService<CustomerAggregate>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.CustomerAggregateRepository = new CustomerAggregateRepository(
                this.DbContext,
                this.EventRecordRepository,
                customerReadModelWriter,
                this.CustomerAggregateSnapshotService,
                this.Clock,
                NullLogger<CustomerAggregateRepository>.Instance,
                serviceProvider);
            this.CustomerService = new CustomerService(
                this.CustomerAggregateRepository,
                this.CustomerReadModelRepository,
                this.UserAggregateRepository,
                this.PersonAggregateRepository,
                this.HttpContextPropertiesResolver,
                this.PersonReadModelRepository,
                this.Clock,
                this.AdditionalPropertyValueService.Object,
                this.CachingResolver);

            // Accounting
            this.PaymentReferenceNumberGenerator = new PaymentReferenceNumberGenerator(uniqueClaimNumberSequenceGenerator);
            this.RefundReferenceNumberGenerator = new RefundReferenceNumberGenerator(uniqueClaimNumberSequenceGenerator);

            // Payments
            this.PaymentReadModelRepository = new PaymentReadModelRepository(this.DbContext);
            var paymentReadModelUpdateRepository = new ReadModelUpdateRepository<PaymentReadModel>(this.DbContext);
            var paymentAllocationsReadModelUpdateRepository = new ReadModelUpdateRepository<PaymentAllocationReadModel>(this.DbContext);

            var invoiceRepository = new Mock<IReadModelRepository<UBind.Domain.Accounting.Invoice>>();
            var fakeInvoice = new UBind.Domain.Accounting.Invoice(
                  TenantFactory.DefaultId,
                  this.Clock.GetCurrentInstant(),
                  new InvoiceNumber(TenantFactory.DefaultId, ProductFactory.DefaultId, DeploymentEnvironment.Development, "AAA123", this.Clock.GetCurrentInstant()),
                  this.Clock.GetCurrentInstant().Plus(Duration.FromDays(7)),
                  Guid.NewGuid());

            invoiceRepository.Setup(x => x.SingleMaybe(TenantFactory.DefaultId, It.IsAny<Expression<Func<UBind.Domain.Accounting.Invoice, bool>>>())).Returns(fakeInvoice);
            invoiceRepository.Setup(x => x.GetByIdMaybe(TenantFactory.DefaultId, It.IsAny<Guid>())).Returns(fakeInvoice);
            invoiceRepository.Setup(x => x.GetById(TenantFactory.DefaultId, It.IsAny<Guid>())).Returns(fakeInvoice);

            var creditNoteRepository = new Mock<IReadModelRepository<UBind.Domain.Accounting.CreditNote>>();
            var fakeCreditNote = new CreditNote(
                  TenantFactory.DefaultId,
                  this.Clock.GetCurrentInstant(),
                  new CreditNoteNumber(TenantFactory.DefaultId, ProductFactory.DefaultId, DeploymentEnvironment.Development, "AAA123", this.Clock.GetCurrentInstant()),
                  this.Clock.GetCurrentInstant().Plus(Duration.FromDays(7)),
                  Guid.NewGuid());

            creditNoteRepository.Setup(x => x.SingleMaybe(TenantFactory.DefaultId, It.IsAny<Expression<Func<CreditNote, bool>>>())).Returns(fakeCreditNote);
            creditNoteRepository.Setup(x => x.GetByIdMaybe(TenantFactory.DefaultId, It.IsAny<Guid>())).Returns(fakeCreditNote);
            creditNoteRepository.Setup(x => x.GetById(TenantFactory.DefaultId, It.IsAny<Guid>())).Returns(fakeCreditNote);

            var paymentReadModelWriter = new PaymentReadModelWriter(paymentReadModelUpdateRepository, paymentAllocationsReadModelUpdateRepository, invoiceRepository.Object);
            this.InvoiceAggregateSnapshotService = new AggregateSnapshotService<FinancialTransactionAggregate<Domain.Accounting.Invoice>>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.PaymentAggregateRepository = new PaymentAggregateRepository(
                this.DbContext,
                this.EventRecordRepository,
                this.InvoiceAggregateSnapshotService,
                paymentReadModelWriter,
                this.Clock,
                NullLogger<PaymentAggregateRepository>.Instance,
                serviceProvider);

            // Refunds
            this.RefundReadModelRepository = new RefundReadModelRepository(this.DbContext);
            var refundReadModelUpdateRepository = new ReadModelUpdateRepository<RefundReadModel>(this.DbContext);
            var refundAllocationsReadModelUpdateRepository = new ReadModelUpdateRepository<RefundAllocationReadModel>(this.DbContext);
            var refundReadModelWriter = new RefundReadModelWriter(refundReadModelUpdateRepository, refundAllocationsReadModelUpdateRepository, creditNoteRepository.Object);
            this.CreditNoteAggregateSnapshotService = new AggregateSnapshotService<FinancialTransactionAggregate<Domain.Accounting.CreditNote>>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.RefundAggregateRepository = new RefundAggregateRepository(
                this.DbContext,
                this.EventRecordRepository,
                this.CreditNoteAggregateSnapshotService,
                refundReadModelWriter,
                this.Clock,
                NullLogger<RefundAggregateRepository>.Instance,
                serviceProvider);

            // Reports
            var reportReadModelUpdateRepository = new ReadModelUpdateRepository<ReportReadModel>(this.DbContext);
            this.ReportReadModelRepository = new ReportReadModelRepository(this.DbContext);
            var reportReadModelWriter = new ReportReadModelWriter(reportReadModelUpdateRepository, this.ProductRepository, this.TenantRepository);
            this.ReportAggregateSnapshotService = new AggregateSnapshotService<ReportAggregate>(
                this.AggregateSnapshotRepository,
                serviceProvider,
                this.Clock);
            this.ReportAggregateRepository = new ReportAggregateRepository(
                this.DbContext,
                this.EventRecordRepository,
                reportReadModelWriter,
                this.ReportAggregateSnapshotService,
                this.Clock,
                NullLogger<ReportAggregateRepository>.Instance,
                serviceProvider);
            this.PolicyTransactionReadModelRepository = new PolicyTransactionReadModelRepository(this.DbContext, this.connectionConfiguration);

            this.ReportService = new ReportService(
                this.ReportAggregateRepository,
                this.Clock,
                this.ReportReadModelRepository,
                new Mock<IReportFileRepository>().Object,
                this.DropGenerationService,
                this.HttpContextPropertiesResolver,
                this.CachingResolver);

            // Tenant Service
            this.TenantService = new TenantService(
                this.TenantRepository,
                this.Clock);

            this.UserSessionRepositoryMock = new Mock<IUserSessionRepository>();

            this.UserSessionDeletionService = new UserSessionDeletionService(
                this.UserSessionRepositoryMock.Object,
                this.UserReadModelRepository,
                this.UserSystemEventEmitter,
                new Mock<IBackgroundJobClient>().Object,
                this.TenantRepository,
                NullLogger<UserSessionDeletionService>.Instance,
                this.Clock,
                this.HttpContextPropertiesResolver);

            // Roles
            this.RoleRepository = new RoleRepository(this.DbContext);

            this.RoleService = new RoleService(
                this.RoleRepository,
                this.CachingResolver,
                this.Clock,
                this.UserSessionDeletionService,
                this.DbContext);

            string[] errors = { "Meh" };
            Maybe<IFundingConfiguration> fundingConfigurationResult = fundingConfiguration != null
                ? Maybe<IFundingConfiguration>.From(fundingConfiguration)
                : Maybe<IFundingConfiguration>.None;
            fundingConfigurationProvider
                .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(fundingConfigurationResult));

            // Funding
            this.ApplicationFundingService = new ApplicationFundingService(
                fundingConfigurationProvider.Object,
                this.FundingFactory,
                this.QuoteAggregateRepository,
                this.HttpContextPropertiesResolver,
                this.Clock,
                this.QuoteAggregateResolverService);

            // ApplicationServices
            var uniqueIdentifierService = new UniqueIdentifierService(this.DbContext, this.CachingResolver, this.Clock);
            var uniqueNumberSequenceGenerator = new UniqueNumberSequenceGenerator(DatabaseFixture.TestConnectionString);
            var quoteNumberGenerator = new QuoteReferenceNumberGenerator(uniqueNumberSequenceGenerator);
            this.ContextAccessor = new Mock<IHttpContextAccessor>().Object;
            this.ApplicationQuoteService = new ApplicationQuoteService(
                new Mock<IQuoteAggregateRepository>().Object,
                this.TenantRepository,
                this.UserAggregateRepository,
                this.PersonAggregateRepository,
                new DefaultQuoteWorkflowProvider(),
                this.CustomerService,
                this.QuoteExpirySettingsProvider,
                this.CustomerAggregateRepository,
                this.UserLoginEmailRepository,
                this.PolicyService,
                this.HttpContextPropertiesResolver,
                this.Clock,
                this.QuoteAggregateResolverService,
                this.OrganisationReadModelRepository,
                this.ProductFeatureSettingService,
                this.SystemEmailService,
                this.Mediator,
                quoteSystemEventEmitterMock.Object);

            var realPolicyNumberRepository = new PolicyNumberRepository(this.DbContext, this.connectionConfiguration, this.Clock);
            this.PolicyNumberRepository = new FakePolicyNumberRepository(realPolicyNumberRepository);
            this.PolicyService = new PolicyService(
                this.QuoteAggregateRepository,
                this.Clock,
                this.CachingResolver,
                this.ClaimReadModelRepository,
                this.ProductConfigurationProvider,
                this.UniqueIdentifierService,
                this.QuoteDocumentReadModelRepository,
                this.PolicyNumberRepository,
                this.PolicyReadModelRepository,
                this.SystemAlertService,
                this.QuoteNumberGenerator,
                this.HttpContextPropertiesResolver,
                this.ProductFeatureSettingService,
                this.DefaultWorkflowProvider,
                this.PolicyTransactionTimeOfDayScheme,
                this.DbContext);

            this.PerilsRepository = new PerilsRepository(new Mock<DvaDbContext>().Object);

            this.BaseUrlResolver = new BaseUrlResolver(this.EmailInvitationConfiguration);

            this.PortalService = new PortalService(
                this.CachingResolver,
                this.BaseUrlResolver,
                this.PortalReadModelRepository);

            // User profile picture repository
            this.UserProfilePictureRepository = new UserProfilePictureRepository(this.DbContext);
            this.MockReleaseQueryService = new Mock<IReleaseQueryService>();
            this.PasswordComplexityValidator = UBind.Domain.Authentication.PasswordComplexityValidator.Default;

            // activation service
            this.UserActivationInvitationService = new UserActivationInvitationService(
                this.PersonAggregateRepository,
                this.UserAggregateRepository,
                this.UserReadModelRepository,
                this.PasswordHashingService,
                this.PasswordComplexityValidator,
                this.SystemEmailService,
                this.OrganisationReadModelRepository,
                this.HttpContextPropertiesResolver,
                this.Mediator,
                this.CachingResolver,
                this.CustomerReadModelRepository,
                this.PersonReadModelRepository,
                this.PortalService,
                this.Clock);

            // User profile picture repository
            this.UserProfilePictureRepository = new UserProfilePictureRepository(this.DbContext);
            this.MockReleaseQueryService = new Mock<IReleaseQueryService>();

            this.PasswordComplexityValidator = UBind.Domain.Authentication.PasswordComplexityValidator.Default;

            // user service
            this.UserService = new Application.User.UserService(
                this.UserAggregateRepository,
                this.CustomerAggregateRepository,
                this.PersonAggregateRepository,
                this.UserReadModelRepository,
                this.RoleRepository,
                this.UserProfilePictureRepository,
                this.OrganisationReadModelRepository,
                this.UserLoginEmailRepository,
                this.PasswordHashingService,
                this.CustomerService,
                this.HttpContextPropertiesResolver,
                this.QuoteAggregateResolverService,
                this.UserActivationInvitationService,
                this.Mediator,
                this.AdditionalPropertyValueService.Object,
                this.PasswordComplexityValidator,
                this.Clock,
                this.DbContext,
                this.CachingResolver,
                this.AuthenticationMethodReadModelRepository,
                this.UserSessionDeletionService,
                this.UserSystemEventEmitter);

            this.UserSessionService = new UserSessionService(
                this.UserSessionRepositoryMock.Object,
                this.CachingResolver,
                this.Clock,
                new Mock<ILogger<UserSessionService>>().Object);

            // caching resolver
            this.CachingResolver = new CachingResolver(
                this.Mediator, this.TenantRepository, this.ProductRepository, this.FeatureSettingRepository, this.ProductFeatureSettingRepository);

            // email repository
            this.EmailRepository = new EmailRepository(this.DbContext, this.Clock);

            this.SecondEmailRepository = new EmailRepository(this.DbContext, this.Clock);

            this.EmailService = new EmailService(this.EmailRepository, this.FileContentRepository, this.Clock);

            this.EmailQueryService = new EmailQueryService(
               this.EmailRepository);

            this.SystemEmailService = new SystemEmailService(
                this.EmailService,
                new Mock<ISmtpClientConfiguration>().Object,
                this.EmailTemplateService,
                new MailClientFactory(),
                new Mock<IJobClient>().Object,
                NullLogger<SystemEmailService>.Instance,
                this.Mediator,
                this.Clock,
                this.FileContentRepository);

            this.ProductService = new Application.ProductService(
                this.TenantRepository,
                this.ProductRepository,
                new Mock<ICachingAuthenticationTokenProvider>().Object,
                new Mock<IFilesystemFileRepository>().Object,
                this.FilesystemStoragePathService,
                new Mock<IJobClient>().Object,
                this.Clock,
                new Mock<IInvoiceNumberRepository>().Object,
                new Mock<IClaimNumberRepository>().Object,
                new Mock<IPolicyNumberRepository>().Object,
                new Mock<ICreditNoteNumberRepository>().Object,
                this.QuoteService,
                new Mock<IProductFeatureSettingService>().Object,
                this.AutomationPeriodicTriggerSchedulerMock.Object,
                NullLogger<UBind.Application.ProductService>.Instance,
                this.MockMediator.Object,
                this.HttpContextPropertiesResolver,
                new Mock<IProductPortalSettingRepository>().Object,
                this.CachingResolver);

            this.ApplicationQuoteService = new ApplicationQuoteService(
                this.QuoteAggregateRepository,
                this.TenantRepository,
                this.UserAggregateRepository,
                this.PersonAggregateRepository,
                this.DefaultWorkflowProvider,
                this.CustomerService,
                this.QuoteExpirySettingsProvider,
                this.CustomerAggregateRepository,
                this.UserLoginEmailRepository,
                this.PolicyService,
                this.HttpContextPropertiesResolver,
                this.Clock,
                this.QuoteAggregateResolverService,
                this.OrganisationReadModelRepository,
                this.ProductFeatureSettingService,
                this.SystemEmailService,
                this.Mediator,
                quoteSystemEventEmitterMock.Object);

            this.ClaimService = new ClaimService(
                this.ClaimWorkflowProvider,
                this.ClaimAggregateRepository,
                this.ClaimReadModelRepository,
                this.QuoteAggregateRepository,
                this.CustomerAggregateRepository,
                this.PersonAggregateRepository,
                this.ClaimNumberRepository,
                this.HttpContextPropertiesResolver,
                this.SystemAlertService,
                this.CachingResolver,
                this.Clock);

            // quote
            this.QuoteService = new QuoteService(
                this.QuoteReadModelRepository,
                this.QuoteVersionReadModelRepository,
                this.QuoteAggregateRepository,
                this.HttpContextPropertiesResolver,
                this.QuoteAggregateResolverService,
                this.Clock,
                this.DbContext,
                NullLogger<QuoteService>.Instance,
                new Mock<IRedisConfiguration>().Object,
                new Mock<IConnectionMultiplexer>().Object,
                this.CachingResolver);

            this.QuoteSearchIndexService = new QuoteLuceneIndexService(
                this.QuoteReadModelRepository,
                this.TenantRepository,
                this.LuceneQuoteRepository,
                NullLogger<QuoteLuceneIndexService>.Instance,
                this.ErrorNotificationServiceMock.Object);

            this.PolicySearchIndexService = new PolicyLuceneIndexService(
                this.PolicyReadModelRepository,
                this.TenantRepository,
                this.LucenePolicyRepository,
                NullLogger<PolicyLuceneIndexService>.Instance,
                this.ErrorNotificationServiceMock.Object);

            // Documents
            this.FileContentRepository = new FileContentRepository(this.DbContext);
            this.ApplicationDocumentService = new ApplicationDocumentService(
                this.FileContentRepository,
                this.QuoteAggregateRepository,
                this.HttpContextPropertiesResolver,
                this.Clock,
                NullLogger<ApplicationDocumentService>.Instance);

            // Import service
            this.MappingTransactionService = new MappingTransactionService(
                this.PersonReadModelRepository,
                this.PersonAggregateRepository,
                this.CustomerAggregateRepository,
                this.PolicyReadModelRepository,
                this.QuoteAggregateRepository,
                this.QuoteReadModelRepository,
                this.DefaultTimeOfDayScheme,
                this.ClaimAggregateRepository,
                this.ClaimReadModelRepository,
                this.UserReadModelRepository,
                this.ClaimNumberGenerator,
                this.QuoteExpirySettingsProvider,
                this.HttpContextPropertiesResolver,
                this.CachingResolver,
                this.Clock,
                this.MediatorMock.Object);
            this.PortalSettingsRepository = new PortalSettingRepository(this.DbContext);

            this.PortalSettingsService = new PortalSettingsService(
                this.PortalReadModelRepository,
                this.TenantRepository,
                this.PortalSettingsRepository,
                this.CachingResolver,
                this.Clock);

            this.FilesystemFileRepositoryMock = new Mock<IFilesystemFileRepository>();

            this.LoginAttemptResultRepository = new EmailRequestRecordRepository<LoginAttemptResult>(this.DbContext);
            this.PasswordResetAttemptResultRepository = new EmailRequestRecordRepository<PasswordResetRecord>(this.DbContext);
            this.EmailAddressBlockingEventRepository = new EmailAddressBlockingEventRepository(this.DbContext);

            this.PasswordResetTrackingService = new PasswordResetTrackingService(
                this.PasswordResetAttemptResultRepository,
                this.Clock);

            this.UserPasswordResetInvitationService = new UserPasswordResetInvitationService(
                this.SystemEmailService,
                this.UserLoginEmailRepository,
                this.UserAggregateRepository,
                this.UserReadModelRepository,
                this.PersonAggregateRepository,
                this.PasswordHashingService,
                this.PasswordComplexityValidator,
                this.PasswordReuseValidator,
                this.HttpContextPropertiesResolver,
                this.Mediator,
                this.UserSessionDeletionService,
                this.Clock,
                this.CachingResolver,
                Mock.Of<ILogger<UserPasswordResetInvitationService>>(),
                this.UserActivationInvitationService);

            this.LoginAttemptTrackingService = new LoginAttemptTrackingService(
                this.LoginAttemptResultRepository,
                this.EmailAddressBlockingEventRepository,
                this.OrganisationAggregateRepository,
                this.UserSystemEventEmitter,
                this.UserLoginEmailRepository,
                this.Clock);

            this.UserActivationInvitationService = new UserActivationInvitationService(
                this.PersonAggregateRepository,
                this.UserAggregateRepository,
                this.UserReadModelRepository,
                this.PasswordHashingService,
                this.PasswordComplexityValidator,
                this.SystemEmailService,
                this.OrganisationReadModelRepository,
                this.HttpContextPropertiesResolver,
                this.Mediator,
                this.CachingResolver,
                this.CustomerReadModelRepository,
                this.PersonReadModelRepository,
                this.PortalService,
                this.Clock);

            this.RazorEngineService = new Mock<IRazorEngineService>().Object;

            this.DeploymentRepository = new DeploymentRepository(this.DbContext);

            // this.BackgroundJobStorageProvider = new SqlServerStorage(this.connectionConfiguration.UBind);
            this.BackgroundJobClient = new BackgroundJobClient(this.BackgroundJobStorageProvider);
            this.BackgroundJobService = new BackgroundJobService(
                this.UserService,
                this.Clock,
                this.BackgroundJobClient,
                this.CachingResolver);

            this.PersonReadModelRepository = new PersonReadModelRepository(
                this.DbContext, this.PolicyReadModelRepository, this.Clock);
            this.PersonService = new PersonService(
                this.CustomerAggregateRepository,
                this.CustomerReadModelRepository,
                this.PersonAggregateRepository,
                this.PersonReadModelRepository,
                this.HttpContextPropertiesResolver,
                this.UserAggregateRepository,
                this.UserReadModelRepository,
                this.TenantRepository,
                this.Clock);
            this.UserLoginEmailRepository = new UserLoginEmailRepository(this.DbContext);

            this.FeatureSettingRepository = new FeatureSettingRepository(this.DbContext, this.Clock);

            this.RenewalInvitationService = new RenewalInvitationService(
                 this.PersonAggregateRepository,
                 this.TenantRepository,
                 this.UserAggregateRepository,
                 this.PolicyService,
                 this.SystemEmailService,
                 this.CustomerAggregateRepository,
                 this.QuoteAggregateRepository,
                 this.HttpContextPropertiesResolver,
                 this.OrganisationReadModelRepository,
                 this.UserLoginEmailRepository,
                 this.ProductFeatureSettingService,
                 this.UserService,
                 this.Mediator,
                 this.CachingResolver,
                 this.Clock);

            this.DomainUserService = new UBind.Domain.Services.UserService(this.UserAggregateRepository, this.Clock);
            this.OrganisationTransferService = new OrganisationTransferService(
                this.UserReadModelRepository,
                this.PersonAggregateRepository,
                this.PersonReadModelRepository,
                this.UserAggregateRepository,
                this.CustomerReadModelRepository,
                this.CustomerAggregateRepository,
                this.ClaimReadModelRepository,
                this.ClaimAggregateRepository,
                this.QuoteReadModelRepository,
                this.QuoteAggregateRepository,
                this.EmailRepository,
                this.RoleRepository,
                this.OrganisationService,
                new Mock<IProgressLoggerFactory>().Object,
                this.Clock);

            // Sms Repository
            this.SmsRepository = new SmsRepository(this.DbContext);

            var dkimSetting = new Mock<IDkimSettingRepository>().Object;
            this.AuthorisationService = new AuthorisationService(
                this.OrganisationService,
                this.UserService,
                this.CustomerService,
                this.RoleService,
                this.PolicyReadModelRepository,
                this.ClaimReadModelRepository,
                this.QuoteReadModelRepository,
                this.Mediator,
                this.CachingResolver,
                this.QuoteAggregateResolverService,
                dkimSetting,
                this.HttpContextPropertiesResolver,
                this.ReportReadModelRepository,
                this.UserSessionService);

            this.AdditionalPropertyTransformHelper = new Mock<IAdditionalPropertyTransformHelper>().Object;
        }

        public UBind.Domain.Services.IUserService DomainUserService { get; private set; }

        public TestClock Clock { get; }

        public TestUBindDbContext DbContext { get; }

        public EventRecordRepository EventRecordRepository { get; }

        public IMessagingService? MessagingService { get; }

        public IPasswordComplexityValidator PasswordComplexityValidator { get; }

        public IHttpContextPropertiesResolver HttpContextPropertiesResolver { get; }

        public ISystemEventService SystemEventService { get; }

        public IPasswordReuseValidator PasswordReuseValidator { get; }

        public TenantService TenantService { get; }

        public AuthorisationService AuthorisationService { get; }

        public DefaultProductConfigurationProvider ConfigurationProvider { get; }

        public UniqueIdentifierService UniqueIdentifierService { get; }

        public IConfigurationService ConfigurationService { get; }

        public UserActivationInvitationService UserActivationInvitationService { get; }

        public UniqueNumberSequenceGenerator UniqueNumberSequenceGenerator { get; }

        public QuoteReferenceNumberGenerator QuoteNumberGenerator { get; }

        public ClaimReferenceNumberGenerator ClaimNumberGenerator { get; }

        public PaymentReferenceNumberGenerator PaymentReferenceNumberGenerator { get; }

        public RefundReferenceNumberGenerator RefundReferenceNumberGenerator { get; }

        public QuoteAggregateRepository QuoteAggregateRepository { get; }

        public IQuoteAggregateResolverService QuoteAggregateResolverService { get; }

        public OrganisationAggregateRepository OrganisationAggregateRepository { get; }

        public ReadModelUpdateRepository<NewQuoteReadModel> QuoteReadModelUpdateRepository { get; }

        public ReadModelUpdateRepository<PolicyReadModel> PolicyReadModelUpdateRepository { get; }

        public ReadModelUpdateRepository<QuoteVersionReadModel> QuoteVersionReadModelUpdateRepository { get; }

        public IQuoteDocumentReadModelRepository QuoteDocumentReadModelRepository { get; }

        public QuoteVersionReadModelRepository QuoteVersionReadModelRepository { get; }

        public TextAdditionalPropertyValueAggregateRepository TextAdditionalPropertyValueAggregateRepository { get; }

        public AdditionalPropertyDefinitionAggregateRepository AdditionalPropertyDefinitionAggregateRepository { get; }

        public TextAdditionalPropertyValueReadModelRepository TextAdditionalPropertyValueReadModelRepository { get; }

        public PasswordHashingService PasswordHashingService { get; }

        public SystemAlertRepository SystemAlertRepository { get; }

        public SystemAlertService SystemAlertService { get; }

        public BackgroundJobService BackgroundJobService { get; }

        public IFeatureSettingRepository FeatureSettingRepository { get; }

        public SqlServerStorage BackgroundJobStorageProvider { get; }

        public PaymentService AccountingPaymentService { get; }

        public ReportService ReportService { get; }

        public ClaimNumberRepository ClaimNumberRepository { get; }

        public ClaimReadModelRepository ClaimReadModelRepository { get; }

        public ClaimAggregateRepository ClaimAggregateRepository { get; }

        public ClaimVersionReadModelRepository ClaimVersionReadModelRepository { get; }

        public ClaimService ClaimService { get; }

        public QuoteService QuoteService { get; }

        public OrganisationReadModelRepository OrganisationReadModelRepository { get; }

        public OrganisationService OrganisationService { get; }

        public UBind.Domain.Services.ProductService DomainProductService { get; }

        public QuoteLuceneIndexService QuoteSearchIndexService { get; }

        public PolicyLuceneIndexService PolicySearchIndexService { get; }

        public DefaultPolicyTransactionTimeOfDayScheme DefaultTimeOfDayScheme { get; }

        public FakePolicyNumberRepository PolicyNumberRepository { get; }

        public ReadModelUpdateRepository<ClaimReadModel> ClaimReadModelUpdateRepository { get; }

        public ReadModelUpdateRepository<ClaimAttachmentReadModel> ClaimAttachmentReadModelRepository { get; }

        public ReadModelUpdateRepository<ClaimVersionReadModel> ClaimVersionReadModelUpdateRepository { get; }

        public ReadModelUpdateRepository<PaymentReadModel> PaymentReadModelUpdateRepository { get; }

        public DeploymentRepository DeploymentRepository { get; }

        public ReadModelUpdateRepository<QuoteFileAttachmentReadModel> QuoteFileAttachmentReadModelUpdateRepository { get; }

        public TenantRepository TenantRepository { get; }

        public PersonReadModelRepository PersonReadModelRepository { get; }

        public PersonService PersonService { get; }

        public UserLoginEmailRepository UserLoginEmailRepository { get; }

        public ISystemEmailTemplateRepository SystemEmailTemplateRepository
        {
            get
            {
                var repo = new Mock<ISystemEmailTemplateRepository>();
                repo.Setup(x => x.GetApplicableTemplates(
                    It.IsAny<Guid>(),
                    It.IsAny<SystemEmailType>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid?>()))
                    .Returns(Enumerable.Empty<ISystemEmailTemplateSummary>());

                return repo.Object;
            }
        }

        public EmailTemplateService EmailTemplateService { get; }

        public SystemEmailService SystemEmailService { get; }

        public IHttpContextAccessor ContextAccessor { get; }

        public EmailInvitationConfiguration EmailInvitationConfiguration { get; }

        public BaseUrlResolver BaseUrlResolver { get; }

        public ProductRepository ProductRepository { get; }

        public ILuceneIndexCache LuceneIndexCache { get; }

        public CachingResolver CachingResolver { get; }

        public FundingServiceFactory FundingFactory { get; }

        public IProductFeatureSettingService ProductFeatureSettingService { get; set; }

        public IProductFeatureSettingRepository ProductFeatureSettingRepository { get; set; }

        public IReleaseQueryService ReleaseQueryService { get; set; }

        public IProductReleaseService ProductReleaseService { get; set; }

        public IQuoteExpirySettingsProvider QuoteExpirySettingsProvider { get; }

        public IJobClient JobClient => new Mock<IJobClient>().Object;

        public IBackgroundJobClient BackgroundJobClient { get; }

        public DefaultProductConfigurationProvider ProductConfigurationProvider { get; }

        public IPolicyTransactionTimeOfDayScheme PolicyTransactionTimeOfDayScheme { get; }

        public DefaultQuoteWorkflowProvider DefaultWorkflowProvider { get; }

        public IQuoteWorkflow DefaultQuoteWorkflow { get; }

        public UBind.Application.ProductService ProductService { get; }

        public QuoteReadModelRepository QuoteReadModelRepository { get; }

        public DropGenerationService DropGenerationService { get; }

        public LuceneQuoteRepository LuceneQuoteRepository { get; }

        public LucenePolicyRepository LucenePolicyRepository { get; }

        public QuoteLuceneDocumentBuilder QuoteLuceneDocumentBuilder { get; }

        public PolicyLuceneDocumentBuilder PolicyLuceneDocumentBuilder { get; }

        public IPolicyReadModelRepository PolicyReadModelRepository { get; }

        public IProductOrganisationSettingRepository ProductOrganisationSettingRepository { get; }

        public IProductPortalSettingRepository ProductPortalSettingRepository { get; }

        public PersonAggregateRepository PersonAggregateRepository { get; }

        public UserProfilePictureRepository UserProfilePictureRepository { get; }

        public Mock<IReleaseQueryService> MockReleaseQueryService { get; }

        public UserReadModelRepository UserReadModelRepository { get; }

        public UserAggregateRepository UserAggregateRepository { get; }

        public SystemEventRepository EventActionRepository { get; }

        public CustomerAggregateRepository CustomerAggregateRepository { get; }

        public PaymentAggregateRepository PaymentAggregateRepository { get; }

        public RefundAggregateRepository RefundAggregateRepository { get; }

        public ReportAggregateRepository ReportAggregateRepository { get; }

        public PolicyTransactionReadModelRepository PolicyTransactionReadModelRepository { get; }

        public PaymentReadModelRepository PaymentReadModelRepository { get; }

        public RefundReadModelRepository RefundReadModelRepository { get; }

        public ReportReadModelRepository ReportReadModelRepository { get; }

        public IPolicyService PolicyService { get; }

        public ApplicationQuoteService ApplicationQuoteService { get; }

        public RenewalInvitationService RenewalInvitationService { get; }

        public IApplicationDocumentService ApplicationDocumentService { get; }

        public IFileContentRepository FileContentRepository { get; }

        public UserSessionService UserSessionService { get; }

        public Mock<IUserSessionRepository> UserSessionRepositoryMock { get; }

        public Mock<ICqrsMediator> MockMediator { get; }

        public ICqrsMediator Mediator { get; }

        public Application.User.UserService UserService { get; }

        public EmailRepository EmailRepository { get; }

        public EmailService EmailService { get; }

        public EmailQueryService EmailQueryService { get; }

        public ApplicationFundingService ApplicationFundingService { get; }

        public CustomerReadModelRepository CustomerReadModelRepository { get; }

        public CustomerService CustomerService { get; }

        public IQuoteDeletionManager QuoteReductor { get; }

        public IMiniProfilerDeletionManager MiniProfilerReductor { get; }

        public IMappingTransactionService MappingTransactionService { get; }

        public IPatchService PolicyDataPatchService { get; }

        public PortalReadModelRepository PortalReadModelRepository { get; set; }

        public PortalSettingRepository PortalSettingsRepository { get; private set; }

        public PortalSettingsService PortalSettingsService { get; private set; }

        public PortalService PortalService { get; private set; }

        public Mock<IFilesystemFileRepository> FilesystemFileRepositoryMock { get; private set; }

        public UserPasswordResetInvitationService UserPasswordResetInvitationService { get; private set; }

        public LoginAttemptTrackingService LoginAttemptTrackingService { get; private set; }

        public IProgressLogger ProgressLogger { get; }

        public RoleRepository RoleRepository { get; }

        public RoleService RoleService { get; }

        public IClaimWorkflowProvider ClaimWorkflowProvider { get; }

        public IEmailRequestRecordRepository<LoginAttemptResult> LoginAttemptResultRepository { get; private set; }

        public IEmailRequestRecordRepository<PasswordResetRecord> PasswordResetAttemptResultRepository { get; private set; }

        public EmailAddressBlockingEventRepository EmailAddressBlockingEventRepository { get; private set; }

        public PasswordResetTrackingService PasswordResetTrackingService { get; private set; }

        public ReleaseRepository ReleaseRepository { get; private set; }

        public IDevReleaseRepository DevReleaseRepository { get; private set; }

        public IEntitySettingsRepository EntitySettingsRepository { get; }

        public ReleaseService ReleaseService { get; }

        public PerilsRepository PerilsRepository { get; private set; }

        public IRazorEngineService RazorEngineService { get; private set; }

        public IFilesystemStoragePathService FilesystemStoragePathService { get; private set; }

        public IOrganisationTransferService OrganisationTransferService { get; private set; }

        public Mock<IAdditionalPropertyValueService> AdditionalPropertyValueService { get; private set; }

        public Mock<ICqrsMediator> MediatorMock { get; private set; }

        public ISmsRepository SmsRepository { get; private set; }

        public IEmailRepository SecondEmailRepository { get; private set; }

        public Mock<IAutomationPeriodicTriggerScheduler> AutomationPeriodicTriggerSchedulerMock { get; private set; }

        public IPortalAggregateRepository PortalAggregateRepository { get; private set; }

        public IAuthenticationMethodReadModelRepository AuthenticationMethodReadModelRepository { get; private set; }

        public IAuthenticationMethodService AuthenticationMethodService { get; private set; }

        public IAdditionalPropertyTransformHelper AdditionalPropertyTransformHelper { get; private set; }

        public IUserSystemEventEmitter UserSystemEventEmitter { get; private set; }

        public IQuoteSystemEventEmitter QuoteSystemEventEmitter { get; private set; }

        public IUserSessionDeletionService UserSessionDeletionService { get; private set; }

        public Mock<IErrorNotificationService> ErrorNotificationServiceMock { get; private set; } = new Mock<IErrorNotificationService>();

        public AggregateSnapshotService<QuoteAggregate> QuoteAggregateSnapshotService { get; }

        public AggregateSnapshotService<TextAdditionalPropertyValue> TextAdditionalPropertyAggregateSnapshotService { get; }

        public AggregateSnapshotService<AdditionalPropertyDefinition> AdditionalPropertyDefinitionAggregateSnapshotService { get; }

        public AggregateSnapshotService<Organisation> OrganisationAggregateSnapshotService { get; }

        public AggregateSnapshotService<ClaimAggregate> ClaimAggregateSnapshotService { get; }

        public AggregateSnapshotService<PersonAggregate> PersonAggregateSnapshotService { get; }

        public AggregateSnapshotService<UserAggregate> UserAggregateSnapshotService { get; }

        public AggregateSnapshotService<PortalAggregate> PortalAggregateSnapshotService { get; }

        public AggregateSnapshotService<CustomerAggregate> CustomerAggregateSnapshotService { get; }

        public AggregateSnapshotService<FinancialTransactionAggregate<Domain.Accounting.Invoice>> InvoiceAggregateSnapshotService { get; }

        public AggregateSnapshotService<FinancialTransactionAggregate<Domain.Accounting.CreditNote>> CreditNoteAggregateSnapshotService { get; }

        public AggregateSnapshotService<ReportAggregate> ReportAggregateSnapshotService { get; }

        public AggregateSnapshotService<StructuredDataAdditionalPropertyValue> StructuredDataAdditionalPropertyValue { get; }

        public AggregateSnapshotRepository AggregateSnapshotRepository { get; }

        public IAggregateLockingService AggregateLockingService { get; }

        public void Dispose()
        {
            this.DbContext.Dispose();
        }

        public IPaymentService GetPaymentService(PaymentGatewayName gateway)
        {
            var paymentConfigurationProvider = new Mock<IPaymentConfigurationProvider>();
            if (gateway == PaymentGatewayName.Stripe)
            {
                paymentConfigurationProvider
                    .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                    .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(new TestStripeConfiguration())));
            }
            else if (gateway == PaymentGatewayName.Deft)
            {
                paymentConfigurationProvider
                    .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                    .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(new TestDeftConfiguration())));
            }
            else if (gateway == PaymentGatewayName.EWay)
            {
                paymentConfigurationProvider
                    .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                    .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(TestEwayConfigurationFactory.CreateWithValidCredentials())));
            }
            else if (gateway == PaymentGatewayName.Zai)
            {
                paymentConfigurationProvider
                    .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                    .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(new TestZaiConfiguration())));
            }

            var crnGenerator = new Mock<IDeftCustomerReferenceNumberGenerator>();
            crnGenerator
                .Setup(g => g.GenerateDeftCrnNumber(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<CrnGenerationConfiguration>()))
                .Returns("1234567890");
            var paymentGatewayFactory = new PaymentGatewayFactory(
                this.Clock, crnGenerator.Object, this.CachingResolver, this.HttpContextPropertiesResolver, this.Mediator, NullLogger<PaymentGatewayFactory>.Instance);
            return new PaymentService(
                paymentConfigurationProvider.Object,
                paymentGatewayFactory);
        }

        public ICqrsMediator GetPaymentComamndMediator(PaymentGatewayName gatewayName)
        {
            var paymentConfigurationProvider = new Mock<IPaymentConfigurationProvider>();
            if (gatewayName == PaymentGatewayName.EWay)
            {
                paymentConfigurationProvider
                    .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                    .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(TestEwayConfigurationFactory.CreateWithValidCredentials())));
            }
            else if (gatewayName == PaymentGatewayName.Deft)
            {
                paymentConfigurationProvider
                    .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                    .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(new TestDeftConfiguration())));
            }
            else if (gatewayName == PaymentGatewayName.Stripe)
            {
                paymentConfigurationProvider
                    .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                    .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(new TestStripeConfiguration())));
            }
            else if (gatewayName == PaymentGatewayName.Zai)
            {
                paymentConfigurationProvider
                    .Setup(p => p.GetConfigurationAsync(It.IsAny<ReleaseContext>()))
                    .Returns(Task.FromResult(Maybe<IPaymentConfiguration>.From(new TestZaiConfiguration())));
            }

            var crnGenerator = new Mock<IDeftCustomerReferenceNumberGenerator>();
            crnGenerator
                .Setup(g => g.GenerateDeftCrnNumber(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<CrnGenerationConfiguration>()))
                .Returns("1234567890");
            var paymentGatewayFactory = new PaymentGatewayFactory(
                this.Clock, crnGenerator.Object, this.CachingResolver, this.HttpContextPropertiesResolver, this.Mediator, NullLogger<PaymentGatewayFactory>.Instance);
            this.mediatorDependencyProvider.AddTransient<PaymentGatewayFactory>(c => paymentGatewayFactory);
            this.mediatorDependencyProvider.AddTransient<IPaymentConfigurationProvider>(c => paymentConfigurationProvider.Object);
            this.mediatorDependencyProvider.AddTransient<ISavedPaymentMethodRepository>(c => new Mock<ISavedPaymentMethodRepository>().Object);
            var services = this.mediatorDependencyProvider.BuildServiceProvider();

            return services.GetService<ICqrsMediator>();
        }

        /// <summary>
        /// Auto generate policy numbers for a given tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        public void AutoServePolicyNumbersForTenant(Guid tenantId)
        {
            this.PolicyNumberRepository.AutoServeTenant(tenantId);
        }

        internal ActiveDeployedRelease CreateRelease(ReleaseContext context)
        {
            var mockReleaseQueryService = new Mock<IReleaseQueryService>();
            var mockdevReleaseRepository = new Mock<IDevReleaseRepository>();
            var activeDeployedRelease = new Mock<ActiveDeployedRelease>();
            var fakeDevRelease = FakeReleaseBuilder
                .CreateForProduct(context.TenantId, context.ProductId)
                .BuildDevRelease();
            var cachedRelease = new ActiveDeployedRelease(fakeDevRelease, context.Environment, null);
            mockReleaseQueryService.Setup(e => e.GetRelease(It.IsAny<ReleaseContext>())).Returns(cachedRelease);
            mockReleaseQueryService.Setup(e => e.GetReleaseContextForReleaseOrDefaultRelease(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>())).Returns(context);
            mockdevReleaseRepository.Setup(e => e.GetDevReleaseForProductWithoutAssets(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(fakeDevRelease);
            mockReleaseQueryService.Setup(e => e.GetDefaultProductReleaseIdOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>())).Returns(cachedRelease.ReleaseId);
            this.ReleaseQueryService = mockReleaseQueryService.Object;
            this.DevReleaseRepository = mockdevReleaseRepository.Object;

            return cachedRelease;
        }

        internal void CreateTenant(Tenant tenant)
        {
            this.TenantRepository.Insert(tenant);
            this.TenantRepository.SaveChanges();
            this.MockMediator.GetTenantByIdOrAliasQuery(tenant);
        }

        internal void CreateProduct(Product product)
        {
            this.ProductRepository.Insert(product);
            this.ProductRepository.SaveChanges();

            var productFeature =
                new ProductFeatureSetting(
                    product.TenantId,
                    product.Id,
                    this.Clock.Now());
            this.ProductFeatureSettingRepository.AddProductFeatureSetting(productFeature);
            this.MockMediator.GetProductByIdOrAliasQuery(product);
        }

        internal async Task CreateOrganisation(Organisation organisation)
        {
            await this.OrganisationAggregateRepository.Save(organisation);
        }

        internal async Task<CustomerAggregate> CreateCustomerForExistingPerson(
            PersonAggregate customerPerson, DeploymentEnvironment environment, Guid? ownerId = null, Guid? portalId = null)
        {
            CustomerAggregate customer;
            using (var transaction = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                this.DbContext.TransactionStack.Push(transaction);
                try
                {
                    customer = await this.CustomerService.CreateCustomerForExistingPerson(customerPerson, environment, ownerId, portalId);
                }
                finally
                {
                    this.DbContext.TransactionStack.Pop();
                }
            }

            this.DbContext.SaveChanges();
            return customer;
        }
    }
}
