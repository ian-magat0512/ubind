// <copyright file="IncrementAdditionalPropertyValueActionConfigModel.cs" company="uBind">
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
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services.AdditionalPropertyValue;

    /// <summary>
    /// Config model class for increment addtional property value.
    /// </summary>
    public class IncrementAdditionalPropertyValueActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public IncrementAdditionalPropertyValueActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            IBuilder<BaseEntityProvider> entity,
            IBuilder<IProvider<Data<string>>> propertyAlias)
        : base(
              name,
              alias,
              description,
              asynchronous,
              runCondition,
              beforeRunConditions,
              afterRunConditions,
              onErrorActions)
        {
            this.Entity = entity;
            this.PropertyAlias = propertyAlias;
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        public IBuilder<BaseEntityProvider> Entity { get; private set; }

        /// <summary>
        /// Gets the property alias of the additional property definition to look for.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> PropertyAlias { get; private set; }

        public override Action Build(IServiceProvider dependencyProvider)
        {
            var tenantAndProductResolver = dependencyProvider.GetService<ICachingResolver>();
            var textAdditionalPropertyValueRepository =
                dependencyProvider.GetService<ITextAdditionalPropertyValueReadModelRepository>();
            var additionalPropertyDefinitionRepository =
                dependencyProvider.GetService<IAdditionalPropertyDefinitionRepository>();
            var addPropertyService = dependencyProvider.GetService<IAdditionalPropertyValueService>();
            var addPropertyEvalService = dependencyProvider.GetService<PropertyTypeEvaluatorService>();
            var clock = dependencyProvider.GetRequiredService<IClock>();
            var mediator = dependencyProvider.GetRequiredService<ICqrsMediator>();
            return new IncrementAdditionalPropertyValueAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                this.BeforeRunErrorConditions?.Select(c => c.Build(dependencyProvider)),
                this.AfterRunErrorConditions?.Select(c => c.Build(dependencyProvider)),
                this.OnErrorActions?.Select(c => c.Build(dependencyProvider)),
                this.Entity?.Build(dependencyProvider),
                this.PropertyAlias?.Build(dependencyProvider),
                textAdditionalPropertyValueRepository,
                additionalPropertyDefinitionRepository,
                tenantAndProductResolver,
                addPropertyService,
                addPropertyEvalService,
                clock,
                mediator);
        }
    }
}
