// <copyright file="SetAdditionalPropertyValueActionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions.AdditionalPropetyValueActions
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
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services.AdditionalPropertyValue;

    /// <summary>
    /// Model for building an instance of <see cref="SetAdditionalPropertyValueAction"/>.
    /// </summary>
    public class SetAdditionalPropertyValueActionConfigModel : BaseActionConfigurationModel, IBuilder<Actions.Action>
    {
        [JsonConstructor]
        public SetAdditionalPropertyValueActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunErrorConditions,
            IEnumerable<IBuilder<Actions.Action>> onErrorActions,
            IBuilder<BaseEntityProvider> entity,
            IBuilder<IProvider<Data<string>>> propertyAlias,
            IBuilder<IProvider<Data<string>>> value)
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
            this.Entity = entity;
            this.PropertyAlias = propertyAlias;
            this.Value = value;
        }

        public IBuilder<BaseEntityProvider> Entity { get; }

        public IBuilder<IProvider<Data<string>>> PropertyAlias { get; }

        public IBuilder<IProvider<Data<string>>> Value { get; }

        /// <inheritdoc/>
        public override Actions.Action Build(IServiceProvider dependencyProvider)
        {
            var beforeRunConditions = this.BeforeRunErrorConditions?.Select(br => br.Build(dependencyProvider));
            var afterRunConditions = this.AfterRunErrorConditions?.Select(ar => ar.Build(dependencyProvider));
            var errorActions = this.OnErrorActions?.Select(oa => oa.Build(dependencyProvider));
            var entity = this.Entity.Build(dependencyProvider);
            var propertyAlias = this.PropertyAlias.Build(dependencyProvider);
            var propertyValue = this.Value.Build(dependencyProvider);
            var addPropertyService = dependencyProvider.GetService<IAdditionalPropertyValueService>();
            var addPropertyEvalService = dependencyProvider.GetService<PropertyTypeEvaluatorService>();
            var additionalPropertyDefinitionRepository = dependencyProvider.GetService<IAdditionalPropertyDefinitionRepository>();
            var clock = dependencyProvider.GetRequiredService<IClock>();
            var mediator = dependencyProvider.GetRequiredService<ICqrsMediator>();
            return new SetAdditionalPropertyValueAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                beforeRunConditions,
                afterRunConditions,
                errorActions,
                entity,
                propertyAlias,
                propertyValue,
                addPropertyService,
                addPropertyEvalService,
                additionalPropertyDefinitionRepository,
                clock,
                mediator);
        }
    }
}
