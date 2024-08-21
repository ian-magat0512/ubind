// <copyright file="AdditionalPropertyServiceTestFixture.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Services.AdditionalPropertyValue
{
    using System.Collections.Generic;
    using Hangfire;
    using Hangfire.Storage;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyDefinition;
    using UBind.Domain.Services.AdditionalPropertyValue;

    public class AdditionalPropertyServiceTestFixture
    {
        public AdditionalPropertyServiceTestFixture()
        {
            this.AdditionalPropertyDefinitionAggregateRepositoryMock =
               new Mock<IAdditionalPropertyDefinitionAggregateRepository>(MockBehavior.Strict);
            this.AdditionalPropertyDefinitionRepositoryMock =
                new Mock<IAdditionalPropertyDefinitionRepository>(MockBehavior.Strict);
            this.ClockMock = new Mock<IClock>(MockBehavior.Strict);
            this.AdditionalPropertyValidatorMock = new Mock<IAdditionalPropertyDefinitionValidator>(
                MockBehavior.Strict);
            this.AdditionalPropertyContextValidatorMock =
                new Mock<AdditionalPropertyDefinitionContextValidator>(MockBehavior.Strict);
            this.HttpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>(MockBehavior.Strict);
            this.AdditionalPropertyBackgroundJobSetting = new Mock<IAdditionalPropertyBackgroundJobSetting>();
            this.Mediator = new Mock<ICqrsMediator>(MockBehavior.Strict);
            this.AdditionalPropertyValueServiceMock = new Mock<IAdditionalPropertyValueService>(MockBehavior.Strict);
            this.BackgroundJobClientMock = new Mock<IBackgroundJobClient>(MockBehavior.Strict);
            this.StorageConnectionMock = new Mock<IStorageConnection>(MockBehavior.Strict);
            this.CustomerReadModelRepositoryMock = new Mock<ICustomerReadModelRepository>(MockBehavior.Strict);
            this.QuoteReadModelRepository = new Mock<IQuoteReadModelRepository>(MockBehavior.Strict);
            this.PolicyTransactionReadModelRepositoryMock = new Mock<IPolicyTransactionReadModelRepository>(MockBehavior.Strict);
            this.PolicyReadModelRepositoryMock = new Mock<IPolicyReadModelRepository>(MockBehavior.Strict);
            this.QuoteVersionReadModelRepositoryMock = new Mock<IQuoteVersionReadModelRepository>(MockBehavior.Strict);
            this.ClaimVersionReadModelRepositoryMock = new Mock<IClaimVersionReadModelRepository>(MockBehavior.Strict);
            this.ClaimReadModelRepositoryMock = new Mock<IClaimReadModelRepository>(MockBehavior.Strict);
            this.TextAdditionalPropertyValueRepositoryMock = new Mock<ITextAdditionalPropertyValueReadModelRepository>(MockBehavior.Strict);
            this.TenantRepositoryMock = new Mock<ITenantRepository>(MockBehavior.Strict);
            this.OrganisationReadModelRepositoryMock = new Mock<IOrganisationReadModelRepository>(MockBehavior.Strict);
            this.UserReadModelRepositoryMock = new Mock<IUserReadModelRepository>(MockBehavior.Strict);
            this.ProductRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
            this.PortalRepositoryMock = new Mock<IPortalReadModelRepository>(MockBehavior.Strict);
            this.QuoteAggregateRepositoryMock = new Mock<IQuoteAggregateRepository>(MockBehavior.Strict);
            this.TextAdditionalPropertyValueAggregateRepositoryMock =
                new Mock<ITextAdditionalPropertyValueAggregateRepository>(MockBehavior.Strict);
            this.TextAdditionalPropertyWritableReadModelMock = new Mock<IWritableReadModelRepository<TextAdditionalPropertyValueReadModel>>(
                MockBehavior.Strict);
            this.ClaimAggregateRepositoryMock = new Mock<IClaimAggregateRepository>(MockBehavior.Strict);
            this.CustomerAggregateRepositoryMock = new Mock<ICustomerAggregateRepository>(MockBehavior.Strict);
            this.UserAggregateRepositoryMock = new Mock<IUserAggregateRepository>(MockBehavior.Strict);
            this.OrganisationAggregateRepositoryMock = new Mock<IOrganisationAggregateRepository>(MockBehavior.Strict);
            this.HostEnvironmentMock = new Mock<IHostEnvironment>(MockBehavior.Strict);
            this.AdditionalPropertyDefinitionJsonValidator = new Mock<IAdditionalPropertyDefinitionJsonValidator>(MockBehavior.Strict);
            this.AdditionalPropertyTransformHelperMock = new Mock<IAdditionalPropertyTransformHelper>(MockBehavior.Strict);

            this.Logger = new Mock<ILogger<AdditionalPropertyValueService>>();
            var dictionary = new Dictionary<AdditionalPropertyDefinitionType, IAdditionalPropertyValueProcessor>
            {
                {
                    AdditionalPropertyDefinitionType.Text,
                    new TextAdditionalPropertyValueProcessor(
                        this.TextAdditionalPropertyValueRepositoryMock.Object,
                        this.ClockMock.Object,
                        this.TextAdditionalPropertyValueAggregateRepositoryMock.Object,
                        this.TextAdditionalPropertyWritableReadModelMock.Object)
                },
            };
            this.PropertyTypeEvaluatorService = new PropertyTypeEvaluatorService(dictionary);

            this.AdditionalPropertyValueService = new AdditionalPropertyValueService(
                this.Mediator.Object,
                this.CustomerReadModelRepositoryMock.Object,
                this.QuoteReadModelRepository.Object,
                this.PolicyTransactionReadModelRepositoryMock.Object,
                this.QuoteVersionReadModelRepositoryMock.Object,
                this.ClaimVersionReadModelRepositoryMock.Object,
                this.AdditionalPropertyDefinitionRepositoryMock.Object,
                this.PropertyTypeEvaluatorService,
                this.HttpContextPropertiesResolver.Object,
                this.QuoteAggregateRepositoryMock.Object,
                this.ClaimAggregateRepositoryMock.Object,
                this.CustomerAggregateRepositoryMock.Object,
                this.UserAggregateRepositoryMock.Object,
                this.OrganisationAggregateRepositoryMock.Object,
                this.ClockMock.Object,
                this.AdditionalPropertyDefinitionJsonValidator.Object,
                this.AdditionalPropertyTransformHelperMock.Object);
        }

        public Mock<IAdditionalPropertyDefinitionAggregateRepository>
            AdditionalPropertyDefinitionAggregateRepositoryMock
        { get; private set; }

        public Mock<IAdditionalPropertyDefinitionRepository>
            AdditionalPropertyDefinitionRepositoryMock
        { get; private set; }

        public Mock<IClock> ClockMock { get; private set; }

        public Mock<IAdditionalPropertyDefinitionValidator> AdditionalPropertyValidatorMock { get; private set; }

        public Mock<AdditionalPropertyDefinitionContextValidator> AdditionalPropertyContextValidatorMock { get; private set; }

        public Mock<IHttpContextPropertiesResolver> HttpContextPropertiesResolver { get; private set; }

        public Mock<ICqrsMediator> Mediator { get; }

        public Mock<IAdditionalPropertyValueService> AdditionalPropertyValueServiceMock { get; private set; }

        public Mock<IBackgroundJobClient> BackgroundJobClientMock { get; }

        public Mock<IStorageConnection> StorageConnectionMock { get; }

        public Mock<ILogger<AdditionalPropertyValueService>> Logger { get; }

        public Mock<IAdditionalPropertyBackgroundJobSetting> AdditionalPropertyBackgroundJobSetting { get; }

        public Mock<ICustomerReadModelRepository> CustomerReadModelRepositoryMock { get; }

        public Mock<IQuoteReadModelRepository> QuoteReadModelRepository { get; }

        public Mock<IPolicyTransactionReadModelRepository> PolicyTransactionReadModelRepositoryMock { get; }

        public Mock<IPolicyReadModelRepository> PolicyReadModelRepositoryMock { get; }

        public Mock<IQuoteVersionReadModelRepository> QuoteVersionReadModelRepositoryMock { get; }

        public Mock<IClaimVersionReadModelRepository> ClaimVersionReadModelRepositoryMock { get; }

        public Mock<IClaimReadModelRepository> ClaimReadModelRepositoryMock { get; }

        public Mock<ITextAdditionalPropertyValueReadModelRepository> TextAdditionalPropertyValueRepositoryMock { get; }

        public Mock<ITenantRepository> TenantRepositoryMock { get; }

        public Mock<IOrganisationReadModelRepository> OrganisationReadModelRepositoryMock { get; }

        public Mock<ITextAdditionalPropertyValueAggregateRepository> TextAdditionalPropertyValueAggregateRepositoryMock { get; }

        public Mock<ITextAdditionalPropertyValueEventObserver> TextAdditionalPropertyValueEventObserverMock { get; }

        public Mock<IWritableReadModelRepository<TextAdditionalPropertyValueReadModel>> TextAdditionalPropertyWritableReadModelMock { get; }

        public Mock<IUserReadModelRepository> UserReadModelRepositoryMock { get; }

        public Mock<IProductRepository> ProductRepositoryMock { get; }

        public Mock<IPortalReadModelRepository> PortalRepositoryMock { get; }

        public Mock<IQuoteAggregateRepository> QuoteAggregateRepositoryMock { get; }

        public AdditionalPropertyValueService AdditionalPropertyValueService { get; }

        public PropertyTypeEvaluatorService PropertyTypeEvaluatorService { get; }

        public Mock<IClaimAggregateRepository> ClaimAggregateRepositoryMock { get; }

        public Mock<ICustomerAggregateRepository> CustomerAggregateRepositoryMock { get; }

        public Mock<IUserAggregateRepository> UserAggregateRepositoryMock { get; }

        public Mock<IOrganisationAggregateRepository> OrganisationAggregateRepositoryMock { get; }

        public Mock<IHostEnvironment> HostEnvironmentMock { get; set; }

        public Mock<IAdditionalPropertyDefinitionJsonValidator> AdditionalPropertyDefinitionJsonValidator { get; set; }

        public Mock<IAdditionalPropertyTransformHelper> AdditionalPropertyTransformHelperMock { get; }
    }
}
