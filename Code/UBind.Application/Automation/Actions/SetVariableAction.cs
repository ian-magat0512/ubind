// <copyright file="SetVariableAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using Void = UBind.Domain.Helpers.Void;

    public class SetVariableAction : Action
    {
        /// <summary>
        /// Regex pattern used for validating JSON property keys.
        /// </summary>
        /// <remarks>Key should start with lowercase letter, does not contain any whitespace
        /// and special characters.</remarks>
        private const string PropertyKeyPattern = "^[a-z]+([a-z]|[A-Z]|[0-9])+[a-zA-Z0-9]*$";
        private readonly IClock clock;
        private IServiceProvider dependencyProvider;
        private IProvider<Data<string>>? pathProvider;
        private IObjectProvider propertiesProvider;

        public SetVariableAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>>? runCondition,
            IEnumerable<ErrorCondition>? beforeErrorConditions,
            IEnumerable<ErrorCondition>? afterErrorConditions,
            IEnumerable<Action>? onErrorActions,
            IProvider<Data<string>> propertyName,
            IProvider<IData> value,
            IClock clock,
            IProvider<Data<string>>? pathProvider,
            IObjectProvider propertiesProvider,
            IServiceProvider dependencyProvider)
            : base(name, alias, description, asynchronous, runCondition, beforeErrorConditions, afterErrorConditions, onErrorActions)
        {
            this.dependencyProvider = dependencyProvider;
            this.pathProvider = pathProvider;
            this.propertiesProvider = propertiesProvider;
            this.PropertyName = propertyName;
            this.Value = value;
            this.clock = clock;
        }

        public IProvider<Data<string>> PropertyName { get; }

        public IProvider<IData> Value { get; }

        public override ActionData CreateActionData() => new SetVariableActionData(this.Name, this.Alias, this.clock);

        public override bool IsReadOnly() => this.AreAllOnErrorActionsReadOnly();

        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false)
        {
            using (MiniProfiler.Current.Step(nameof(SetVariableAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);

                string? path = (await this.pathProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                string? propertyName = null;
                object? value = null;

                // if value is present, propertyName should be also present.
                if (this.PropertyName != null)
                {
                    var resolvePropertyName = (await this.PropertyName.Resolve(providerContext)).GetValueOrThrowIfFailed();
                    propertyName = resolvePropertyName.DataValue;
                    if (!this.IsValidPropertyName(propertyName))
                    {
                        var errorData = await providerContext.GetDebugContext();
                        var errorMessage = $"The property name \"{propertyName}\" must only contain valid ASCII characters. It should start with" +
                                " a lowercase letter and must not contain any whitespaces.";
                        errorData.Add(ErrorDataKey.ErrorMessage, errorMessage);
                        var error = Domain.Errors.Automation.Provider.PropertyKeyInvalid(
                            actionData.Type.Humanize(), propertyName, errorData);
                        return Result.Failure<Void, Domain.Error>(error);
                    }
                }

                if (this.Value != null)
                {
                    var variableValue = (await this.Value.Resolve(providerContext)).GetValueOrThrowIfFailed();
                    value = await this.RecursiveCallValue(providerContext, variableValue?.GetValueFromGeneric());
                }
                else if (this.propertiesProvider != null)
                {
                    var resolveValue = (await this.propertiesProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
                    value = resolveValue.DataValue;
                }

                try
                {
                    ((SetVariableActionData)actionData).SetParameters(path!, propertyName!, value!);
                    providerContext.AutomationData.AddOrUpdateVariableByPath(value!, path!, propertyName!);
                }
                catch (ArgumentException ex)
                {
                    var errorData = await providerContext.GetDebugContext();
                    errorData.Add(ErrorDataKey.ErrorMessage, ex.Message);
                    var error = Domain.Errors.Automation.VariableAction.VariableSettingError(this.Alias, ex.Message, errorData);
                    return Result.Failure<Void, Domain.Error>(error);
                }

                return Result.Success<Void, Domain.Error>(default);
            }
        }

        private bool IsValidPropertyName(string variableName)
        {
            // only accept ascii characters for names as part of naming convention.
            if (Encoding.UTF8.GetByteCount(variableName) != variableName.Length)
            {
                return false;
            }

            var match = Regex.Match(variableName, PropertyKeyPattern, RegexOptions.None);
            return match.Success;
        }

        private async Task<object> RecursiveCallValue(IProviderContext providerContext, dynamic variableValue)
        {
            if (!DataObjectHelper.IsArray(variableValue) || variableValue is byte[])
            {
                return variableValue;
            }

            List<object> list = new List<object>();
            foreach (var item in (IEnumerable<object>)variableValue)
            {
                if (!(item is DynamicObjectProviderConfigModel))
                {
                    list.Add(item);
                    continue;
                }

                var resolveValue = await ((DynamicObjectProviderConfigModel)item).Build(this.dependencyProvider).Resolve(providerContext);
                var value = resolveValue.GetValueOrThrowIfFailed().DataValue;
                if (!DataObjectHelper.IsStructuredObjectOrArray(value))
                {
                    list.Add(value);
                    continue;
                }

                list.Add(JObject.FromObject(value));
            }

            return list;
        }
    }
}
