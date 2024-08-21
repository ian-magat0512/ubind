// <copyright file="IncrementAdditionalPropertyValueAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Actions.AdditionalPropetyValueActions;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using Void = UBind.Domain.Helpers.Void;

    public class IncrementAdditionalPropertyValueAction : AdditionalPropertyValueActionBase
    {
        private readonly ITextAdditionalPropertyValueReadModelRepository textAdditionalPropertyValueReadModelRepository;
        private readonly IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository;
        private readonly ICachingResolver tenantAndProductResolver;
        private readonly IAdditionalPropertyValueService additionalPropertyService;
        private readonly IClock clock;
        private readonly PropertyTypeEvaluatorService additionalPropertyEvaluatorService;

        public IncrementAdditionalPropertyValueAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>> runCondition,
            IEnumerable<ErrorCondition> beforeRunConditions,
            IEnumerable<ErrorCondition> afterRunConditions,
            IEnumerable<IRunnableAction> errorActions,
            BaseEntityProvider entity,
            IProvider<Data<string>> propertyAlias,
            ITextAdditionalPropertyValueReadModelRepository textAdditionalPropertyValueReadModelRepository,
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository,
            ICachingResolver tenantAndProductResolver,
            IAdditionalPropertyValueService additionalPropertyService,
            PropertyTypeEvaluatorService additionalPropertyEvaluatorService,
            IClock clock,
            ICqrsMediator mediator)
            : base(
                  name,
                  alias,
                  description,
                  asynchronous,
                  runCondition,
                  beforeRunConditions,
                  afterRunConditions,
                  errorActions,
                  additionalPropertyService,
                  additionalPropertyEvaluatorService,
                  additionalPropertyDefinitionRepository,
                  mediator)
        {
            this.Entity = entity;
            this.PropertyAlias = propertyAlias;
            this.textAdditionalPropertyValueReadModelRepository = textAdditionalPropertyValueReadModelRepository;
            this.additionalPropertyDefinitionRepository = additionalPropertyDefinitionRepository;
            this.tenantAndProductResolver = tenantAndProductResolver;
            this.additionalPropertyService = additionalPropertyService;
            this.additionalPropertyEvaluatorService = additionalPropertyEvaluatorService;
            this.clock = clock;
        }

        public BaseEntityProvider Entity { get; }

        public IProvider<Data<string>> PropertyAlias { get; }

        public override ActionData CreateActionData() => new IncrementAdditionalPropertyValueActionData(this.Name, this.Alias, this.clock);

        public override bool IsReadOnly() => false;

        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext, ActionData actionData, bool isInternal = false)
        {
            using (MiniProfiler.Current.Step($"{this.GetType().Name}.{nameof(this.Execute)}"))
            {
                actionData.UpdateState(ActionState.Running);
                var resolveEntity = await this.Entity.Resolve(providerContext);
                var entity = resolveEntity.GetValueOrThrowIfFailed().DataValue;
                var entityType = entity.GetType().Name;
                var resolvePropertyAlias = await this.PropertyAlias.Resolve(providerContext);
                var propertyAlias = resolvePropertyAlias.GetValueOrThrowIfFailed().DataValue;
                var entityId = entity.Id;

                await this.RetrieveErrorDataFromContextAndEntity(providerContext, entity);
                this.AdditionalDetails = new List<string>();

                if (!entity.SupportsAdditionalProperties)
                {
                    throw new ErrorException(
                        Errors.Automation.AdditionalPropertiesNotSupportedOnEntityType(this.ErrorData, entity.GetType().Name));
                }

                if (!Enum.TryParse(entity.GetType().Name, out AdditionalPropertyEntityType addPropEntityType))
                {
                    throw new ErrorException(
                        Errors.Automation.AdditionalPropertiesNotSupportedOnEntityType(this.ErrorData, entityType));
                }

                var setAdditionalPropertyValueActionData = (IncrementAdditionalPropertyValueActionData)actionData;
                setAdditionalPropertyValueActionData.EntityType = entity.GetType().Name;
                setAdditionalPropertyValueActionData.EntityId = entity.Id;

                var property = await this.GetAdditionalPropertyValue(providerContext, entityId, entityType, propertyAlias);

                if (string.IsNullOrEmpty(property.Value))
                {
                    throw new ErrorException(
                        Errors.Automation.Action.ActionRequiresAdditionalPropertyToHaveAValue(
                            propertyAlias, this.ErrorData, entity.GetType().Name));
                }

                var isNumber = int.TryParse(property.Value, out int parsedValue);

                if (!isNumber)
                {
                    throw new ErrorException(Errors.Automation.Action.AdditionalPropertyValueCannotBeParsedAsInteger(
                        propertyAlias, property.Value, this.AdditionalDetails, this.ErrorData, entity.GetType().Name));
                }

                setAdditionalPropertyValueActionData.PreviousValue = parsedValue.ToString();
                int newValue = parsedValue + 1;
                await this.SetAdditionalPropertyValue(
                        providerContext, entity, entityId, entityType, propertyAlias, newValue.ToString());
                setAdditionalPropertyValueActionData.ResultingValue = newValue.ToString();
                return Result.Success<Void, Domain.Error>(default);
            }
        }
    }
}
