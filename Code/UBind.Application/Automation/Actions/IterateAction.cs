// <copyright file="IterateAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Object;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Processing;
    using Void = UBind.Domain.Helpers.Void;

    /// <summary>
    /// This class is needed because we need an automation action that iterates over a list of items and executing a collection of actions for each one.
    /// </summary>
    /// <remarks>Schema reference: #iterateAction.</remarks>
    public class IterateAction : Action, IRunnableParentAction
    {
        private readonly IActionRunner actionRunner;
        private readonly IJobClient backgroundJobClient;
        private readonly IClock clock;
        private IterationItem? currentIteration;
        private int iterationCtr;

        public IterateAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>>? runCondition,
            IEnumerable<ErrorCondition>? beforeRunConditions,
            IEnumerable<ErrorCondition>? afterRunConditions,
            IEnumerable<IRunnableAction>? errorActions,
            IDataListProvider<object> list,
            IProvider<Data<long>>? startIndex,
            IProvider<Data<long>>? endIndex,
            IProvider<Data<bool>>? reverseCondition,
            IProvider<Data<bool>>? doWhileCondition,
            IEnumerable<IRunnableAction> actions,
            IActionRunner actionRunner,
            IJobClient backgroundJobClient,
            IClock clock)
            : base(name, alias, description, asynchronous, runCondition, beforeRunConditions, afterRunConditions, errorActions)
        {
            this.List = list;
            this.StartIndex = startIndex;
            this.EndIndex = endIndex;
            this.IsReversed = reverseCondition;
            this.DoWhileCondition = doWhileCondition;
            this.ChildActions = actions;
            this.actionRunner = actionRunner;
            this.backgroundJobClient = backgroundJobClient;
            this.clock = clock;
        }

        public IDataListProvider<object> List { get; }

        public IProvider<Data<long>>? StartIndex { get; }

        public IProvider<Data<long>>? EndIndex { get; }

        public IProvider<Data<bool>>? IsReversed { get; }

        public IProvider<Data<bool>>? DoWhileCondition { get; }

        public IEnumerable<IRunnableAction> ChildActions { get; } = Enumerable.Empty<IRunnableAction>();

        public override ActionData CreateActionData() => new IterateActionData(this.Name, this.Alias, this.clock);

        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal)
        {
            using (MiniProfiler.Current.Step(nameof(IterateAction) + "." + nameof(this.Execute)))
            {
                IterateActionData iterateActionData = (IterateActionData)actionData;
                actionData.UpdateState(ActionState.Running);
                var dataDictionary = new Dictionary<string, object>();

                var list = (await this.List.Resolve(providerContext)).GetValueOrThrowIfFailed();
                var indexCount = list.Count();
                long? startIndex = (await this.StartIndex.ResolveValueIfNotNull(providerContext))?.DataValue;
                if (startIndex != null)
                {
                    iterateActionData.StartIndex = startIndex;
                }

                long? endIndex = (await this.EndIndex.ResolveValueIfNotNull(providerContext))?.DataValue;
                if (endIndex != null)
                {
                    iterateActionData.EndIndex = endIndex;
                }

                var isReverse = await this.IsExecutionReversed(startIndex, endIndex, providerContext);
                iterateActionData.Reverse = isReverse;

                if (startIndex == null)
                {
                    startIndex = isReverse ? indexCount.DataValue - 1 : 0;
                }

                if (endIndex == null)
                {
                    endIndex = isReverse ? -1 : indexCount.DataValue;
                }

                if (isReverse && indexCount > 0)
                {
                    for (int i = (int)startIndex; i > endIndex; i--)
                    {
                        providerContext.CancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            await this.ExecuteOnIteration(
                                providerContext, list.ElementAt(i), iterateActionData, i, isInternal);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // DO nothing.
                        }
                        catch (ErrorException ex)
                        {
                            return Result.Failure<Void, Domain.Error>(ex.Error);
                        }

                        iterateActionData.IterationsCompleted = this.iterationCtr;
                    }
                }
                else
                {
                    for (var i = (int)startIndex; i < endIndex; i++)
                    {
                        providerContext.CancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            await this.ExecuteOnIteration(
                                providerContext, list.ElementAt(i), iterateActionData, i, isInternal);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // DO nothing.
                        }
                        catch (ErrorException ex)
                        {
                            return Result.Failure<Void, Domain.Error>(ex.Error);
                        }

                        iterateActionData.IterationsCompleted = this.iterationCtr;
                    }
                }

                var iterationData = (IterateActionData)actionData;
                if (iterationData.CurrentIteration != null ||
                    (iterationData.CurrentIteration == null &&
                     iterationData.PreviousIteration != null))
                {
                    var lastIterationValue = this.currentIteration != null ?
                        this.currentIteration.Clone() :
                        iterationData.PreviousIteration?.Clone();
                    iterateActionData.LastIteration = lastIterationValue;
                    iterateActionData.CurrentIteration = null;
                    iterateActionData.PreviousIteration = null;
                }

                return Result.Success<Void, Domain.Error>(default);
            }
        }

        public override bool IsReadOnly()
        {
            return this.AreAllActionsReadOnly(this.ChildActions) && this.AreAllOnErrorActionsReadOnly();
        }

        private async Task ExecuteOnIteration(
            IProviderContext providerContext,
            object listItem,
            IterateActionData iterateActionData,
            int index,
            bool isInternal)
        {
            if (listItem != null)
            {
                var previousIteration = this.currentIteration?.Clone() ?? null;
                this.currentIteration = new IterationItem(listItem, index, this.iterationCtr);
                iterateActionData.CurrentIteration = this.currentIteration;
                bool doWhile = (await this.DoWhileCondition.ResolveValueIfNotNull(providerContext))?.DataValue ?? true;
                if (doWhile)
                {
                    providerContext.CancellationToken.ThrowIfCancellationRequested();

                    if (previousIteration != null)
                    {
                        iterateActionData.PreviousIteration = previousIteration;
                    }

                    foreach (var action in this.ChildActions)
                    {
                        providerContext.CancellationToken.ThrowIfCancellationRequested();

                        var childActionData = action.CreateActionData();
                        this.currentIteration.UpdateActionsExecuted(action.Alias, childActionData);
                        if (action.Asynchronous)
                        {
                            providerContext.AutomationData.Automation.TryGetValue("automation", out object? automationAlias);
                            if (automationAlias == null)
                            {
                                throw new InvalidOperationException(
                                    "Unexpected: when performing iterateAction, the automation alias was not found in the automation data.");
                            }
                            childActionData.ToggleStatusValuesForAsyncActions();
                            await this.actionRunner.HandleAsyncAction(
                                providerContext.AutomationData,
                                action,
                                (string)automationAlias,
                                childActionData,
                                isInternal,
                                providerContext.CurrentActionDataPath + "/currentIteration/actions");
                        }
                        else
                        {
                            await this.actionRunner.HandleAction(
                                providerContext.AutomationData,
                                action,
                                childActionData,
                                providerContext.CancellationToken,
                                isInternal,
                                providerContext.CurrentActionDataPath + "/currentIteration/actions");
                            if (childActionData.Error != null)
                            {
                                throw new ErrorException(childActionData.Error);
                            }
                        }
                    }

                    this.iterationCtr++;
                }
                else
                {
                    iterateActionData.CurrentIteration = null;
                    this.currentIteration = previousIteration?.Clone() ?? null;
                }
            }
        }

        private async Task<bool> IsExecutionReversed(long? startIndex, long? endIndex, IProviderContext providerContext)
        {
            if (startIndex.HasValue && endIndex.HasValue)
            {
                if (startIndex > endIndex)
                {
                    return true;
                }

                if (endIndex > startIndex)
                {
                    return false;
                }
            }

            return (await this.IsReversed.ResolveValueIfNotNull(providerContext))?.DataValue ?? false;
        }
    }
}
