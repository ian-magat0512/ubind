// <copyright file="CreateQuoteActionConfigModel.cs" company="uBind">
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
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;

    public class CreateQuoteActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public CreateQuoteActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunErrorConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            string policyTransactionType,
            PolicyEntityProviderConfigModel policyReference,
            OrganisationEntityProviderConfigModel organisationReference,
            ProductEntityProviderConfigModel productReference,
            IBuilder<IProvider<Data<string>>> environment,
            IBuilder<IObjectProvider> formData,
            CustomerEntityProviderConfigModel customerReference,
            IBuilder<IProvider<Data<string>>> initialQuoteState,
            IBuilder<IObjectProvider> additionalProperties,
            IBuilder<IProvider<Data<bool>>> testData)
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
            this.PolicyTransactionType = policyTransactionType;
            this.PolicyReference = policyReference;
            this.OrganisationReference = organisationReference;
            this.ProductReference = productReference;
            this.Environment = environment;
            this.FormData = formData;
            this.CustomerReference = customerReference;
            this.InitialQuoteState = initialQuoteState;
            this.AdditionalProperties = additionalProperties;
            this.TestData = testData;
        }

        [JsonProperty("policyTransactionType")]
        public string PolicyTransactionType { get; }

        [JsonProperty("policy")]
        public PolicyEntityProviderConfigModel PolicyReference { get; }

        [JsonProperty("organisation")]
        public OrganisationEntityProviderConfigModel OrganisationReference { get; }

        [JsonProperty("product")]
        public ProductEntityProviderConfigModel ProductReference { get; }

        [JsonProperty("environment")]
        public IBuilder<IProvider<Data<string>>> Environment { get; }

        [JsonProperty("formData")]
        public IBuilder<IObjectProvider> FormData { get; }

        [JsonProperty("customer")]
        public CustomerEntityProviderConfigModel CustomerReference { get; }

        [JsonProperty("initialQuoteState")]
        public IBuilder<IProvider<Data<string>>> InitialQuoteState { get; }

        [JsonProperty("additionalProperties")]
        public IBuilder<IObjectProvider> AdditionalProperties { get; }

        [JsonProperty("testData")]
        public IBuilder<IProvider<Data<bool>>> TestData { get; }

        public override Action Build(IServiceProvider dependencyProvider)
        {
            return new CreateQuoteAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                this.BeforeRunErrorConditions?.Select(bc => bc.Build(dependencyProvider)),
                this.AfterRunErrorConditions?.Select(ac => ac.Build(dependencyProvider)),
                this.OnErrorActions?.Select(ea => ea.Build(dependencyProvider)),
                this.PolicyTransactionType,
                this.PolicyReference?.Build(dependencyProvider) as PolicyEntityProvider,
                this.OrganisationReference?.Build(dependencyProvider) as OrganisationEntityProvider,
                this.ProductReference?.Build(dependencyProvider) as ProductEntityProvider,
                this.Environment?.Build(dependencyProvider),
                this.FormData?.Build(dependencyProvider),
                this.CustomerReference?.Build(dependencyProvider) as CustomerEntityProvider,
                this.InitialQuoteState?.Build(dependencyProvider),
                this.AdditionalProperties?.Build(dependencyProvider),
                this.TestData?.Build(dependencyProvider),
                dependencyProvider.GetRequiredService<IProductFeatureSettingService>(),
                dependencyProvider.GetRequiredService<IPolicyReadModelRepository>(),
                dependencyProvider.GetRequiredService<ICustomerReadModelRepository>(),
                dependencyProvider.GetRequiredService<ICachingResolver>(),
                dependencyProvider.GetRequiredService<ICqrsMediator>(),
                dependencyProvider.GetRequiredService<IClock>());
        }
    }
}
