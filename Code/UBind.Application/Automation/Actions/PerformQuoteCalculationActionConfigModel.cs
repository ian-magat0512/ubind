// <copyright file="PerformQuoteCalculationActionConfigModel.cs" company="uBind">
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
    using UBind.Application.Automation.Providers.Object;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    public class PerformQuoteCalculationActionConfigModel : BaseActionConfigurationModel
    {
        [JsonConstructor]
        public PerformQuoteCalculationActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>>? runCondition,
            IEnumerable<ErrorConditionConfigModel>? beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel>? afterRunErrorConditions,
            IEnumerable<IBuilder<Action>>? onErrorActions,
            IBuilder<IProvider<Data<string>>>? quote,
            IBuilder<IProvider<Data<string>>>? policy,
            IBuilder<IProvider<Data<string>>>? product,
            IBuilder<IProvider<Data<string>>>? environment,
            IBuilder<IProvider<Data<string>>>? policyTransactionType,
            IBuilder<IObjectProvider> inputData,
            IBuilder<IObjectProvider>? paymentData,
            IBuilder<IProvider<Data<bool>>>? persistResults)
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
            this.Quote = quote;
            this.Policy = policy;
            this.Product = product;
            this.Environment = environment;
            this.PolicyTransactionType = policyTransactionType;
            this.InputData = inputData;
            this.PaymentData = paymentData;
            this.PersistResults = persistResults;
        }

        public IBuilder<IProvider<Data<string>>>? Quote { get; private set; }

        public IBuilder<IProvider<Data<string>>>? Policy { get; private set; }

        public IBuilder<IProvider<Data<string>>>? Product { get; private set; }

        public IBuilder<IProvider<Data<string>>>? Environment { get; set; }

        public IBuilder<IProvider<Data<string>>>? PolicyTransactionType { get; set; }

        public IBuilder<IObjectProvider> InputData { get; set; }

        public IBuilder<IObjectProvider>? PaymentData { get; set; }

        public IBuilder<IProvider<Data<bool>>>? PersistResults { get; }

        public override Action Build(IServiceProvider dependencyProvider)
        {
            var beforeRunConditions = this.BeforeRunErrorConditions?.Select(br => br.Build(dependencyProvider));
            var afterRunConditions = this.AfterRunErrorConditions?.Select(ar => ar.Build(dependencyProvider));
            var errorActions = this.OnErrorActions?.Select(oa => oa.Build(dependencyProvider));
            return new PerformQuoteCalculationAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                beforeRunConditions,
                afterRunConditions,
                errorActions,
                this.Quote?.Build(dependencyProvider),
                this.Policy?.Build(dependencyProvider),
                this.Product?.Build(dependencyProvider),
                this.Environment?.Build(dependencyProvider),
                this.PolicyTransactionType?.Build(dependencyProvider),
                this.InputData.Build(dependencyProvider),
                this.PaymentData?.Build(dependencyProvider),
                this.PersistResults?.Build(dependencyProvider),
                dependencyProvider.GetRequiredService<ICachingResolver>(),
                dependencyProvider.GetRequiredService<IClock>(),
                dependencyProvider.GetRequiredService<ICqrsMediator>());
        }
    }
}
