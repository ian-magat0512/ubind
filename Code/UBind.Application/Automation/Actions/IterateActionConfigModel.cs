// <copyright file="IterateActionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;
    using UBind.Domain.Processing;

    /// <summary>
    /// Model for building an instance of <see cref="IterateAction"/>.
    /// </summary>
    public class IterateActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public IterateActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>>? runCondition,
            IEnumerable<ErrorConditionConfigModel>? beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel>? afterRunErrorConditions,
            IEnumerable<IBuilder<Action>>? onErrorActions,
            IBuilder<IDataListProvider<object>> list,
            IBuilder<IProvider<Data<long>>>? startIndex,
            IBuilder<IProvider<Data<long>>>? endIndex,
            IBuilder<IProvider<Data<bool>>>? reverse,
            IBuilder<IProvider<Data<bool>>>? doWhileCondition,
            IEnumerable<IBuilder<Action>> actions)
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
            this.List = list;
            this.StartIndex = startIndex;
            this.EndIndex = endIndex;
            this.Reverse = reverse;
            this.DoWhileCondition = doWhileCondition;
            this.Actions = actions;
        }

        [JsonProperty(Required = Required.Always)]
        public IBuilder<IDataListProvider<object>> List { get; private set; }

        public IBuilder<IProvider<Data<long>>>? StartIndex { get; private set; }

        public IBuilder<IProvider<Data<long>>>? EndIndex { get; private set; }

        public IBuilder<IProvider<Data<bool>>>? Reverse { get; private set; }

        public IBuilder<IProvider<Data<bool>>>? DoWhileCondition { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public IEnumerable<IBuilder<Action>> Actions { get; private set; } = Enumerable.Empty<IBuilder<Action>>();

        public override Action Build(System.IServiceProvider dependencyProvider)
        {
            return new IterateAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                this.BeforeRunErrorConditions?.Select(errorCondition => errorCondition.Build(dependencyProvider)),
                this.AfterRunErrorConditions?.Select(errorCondition => errorCondition.Build(dependencyProvider)),
                this.OnErrorActions?.Select(action => action.Build(dependencyProvider)),
                this.List.Build(dependencyProvider),
                this.StartIndex?.Build(dependencyProvider),
                this.EndIndex?.Build(dependencyProvider),
                this.Reverse?.Build(dependencyProvider),
                this.DoWhileCondition?.Build(dependencyProvider),
                this.Actions.Select(act => act.Build(dependencyProvider)),
                dependencyProvider.GetRequiredService<IActionRunner>(),
                dependencyProvider.GetRequiredService<IJobClient>(),
                dependencyProvider.GetRequiredService<IClock>());
        }
    }
}
