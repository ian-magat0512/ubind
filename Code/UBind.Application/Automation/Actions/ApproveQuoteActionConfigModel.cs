// <copyright file="ApproveQuoteActionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

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
    using UBind.Domain.Patterns.Cqrs;

    public class ApproveQuoteActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public ApproveQuoteActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunErrorConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            QuoteEntityProviderConfigModel quote)
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
        }

        [JsonProperty("quote")]
        public QuoteEntityProviderConfigModel Quote { get; set; }

        public override Action Build(IServiceProvider dependencyProvider)
        {
            return new ApproveQuoteAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                this.BeforeRunErrorConditions?.Select(bc => bc.Build(dependencyProvider)),
                this.AfterRunErrorConditions?.Select(ac => ac.Build(dependencyProvider)),
                this.OnErrorActions?.Select(ea => ea.Build(dependencyProvider)),
                this.Quote.Build(dependencyProvider),
                dependencyProvider.GetRequiredService<ICqrsMediator>(),
                dependencyProvider.GetRequiredService<IClock>());
        }
    }
}
