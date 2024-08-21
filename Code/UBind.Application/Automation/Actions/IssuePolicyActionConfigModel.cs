// <copyright file="IssuePolicyActionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;

    public class IssuePolicyActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public IssuePolicyActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>>? runCondition,
            IEnumerable<ErrorConditionConfigModel>? beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel>? afterRunErrorConditions,
            IEnumerable<IBuilder<Action>>? onErrorActions,
            QuoteEntityProviderConfigModel? quote,
            CustomerEntityProviderConfigModel? customer,
            OrganisationEntityProviderConfigModel? organisation,
            ProductEntityProviderConfigModel? product,
            IBuilder<IProvider<Data<string>>>? environment,
            IBuilder<IObjectProvider>? inputData,
            IBuilder<IObjectProvider>? additionalProperties,
            IBuilder<IProvider<Data<bool>>>? testData,
            IBuilder<IProvider<Data<string>>>? policyNumber)
            : base(
              name,
              alias,
              description,
              asynchronous,
              runCondition,
              beforeRunErrorConditions,
              afterRunErrorConditions,
              onErrorActions)
        {
            this.Customer = customer;
            this.Organisation = organisation;
            this.Product = product;
            this.Environment = environment;
            this.InputData = inputData;
            this.AdditionalProperties = additionalProperties;
            this.TestData = testData;
            this.Quote = quote;
            this.PolicyNumber = policyNumber;
        }

        public QuoteEntityProviderConfigModel? Quote { get; private set; }

        public CustomerEntityProviderConfigModel? Customer { get; }

        public OrganisationEntityProviderConfigModel? Organisation { get; }

        public ProductEntityProviderConfigModel? Product { get; }

        public IBuilder<IProvider<Data<string>>>? PolicyNumber { get; }

        public IBuilder<IProvider<Data<string>>>? Environment { get; }

        public IBuilder<IObjectProvider>? InputData { get; }

        public IBuilder<IObjectProvider>? AdditionalProperties { get; }

        public IBuilder<IProvider<Data<bool>>>? TestData { get; }

        public override Action Build(IServiceProvider dependencyProvider)
        {
            var beforeRunConditions = this.BeforeRunErrorConditions?.Select(br => br.Build(dependencyProvider));
            var afterRunConditions = this.AfterRunErrorConditions?.Select(ar => ar.Build(dependencyProvider));
            var errorActions = this.OnErrorActions?.Select(oa => oa.Build(dependencyProvider));
            return new IssuePolicyAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                beforeRunConditions,
                afterRunConditions,
                errorActions,
                (QuoteEntityProvider?)this.Quote?.Build(dependencyProvider),
                (CustomerEntityProvider?)this.Customer?.Build(dependencyProvider),
                (OrganisationEntityProvider?)this.Organisation?.Build(dependencyProvider),
                (ProductEntityProvider?)this.Product?.Build(dependencyProvider),
                this.Environment?.Build(dependencyProvider),
                this.InputData?.Build(dependencyProvider),
                this.AdditionalProperties?.Build(dependencyProvider),
                this.TestData?.Build(dependencyProvider),
                dependencyProvider.GetRequiredService<IQuoteAggregateResolverService>(),
                dependencyProvider.GetRequiredService<IProductConfigurationProvider>(),
                dependencyProvider.GetRequiredService<IQuoteWorkflowProvider>(),
                dependencyProvider.GetRequiredService<IHttpContextPropertiesResolver>(),
                dependencyProvider.GetRequiredService<IPolicyService>(),
                dependencyProvider.GetRequiredService<IQuoteAggregateRepository>(),
                dependencyProvider.GetRequiredService<IPersonAggregateRepository>(),
                dependencyProvider.GetRequiredService<IAdditionalPropertyValueService>(),
                dependencyProvider.GetRequiredService<IAdditionalPropertyTransformHelper>(),
                dependencyProvider.GetRequiredService<ICachingResolver>(),
                dependencyProvider.GetRequiredService<IPolicyTransactionTimeOfDayScheme>(),
                dependencyProvider.GetRequiredService<IProductService>(),
                dependencyProvider.GetRequiredService<IReleaseQueryService>(),
                dependencyProvider.GetRequiredService<ICqrsMediator>(),
                dependencyProvider.GetRequiredService<IClock>(),
                this.PolicyNumber?.Build(dependencyProvider),
                dependencyProvider.GetRequiredService<IPolicyNumberRepository>(),
                dependencyProvider.GetRequiredService<IUBindDbContext>());
        }
    }
}
