// <copyright file="SetAdditionalPropertyValueAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions.AdditionalPropetyValueActions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using Void = UBind.Domain.Helpers.Void;

    /// <summary>
    /// This class is needed because we need a automation action to set the value of additional property of an entity.
    /// </summary>
    public class SetAdditionalPropertyValueAction : AdditionalPropertyValueActionBase
    {
        private readonly IAdditionalPropertyValueService addPropertyService;
        private readonly PropertyTypeEvaluatorService addpropertyEvaluatorService;
        private readonly IClock clock;

        public SetAdditionalPropertyValueAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>> runCondition,
            IEnumerable<ErrorCondition> beforeRunErrorConditions,
            IEnumerable<ErrorCondition> afterRunErrorConditions,
            IEnumerable<IRunnableAction> onErrorActions,
            IProvider<Data<IEntity>> entity,
            IProvider<Data<string>> propertyAlias,
            IProvider<Data<string>> propertyValue,
            IAdditionalPropertyValueService addPropertyService,
            PropertyTypeEvaluatorService addpropertyEvaluatorService,
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository,
            IClock clock,
            ICqrsMediator mediator)
            : base(
                  name,
                  alias,
                  description,
                  asynchronous,
                  runCondition,
                  beforeRunErrorConditions,
                  afterRunErrorConditions,
                  onErrorActions,
                  addPropertyService,
                  addpropertyEvaluatorService,
                  additionalPropertyDefinitionRepository,
                  mediator)
        {
            this.Entity = entity;
            this.PropertyAlias = propertyAlias;
            this.PropertyValue = propertyValue;
            this.addPropertyService = addPropertyService;
            this.addpropertyEvaluatorService = addpropertyEvaluatorService;
            this.clock = clock;
        }

        public IProvider<Data<IEntity>> Entity { get; }

        public IProvider<Data<string>> PropertyAlias { get; }

        public IProvider<Data<string>> PropertyValue { get; }

        /// <inheritdoc/>
        public override ActionData CreateActionData() => new SetAdditionalPropertyValueActionData(this.Name, this.Alias, this.clock);

        public override bool IsReadOnly() => false;

        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false)
        {
            using (MiniProfiler.Current.Step(nameof(SetAdditionalPropertyValueAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);
                try
                {
                    var resolveEntity = await this.Entity.Resolve(providerContext);
                    var entity = resolveEntity.GetValueOrThrowIfFailed().DataValue;
                    var entityType = entity.GetType().Name;
                    var resolvePropertyAlias = await this.PropertyAlias.Resolve(providerContext);
                    var propertyAlias = resolvePropertyAlias.GetValueOrThrowIfFailed().DataValue;
                    var resolvePropertyValue = await this.PropertyValue.Resolve(providerContext);
                    var newValue = resolvePropertyValue.GetValueOrThrowIfFailed().DataValue;
                    var entityId = entity.Id;

                    this.AdditionalDetails = new List<string>();

                    if (!entity.SupportsAdditionalProperties)
                    {
                        await this.RetrieveErrorDataFromContextAndEntity(providerContext, entity);
                        throw new ErrorException(
                            Errors.Automation.AdditionalPropertiesNotSupportedOnEntityType(
                                this.ErrorData, entity.GetType().Name));
                    }

                    if (!Enum.TryParse(entity.GetType().Name, out AdditionalPropertyEntityType addPropEntityType))
                    {
                        await this.RetrieveErrorDataFromContextAndEntity(providerContext, entity);
                        throw new ErrorException(
                            Errors.Automation.AdditionalPropertiesNotSupportedOnEntityType(this.ErrorData, entityType));
                    }

                    await this.SetAdditionalPropertyValue(
                        providerContext, entity, entityId, entityType, propertyAlias, newValue);

                    var actionProperties = new Dictionary<string, object>();
                    var setAdditionalPropertyValueActionData = (SetAdditionalPropertyValueActionData)actionData;
                    setAdditionalPropertyValueActionData.EntityType = entityType.ToString().ToCamelCase();
                    setAdditionalPropertyValueActionData.EntityId = entity.Id.ToString();
                    setAdditionalPropertyValueActionData.PropertyAlias = propertyAlias.ToCamelCase();
                    setAdditionalPropertyValueActionData.ResultingValue = newValue;

                    return Result.Success<Void, Domain.Error>(default);
                }
                catch (Exception ex)
                {
                    var errorData = await providerContext.GetDebugContext();
                    errorData.Add(ErrorDataKey.ErrorMessage, ex.Message);
                    errorData.Add("source", ex.Source);
                    errorData.Add("stackTrace", ex.StackTrace);

                    var errorDetails = ex is ErrorException exception ?
                        exception.Error :
                        Errors.Automation.ActionExecutionErrorEncountered(this.Alias, errorData);
                    return Result.Failure<Void, Domain.Error>(errorDetails);
                }
            }
        }
    }
}
